using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialApi.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        [Authorize]
        [HttpGet]
        public string Index()
        {
            return "I am index Allowd for authorized";
        }

        [HttpGet("/")]
        public string About()
        {
            return "I am allowd for everyone!";
        }
    }
}
