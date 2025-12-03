using Client.Engines;
using Client.Models;
using Client.Renderables.Interfaces;
using Client.Utils;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client.Renderables
{
    public class Renderer
    {
        public static List<IRenderable> FromFeatures(DisplayState displayState, IEnumerable<ProcessedFeature> features)
        {
            File.WriteAllText("D:\\GitHub\\DisplayState.json", JsonConvert.SerializeObject(displayState));
            var list = new List<IRenderable>();
            try
            {
                foreach (var f in features)
                {
                    var geomType = f.GeometryType == "Line" ? "LineString" : f.GeometryType;
                    var geomObj = f.Geometry as JObject ?? (f.Geometry is JToken t ? (JObject)t : null);
                    if (geomObj == null) continue;

                    var coords = geomObj["coordinates"] as JArray;
                    if (coords == null) continue;

                    var style = (f.AppliedAttributes.TryGetValue("style", out var sv) && sv != null) ? sv.ToString() : "OtherWaypoints";
                    string rgb = (f.AppliedAttributes.TryGetValue("rgb", out var argb) && argb != null) ? argb.ToString() : string.Empty;
                    double[] parts = rgb.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

                    byte r = (byte)parts[0];
                    byte g = (byte)parts[1];
                    byte b = (byte)parts[2];
                    switch (geomType)
                    {
                        case "LineString":
                            {
                                var segments = ToSegments(geomType, coords);
                                if (segments.Count == 0) break;
                                SKPaint paint = Paints.VideoMapLine(f, r, g, b);
                                foreach (var segment in segments)
                                {
                                    if (segment.Count < 2) continue;
                                    for (int i = 0; i < segment.Count - 1; i++)
                                    {
                                        Coordinate start = new Coordinate { Lat = segment[i].lat, Lon = segment[i].lon };
                                        Coordinate end = new Coordinate { Lat = segment[i + 1].lat, Lon = segment[i + 1].lon };
                                        SKPoint startScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, start);
                                        SKPoint endScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, end);
                                        list.Add(new Line(startScreen, endScreen, paint, 0));
                                    }
                                }
                                break;
                            }
                        case "MultiLineString":
                            {
                                var segments = ToSegments(geomType, coords);
                                if (segments.Count == 0) break;
                                SKPaint paint = Paints.VideoMapLine(f, r, g, b);
                                foreach (var segment in segments)
                                {
                                    if (segment.Count < 2) continue;
                                    for (int i = 0; i < segment.Count - 1; i++)
                                    {
                                        Coordinate start = new Coordinate { Lat = segment[i].lat, Lon = segment[i].lon };
                                        Coordinate end = new Coordinate { Lat = segment[i + 1].lat, Lon = segment[i + 1].lon };
                                        SKPoint startScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, start);
                                        SKPoint endScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, end);
                                        list.Add(new Line(startScreen, endScreen, paint, 0));
                                    }
                                }
                                break;
                            }
                        case "Text":
                            {
                                double lon = coords[0].Value<double>();
                                double lat = coords[1].Value<double>();
                                if (lat != 0 || lon != 0)
                                {
                                    SKPaint paint = Paints.VideoMapText(r, g, b, f.FontSize);
                                    Coordinate coordinate = new Coordinate { Lat = lat, Lon = lon };
                                    SKPoint screenPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, coordinate);
                                    SKPoint drawPoint = new SKPoint(screenPoint.X + f.XOffset, screenPoint.Y - (f.YOffset * 1.5f));
                                    list.Add(new Text(f.TextContent, drawPoint, paint, 0));
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Renderables.FromFeatures", ex.ToString());
            }
            return list;
        }

        public static List<IRenderable> FromAirplanes(DisplayState displayState, List<JObject> airplanes)
        {
            List<IRenderable> renderables = new();
            SKPaint rectPaint = Paints.Rect(SKColors.Cyan, SKPaintStyle.Fill, 0);
            SKPaint textPaint = Paints.Text(rectPaint.Color, 12, SKPaintStyle.StrokeAndFill, 0);
            foreach (JObject airplane in airplanes)
            {
                bool airborne = airplane["airborne"]?.ToObject<bool>() ?? false;
                double alt = airplane["alt"]?.ToObject<double>() ?? 0;
                double heading = airplane["heading"]?.ToObject<double>() ?? 0;
                double lat = airplane["lat"]?.ToObject<double>() ?? 0;
                double lon = airplane["lon"]?.ToObject<double>() ?? 0;
                string name = airplane["name"]?.ToObject<string>() ?? "Unknown";
                bool player = airplane["player"]?.ToObject<bool>() ?? false;
                string type = airplane["type"]?.ToObject<string>() ?? "Unknown";
                Vector3 velocity = airplane["velocity"]?.ToObject<Vector3>() ?? Vector3.Zero;
                Vector3 wind = airplane["wind"]?.ToObject<Vector3>() ?? Vector3.Zero;
                Coordinate coordinate = new Coordinate { Lat = lat, Lon = lon };
                SKPoint screenPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, coordinate);
                SKPoint newt = new();
                string t = "F";
                if (name == "Aerial-1-1") t = "C";
                newt.X = screenPoint.X - (SkiaEngine.MeasureText(textPaint, t) / 2);
                newt.Y = screenPoint.Y;
                renderables.Add(new Text(t, newt, Paints.Text(rectPaint.Color, 14, SKPaintStyle.StrokeAndFill, 0), 2));
                renderables.Add(new Arc(screenPoint, 12, -180, 180, rectPaint, 2));
            }
            return renderables;
        }

        private static List<List<(double lat, double lon)>> ToSegments(string geomType, JArray coords)
        {
            var result = new List<List<(double lat, double lon)>>();
            if (geomType == "LineString")
            {
                var s = ParseLine(coords);
                if (s.Count > 0) result.Add(s);
            }
            else
            {
                foreach (var item in coords)
                {
                    if (item is JArray sub)
                    {
                        if (sub.First is JArray inner && inner.First is JArray)
                            result.AddRange(ToSegments("MultiLineString", sub));
                        else
                        {
                            var s = ParseLine(sub);
                            if (s.Count > 0) result.Add(s);
                        }
                    }
                }
            }
            return result;
        }

        private static List<(double lat, double lon)> ParseLine(JArray arr)
        {
            var list = new List<(double lat, double lon)>(arr.Count);
            foreach (var pt in arr)
            {
                if (pt is not JArray a || a.Count < 2) continue;
                if ((a[0].Type != JTokenType.Float && a[0].Type != JTokenType.Integer) ||
                    (a[1].Type != JTokenType.Float && a[1].Type != JTokenType.Integer)) continue;
                double lon = a[0].ToObject<double>();
                double lat = a[1].ToObject<double>();
                list.Add((lat, lon));
            }
            return list;
        }
    }
}
