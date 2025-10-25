using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class AreaEntity : BaseEntity
    {
        public long ChatId { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLon { get; set; }
        public double? RadiusInMeters { get; set; }
        public string? PolygonJson { get; set; }
    }
}
