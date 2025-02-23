using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.Windows.Input;
using SkiaSharp;
using System.Windows.Navigation;
using Client.Views;

namespace Client.Models
{
    public class Radar
    {
        private MainWindowView mainWindowView;
        private VideoMap videoMap = new VideoMap();
        public static Dictionary<string, Contact> Contacts = new Dictionary<string, Contact>();
        private SKPoint panStartPoint;
        public SKPoint panOffset;
        private bool isPanning = false;

        public readonly Dictionary<double, string> zoomLevels = new Dictionary<double, string>();

        public double scale = 0.0025;
        private double initialZoom = 0.001;
        private string initialDisplayValue = "1";
        private double finalZoom = 10;
        private string finalDisplayValue = "100";
        public int currentZoomIndex = 9;
        private int numIntermediateLevels = 99;

        private double centerLat = 32.231733;
        private double centerLon = 35.796267;
        private double offCenterLat;
        private double offCenterLon;
        private double curLat;
        private double curLon;

        private int Width;
        private int Height;

        public Radar(MainWindowView mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            videoMap.Load(LoadFile.Load("VideoMaps/Regions", "Caucasus.geojson"));
            InterpolateZoomLevels();
            curLat = centerLat;
            curLon = centerLon;
            VideoMap.CenterAtCoordinates(Width, Height, scale, ref panOffset, curLat, curLon);
        }

        private void InterpolateZoomLevels()
        {
            for (int i = 0; i <= numIntermediateLevels; i++)
            {
                double t = (double)i / numIntermediateLevels;
                double interpolatedZoom = initialZoom * Math.Pow(finalZoom / initialZoom, t);
                int interpolatedValue = (int)Math.Round(double.Parse(initialDisplayValue) + t * (double.Parse(finalDisplayValue) - double.Parse(initialDisplayValue)));
                string interpolatedDisplayValue = interpolatedValue.ToString();
                zoomLevels.Add(interpolatedZoom, interpolatedDisplayValue);
            }
            zoomLevels[finalZoom] = finalDisplayValue;
        }

        public SKPoint GetCenter()
        {
            return new SKPoint(Width / 2, Height / 2);
        }

        public void MouseDown(System.Windows.Point mousePos)
        {
            isPanning = true;
            panStartPoint = new SKPoint((float)mousePos.X, (float)mousePos.Y);
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            SKPoint latlon = ScreenMap.ScreenToCoordinate(new Size(Width, Height), scale, panOffset, GetCenter());
            curLat = latlon.X;
            curLon = latlon.Y;
        }

        public void MouseMove(System.Windows.Point mousePos)
        {
            if (isPanning)
            {
                double dx = mousePos.X - panStartPoint.X;
                double dy = mousePos.Y - panStartPoint.Y;
                panOffset.X += (float)dx;
                panOffset.Y += (float)dy;
                panStartPoint = new SKPoint((float)mousePos.X, (float)mousePos.Y);
                SKPoint latlon = ScreenMap.ScreenToCoordinate(new Size(Width, Height), scale, panOffset, GetCenter());
                curLat = latlon.X;
                curLon = latlon.Y;
            }
        }

        public double MouseWheel(int delta, bool invert)
        {
            int centerXBeforeZoom = Width / 2;
            int centerYBeforeZoom = Height / 2;
            bool zoomIn = delta > 0;

            int indexIncrement = zoomIn ? 1 : -1;
            int nextZoomIndex = currentZoomIndex + indexIncrement;
            if (nextZoomIndex >= 0 && nextZoomIndex < zoomLevels.Count)
            {
                double newScale = zoomLevels.Keys.ElementAt(nextZoomIndex);
                double scaleFactor = newScale / scale;
                int shiftX = (int)((centerXBeforeZoom - panOffset.X) * (1 - (1 / scaleFactor)));
                int shiftY = (int)((centerYBeforeZoom - panOffset.Y) * (1 - (1 / scaleFactor)));
                panOffset.X += shiftX;
                panOffset.Y += shiftY;
                scale = newScale;
                currentZoomIndex = nextZoomIndex;
                VideoMap.CenterAtCoordinates(Width, Height, scale, ref panOffset, curLat, curLon);
            }
            return double.Parse(zoomLevels[scale]);
        }

        public void Invalidate(SKPaintSurfaceEventArgs e)
        {
            Width = e.Info.Width;
            Height = e.Info.Height;
            var canvas = e.Surface.Canvas;
            videoMap.Render(canvas, new Size(Width, Height), scale, panOffset);
            var contactsPairs = Contacts.ToArray();
            foreach (var (name, contact) in contactsPairs)
            {
                Logger.Debug("Radar.Invalidate", $"Updating {name}");
                contact.Render(canvas, new Size(Width, Height), scale, panOffset);
            }
        }
    }
}
