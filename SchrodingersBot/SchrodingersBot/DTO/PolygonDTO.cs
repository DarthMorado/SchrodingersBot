using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO
{
    public class PolygonDTO : IArea
    {
        public List<CoordinatesDTO> Points { get; set; }

        public async Task<bool> InArea(CoordinatesDTO coordinates)
        {
            try
            {
                return PointInPolygon(coordinates.Lat, coordinates.Lon, Points.Select(x => (x.Lat, x.Lon)).ToList());
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool PointInPolygon(double lat, double lon, IReadOnlyList<(double lat, double lon)> polygon)
        {
            if (polygon == null || polygon.Count < 3) return false;

            const double EPS = 1e-12;

            // Shift longitudes so each vertex is the nearest wrap relative to the test point's lon.
            static double UnwrapLon(double L, double referenceLon)
            {
                // add k*360 such that |(L - referenceLon)| is minimized
                return L + 360.0 * Math.Round((referenceLon - L) / 360.0);
            }

            int n = polygon.Count;
            var unwrapped = new (double lat, double lon)[n];
            for (int i = 0; i < n; i++)
            {
                var (phi, lam) = polygon[i];
                unwrapped[i] = (phi, UnwrapLon(lam, lon));
            }

            // Edge-inclusive helper
            static bool OnSegment(
                double x, double y,
                double x1, double y1,
                double x2, double y2,
                double eps)
            {
                // area2 == 0 for colinearity (cross product of vectors (x1->x2) and (x1->p))
                double area2 = (y2 - y1) * (x - x1) - (x2 - x1) * (y - y1);
                if (Math.Abs(area2) > eps) return false;

                // bounding-box check
                return (Math.Min(x1, x2) - eps <= x && x <= Math.Max(x1, x2) + eps) &&
                       (Math.Min(y1, y2) - eps <= y && y <= Math.Max(y1, y2) + eps);
            }

            double x = lon, y = lat;
            bool inside = false;

            for (int i = 0; i < n; i++)
            {
                var (lat1, lon1) = unwrapped[i];
                var (lat2, lon2) = unwrapped[(i + 1) % n];

                // Edge-inclusive check
                if (OnSegment(x, y, lon1, lat1, lon2, lat2, EPS))
                    return true;

                // Ray-casting: count intersections of a horizontal ray to +∞ in longitude
                bool cond = (lat1 > y) != (lat2 > y);
                if (cond)
                {
                    double xInt = lon1 + (lon2 - lon1) * (y - lat1) / (lat2 - lat1); // intersection lon
                    if (x < xInt - EPS) inside = !inside;
                }
            }

            return inside;
        }
    }
}
