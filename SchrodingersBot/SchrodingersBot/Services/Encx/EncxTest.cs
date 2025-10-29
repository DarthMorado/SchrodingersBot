using SchrodingersBot.DTO.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Encx
{
    public static class EncxTest
    {
        private const string _login = "";
        private const string _password = "";
        private static string _domain;
        private static string _gameId;
        private static string LoginUrl => $"https://{_domain}/login/signin?json=1";
        private static string GameUrl => $"https://{_domain}/GameEngines/Encounter/Play/{_gameId}?json=1";
        private static string LogPath = "c:/Temp/encxlog.txt";

        private static string _guidCookie;
        private static string _stokenCookie;
        private static string _atokenCookie;

        private static EncxGameEngineModel _gameObject;

        public static async Task Test(string url)
        {
            File.WriteAllLines(LogPath, new string[] { });

            var uri = new Uri(url);
            _domain = uri.Host;

            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                _gameId = uri.Query.Split(new char[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=')).Where(x => x[0].ToLower() == "gid").First()[1];
            }

            await LogIn();

            await GetGameInfo();

            if (CheckIfCanSendCode())
            {
                bool isCorrect = await SendWrongCode();
            }
        }

        private static bool CheckIfCanSendCode()
        {
            if (_gameObject == null) return false;

            if (_gameObject.Level.IsPassed == true ||
                _gameObject.Level.Dismissed == true ||
                !(_gameObject.Level.HasAnswerBlockRule == false || _gameObject.Level.BlockDuration <= 0))
            {
                return false;
            }

            return true;
        }

        public static async Task<int> LogIn(string? magicNumbers = null)
        {
            using HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "Login", _login },
                { "Password", _password },
                { "ddlNetwork", "1" },
            };

            if (!String.IsNullOrWhiteSpace(magicNumbers))
            {
                values.Add("MagicNumbers", magicNumbers);
            }

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(LoginUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();

            File.AppendAllText(LogPath, $"\r\n{responseString}\r\n");

            EncxLoginResponse responseObj = JsonSerializer.Deserialize<EncxLoginResponse>(responseString);

            if (responseObj.Error == 0)
            {
                //get cookies
                List<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.ToList();
                _guidCookie = GetCookieValue(cookies, "guid");
                _stokenCookie = GetCookieValue(cookies, "stoken");
                _atokenCookie = GetCookieValue(cookies, "atoken");
            }

            return responseObj.Error ?? 0;
        }

        private static string GetCookieValue(IEnumerable<string> cookies, string key)
        {
            if (cookies.Any(x => x.ToLower().StartsWith($"{key}=")))
            {
                var keyCookie = cookies.Where(x => x.ToLower().StartsWith($"{key}=")).First();
                keyCookie = keyCookie.Substring(keyCookie.ToLower().IndexOf($"{key}=") + key.Length + 1);
                if (keyCookie.IndexOf(";") >= 0)
                {
                    keyCookie = keyCookie.Substring(0, keyCookie.IndexOf(";"));
                }
                return keyCookie;
            }
            return null;
        }

        public static async Task GetGameInfo()
        {
            // Create a handler that stores cookies
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);

            // Add a cookie to the container
            var uri = new Uri(GameUrl);
            handler.CookieContainer.Add(uri, new Cookie("GUID", _guidCookie));
            handler.CookieContainer.Add(uri, new Cookie("stoken", _stokenCookie));
            handler.CookieContainer.Add(uri, new Cookie("atoken", _atokenCookie));

            // Send the POST request
            HttpResponseMessage response = await client.GetAsync(GameUrl);

            // Read the response
            string responseBody = await response.Content.ReadAsStringAsync();

            File.AppendAllText(LogPath, responseBody);

            _gameObject = JsonSerializer.Deserialize<EncxGameEngineModel>(responseBody);

            Console.WriteLine(FormatGameInfo(_gameObject));
            Console.WriteLine(FormatGameLevelInfo(_gameObject.Level));
        }

        public static async Task<bool> SendWrongCode()
        {
            // Create a handler that stores cookies
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);

            // Add a cookie to the container
            var uri = new Uri(GameUrl);
            handler.CookieContainer.Add(uri, new Cookie("GUID", _guidCookie));
            handler.CookieContainer.Add(uri, new Cookie("stoken", _stokenCookie));
            handler.CookieContainer.Add(uri, new Cookie("atoken", _atokenCookie));

            var values = new Dictionary<string, string>
            {
                { "LevelId", _gameObject.Level.LevelId.ToString() },
                { "LevelNumber", _gameObject.Level.Number.ToString() },
                { "LevelAction.Answer", "qwersdf#sd!as" },
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(GameUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();

            File.AppendAllText(LogPath, $"\r\n{responseString}\r\n");

            _gameObject = JsonSerializer.Deserialize<EncxGameEngineModel>(responseString);

            return ((_gameObject.EngineAction.LevelAction.IsCorrectAnswer ?? false) || (_gameObject.EngineAction.BonusAction.IsCorrectAnswer ?? false));
        }

        public static string FormatGameInfo(EncxGameEngineModel game)
        {
            StringBuilder sb = new();
            sb.Append($"{game.GameTitle}");
            sb.Append($"Level {game.Level.Number}/{game.Levels.Count}");

            return sb.ToString();
        }

        public static string FormatGameLevelInfo(EncxLevel lvl)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{lvl.Number}: {lvl.Name}");
            sb.AppendLine($"Avtoperexod v {DateTime.Now.AddSeconds(lvl.Timeout).ToString("hh:mm:ss")} (wtraf {new DateTime(0).AddSeconds(lvl.TimeoutAward).ToString("hh:mm:ss")})");
            sb.AppendLine($"Sektorov {lvl.PassedSectorsCount}/{lvl.Sectors.Count} (Dlja zakritija: {lvl.RequiredSectorsCount})");
            if (lvl.Bonuses != null && lvl.Bonuses.Any())
            {
                sb.AppendLine($"Na urovne estj bonusi ({lvl.Bonuses.Count})");
            }


            if (lvl.Messages != null && lvl.Messages.Any())
            {
                sb.AppendLine("Soobwenija ot Adminov:");
                foreach (var message in lvl.Messages)
                {
                    sb.AppendLine($"{message.OwnerLogin}: {message.MessageText}");
                }
            }
            sb.AppendLine("Task:");
            sb.AppendLine(lvl.Task?.TaskText);

            return sb.ToString();
        }
    }

}
