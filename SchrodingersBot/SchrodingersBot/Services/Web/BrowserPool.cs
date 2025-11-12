using System.Collections.Concurrent;
using PuppeteerSharp;

namespace SchrodingersBot.Services.Web
{
    public class BrowserPool : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, IBrowser> _browsers = new();

        public async Task<IPage> GetOrCreateAsync(string key)
        {
            // Return existing browser if it’s still connected
            if (_browsers.TryGetValue(key, out var existing) && existing.IsConnected)
            {
                var pages = await existing.PagesAsync();
                if (pages != null && pages.Any())
                {
                    return pages.Last();
                }
            }

            // Otherwise create a new one
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();

            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {
                var request = e.Request;

                // 3. Define a condition (e.g., if the URL contains "/api/")
                if (request.Url.Contains("?json=1"))
                {
                    // Create a copy of the existing headers
                    var headers = new Dictionary<string, string>(request.Headers.ToDictionary(h => h.Key, h => h.Value));

                    // 4. Set/Override the Accept header
                    headers["Accept"] = "application/json";

                    // 5. Continue the request with modified headers
                    await request.ContinueAsync(new Payload { Headers = headers });
                }
                else
                {
                    // For all other requests, continue without changes
                    await request.ContinueAsync();
                }
            };
            _browsers[key] = browser;
            return page;
        }

        public async Task CloseAsync(string key)
        {
            if (_browsers.TryRemove(key, out var browser))
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var pair in _browsers)
            {
                await pair.Value.CloseAsync();
                await pair.Value.DisposeAsync();
            }
            _browsers.Clear();
        }
    }
}
