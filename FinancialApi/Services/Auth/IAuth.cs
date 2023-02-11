using FinancialApi.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace FinancialApi.Services.Auth
{
    public interface IAuth
    {
        Task<ActionResult<User>> Register(UserDto request);
        Task<ActionResult<string>> Login(UserDto request);
        Task<ActionResult<string>> Logout();
        Task<ActionResult<string>> RefreshToken();

    }
}
