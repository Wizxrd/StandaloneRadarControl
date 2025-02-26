using System.Drawing;
using System.Numerics;
using SkiaSharp;

namespace Client.Models;

public class Contact
{
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public double Heading { get; set; }
	public Vector3 Wind { get; set; }
	public Vector3 Velocity { get; set; }

	private static float GetGroundSpeed(Vector3 velocity)
	{
		var groundComponent = velocity - Vector3.Dot(velocity, Vector3.UnitY) * Vector3.UnitY;
		return groundComponent.Length();
	}

	public void Render(SKCanvas canvas, Size size, double scale, SKPoint panOffset)
	{
		using var contactcolor = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColor.Parse("#1e78ff"),
			StrokeWidth = 2,
			IsAntialias = true
		};

		var screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, Latitude, Longitude);

		float squareSize = 10;
		var halfSize = squareSize / 2;

		var rect = new SKRect(
			screenPoint.X - halfSize,
			screenPoint.Y - halfSize,
			screenPoint.X + halfSize,
			screenPoint.Y + halfSize
		);

		using var velocityveccolor = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.White,
			StrokeWidth = 1,
			IsAntialias = true
		};

		var groundSpeedMPS = GetGroundSpeed(Velocity);
		var groundSpeedKnots = (float)Math.Round(groundSpeedMPS * 3600 / 1852);

		var angleDegrees = (float)(Heading * 180 / Math.PI);
		angleDegrees = 90 - angleDegrees; // Adjust for screen coordinates
		var angleRadians = angleDegrees * (float)Math.PI / 180f;

		var minVecLength = 10f;
		var maxVecLength = 100f;
		var lineLength = minVecLength + (maxVecLength - minVecLength) * (groundSpeedKnots / 1000f);
		lineLength = Math.Clamp(lineLength, minVecLength, maxVecLength);

		var startX = screenPoint.X;
		var startY = screenPoint.Y;
		var lineEndX = startX + lineLength * (float)Math.Cos(angleRadians);
		var lineEndY = startY - lineLength * (float)Math.Sin(angleRadians);

		canvas.DrawLine(new SKPoint(startX, startY), new SKPoint(lineEndX, lineEndY), velocityveccolor);
		canvas.DrawRect(rect, contactcolor);
	}
}