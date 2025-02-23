using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ScreenMap
    {
        private static double EarthRadiusKM = 6371.0;
        private static double RadPerDeg = Math.PI / 180.0;

        /* Oblique Stereographic projection
        public static SKPoint CoordinateToScreen(int width, int height, double scale, SKPoint panOffset, double lat, double lon, double centerLon = 0)
        {
            double phi = lat * RadPerDeg;
            double lambda = lon * RadPerDeg;
            double lambda0 = centerLon * RadPerDeg;
            double x = 2 * EarthRadiusKM * Math.Cos(phi) * Math.Sin(lambda - lambda0) / (1 + Math.Sin(phi));
            double y = 2 * EarthRadiusKM * Math.Cos(phi) * Math.Cos(lambda - lambda0) / (1 + Math.Sin(phi));
            double rotatedX = x * Cos45 + y * Sin45;
            double rotatedY = -x * Sin45 + y * Cos45;
            int screenX = (int)(rotatedX * scale + width / 2 + panOffset.X);
            int screenY = (int)(rotatedY * scale + height / 2 + panOffset.Y);
            return new SKPoint(screenX, screenY);
        }

        public static SKPoint ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point, double centerLon = 0)
        {
            int screenWidth = size.Width;
            int screenHeight = size.Height;
            double x = (point.X - screenWidth / 2 - panOffset.X) / scale;
            double y = (point.Y - screenHeight / 2 - panOffset.Y) / scale;
            double unrotatedX = x * Cos45 - y * Sin45;
            double unrotatedY = x * Sin45 + y * Cos45;
            double rho = Math.Sqrt(unrotatedX * unrotatedX + unrotatedY * unrotatedY);
            double c = 2 * Math.Atan(rho / (2 * EarthRadiusKM));
            double phi = Math.Asin(Math.Cos(c));
            double lambda = centerLon * RadPerDeg + Math.Atan2(unrotatedX, unrotatedY);
            double lat = phi / RadPerDeg;
            double lon = lambda / RadPerDeg;
            return new SKPoint((float)lat, (float)lon);
        }*/

        // Equirectangular Projection WGS84

        public static SKPoint CoordinateToScreen(int width, int height, double scale, SKPoint panOffset, double lat, double lon, double centerLat = 0, double centerLon = 0)
        {
            double x = EarthRadiusKM * (lon - centerLon) * Math.Cos(centerLat * RadPerDeg);
            double y = EarthRadiusKM * (lat - centerLat);
            int screenX = (int)(x * scale + width / 2 + panOffset.X);
            int screenY = (int)(-y * scale + height / 2 + panOffset.Y);
            return new SKPoint(screenX, screenY);
        }

        public static SKPoint ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point, double centerLat = 0, double centerLon = 0)
        {
            int screenWidth = size.Width;
            int screenHeight = size.Height;
            double x = (point.X - screenWidth / 2 - panOffset.X) / scale;
            double y = -(point.Y - screenHeight / 2 - panOffset.Y) / scale;
            double lat = centerLat + (y / EarthRadiusKM);
            double lon = centerLon + (x / (EarthRadiusKM * Math.Cos(centerLat * RadPerDeg)));
            return new SKPoint((float)lat, (float)lon);
        }
    }
}
