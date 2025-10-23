using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO
{
    public class RadiusDTO : IArea
    {
        public CoordinatesDTO Center { get; set; }
        public double RadiusInMeters { get; set; }
        public async Task<bool> InArea(CoordinatesDTO coordinates)
        {
            if (Center is null) return false;

            var baseRad = Math.PI * coordinates.Lat / 180;
            var targetRad = Math.PI * Center.Lat / 180;
            var theta = coordinates.Lon - Center.Lon;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return (dist * 1609.344) < RadiusInMeters;
        }
    }
}
