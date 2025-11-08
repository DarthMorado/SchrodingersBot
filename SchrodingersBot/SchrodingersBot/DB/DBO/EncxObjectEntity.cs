using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class EncxObjectEntity : BaseEntity
    {
        public EncxLevelEntity Level { get; set; }
        public int LevelId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsImage { get; set; }
        public bool IsScreenshot { get; set; }
        public byte[] Data { get; set; }
    }
}
