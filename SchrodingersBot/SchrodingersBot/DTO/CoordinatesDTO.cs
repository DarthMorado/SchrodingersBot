using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO
{
    public class CoordinatesDTO
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public CoordinatesDTO(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }
}
