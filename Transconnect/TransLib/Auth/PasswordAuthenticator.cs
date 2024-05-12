using System.Security.Cryptography;

namespace TransLib.Auth {
    public static class PasswordAuthenticator {
        /// <summary>
        /// Generates a random salt for password hashing, with a 128-bit length.
        /// </summary>
        /// <returns>The generated salt.</returns>
        private static byte[] generate_salt() {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Hashes a password/salt with SHA512.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>B64-encoded hash including the salt.</returns>
        public static string hash_password(string password) {
            byte[] salt = generate_salt();
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);

            byte[] hash = pbkdf2.GetBytes(32);
            byte[] hash_bytes = new byte[1 + 32 + 16];
            // store the version number in the first byte
            hash_bytes[0] = 0x01;
            // copy the salt to the next 16 bytes
            Array.Copy(salt, 0, hash_bytes, 1, 16);
            // copy the hash to the next 32 bytes
            Array.Copy(hash, 0, hash_bytes, 17, 32);

            // b64-encode the hash
            return System.Convert.ToBase64String(hash_bytes);
        }

        /// <summary>
        /// Verifies a password against a hash.
        /// </summary>
        /// <param name="password">The password from the user input.</param>
        /// <param name="hash">The stored HashedPassword from the DB.</param>
        /// <returns>True if the password matches the hash, false otherwise.</returns>
        public static bool verify_password(string password, string hash) {
            // b64-decode the hash
            byte[] hash_bytes = System.Convert.FromBase64String(hash);
            
            // check the version number
            if (hash_bytes[0] != 0x01) {
                return false;
            }
            
            // extract salt from the next 16 bytes
            byte[] salt = new byte[16];
            Array.Copy(hash_bytes, 0, salt, 1, 16);

            // hash the password with the retrieved salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
            byte[] hash_to_check = pbkdf2.GetBytes(32);

            // compare the hashes
            for (int i = 0; i < 32; i++) {
                if (hash_bytes[i + 17] != hash_to_check[i]) {
                    return false;
                }
            }

            return true;
        }

        public struct PasswordAuthenticationResult {
            public bool success;
            public string message;

            public PasswordAuthenticationResult(bool success, string message) {
                this.success = success;
                this.message = message;
            }
        }
    }
}