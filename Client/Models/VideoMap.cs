using System.Drawing;
using System.IO;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace Client.Models;

public class VideoMap
{
	private readonly List<List<Coordinate>> lineStrings = new();
	public List<string> activeMaps = new();
	private Dictionary<Coordinate, PointF> projectedPointCache = new();

	private JObject theatreJson;

	public static void CenterAtCoordinates(int Width, int Height, double scale, ref SKPoint panOffset, double lat,
		double lon)
	{
		var screenPoint = ScreenMap.CoordinateToScreen(Width, Height, scale, panOffset, lat, lon);
		var centerX = Width / 2;
		var centerY = Height / 2;
		var shiftX = centerX - screenPoint.X;
		var shiftY = centerY - screenPoint.Y;
		panOffset.X += shiftX;
		panOffset.Y += shiftY;
	}

	public void Unload(string file)
	{
		var jsonText = File.ReadAllText(file);
		var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(jsonText);
		if (featureCollection == null) return;
		foreach (var feature in featureCollection.Features)
			if (feature.Geometry is LineString lineString)
				foreach (var coordinates in lineStrings.ToList())
					if (coordinates.Count == lineString.Coordinates.Count &&
						!coordinates.Where((coord, i) =>
							coord.Latitude != lineString.Coordinates[i].Latitude ||
							coord.Longitude != lineString.Coordinates[i].Longitude).Any())
					{
						lineStrings.Remove(coordinates);
						break;
					}

		activeMaps.Remove(Path.GetFileName(file));
	}

	public void Load(string file)
	{
		try
		{
			// Read the JSON file content
			var jsonText = File.ReadAllText(file);

			// Parse the JSON
			var featureCollection = JObject.Parse(jsonText);

			// Loop through features in the parsed JSON
			foreach (var feature in featureCollection["features"])
				// Check if the geometry type is "MultiPolygon"
				if (feature["geometry"]?["type"]?.ToString() == "MultiPolygon")
				{
					var multiPolygonCoordinates = feature["geometry"]["coordinates"];

					foreach (var polygon in multiPolygonCoordinates)
					{
						var coordinates = new List<Coordinate>();

						// Iterate through each position in the polygon (each position is a pair of [longitude, latitude])
						foreach (var position in polygon)
						{
							// Convert the current position to a List<double> (which should be the coordinate pair)
							var coordArray = position.ToObject<List<List<double>>>();

							foreach (var coord in coordArray)
								if (coord.Count >= 2)
									coordinates.Add(new Coordinate(coord[1],
										coord[0])); // Assuming [latitude, longitude]
						}

						// Add the processed polygon coordinates to lineStrings
						lineStrings.Add(coordinates);
					}
				}
		}
		catch (Exception ex)
		{
			Logger.Error("VideoMap.Load", ex.ToString());
		}
	}

	public void Render(SKCanvas canvas, Size clientSize, double scale, SKPoint panOffset)
	{
		try
		{
			var centerX = clientSize.Width / 2;
			var centerY = clientSize.Height / 2;

			using var paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = SKColors.White,
				StrokeWidth = 1,
				IsAntialias = true
			};

			var drawnSegments = new HashSet<(double, double, double, double)>();

			foreach (var lineString in lineStrings)
			{
				if (lineString.Count < 2)
					continue;

				var skPoints = new List<SKPoint>();

				for (var i = 0; i < lineString.Count - 1; i++)
				{
					var coord1 = lineString[i];
					var coord2 = lineString[i + 1];

					var segment = (Math.Min(coord1.Latitude, coord2.Latitude),
						Math.Min(coord1.Longitude, coord2.Longitude),
						Math.Max(coord1.Latitude, coord2.Latitude),
						Math.Max(coord1.Longitude, coord2.Longitude));

					if (drawnSegments.Contains(segment))
						continue;

					drawnSegments.Add(segment);

					var screenPoint1 = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale,
						panOffset,
						coord1.Latitude, coord1.Longitude);
					var screenPoint2 = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale,
						panOffset,
						coord2.Latitude, coord2.Longitude);

					var offsetX1 = screenPoint1.X - centerX;
					var offsetY1 = screenPoint1.Y - centerY;
					screenPoint1 = new SKPoint(centerX + offsetX1, centerY + offsetY1);

					var offsetX2 = screenPoint2.X - centerX;
					var offsetY2 = screenPoint2.Y - centerY;
					screenPoint2 = new SKPoint(centerX + offsetX2, centerY + offsetY2);

					skPoints.Add(screenPoint1);
					skPoints.Add(screenPoint2);
				}

				if (skPoints.Count > 1) canvas.DrawPoints(SKPointMode.Lines, skPoints.ToArray(), paint);
			}
		}
		catch (Exception ex)
		{
			Logger.Error("VideoMap.Render", ex.ToString());
		}
	}
}