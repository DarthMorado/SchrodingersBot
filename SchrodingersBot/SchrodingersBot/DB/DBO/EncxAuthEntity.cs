using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class EncxAuthEntity : BaseEntity
    {
        public long ChatId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Atoken { get; set; }
        public string Stoken { get; set; }
        public string Guid { get; set; }
        public string Domain { get; set; }
        public string GameId { get; set; }
        public string? BrowserCookiesJson { get; set; }

        public List<EncxGameSubscriptionEntity> Subscriptions { get; set; }
    }
}
