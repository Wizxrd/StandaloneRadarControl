using Common.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils;

public class Paints
{
    public static readonly SKTypeface FontTypeFace = SKTypeface.FromFile(PathFinder.GetFilePath("Resources/Fonts", "Retro.ttf"));

    public static SKPaint Text(SKColor color, float textSize, SKPaintStyle style, float stroke = 0f)
    {
        {
            return new SKPaint
            {
                Color = color,
                Typeface = FontTypeFace,
                TextSize = textSize,
                IsAntialias = true,
                Style = style,
                StrokeWidth = stroke
            };
        }
    }

    public static SKPaint Line(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }

    public static SKPaint Rect(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }

    public static SKPaint Circle(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }
}
