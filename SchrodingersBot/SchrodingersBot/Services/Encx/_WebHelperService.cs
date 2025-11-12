//using Azure.Identity;
//using PuppeteerSharp;
//using SchrodingersBot.DTO.EnGame;
//using System;
//using System.Buffers.Text;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Telegram.Bot.Types;
//using HtmlAgilityPack;

//namespace SchrodingersBot.Services.Encx
//{
//    public interface IWebHelperService
//    {
//        public Task<byte[]> GetScreenshot(string url, string domain, LoginInfoDTO loginInfo, List<CookieParam> cookies);
//    }

//    public class WebHelperService : IWebHelperService
//    {


//        public async Task<byte[]> GetScreenshot(string url, string domain, LoginInfoDTO loginInfo, List<CookieParam> cookies)
//        {
//            Uri uri = new Uri(url);

//            var BaseUrl = $"{uri.Scheme}://{uri.Host}";

//            // Set up
//            await new BrowserFetcher().DownloadAsync();
//            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
//            {
//                Headless = true // Set to false to see the browser UI
//            });
//            var page = await browser.NewPageAsync();


//            await page.SetCookieAsync(cookies.ToArray());

//            // Go to default page
//            await page.GoToAsync(BaseUrl); // Need this!

//            await page.GoToAsync(url);

//            var content = await page.GetContentAsync();

//            HtmlDocument document = new HtmlDocument();
//            document.LoadHtml(content);
//            if (IsLoginPage(document))
//            {
//                var newCookies = await GetBrowserCookies(page, domain, loginInfo.Username, loginInfo.Password);

//                //todo - check if need to set up cookies again

//                await page.GoToAsync(url);
//            }

//            var filename = $"c:/temp/test.png";

//            using Stream stream = await page.ScreenshotStreamAsync();
//            using MemoryStream ms = new MemoryStream();
//            stream.CopyTo(ms);
//            return ms.ToArray();

//        }

//        public async Task<string> GetBrowserCookies(IPage page, string domain, string username, string password)
//        {
//            await page.GoToAsync($"https://{domain}/login/signin");

//            // Fill in the login form
//            await page.TypeAsync("input[id='txtLogin']", username);
//            await page.TypeAsync("input[id='txtPassword']", password);

//            // Click the login button. Adjust the selector to match your login button.
//            await page.ClickAsync("input[type='submit']");

//            // Wait for navigation to complete after login.
//            // You might need a different wait condition based on your site,
//            // like waiting for a specific selector to appear on the new page.
//            await page.WaitForNavigationAsync();

//            // Get all cookies for the current page.
//            var cookies = await page.GetCookiesAsync();

//            // Serialize the cookies to a JSON string for storage
//            return JsonSerializer.Serialize(cookies);
//        }


//        public bool IsLoginPage(HtmlDocument document)
//        {
//            var passwordNode = document.DocumentNode.SelectSingleNode(LoginPagePasswordNodePath);
//            return passwordNode != null;
//        }

//        public string LoginPagePasswordNodePath { get => "//div[@class='formmain']//input[@id='txtPassword']"; }

//    }
//}
