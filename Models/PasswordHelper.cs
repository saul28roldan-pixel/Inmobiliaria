using System.Security.Cryptography;

namespace Inmobiliaria.Models
{
    /// <summary>
    /// Genera y verifica hashes de contraseña usando PBKDF2 (estándar, seguro).
    /// No guarda la contraseña en texto plano, solo el hash + la sal.
    /// </summary>
    public static class PasswordHelper
    {
        private const int Iteraciones = 100_000;
        private const int TamanioSal = 16;   // bytes
        private const int TamanioHash = 32;  // bytes

        // Genera un hash nuevo a partir de una contraseña en texto plano.
        // Formato guardado: "iteraciones.saltEnBase64.hashEnBase64"
        public static string GenerarHash(string password)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanioSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, TamanioHash);

            return $"{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
        }

        // Verifica que una contraseña en texto plano coincida con un hash almacenado.
        public static bool VerificarHash(string password, string hashAlmacenado)
        {
            var partes = hashAlmacenado.Split('.', 3);
            if (partes.Length != 3) return false;

            if (!int.TryParse(partes[0], out int iteraciones)) return false;
            byte[] sal = Convert.FromBase64String(partes[1]);
            byte[] hashGuardado = Convert.FromBase64String(partes[2]);

            byte[] hashIngresado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, HashAlgorithmName.SHA256, hashGuardado.Length);

            return CryptographicOperations.FixedTimeEquals(hashIngresado, hashGuardado);
        }
    }
}