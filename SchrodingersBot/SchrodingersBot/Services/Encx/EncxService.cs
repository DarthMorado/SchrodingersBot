using AutoMapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO.Encx;
using SchrodingersBot.DTO.EnGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SchrodingersBot.Services.Encx
{
    public interface IEncxService
    {
        public static string LoginUrl(string domain) => $"https://{domain}/login/signin?json=1";
        public static string GameUrl(string domain, string gameId) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}?json=1";

        public Task<LoginInfoDTO> GetLoginInfo(long chatId, string domain, string gameId, string username = null, string password = null);
        public Task<EncxGameEngineModel> GetGameAsync(long chatId, string url, LoginInfoDTO loginInfo);
        public Task<EncxGameEngineModel> GetGameAsync(long chatId, string domain, string gameId, LoginInfoDTO loginInfo);
        public Task<bool?> EnterCode(string domain, string gameId, int levelId, int levelNumber, LoginInfoDTO loginInfo, string code);
    }

    public class EncxService : IEncxService
    {
        private readonly IDbRepository<EncxAuthEntity> _loginInfoRepository;
        private readonly IMapper _mapper;

        public static string LoginUrl(string domain) => $"https://{domain}/login/signin?json=1";
        public static string GameUrl(string domain, string gameId) => $"https://{domain}/GameEngines/Encounter/Play/{gameId}?json=1";

        public EncxService(IDbRepository<EncxAuthEntity> loginInfoRepository,
            IMapper mapper)
        {
            _loginInfoRepository = loginInfoRepository;
            _mapper = mapper;
        }


        public async Task<LoginInfoDTO> Login(string domain, string username, string password, string? magicNumbers)
        {
            LoginInfoDTO loginInfo = new()
            {
                Username = username,
                Password = password
            };

            using HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "Login", username },
                { "Password", password },
                { "ddlNetwork", "1" },
            };

            if (!String.IsNullOrWhiteSpace(magicNumbers))
            {
                values.Add("MagicNumbers", magicNumbers);
            }

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(LoginUrl(domain), content);

            var responseString = await response.Content.ReadAsStringAsync();
            
            EncxLoginResponse loginResponse = JsonSerializer.Deserialize<EncxLoginResponse>(responseString);

            if (loginResponse.Error == 0)
            {
                //get cookies
                List<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.ToList();
                loginInfo.Guid = GetCookieValue(cookies, "guid");
                loginInfo.Stoken = GetCookieValue(cookies, "stoken");
                loginInfo.Atoken = GetCookieValue(cookies, "atoken");
            }
            else
            {
                throw new Exception($"Could not log in with user {username} for domain {domain}. (Error code:{loginResponse.Error})");
            }

            return loginInfo;
        }

        public async Task<LoginInfoDTO> GetLoginInfo(long chatId, string domain, string gameId, string username, string password)
        {
            var availableInfo = await _loginInfoRepository.FindAsync(x => x.ChatId == chatId && (string.IsNullOrEmpty(username) || x.Username == username));

            LoginInfoDTO loginInfo = new();

            if (!availableInfo.Any())
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return null;
                }    

                loginInfo = await Login(domain, username, password, null);
                var loginEntity = _mapper.Map<EncxAuthEntity>(loginInfo);
                loginEntity.ChatId = chatId;
                loginEntity.Domain = domain;
                loginEntity.GameId = gameId;
                await _loginInfoRepository.CreateAsync(loginEntity);
            }
            else
            {
                loginInfo = _mapper.Map<LoginInfoDTO>(availableInfo.First());
            }

            return loginInfo;
        }

        public async Task<EncxGameEngineModel> GetGameAsync(long chatId, string domain, string gameId, LoginInfoDTO loginInfo)
        {
            var url = GameUrl(domain, gameId);
            var uri = new Uri(url);

            // Connect
            // Create a handler that stores cookies
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);

            // Add a cookie to the container
            handler.CookieContainer.Add(uri, new Cookie("GUID", loginInfo.Guid));
            handler.CookieContainer.Add(uri, new Cookie("stoken", loginInfo.Stoken));
            handler.CookieContainer.Add(uri, new Cookie("atoken", loginInfo.Atoken));

            // Send the POST request
            HttpResponseMessage response = await client.GetAsync(GameUrl(domain, gameId));

            // Read the response
            string responseBody = await response.Content.ReadAsStringAsync();

            var gameObject = JsonSerializer.Deserialize<EncxGameEngineModel>(responseBody);

            return gameObject;
        }

        public async Task<EncxGameEngineModel> GetGameAsync(long chatId, string url, LoginInfoDTO loginInfo)
        {
            string domain = null;
            string gameId = null;

            var uri = new Uri(url);
            domain = uri.Host;

            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                gameId = uri.Query.Split(new char[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=')).Where(x => x[0].ToLower() == "gid").First()[1];
            }

            if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(domain))
            {
                throw new Exception($"Incorrect url for encx game: {url}");
            }

            return await GetGameAsync(chatId, domain, gameId, loginInfo);
        }

        public async Task<bool?> EnterCode(string domain, string gameId, int levelId, int levelNumber, LoginInfoDTO loginInfo, string code)
        {
            // Create a handler that stores cookies
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);

            string url = GameUrl(domain, gameId);

            // Add a cookie to the container
            var uri = new Uri(url);
            handler.CookieContainer.Add(uri, new Cookie("GUID", loginInfo.Guid));
            handler.CookieContainer.Add(uri, new Cookie("stoken", loginInfo.Stoken));
            handler.CookieContainer.Add(uri, new Cookie("atoken", loginInfo.Atoken));

            var values = new Dictionary<string, string>
            {
                { "LevelId", levelId.ToString() },
                { "LevelNumber", levelNumber.ToString() },
                { "LevelAction.Answer", code },
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();

            var game = JsonSerializer.Deserialize<EncxGameEngineModel>(responseString);
            
            return ((game.EngineAction.LevelAction.IsCorrectAnswer ?? false) || (game.EngineAction.BonusAction.IsCorrectAnswer ?? false));
        }

        private string GetCookieValue(IEnumerable<string> cookies, string key)
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
    }
}
