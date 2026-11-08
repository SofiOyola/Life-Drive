using System;
using NUnit.Framework;
using UnityEditor;
using LifeDrive.Mapas;
using LifeDrive.Mapas.Demo;

namespace LifeDrive.Tests
{
    public class RecorridoVisualValidadoTests
    {
        [Test]
        public void OchoRutasAlcanzanDeliverySinSaltos()
        {
            var catalogo=AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
            var rutas=RecorridosVisualesDemo.Obtener(catalogo);
            Assert.AreEqual(8,rutas.Length);
            foreach(var ruta in rutas)
            {
                var cursor=new RecorridoVisualValidado(catalogo);
                Assert.Throws<InvalidOperationException>(()=>cursor.AvanzarConfirmado("INEXISTENTE"));
                Assert.Throws<InvalidOperationException>(()=>cursor.AvanzarConfirmado("DELIVERY"));
                for(int i=1;i<ruta.Length;i++)cursor.AvanzarConfirmado(ruta[i].Id);
                Assert.IsTrue(cursor.Entregado);
                Assert.AreEqual(5,cursor.Historial.Length);
                Assert.Throws<InvalidOperationException>(()=>cursor.AvanzarConfirmado("S0"));
            }
        }
    }
}
