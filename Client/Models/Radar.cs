using System.Windows;
using System.Windows.Input;
using Client.Views;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Size = System.Drawing.Size;

namespace Client.Models;

public class Radar
{
	public static Dictionary<string, Contact> Contacts = new();

	private readonly double centerLat = 32.231733;
	private readonly double centerLon = 35.796267;
	private readonly string finalDisplayValue = "100";
	private readonly double finalZoom = 10;
	private readonly string initialDisplayValue = "1";
	private readonly double initialZoom = 0.001;
	private readonly int numIntermediateLevels = 99;
	private readonly VideoMap videoMap = new();

	public readonly Dictionary<double, string> zoomLevels = new();
	private double curLat;
	private double curLon;
	public int currentZoomIndex = 9;
	private int Height;
	private bool isPanning;
	private MainWindowView mainWindowView;
	private double offCenterLat;
	private double offCenterLon;
	public SKPoint panOffset;
	private SKPoint panStartPoint;

	public double scale = 0.0025;

	private int Width;

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
		for (var i = 0; i <= numIntermediateLevels; i++)
		{
			var t = (double)i / numIntermediateLevels;
			var interpolatedZoom = initialZoom * Math.Pow(finalZoom / initialZoom, t);
			var interpolatedValue = (int)Math.Round(double.Parse(initialDisplayValue) +
													t * (double.Parse(finalDisplayValue) -
														double.Parse(initialDisplayValue)));
			var interpolatedDisplayValue = interpolatedValue.ToString();
			zoomLevels.Add(interpolatedZoom, interpolatedDisplayValue);
		}

		zoomLevels[finalZoom] = finalDisplayValue;
	}

	public SKPoint GetCenter()
	{
		return new SKPoint(Width / 2, Height / 2);
	}

	public void MouseDown(Point mousePos)
	{
		isPanning = true;
		panStartPoint = new SKPoint((float)mousePos.X, (float)mousePos.Y);
	}

	public void MouseUp(object sender, MouseButtonEventArgs e)
	{
		isPanning = false;
		var latlon = ScreenMap.ScreenToCoordinate(new Size(Width, Height), scale, panOffset, GetCenter());
		curLat = latlon.X;
		curLon = latlon.Y;
	}

	public void MouseMove(Point mousePos)
	{
		if (isPanning)
		{
			var dx = mousePos.X - panStartPoint.X;
			var dy = mousePos.Y - panStartPoint.Y;
			panOffset.X += (float)dx;
			panOffset.Y += (float)dy;
			panStartPoint = new SKPoint((float)mousePos.X, (float)mousePos.Y);
			var latlon = ScreenMap.ScreenToCoordinate(new Size(Width, Height), scale, panOffset, GetCenter());
			curLat = latlon.X;
			curLon = latlon.Y;
		}
	}

	public double MouseWheel(int delta, bool invert)
	{
		var centerXBeforeZoom = Width / 2;
		var centerYBeforeZoom = Height / 2;
		var zoomIn = delta > 0;

		var indexIncrement = zoomIn ? 1 : -1;
		var nextZoomIndex = currentZoomIndex + indexIncrement;
		if (nextZoomIndex >= 0 && nextZoomIndex < zoomLevels.Count)
		{
			var newScale = zoomLevels.Keys.ElementAt(nextZoomIndex);
			var scaleFactor = newScale / scale;
			var shiftX = (int)((centerXBeforeZoom - panOffset.X) * (1 - 1 / scaleFactor));
			var shiftY = (int)((centerYBeforeZoom - panOffset.Y) * (1 - 1 / scaleFactor));
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