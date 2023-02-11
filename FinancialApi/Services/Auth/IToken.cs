using FinancialApi.Models.Account;
using FinancialApi.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FinancialApi.Services.Auth
{
    public interface IToken
    {
        string CreateToken(User user);
        RefreshToken GenerateRefreshToken();
        void SetRefreshToken(IResponseCookies cookies, User user, RefreshToken newRefreshToken);

    }
}
