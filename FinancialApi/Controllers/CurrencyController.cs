using FinancialApi.Data;
using FinancialApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinancialApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ILogger<CurrencyController> _logger;
        private readonly FinancialDbContext _context;

        public CurrencyController(ILogger<CurrencyController> logger, FinancialDbContext context)
        {
            _logger = logger;
            _context = context;

            SeedData.Initialize(context);
        }

       
        [HttpGet("~/Currencies")]
        public async Task<List<Currency>> GetCurrencies()
        {
            return await _context.Currencies.ToListAsync();
        }

        [HttpGet("/conversion")]
        public async Task<JsonResult> CurrencyConversion([FromQuery] CurrencyConversionParams conversionParams)
        {
            string result = String.Format("{0:0.##}", _context.Currencies.
                FirstOrDefault(c => c.Code == conversionParams.to).Value / _context.Currencies.
                FirstOrDefault(c => c.Code == conversionParams.from).Value);
            return new JsonResult(new { result });
        }
    }
}
