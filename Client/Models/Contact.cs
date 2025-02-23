using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
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

        public void Render(SKCanvas canvas, System.Drawing.Size size, double scale, SKPoint panOffset)
        {
            using var contactcolor = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#1e78ff"),
                StrokeWidth = 2,
                IsAntialias = true
            };

            SKPoint screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, Latitude, Longitude);

            float squareSize = 10;
            float halfSize = squareSize / 2;

            SKRect rect = new SKRect(
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

            float groundSpeedMPS = GetGroundSpeed(Velocity);
            float groundSpeedKnots = (float)Math.Round(groundSpeedMPS * 3600 / 1852);

            float angleDegrees = (float)(Heading * 180 / Math.PI);
            angleDegrees = 90 - angleDegrees; // Adjust for screen coordinates
            float angleRadians = angleDegrees * (float)Math.PI / 180f;

            float minVecLength = 10f;
            float maxVecLength = 100f;
            float lineLength = minVecLength + (maxVecLength - minVecLength) * (groundSpeedKnots / 1000f);
            lineLength = Math.Clamp(lineLength, minVecLength, maxVecLength);

            float startX = screenPoint.X;
            float startY = screenPoint.Y;
            float lineEndX = startX + lineLength * (float)Math.Cos(angleRadians);
            float lineEndY = startY - lineLength * (float)Math.Sin(angleRadians);

            canvas.DrawLine(new SKPoint(startX, startY), new SKPoint(lineEndX, lineEndY), velocityveccolor);
            canvas.DrawRect(rect, contactcolor);
        }

    }
}
