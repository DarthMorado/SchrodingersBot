using SchrodingersBot.DTO;
using SchrodingersBot.Services.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Location
{
    public interface ICoordinatesProcessingService
    {
        string FormatCoordinatesString(CoordinatesDTO coordinatesDTO);
        public List<CoordinatesDTO> Search(string input);
    }

    public class CoordinatesProcessingService : ICoordinatesProcessingService
    {
        private readonly ITextProcessingService _textProcessingService;

        public CoordinatesProcessingService(ITextProcessingService textProcessingService)
        {
            _textProcessingService = textProcessingService;
        }

        public List<CoordinatesDTO> Search(string input)
        {
            List<CoordinatesDTO> result = new List<CoordinatesDTO>();

            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            var patternDD = @"(\d\d*[/.,]\d\d*)[/.,\s](\s*\d\d*[/.,]\d\d*)";
            Regex regex = new(patternDD);
            MatchCollection matches = regex.Matches(input);
            if (matches.Any())
            {
                var match = matches.First();

            }
            for (int i = 0; i < matches.Count; i++)
            {
                try
                {
                    var groups = matches[i].Groups;
                    if (groups is null || groups.Count != 3) continue;

                    var lat = double.Parse(_textProcessingService.ReplaceDecimalSepparator(groups[1].Value));
                    var lon = double.Parse(_textProcessingService.ReplaceDecimalSepparator(groups[2].Value));

                    result.Add(new CoordinatesDTO(lat, lon));
                }
                catch (Exception ex)
                {
                }
            }

            //Degrees, minutes, and seconds (DMS): 41°24'12.2"N 2°10'26.5"E
            var patternDMS = @"(\d\d?)° *(\d\d?)' *(\d\d?[\.,]?\d?[\.,]*\d?)\"" *N *(\d\d?)° *(\d\d?)' *(\d\d?[\.,]?\d?[\.,]?\d?)\"" *E";
            regex = new(patternDMS);
            matches = regex.Matches(input);
            for (int i = 0; i < matches.Count; i++)
            {
                var groups = matches[i].Groups;
                if (groups is null || groups.Count != 7) continue;

                var coordinates = ConvertCoordinatesFromDegrees(groups[1].Value, groups[2].Value, groups[3].Value, groups[4].Value, groups[5].Value, groups[6].Value);

                if (coordinates != null)
                {
                    result.Add(coordinates);
                }
            }

            return result;
        }

        public CoordinatesDTO? ConvertCoordinatesFromDegrees(string latDeg, string latMin, string latSec, string lonDeg, string lonMin, string lonSec)
        {
            int latDegValue, latMinValue, lonDegValue, lonMinValue;
            double lat, lon, latSecValue, lonSecValue;

            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (!int.TryParse(latDeg, out latDegValue)) return null;
            if (!int.TryParse(latMin, out latMinValue)) return null;
            if (!int.TryParse(lonDeg, out lonDegValue)) return null;
            if (!int.TryParse(lonMin, out lonMinValue)) return null;
            if (!double.TryParse(latSec.Replace(".", separator).Replace(",", separator), out latSecValue)) return null;
            if (!double.TryParse(lonSec.Replace(".", separator).Replace(",", separator), out lonSecValue)) return null;

            lat = latDegValue + ((double)latMinValue / 60) + (latSecValue / 3600);
            lon = lonDegValue + ((double)lonMinValue / 60) + (lonSecValue / 3600);

            return new CoordinatesDTO(lat, lon);
        }

        public string FormatCoordinatesString(CoordinatesDTO cords)
        {
            string wazeLink = $"<a href=\"https://www.waze.com/ul?ll={cords.Lat}%2C{cords.Lon}&navigate=yes\">Waze</a>";
            string googleLink = $"<a href=\"https://www.google.com/maps/dir/?api=1&destination={cords.Lat}%2C{cords.Lon}\">Google</a>";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{cords.Lat} {cords.Lon}");
            sb.AppendLine($"{wazeLink} {googleLink}");

            return sb.ToString();
        }
    }
}
