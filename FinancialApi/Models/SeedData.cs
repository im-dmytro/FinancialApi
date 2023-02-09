using FinancialApi.Models;
using FinancialApi.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection.PortableExecutable;

public class SeedData
{
    public static void Initialize(FinancialDbContext context)
    {

        if (context.Currencies.Any())
        {
            return;
        }

        string url = "https://currencyscoop.p.rapidapi.com/latest";
        HttpClient client = new HttpClient(); ;

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://currencyscoop.p.rapidapi.com/latest"),
            Headers =
            {
                { "X-RapidAPI-Key", "dfc6494c3cmsh52d7aacf8f1df36p1d6cb9jsn2cfc75e705e9" },
                { "X-RapidAPI-Host", "currencyscoop.p.rapidapi.com" },
            },
        };
        var json = client.Send(request).Content.ReadAsStringAsync().Result;
        JObject parsedJson = JObject.Parse(json);
        JToken ratesToken = parsedJson["response"]["rates"];
        Dictionary<string, string> rates = ratesToken.ToObject<Dictionary<string, string>>();
        foreach (var item in rates)
        {
            context.Currencies.Add(new Currency
            {
                Code = item.Key,
                Value = Decimal.Parse(item.Value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
CultureInfo.InvariantCulture)
            });
        }


        context.SaveChanges();

    }
}
