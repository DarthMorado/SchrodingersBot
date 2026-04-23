using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class NotificationEntity : BaseEntity
    {
        public long ChatId { get; set; }
        public string NotificationTypeCode { get; set; }
        public DateTime Date { get; set; }
        public string ForGameId { get; set; }
    }
}
