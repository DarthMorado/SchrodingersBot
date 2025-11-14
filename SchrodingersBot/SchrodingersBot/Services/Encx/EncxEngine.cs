using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.Services.Web;
using SchrodingersBot.DTO.Encx;
using System.Text.Encodings.Web;
using System.Web;

namespace SchrodingersBot.Services.Encx
{
    public class EncxEngine : IEncxEngine
    {
        BrowserPool _browserPool;
        private string _BrowserKey(long Id) => $"gamebrowser_{Id}";
        private string _LoginPage(string domain) => $"https://{domain}/login/signin";
        private string _LoginPagePasswordNodePath { get => "//div[@class='formmain']//input[@id='txtPassword']"; }
        private string _GameUrlJson(string domain, string gameId) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}?json=1";
        private string _GamePOSTUrlJson(string domain, string gameId, string postData) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}?json=1&postdata={HttpUtility.UrlEncode(postData)}";
        private string _GameUrl(string domain, string gameId) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}";

        public EncxEngine(BrowserPool browserPool)
        {
            _browserPool = browserPool;
        }

        public async Task<EncxAuthEntity> EnsureAuth(EncxAuthEntity loginInfo)
        {
            var browserKey = _BrowserKey(loginInfo.Id);
            var page = await _browserPool.GetOrCreateAsync(browserKey);

            if (!String.IsNullOrEmpty(loginInfo.BrowserCookiesJson))
            {
                await page.DeleteCookieAsync();
                var cookies = JsonSerializer.Deserialize<CookieParam[]?>(loginInfo.BrowserCookiesJson);
                await page.SetCookieAsync(cookies);
            }
            await page.GoToAsync(_GameUrlJson(loginInfo.Domain, loginInfo.GameId));

            var content = await page.GetContentAsync();
            var document = new HtmlDocument();
            document.LoadHtml(content);
            if (await IsLoginPage(document))
            {
                loginInfo = await Login(loginInfo);
            }

            return loginInfo;
        }

        public async Task<EncxGameEngineModel?> EnterCode(EncxAuthEntity loginInfo, int lvlId, int lvlNumber, string code)
        {
            loginInfo = await EnsureAuth(loginInfo);

            var browserKey = _BrowserKey(loginInfo.Id);
            var page = await _browserPool.GetOrCreateAsync(browserKey);

            var payload = new
            {
                LevelId = lvlId,
                LevelNumber = lvlNumber,
                LevelAction = new { Answer = code }
            };

            var json = JsonSerializer.Serialize(payload);

            var result = await page.EvaluateFunctionAsync<string>(@"async (url, data) => {
    const resp = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body:  data
    });
    return resp.text();  // or resp.json()
}", _GameUrlJson(loginInfo.Domain, loginInfo.GameId), json);

            //var response = await page.GoToAsync(_GamePOSTUrlJson(loginInfo.Domain, loginInfo.GameId, payload), new NavigationOptions
            //{
            //    WaitUntil = new[] { WaitUntilNavigation.Load }

            //});

            //var content = await response.TextAsync();

            var gameObject = JsonSerializer.Deserialize<EncxGameEngineModel>(result);

            //return gameObject;

            return gameObject;
        }

        public async Task<EncxGameEngineModel?> GetGameObject(EncxAuthEntity loginInfo)
        {
            loginInfo = await EnsureAuth(loginInfo);

            var browserKey = _BrowserKey(loginInfo.Id);
            var page = await _browserPool.GetOrCreateAsync(browserKey);

            var response = await page.GoToAsync(_GameUrlJson(loginInfo.Domain, loginInfo.GameId), new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load }
            });

            var content = await response.TextAsync();

            var gameObject = JsonSerializer.Deserialize<EncxGameEngineModel>(content);

            return gameObject;
        }

        public async Task<bool> IsLoginPage(HtmlDocument document)
        {
            var passwordNode = document.DocumentNode.SelectSingleNode(_LoginPagePasswordNodePath);
            return passwordNode != null;
        }

        public async Task<EncxAuthEntity> Login(EncxAuthEntity loginInfo)
        {
            var browserKey = _BrowserKey(loginInfo.Id);
            var page = await _browserPool.GetOrCreateAsync(browserKey);

            await page.GoToAsync(_LoginPage(loginInfo.Domain));
            var cnt = await page.GetContentAsync();
            await page.TypeAsync("input[id='txtLogin']", loginInfo.Username);
            await page.TypeAsync("input[id='txtPassword']", loginInfo.Password);
            await page.WaitForSelectorAsync("input[type='submit']", new WaitForSelectorOptions
            {
                Visible = true // ensures it’s rendered
            });
            var box = await page.QuerySelectorAsync("input[type='submit']");
            await box.ScrollIntoViewAsync();
            var visible = await box.IsIntersectingViewportAsync();
            if (!visible)
            {
                await box.ScrollIntoViewAsync();
            }
            await page.ClickAsync("input[type='submit']");
            await page.WaitForNavigationAsync();

            var cookies = await page.GetCookiesAsync();

            loginInfo.BrowserCookiesJson = JsonSerializer.Serialize(cookies);

            return loginInfo;
        }

        public async Task<byte[]> Screenshot(EncxAuthEntity loginInfo)
        {
            loginInfo = await EnsureAuth(loginInfo);

            var browserKey = _BrowserKey(loginInfo.Id);
            var page = await _browserPool.GetOrCreateAsync(browserKey);

            await page.GoToAsync(_GameUrl(loginInfo.Domain, loginInfo.GameId), new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load }
            });

            var screenshotOptions = new ScreenshotOptions
            {
                FullPage = true,
                CaptureBeyondViewport = true,
                OptimizeForSpeed = true,
                Type = ScreenshotType.Png
            };

            using Stream stream = await page.ScreenshotStreamAsync(screenshotOptions);
            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
