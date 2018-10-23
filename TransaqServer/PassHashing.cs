using System;
using System.Security.Cryptography;

namespace TransaqServer
{
    public static class PassHashing
    {
        public static string GetPasswordHashWithSalt(string password)
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            var hash = new Rfc2898DeriveBytes(password, salt, 1000).GetBytes(20);
            var hashPlusSalt = new byte[36];
            Array.Copy(salt, 0, hashPlusSalt, 0, 16);
            Array.Copy(hash, 0, hashPlusSalt, 16, 20);
            return Convert.ToBase64String(hashPlusSalt);
        }

        public static bool CheckPasssword(string dbPass, string inputPass)
        {
            var hashPlusSalt = Convert.FromBase64String(dbPass);
            var salt = new byte[16];
            Array.Copy(hashPlusSalt, 0, salt, 0, 16);
            var hash = new Rfc2898DeriveBytes(inputPass, salt, 1000).GetBytes(20);
            var ok = true;
            for (var i = 0; i < 20; i++)
            {
                if (hashPlusSalt[i + 16] != hash[i])
                    ok = false;
            }
            return ok;
        }
    }
}