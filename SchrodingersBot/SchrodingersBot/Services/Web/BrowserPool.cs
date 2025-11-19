using System;
using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
using System.Web;
using AutoMapper.Configuration.Annotations;
using Azure;
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

            if (!_browsers.Any())
            {
                await new BrowserFetcher().DownloadAsync();
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
                var uri = new Uri(request.Url);
                var query = HttpUtility.ParseQueryString(uri.Query);

                Payload? payload = null;

                // 3. Define a condition (e.g., if the URL contains "/api/")
                if (request.Url.Contains("?json=1"))
                {
                    var headers = new Dictionary<string, string>(request.Headers.ToDictionary(h => h.Key, h => h.Value));
                    headers["Accept"] = "application/json";

                    payload = payload ?? new Payload();
                    payload.Headers = headers;
                }

                if (query.HasKeys() && query.AllKeys.Contains("postdata"))
                {
                    var postData = query.GetValues("postdata").First();

                    payload = payload ?? new Payload();
                    payload.Url = request.Url;
                    payload.HasPostData = true;
                    payload.Method = HttpMethod.Post;
                    payload.PostData = HttpUtility.UrlDecode(postData);
                }

                if (payload is null)
                {
                    await request.ContinueAsync();
                }
                else
                {
                    await request.ContinueAsync(payload);
                }
            };

            page.Request += (sender, e) =>
            {
                Console.WriteLine($"[Request] {e.Request.Method} {e.Request.Url}");
            };

            page.Response += (sender, e) =>
            {
                Console.WriteLine($"[Response] {e.Response.Status} {e.Response.Url}");
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
