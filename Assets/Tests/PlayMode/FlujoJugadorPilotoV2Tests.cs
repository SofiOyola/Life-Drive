#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using LifeDrive.Datos;
using LifeDrive.IntegracionV2;
using LifeDrive.Mapas;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Fase = LifeDrive.IntegracionV2.PruebaIntegracionV2.Fase;

namespace LifeDrive.Tests
{
    public partial class FlujoJugadorPilotoV2Tests
    {
        static readonly int[] Rutas = { 0, 1, 2, 3, 4, 5, 6, 7 };
        static readonly int[] ResolucionesApariencia = { 1920, 1280 };
        static readonly string[] Hojas = { "R3A", "R3B", "R3C", "R3D", "R3E", "R3F", "R3G", "R3H" };
        FlujoJugadorPilotoV2 piloto;
        string sesion, carpeta;
        float escala;
        int ancho, alto, indicePrevio;
        EditorWindow ventana;
        PropertyInfo propiedadIndice;

        [SetUp]
        public void Preparar()
        {
            escala = Time.timeScale; Time.timeScale = 28;
            sesion = Guid.NewGuid().ToString("N");
        }

        [UnityTearDown]
        public IEnumerator Limpiar()
        {
            Time.timeScale = escala;
            if (piloto != null) Object.Destroy(piloto.gameObject);
            yield return null;
            if (carpeta != null && Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
            if (ventana != null && propiedadIndice != null) propiedadIndice.SetValue(ventana, indicePrevio);
        }

        static void Cargar(string nombre) => EditorSceneManager.LoadSceneInPlayMode(
            "Assets/Scenes/" + nombre + ".unity", new LoadSceneParameters(LoadSceneMode.Single));

        void Activar()
        {
            piloto = FlujoJugadorPilotoV2.Activar(AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset"), sesion, Cargar);
            carpeta = piloto.CarpetaDatos;
        }

        IEnumerator Pantalla(string nombre)
        {
            float limite = Time.realtimeSinceStartup + 12;
            while (SceneManager.GetActiveScene().name != nombre)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite), nombre);
                yield return null;
            }
            yield return null; yield return null;
        }

        void Resolucion(bool grande)
        {
            ancho = grande ? 1920 : 1280; alto = grande ? 1080 : 720;
            var asm = typeof(UnityEditor.Editor).Assembly;
            var tipo = asm.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(tipo);
            var sizes = singleton.GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            var group = tipo.GetProperty("currentGroup").GetValue(sizes);
            var gt = group.GetType();
            int total = (int)gt.GetMethod("GetTotalCount").Invoke(group, null);
            int indice = -1;
            for (int i = 0; i < total; i++)
            {
                var size = gt.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                if ((int)size.GetType().GetProperty("width").GetValue(size) == ancho &&
                    (int)size.GetType().GetProperty("height").GetValue(size) == alto) { indice = i; break; }
            }
            if (indice < 0)
            {
                var size = Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),
                    Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"), 1), ancho, alto, "Piloto V2 " + ancho);
                gt.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                indice = total;
            }
            ventana = EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));
            propiedadIndice = ventana.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            indicePrevio = (int)propiedadIndice.GetValue(ventana);
            propiedadIndice.SetValue(ventana, indice);
            ventana.Focus(); ventana.Repaint();
        }

        IEnumerator Evidencia(string pantalla, int ruta)
        {
            if (ruta > 1) yield break;
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width, Is.EqualTo(ancho)); Assert.That(Screen.height, Is.EqualTo(alto));
            foreach (var texto in Object.FindObjectsByType<TMP_Text>())
            {
                if (!texto.gameObject.activeInHierarchy || !texto.enabled) continue;
                Assert.That(texto.rectTransform.rect.width, Is.GreaterThan(0), texto.name);
            }
            var imagen = ScreenCapture.CaptureScreenshotAsTexture();
            string destino = Path.GetFullPath(Path.Combine(Application.dataPath,
                pantalla.StartsWith("Apariencia") ? "../TestEvidence/PersonalizacionV2" :
                pantalla.StartsWith("Gamificacion") ? "../TestEvidence/GamificacionV2" : "../TestEvidence/FlujoJugadorV2"));
            Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, pantalla + "-" + ancho + "x" + alto + ".png"), imagen.EncodeToPNG());
            Object.Destroy(imagen);
        }

        IEnumerator Registrar(int ruta)
        {
            yield return Pantalla("Inicio");
            yield return Evidencia("Inicio", ruta);
            var inicio = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            Click(inicio.jugar);
            yield return Pantalla("Registro");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            v.nombre.text = "Piloto Ficticio"; v.documento.text = "990033";
            v.ElegirEstilo(1); v.ElegirColor(0, 2); v.ElegirColor(1, 3); v.ElegirColor(2, 4); v.ElegirColor(3, 2);
            int jugadoresPrevios = piloto.Juego.BaseDatos.Jugadores.Count;
            Click(v.comprobar);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Registro"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Count, Is.EqualTo(jugadoresPrevios), "No elegir ID debe impedir el registro.");
            Click(Boton("Ficha piloto 3"));
            yield return null;
            foreach (var ficha in Object.FindObjectsByType<Button>().Where(b => b.name.StartsWith("Ficha piloto ")))
            {
                var etiqueta = ficha.GetComponentInChildren<TMP_Text>();
                etiqueta.ForceMeshUpdate();
                Assert.That(etiqueta.textInfo.characterInfo.Take(etiqueta.textInfo.characterCount).Any(c => c.isVisible), Is.True, ficha.name);
            }
            yield return Evidencia("Registro", ruta);
            Click(v.comprobar); Click(v.comprobar);
            yield return Pantalla("PruebaIntegracionV2");
            var jugador = new BaseDatos(carpeta).Jugadores.Single();
            Assert.That(jugador.nombre, Is.EqualTo("Piloto Ficticio"));
            Assert.That(jugador.cedula, Is.EqualTo("990033")); Assert.That(jugador.personaje_id, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator InicioAResultadoOchoRutas([ValueSource(nameof(Rutas))] int ruta)
        {
            Resolucion(ruta % 2 == 0);
            Activar();
            yield return Registrar(ruta);
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            int bifurcacion = 0, respuestas = 0;
            while (prueba.Estado != Fase.Entregada)
            {
                yield return Esperar(prueba, Fase.Situacion);
                yield return Evidencia("Partida", ruta);
                int id = prueba.SituacionVisibleId;
                int indice = id == 5 || id >= 101 ? (ruta >> (2 - bifurcacion++)) & 1 : ruta == 1 && id == 1 ? 1 : 0;
                var boton = Boton("Respuesta " + (indice + 1));
                Click(boton); Click(boton); respuestas++;
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                Assert.That(new BaseDatos(carpeta).Decisiones.Count, Is.EqualTo(respuestas));
                int delta = prueba.Gamificacion.UltimaDecision.puntos_aplicados;
                yield return Evidencia("GamificacionCambio" + (delta > 0 ? "Positivo" : delta < 0 ? "Negativo" : "Neutro"), ruta);
                if (respuestas == 1)
                {
                    Click(Boton("Reconocimientos"));
                    yield return Evidencia("GamificacionReconocimientos", ruta);
                    Click(Boton("Cerrar reconocimientos"));
                }
                Click(Boton("Continuar"));
            }
            Assert.That(prueba.MapaVisible, Is.EqualTo("DELIVERY"));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PruebaIntegracionV2"), "La entrega debe preceder a Resultado.");
            var datos = new BaseDatos(carpeta);
            int puntos = datos.Partidas.Single().puntos;
            Assert.That(puntos, Is.EqualTo(datos.Decisiones.Sum(d => d.puntos_aplicados)));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados), Is.EqualTo(respuestas));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PartidaFinalizada), Is.EqualTo(1));
            Click(Boton("Nueva prueba"));
            yield return Pantalla("Resultado");
            var vista = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            Assert.That(vista.enabled, Is.False);
            Assert.That(vista.titulo.text, Is.EqualTo("Resumen de la operación"));
            Assert.That(vista.puntos.text, Is.EqualTo(puntos + " pts de seguridad"));
            Assert.That(vista.resumen.text, Does.Contain("Buenas prácticas registradas: " + datos.Decisiones.Count(d => d.es_buena_practica)));
            vista.puntos.ForceMeshUpdate();
            Assert.That(vista.puntos.isTextTruncated, Is.False);
            Assert.That(vista.educativo.text, Does.Contain(Hojas[ruta]));
            Assert.That(vista.educativo.text, Does.EndWith("DELIVERY"));
            Assert.That(vista.retrato.GetComponentInChildren<AvatarPersonalizableUI>().mujer, Is.True);
            Assert.That(vista.estadosInsignias.All(t => t.text == "PENDIENTE DE REGLAS APROBADAS"), Is.True);
            yield return Evidencia("Resultado", ruta);
            Click(vista.revisar);
            for (int pagina = 0; pagina * vista.filasDecisiones.Length < respuestas; pagina++)
            {
                for (int i = 0; i < vista.filasDecisiones.Length && pagina * vista.filasDecisiones.Length + i < respuestas; i++)
                    Assert.That(vista.filasDecisiones[i].text, Does.Contain(datos.Decisiones[pagina * vista.filasDecisiones.Length + i].consecuencia));
                if (vista.selectores[1].interactable) Click(vista.selectores[1]);
            }
            while (vista.selectores[0].interactable) Click(vista.selectores[0]);
            yield return Evidencia("Historial", ruta);
            Click(vista.volver);
            yield return Pantalla("Inicio");
            Assert.That(new BaseDatos(carpeta).Decisiones.Count, Is.EqualTo(respuestas));
        }

        [UnityTest]
        public IEnumerator CierreDelPilotoRetomaConsecuenciaSinEvaluarDeNuevo()
        {
            Activar(); yield return Registrar(7);
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            yield return Esperar(prueba, Fase.Situacion);
            Click(Boton("Respuesta 1")); Click(Boton("Continuar"));
            yield return Esperar(prueba, Fase.Situacion);
            Click(Boton("Respuesta 2")); Click(Boton("Continuar"));
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Transicion));
            int puntos = new BaseDatos(carpeta).Partidas.Single().puntos;
            Object.Destroy(piloto.gameObject); yield return null;
            Activar(); yield return Registrar(7);
            prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Assert.That(prueba.MapaVisible, Is.EqualTo("S0"));
            Assert.That(new BaseDatos(carpeta).Decisiones.Count, Is.EqualTo(2));
            Assert.That(new BaseDatos(carpeta).Partidas.Single().puntos, Is.EqualTo(puntos));
            Click(Boton("Continuar")); yield return Esperar(prueba, Fase.Situacion);
            Assert.That(prueba.MapaVisible, Is.EqualTo("R1B"));
            Assert.That(new BaseDatos(carpeta).Decisiones.Count, Is.EqualTo(2));
        }

        IEnumerator Esperar(PruebaIntegracionV2 prueba, Fase fase)
        {
            float limite = Time.realtimeSinceStartup + 12;
            while (prueba.Estado != fase)
            {
                Assert.That(prueba.Estado, Is.Not.EqualTo(Fase.Error));
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite));
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator RecuperaAparienciaTrasCerrarSinSobrescribirDefaults([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            Resolucion(resolucion == 1920); Activar(); yield return Registrar(7);
            var original = piloto.Apariencias.LeerPerfilActual();
            Assert.That(original.personaje_id, Is.EqualTo(3));
            Assert.That(original.presentacion, Is.EqualTo("mujer"));
            Assert.That(original.piel, Is.EqualTo("canela")); Assert.That(original.ojos, Is.EqualTo("miel"));
            Assert.That(original.cabello, Is.EqualTo("plata")); Assert.That(original.traje, Is.EqualTo("terracota"));
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            yield return Esperar(prueba, Fase.Situacion); Click(Boton("Respuesta 1"));
            int puntos = new BaseDatos(carpeta).Partidas.Single().puntos;
            Object.Destroy(piloto.gameObject); yield return null;
            Activar(); yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            v.nombre.text = "Piloto Ficticio"; v.documento.text = "990033";
            Assert.That(v.Estilo, Is.Zero, "El formulario nuevo empieza con la vista base.");
            Click(Boton("Ficha piloto 4")); Click(v.comprobar); yield return Pantalla("PruebaIntegracionV2");
            Assert.That(piloto.PersonajeId, Is.EqualTo(3), "El ID existente tiene prioridad sobre la ficha solicitada.");
            Assert.That(JsonUtility.ToJson(piloto.Apariencias.LeerPerfilActual()), Is.EqualTo(JsonUtility.ToJson(original)));
            Assert.That(JsonUtility.ToJson(piloto.AparienciaPartidaActual), Is.EqualTo(JsonUtility.ToJson(original)));
            prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(puntos)); Assert.That(new BaseDatos(carpeta).Decisiones.Count, Is.EqualTo(1));

            piloto.Navegar("Inicio"); yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>(); v.nombre.text = "Piloto Ficticio"; v.documento.text = "990033";
            Click(Boton("Recuperar apariencia"));
            Assert.That(v.Estilo, Is.EqualTo(1)); Assert.That(v.Selecciones, Is.EqualTo(new[] { 2, 3, 4, 2 }));
            Assert.That(v.avatar.traje, Is.EqualTo(VariantesAparienciaPilotoV1.ColorDe(3, 2)));
            yield return Evidencia("AparienciaRegistroRecuperado", 0);
            Click(v.comprobar); yield return Pantalla("PruebaIntegracionV2");
            var datos = new BaseDatos(carpeta);
            Assert.That(datos.Eventos.Count(e => e.tipo == PersistenciaAparienciaPilotoV2.EventoPerfil), Is.EqualTo(1));
            Assert.That(datos.Eventos.Count(e => e.tipo == PersistenciaAparienciaPilotoV2.EventoPartida), Is.EqualTo(1));
            Assert.That(datos.Decisiones.Count, Is.EqualTo(1)); Assert.That(datos.Partidas.Single().puntos, Is.EqualTo(puntos));

            // A later profile edit must not change the appearance already fixed to this match.
            piloto.Apariencias.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(3, 0, 4, 4, 4, 4));
            prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            yield return Terminar(prueba);
            Click(Boton("Nueva prueba")); yield return Pantalla("Resultado");
            var resultado = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            var avatar = resultado.retrato.GetComponentInChildren<AvatarPersonalizableUI>();
            Assert.That(avatar.mujer, Is.True); Assert.That(avatar.piel, Is.EqualTo(VariantesAparienciaPilotoV1.ColorDe(0, 2)));
            Assert.That(avatar.ojos, Is.EqualTo(VariantesAparienciaPilotoV1.ColorDe(1, 3)));
            Assert.That(avatar.cabello, Is.EqualTo(VariantesAparienciaPilotoV1.ColorDe(2, 4)));
            Assert.That(avatar.traje, Is.EqualTo(VariantesAparienciaPilotoV1.ColorDe(3, 2)));
            Assert.That(resultado.personaje.text, Does.Contain("Ficha 3 · apariencia guardada"));
            yield return Evidencia("AparienciaResultado", 0);
        }

        [UnityTest]
        public IEnumerator JugadorAntiguoRecuperaBaseYConservaIdOriginal()
        {
            Activar(); yield return Pantalla("Inicio");
            var j = piloto.Juego.Usuarios.RegistrarJugador("Antiguo Ficticio", "99003603", 3);
            piloto.Juego.RecorridosV2.IniciarPartida();
            Assert.That(piloto.Apariencias.LeerPerfilActual(), Is.Null);
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>(); v.nombre.text = j.nombre; v.documento.text = j.cedula;
            Click(Boton("Recuperar apariencia"));
            Assert.That(v.mensaje.text, Does.Contain("sin apariencia guardada"));
            Assert.That(piloto.PersonajeId, Is.EqualTo(3)); Assert.That(piloto.Apariencias.LeerPerfilActual(), Is.Null);
            v.ElegirEstilo(1); v.ElegirColor(3, 4); Click(Boton("Ficha piloto 1")); Click(v.comprobar);
            yield return Pantalla("PruebaIntegracionV2");
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Single().personaje_id, Is.EqualTo(3));
            Assert.That(piloto.AparienciaPartidaActual.traje, Is.EqualTo("ciruela"));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().puntos, Is.Zero);
            Assert.That(piloto.Juego.BaseDatos.Decisiones, Is.Empty);
        }

        IEnumerator Terminar(PruebaIntegracionV2 prueba)
        {
            for (int paso = 0; paso < 10 && prueba.Estado != Fase.Entregada; paso++)
            {
                if (prueba.Estado != Fase.Consecuencia)
                { yield return Esperar(prueba, Fase.Situacion); Click(Boton("Respuesta 1")); }
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia)); Click(Boton("Continuar"));
            }
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Entregada));
        }

        [UnityTest]
        public IEnumerator FalloAlFijarAparienciaSeReintentaAntesDeContinuar()
        {
            Activar(); yield return Pantalla("Inicio");
            piloto.Juego.Usuarios.RegistrarJugador("Fallo Ficticio", "99003604", 2);
            var apariencia = VariantesAparienciaPilotoV1.Crear(2, 1, 4, 3, 2, 1);
            piloto.Apariencias.GuardarPerfilActual(apariencia);
            var partida = piloto.Juego.RecorridosV2.IniciarPartida();
            string bloqueo = Path.Combine(carpeta, "lifedrive-db.json.tmp"); Directory.CreateDirectory(bloqueo);
            piloto.Navegar("PruebaIntegracionV2"); yield return Pantalla("PruebaIntegracionV2");
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Error));
            Assert.That(piloto.Apariencias.LeerPartida(partida.id), Is.Null);
            Assert.That(piloto.Juego.BaseDatos.Decisiones, Is.Empty);
            Directory.Delete(bloqueo);
            Click(Boton("Reinicio visual"));
            Assert.That(prueba.Estado, Is.Not.EqualTo(Fase.Error));
            Assert.That(JsonUtility.ToJson(piloto.AparienciaPartidaActual), Is.EqualTo(JsonUtility.ToJson(apariencia)));
            Click(Boton("Reinicio visual"));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == PersistenciaAparienciaPilotoV2.EventoPartida), Is.EqualTo(1));
            Assert.That(piloto.Juego.BaseDatos.Decisiones, Is.Empty); Assert.That(piloto.Juego.BaseDatos.Partidas.Single().puntos, Is.Zero);
        }
        static Button Boton(string nombre) => Object.FindObjectsByType<Button>(FindObjectsInactive.Include).Single(b => b.name == nombre);
        static void Click(Button boton) => ExecuteEvents.Execute(boton.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    }
}
#endif
