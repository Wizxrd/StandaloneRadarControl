using Client.Models;
using SkiaSharp;
using System.Drawing;
namespace Client.Utils;

public class ScreenMap
{
    public static double EarthRadiusNM = 3440.065;
    private static double EarthRadiusKM = 6371.0;
    public static double RadPerDeg = Math.PI / 180.0;
    public static double Deg2Rad(double d) => d * Math.PI / 180.0;

    public static SKPoint CoordinateToScreen(int width, int height, double scale, SKPoint panOffset, Coordinate coordinate)
    {
        double x = EarthRadiusKM * coordinate.Lon * RadPerDeg;

        double latRad = coordinate.Lat * RadPerDeg;
        double y = EarthRadiusKM * Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));

        int screenX = (int)(x * scale + width / 2 + panOffset.X);
        int screenY = (int)(-y * scale + height / 2 + panOffset.Y);
        return new SKPoint(screenX, screenY);
    }

    public static Coordinate ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point)
    {
        double x = (point.X - size.Width / 2 - panOffset.X) / scale;
        double y = -(point.Y - size.Height / 2 - panOffset.Y) / scale;

        double latRad = 2 * Math.Atan(Math.Exp(y / EarthRadiusKM)) - Math.PI / 2;
        double lat = latRad / RadPerDeg;
        double lon = x / EarthRadiusKM / RadPerDeg;
        return new Coordinate { Lat = lat, Lon = lon };
    }

    public static double DistanceInNM(Coordinate coord1, Coordinate coord2)
    {
        double lat1 = coord1.Lat * Math.PI / 180.0;
        double lon1 = coord1.Lon * Math.PI / 180.0;
        double lat2 = coord2.Lat * Math.PI / 180.0;
        double lon2 = coord2.Lon * Math.PI / 180.0;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusNM * c;
    }
}
