using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace BudgetWise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmojiApiController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public EmojiApiController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("flattened")]
        public async Task<List<EmojiItem>> GetFlattenedEmojis()
        {
            var emojiApiUrl = "https://emojihub.yurace.pro/api/all";
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; BudgetWise/1.0)");

            try
            {
                Console.WriteLine("Fetching emojis from: " + emojiApiUrl);
                var responseStream = await _httpClient.GetStreamAsync(emojiApiUrl);
                Console.WriteLine("Response stream obtained successfully.");

                var emojis = await JsonSerializer.DeserializeAsync<List<Emoji>>(responseStream);
                Console.WriteLine("Deserialization completed.");

                if (emojis == null)
                {
                    Console.WriteLine("emojis is null.");
                    return new List<EmojiItem>();
                }

                Console.WriteLine("Original emojis count: " + emojis.Count);

                // Flatten the emojis with additional details
                var flattenedEmojis = emojis.SelectMany(e =>
                    e.htmlCode.Select(code => new EmojiItem
                    {
                        name = e.name,
                        category = e.category,
                        group = e.group,
                        htmlCode = HttpUtility.HtmlDecode(code) // Decode the HTML entities
                    })
                ).ToList();

                Console.WriteLine("Flattened emojis count: " + flattenedEmojis.Count);

                foreach (var emoji in flattenedEmojis)
                {
                    Console.WriteLine($"Name: {emoji.name}, Category: {emoji.category}, Group: {emoji.group}, HTML Code: {emoji.htmlCode}");
                }

                return flattenedEmojis;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
                return new List<EmojiItem>();
            }
        }

        // Original emoji structure
        public class Emoji
        {
            public string name { get; set; }
            public string category { get; set; }
            public string group { get; set; }
            public List<string> htmlCode { get; set; }
        }

        // Flattened emoji structure
        public class EmojiItem
        {
            public string name { get; set; }
            public string category { get; set; }
            public string group { get; set; }
            public string htmlCode { get; set; }
        }
    }
}
