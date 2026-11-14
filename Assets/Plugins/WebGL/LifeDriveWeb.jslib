// Funciones del navegador que usa el juego en WebGL (ver ArchivosWeb.cs).
mergeInto(LibraryManager.library, {

  // Descarga un archivo con el navegador (por ejemplo, el PDF del reporte).
  LD_DescargarArchivo: function (nombre, datos, longitud, tipo) {
    var bytes = HEAPU8.slice(datos, datos + longitud);
    var blob = new Blob([bytes], { type: UTF8ToString(tipo) });
    var url = URL.createObjectURL(blob);
    var enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = UTF8ToString(nombre);
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
    setTimeout(function () { URL.revokeObjectURL(url); }, 2000);
  }
});
