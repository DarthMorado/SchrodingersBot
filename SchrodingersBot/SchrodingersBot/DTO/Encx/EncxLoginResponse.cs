using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxLoginResponse
    {
        public int? Error { get; set; }
        public string Message { get; set; }
        public string IpUnblockUrl { get; set; }
        public string BruteForceUnblockUrl { get; set; }
        public string ConfirmEmailUrl { get; set; }
        public string CaptchaUrl { get; set; }
        public string AdminWhoCanActivate { get; set; }
    }
}
