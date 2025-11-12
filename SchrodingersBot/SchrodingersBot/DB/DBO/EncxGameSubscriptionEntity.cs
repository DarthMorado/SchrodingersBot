using SchrodingersBot.DTO.Encx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class EncxGameSubscriptionEntity : BaseEntity
    {
        public long ChatId { get; set; }
        public EncxAuthEntity LoginInfo { get; set; }
        public int? LoginInfoId { get; set; }
        public string Domain { get; set; }
        public string GameId { get; set; }
        public int ActiveLevelId { get; set; } = 0;
        public int ActiveLevelNumber { get; set; } = 0;
        public bool IsActive { get; set; } = false;

        public IEnumerable<EncxLevelEntity> Levels { get; set; }
    }
}
