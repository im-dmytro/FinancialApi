using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialApi.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public string Index()
        {
            return "I am index Allowd for authorized";
        }

        [HttpGet("/"), AllowAnonymous]
        public string About()
        {
            return "I am allowed for everyone!";
        }
    }
}
