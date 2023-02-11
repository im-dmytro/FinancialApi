using System.Security.Cryptography;
using System.Text;

namespace FinancialApi.Services.Auth
{
    public interface IPassword
    {
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
