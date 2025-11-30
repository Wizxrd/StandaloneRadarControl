using Client.Models;
using SkiaSharp;
namespace Client.Utils;

public class Zoom
{

    public static List<double> BuildLevels()
    {
        var levels = new List<double>();
        for (double z = 0.2; z <= 2; z += 0.2) levels.Add(z);
        for (double z = 3; z <= 10; z += 1) levels.Add(z);
        for (double z = 20; z <= 1300; z += 10) levels.Add(z);
        return levels;
    }

    public static List<double> BuildScale(DisplayState displayState, List<double> zoomLevels)
    {
        var list = new List<double>();
        int screenWidth = displayState.Width;
        int screenHeight = displayState.Height;
        var screenSize = new System.Drawing.Size(screenWidth, screenHeight);
        const double testScale = 0.025;

        var panForTest = new SKPoint(0, 0);
        var start = new SKPoint(screenWidth / 2f, 0);
        var end = new SKPoint(screenWidth / 2f, screenHeight);
        foreach (var nm in zoomLevels)
        {
            Coordinate c1 = ScreenMap.ScreenToCoordinate(screenSize, testScale, panForTest, start);
            Coordinate c2 = ScreenMap.ScreenToCoordinate(screenSize, testScale, panForTest, end);
            double heightNm = ScreenMap.DistanceInNM(new Coordinate { Lat = c1.Lat, Lon = c1.Lon }, new Coordinate { Lat = c2.Lat, Lon = c2.Lon });
            double adjustedScale = testScale * (heightNm / nm);
            list.Add(adjustedScale);
        }

        return list;
    }
}