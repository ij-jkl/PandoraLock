using Application.Auth.BCrypt.Interfaces;

namespace Infrastructure.Auth.BCrypt
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly int _workFactor = 12;

        public string Hash(string password) =>
            global::BCrypt.Net.BCrypt.HashPassword(password, workFactor: _workFactor);

        public bool Verify(string password, string hash) => 
            global::BCrypt.Net.BCrypt.Verify(password, hash);
    }
}