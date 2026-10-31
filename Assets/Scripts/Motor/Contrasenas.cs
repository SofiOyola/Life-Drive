// HU-03: solo se guarda un hash PBKDF2-SHA256 con sal; la contraseña nunca queda en texto plano.
using System;
using System.Security.Cryptography;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public static class Contrasenas
    {
        const int Iteraciones = 100_000;

        public static Administrador Crear(string usuario, string contrasena)
        {
            var sal = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(sal);
            return new Administrador
            {
                usuario = usuario,
                sal = Convert.ToBase64String(sal),
                iteraciones = Iteraciones,
                hash = Convert.ToBase64String(Derivar(contrasena, sal, Iteraciones)),
            };
        }

        public static bool Verificar(Administrador admin, string contrasena)
        {
            var esperado = Convert.FromBase64String(admin.hash);
            var calculado = Derivar(contrasena, Convert.FromBase64String(admin.sal), admin.iteraciones);
            var diferencia = esperado.Length ^ calculado.Length;   // comparación en tiempo constante
            for (var i = 0; i < Math.Min(esperado.Length, calculado.Length); i++) diferencia |= esperado[i] ^ calculado[i];
            return diferencia == 0;
        }

        static byte[] Derivar(string contrasena, byte[] sal, int iteraciones)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(contrasena ?? "", sal, iteraciones, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }
    }
}
