using HtmlAgilityPack;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DTO.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Encx
{
    public interface IEncxEngine
    {
        Task<EncxAuthEntity> Login(EncxAuthEntity loginInfo); //add magic numbers
        Task<EncxAuthEntity> EnsureAuth(EncxAuthEntity loginInfo);
        Task<byte[]> Screenshot(EncxAuthEntity loginInfo);
        
        Task<bool> IsLoginPage(HtmlDocument document);

        /// <summary>
        /// Visit page
        /// Check if login
        /// revisit again
        /// </summary>
        Task<EncxGameEngineModel?> GetGameObject(EncxAuthEntity loginInfo); 
        Task<EncxGameEngineModel?> EnterCode(EncxAuthEntity loginInfo, string code);
    }
}
