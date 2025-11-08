using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class EncxLevelEntity : BaseEntity
    {
        public EncxGameSubscriptionEntity Game { get; set; }
        public int? GameId { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Task { get; set; }
        
        public IEnumerable<EncxObjectEntity> Objects { get; set; }
    }
}
