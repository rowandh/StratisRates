using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StratisRates
{
    public static class Rates
    {
        public static string RatesUrl =
            "https://api.coingecko.com/api/v3/simple/price?ids=stratis&vs_currencies=btc%2Ceth%2Cltc%2Cbch%2Cbnb%2Ceos%2Cxrp%2Cxlm%2Cusd%2Caed%2Cars%2Caud%2Cbdt%2Cbhd%2Cbmd%2Cbrl%2Ccad%2Cchf%2Cclp%2Ccny%2Cczk%2Cdkk%2Ceur%2Cgbp%2Chkd%2Chuf%2Cidr%2Cils%2Cinr%2Cjpy%2Ckrw%2Ckwd%2Clkr%2Cmmk%2Cmxn%2Cmyr%2Cnok%2Cnzd%2Cphp%2Cpkr%2Cpln%2Crub%2Csar%2Csek%2Csgd%2Cthb%2Ctry%2Ctwd%2Cuah%2Cvef%2Cvnd%2Czar%2Cxdr%2Cxag%2Cxau";

        // Lazy way to map codes to names.
        public static string Names = "{\"btc\":{\"name\":\"Bitcoin\",\"unit\":\"BTC\",\"value\":1.0,\"type\":\"crypto\"},\"eth\":{\"name\":\"Ether\",\"unit\":\"ETH\",\"value\":52.294,\"type\":\"crypto\"},\"ltc\":{\"name\":\"Litecoin\",\"unit\":\"LTC\",\"value\":173.25,\"type\":\"crypto\"},\"bch\":{\"name\":\"Bitcoin Cash\",\"unit\":\"BCH\",\"value\":35.237,\"type\":\"crypto\"},\"bnb\":{\"name\":\"Binance Coin\",\"unit\":\"BNB\",\"value\":519.854,\"type\":\"crypto\"},\"eos\":{\"name\":\"EOS\",\"unit\":\"EOS\",\"value\":2938.547,\"type\":\"crypto\"},\"xrp\":{\"name\":\"XRP\",\"unit\":\"XRP\",\"value\":34717.979,\"type\":\"crypto\"},\"xlm\":{\"name\":\"Lumens\",\"unit\":\"XLM\",\"value\":148800.764,\"type\":\"crypto\"},\"usd\":{\"name\":\"US Dollar\",\"unit\":\"$\",\"value\":6887.644,\"type\":\"fiat\"},\"aed\":{\"name\":\"United Arab Emirates Dirham\",\"unit\":\"DH\",\"value\":25298.842,\"type\":\"fiat\"},\"ars\":{\"name\":\"Argentine Peso\",\"unit\":\"$\",\"value\":411397.92,\"type\":\"fiat\"},\"aud\":{\"name\":\"Australian Dollar\",\"unit\":\"A$\",\"value\":10027.287,\"type\":\"fiat\"},\"bdt\":{\"name\":\"Bangladeshi Taka\",\"unit\":\"৳\",\"value\":585049.397,\"type\":\"fiat\"},\"bhd\":{\"name\":\"Bahraini Dinar\",\"unit\":\"BD\",\"value\":2596.979,\"type\":\"fiat\"},\"bmd\":{\"name\":\"Bermudian Dollar\",\"unit\":\"$\",\"value\":6887.644,\"type\":\"fiat\"},\"brl\":{\"name\":\"Brazil Real\",\"unit\":\"R$\",\"value\":27986.566,\"type\":\"fiat\"},\"cad\":{\"name\":\"Canadian Dollar\",\"unit\":\"CA$\",\"value\":9066.268,\"type\":\"fiat\"},\"chf\":{\"name\":\"Swiss Franc\",\"unit\":\"Fr.\",\"value\":6768.839,\"type\":\"fiat\"},\"clp\":{\"name\":\"Chilean Peso\",\"unit\":\"CLP$\",\"value\":5255262.453,\"type\":\"fiat\"},\"cny\":{\"name\":\"Chinese Yuan\",\"unit\":\"¥\",\"value\":48197.671,\"type\":\"fiat\"},\"czk\":{\"name\":\"Czech Koruna\",\"unit\":\"Kč\",\"value\":157210.041,\"type\":\"fiat\"},\"dkk\":{\"name\":\"Danish Krone\",\"unit\":\"kr.\",\"value\":46169.121,\"type\":\"fiat\"},\"eur\":{\"name\":\"Euro\",\"unit\":\"€\",\"value\":6177.686,\"type\":\"fiat\"},\"gbp\":{\"name\":\"British Pound Sterling\",\"unit\":\"£\",\"value\":5179.646,\"type\":\"fiat\"},\"hkd\":{\"name\":\"Hong Kong Dollar\",\"unit\":\"HK$\",\"value\":53663.361,\"type\":\"fiat\"},\"huf\":{\"name\":\"Hungarian Forint\",\"unit\":\"Ft\",\"value\":2030803.853,\"type\":\"fiat\"},\"idr\":{\"name\":\"Indonesian Rupiah\",\"unit\":\"Rp\",\"value\":96424710.345,\"type\":\"fiat\"},\"ils\":{\"name\":\"Israeli New Shekel\",\"unit\":\"₪\",\"value\":24088.71,\"type\":\"fiat\"},\"inr\":{\"name\":\"Indian Rupee\",\"unit\":\"₹\",\"value\":488790.664,\"type\":\"fiat\"},\"jpy\":{\"name\":\"Japanese Yen\",\"unit\":\"¥\",\"value\":754569.023,\"type\":\"fiat\"},\"krw\":{\"name\":\"South Korean Won\",\"unit\":\"₩\",\"value\":8024932.555,\"type\":\"fiat\"},\"kwd\":{\"name\":\"Kuwaiti Dinar\",\"unit\":\"KD\",\"value\":2090.062,\"type\":\"fiat\"},\"lkr\":{\"name\":\"Sri Lankan Rupee\",\"unit\":\"Rs\",\"value\":1248853.886,\"type\":\"fiat\"},\"mmk\":{\"name\":\"Burmese Kyat\",\"unit\":\"K\",\"value\":10386454.588,\"type\":\"fiat\"},\"mxn\":{\"name\":\"Mexican Peso\",\"unit\":\"MX$\",\"value\":130464.387,\"type\":\"fiat\"},\"myr\":{\"name\":\"Malaysian Ringgit\",\"unit\":\"RM\",\"value\":28525.869,\"type\":\"fiat\"},\"nok\":{\"name\":\"Norwegian Krone\",\"unit\":\"kr\",\"value\":62115.961,\"type\":\"fiat\"},\"nzd\":{\"name\":\"New Zealand Dollar\",\"unit\":\"NZ$\",\"value\":10431.813,\"type\":\"fiat\"},\"php\":{\"name\":\"Philippine Peso\",\"unit\":\"₱\",\"value\":348524.449,\"type\":\"fiat\"},\"pkr\":{\"name\":\"Pakistani Rupee\",\"unit\":\"₨\",\"value\":1068659.154,\"type\":\"fiat\"},\"pln\":{\"name\":\"Polish Zloty\",\"unit\":\"zł\",\"value\":26317.566,\"type\":\"fiat\"},\"rub\":{\"name\":\"Russian Ruble\",\"unit\":\"₽\",\"value\":431358.032,\"type\":\"fiat\"},\"sar\":{\"name\":\"Saudi Riyal\",\"unit\":\"SR\",\"value\":25831.987,\"type\":\"fiat\"},\"sek\":{\"name\":\"Swedish Krona\",\"unit\":\"kr\",\"value\":64517.635,\"type\":\"fiat\"},\"sgd\":{\"name\":\"Singapore Dollar\",\"unit\":\"S$\",\"value\":9328.901,\"type\":\"fiat\"},\"thb\":{\"name\":\"Thai Baht\",\"unit\":\"฿\",\"value\":208335.354,\"type\":\"fiat\"},\"try\":{\"name\":\"Turkish Lira\",\"unit\":\"₺\",\"value\":40289.38,\"type\":\"fiat\"},\"twd\":{\"name\":\"New Taiwan Dollar\",\"unit\":\"NT$\",\"value\":207535.065,\"type\":\"fiat\"},\"uah\":{\"name\":\"Ukrainian hryvnia\",\"unit\":\"₴\",\"value\":161982.153,\"type\":\"fiat\"},\"vef\":{\"name\":\"Venezuelan bolívar fuerte\",\"unit\":\"Bs.F\",\"value\":1711494584.025,\"type\":\"fiat\"},\"vnd\":{\"name\":\"Vietnamese đồng\",\"unit\":\"₫\",\"value\":159580475.492,\"type\":\"fiat\"},\"zar\":{\"name\":\"South African Rand\",\"unit\":\"R\",\"value\":98892.629,\"type\":\"fiat\"},\"xdr\":{\"name\":\"IMF Special Drawing Rights\",\"unit\":\"XDR\",\"value\":4982.749,\"type\":\"fiat\"},\"xag\":{\"name\":\"Silver - Troy Ounce\",\"unit\":\"XAG\",\"value\":403.518,\"type\":\"commodity\"},\"xau\":{\"name\":\"Gold - Troy Ounce\",\"unit\":\"XAU\",\"value\":4.662,\"type\":\"commodity\"}}";

        [FunctionName("rates")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            HttpClient newClient = new HttpClient();
            HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, RatesUrl);

            //Read Server Response
            HttpResponseMessage response = await newClient.SendAsync(newRequest);
            var body  = await response.Content.ReadAsStringAsync();

            var resp = JsonConvert.DeserializeAnonymousType(body, new  { stratis = new Dictionary<string, string>()});

            dynamic names = JsonConvert.DeserializeObject(Names);

            var mapped = resp.stratis.Select(kvp => new 
            {
                code = kvp.Key.ToUpperInvariant(),
                rate = decimal.TryParse(kvp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var res) ? res.ToString("N8") : kvp.Value,
                name = names[kvp.Key] != null ? names[kvp.Key]["name"] : null
            });

            return new OkObjectResult(mapped);
        }
    }
}
