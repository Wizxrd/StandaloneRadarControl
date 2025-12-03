using Client.Models;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils;

public class GeoJson
{
    public static async Task<Dictionary<string, List<ProcessedFeature>>> GetEramFacilityFeatures(JArray activeVideoMapIds)
    {
        try
        {
            Logger.Info("GeoJson.LoadFacilityFeatures", "Creating FacilityFeatures...");
            string sourcePath = Path.Combine(PathFinder.GetAppDirectory(), $"VideoMaps\\Caucasus");
            var features = new Dictionary<string, List<ProcessedFeature>>();
            var videoMapTdmLookup = new Dictionary<string, bool>();
            var matchedFiles = activeVideoMapIds
                .Select(id => Path.Combine(sourcePath, id + ".geojson"))
                .Where(File.Exists)
                .ToList();

            await Task.Run(() =>
            {
                try
                {
                    var rawFeatures = new Dictionary<int, List<ProcessedFeature>>();

                    foreach (var file in matchedFiles)
                    {
                        string fileId = Path.GetFileNameWithoutExtension(file);
                        bool tdmOnly = videoMapTdmLookup.TryGetValue(fileId, out var tdm) && tdm;

                        JObject geoJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file));
                        if (geoJson == null) continue;
                        var defaults = ExtractLastIsDefaults(geoJson);

                        foreach (JObject feature in geoJson["features"] ?? Enumerable.Empty<JToken>())
                        {
                            var properties = feature["properties"] as JObject
                                           ?? feature["geometry"]?["properties"] as JObject
                                           ?? new JObject();

                            if (IsDefaultsFeature(properties))
                                continue;

                            var geometryToken = feature["geometry"];
                            if (geometryToken == null || geometryToken.Type == JTokenType.Null || geometryToken["coordinates"] == null)
                            {
                                Logger.Info("GeoJson.LoadFacilityFeatures", $"Missing or invalid geometry. Substituting default Point.\n{feature.ToString(Formatting.None)}");
                                geometryToken = new JObject
                                {
                                    ["type"] = "Point",
                                    ["coordinates"] = new JArray(0.0, 0.0)
                                };
                                feature["geometry"] = geometryToken;
                            }

                            var geomObj = geometryToken as JObject;
                            string geometryType = geomObj?.Value<string>("type") ?? "Point";

                            var finalProperties = MergeProperties(properties, defaults, geometryType);
                            finalProperties["tdmOnly"] = tdmOnly;

                            List<int> filters = new List<int>();
                            if (finalProperties.TryGetValue("filters", out var filtersObj) && filtersObj != null)
                            {
                                if (filtersObj is JArray arr)
                                    filters.AddRange(arr.Where(f => f != null && f.Type != JTokenType.Null).Select(f => (int)f));
                                else if (filtersObj is JValue val && val.Value != null)
                                    filters.Add(val.Value<int>());
                                else if (int.TryParse(filtersObj.ToString(), out int parsed))
                                    filters.Add(parsed);
                            }

                            if (filters.Count == 0)
                            {
                                if (defaults.LineDefaults.TryGetValue("filters", out var defaultFiltersObj) && defaultFiltersObj != null)
                                {
                                    if (defaultFiltersObj is JArray arr)
                                        filters.AddRange(arr.Select(f => Convert.ToInt32(f)));
                                    else if (defaultFiltersObj is int dfInt)
                                        filters.Add(dfInt);
                                    else if (int.TryParse(defaultFiltersObj.ToString(), out int parsedDf))
                                        filters.Add(parsedDf);
                                }
                            }

                            foreach (int filter in filters)
                            {
                                if (!rawFeatures.ContainsKey(filter))
                                    rawFeatures[filter] = new List<ProcessedFeature>();

                                rawFeatures[filter].Add(new ProcessedFeature
                                {
                                    GeometryType = ResolveFeatureType(geometryType, properties),
                                    Geometry = feature["geometry"],
                                    AppliedAttributes = finalProperties
                                });
                            }
                        }
                    }

                    foreach (var kvp in rawFeatures)
                    {
                        int filterIndex = kvp.Key;
                        var featuresList = kvp.Value;
                        var condensedList = new List<ProcessedFeature>();

                        foreach (var group in featuresList
                            .Where(f => f.GeometryType is "Line" or "LineString" or "MultiLineString")
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            var mergedCoords = new JArray();

                            foreach (var f in group)
                            {
                                var coords = (f.Geometry as JObject)?["coordinates"] as JArray;
                                if (coords == null || coords.Count == 0) continue;

                                if (f.GeometryType is "Line" or "LineString")
                                {
                                    mergedCoords.Add(coords);
                                }
                                else if (f.GeometryType == "MultiLineString")
                                {
                                    foreach (var segment in coords.OfType<JArray>())
                                        mergedCoords.Add(segment);
                                }
                            }

                            if (mergedCoords.Count > 0)
                            {
                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "MultiLineString",
                                    Geometry = new JObject
                                    {
                                        ["type"] = "MultiLineString",
                                        ["coordinates"] = mergedCoords
                                    },
                                    AppliedAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key)
                                });
                            }
                        }

                        foreach (var group in featuresList
                            .Where(f => f.GeometryType == "Symbol" && ((f.Geometry as JObject)?["type"]?.ToString() == "Point"))
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            foreach (var f in group)
                            {
                                var coords = (f.Geometry as JObject)?["coordinates"] as JArray;
                                if (coords == null || coords.Count != 2) continue;

                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "Point",
                                    Geometry = new JObject { ["type"] = "Point", ["coordinates"] = coords },
                                    AppliedAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key)
                                });
                            }
                        }

                        foreach (var group in featuresList
                            .Where(f => f.GeometryType == "Text" && ((f.Geometry as JObject)?["type"]?.ToString() == "Point"))
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            foreach (var f in group)
                            {
                                var coords = (f.Geometry as JObject)?["coordinates"] as JArray;
                                if (coords == null || coords.Count != 2) continue;

                                var attrs = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key);
                                string textStr = attrs.TryGetValue("text", out var tVal) && tVal is JArray tArr
                                    ? string.Join("\n", tArr.Select(t => t.ToString()))
                                    : string.Empty;

                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "Text",
                                    Geometry = new JObject { ["type"] = "Point", ["coordinates"] = coords },
                                    AppliedAttributes = attrs,
                                    TextContent = textStr,
                                    FontSize = SafeGetFloat(attrs, "size", 14f) * 12f,
                                    XOffset = SafeGetFloat(attrs, "xOffset", 0f),
                                    YOffset = SafeGetFloat(attrs, "yOffset", 0f),
                                    Underline = attrs.TryGetValue("underline", out var ul) && Convert.ToBoolean(ul)
                                });
                            }
                        }

                        features[filterIndex.ToString()] = condensedList;
                    }

                    Logger.Info("GeoJson.LoadFacilityFeatures", "Created FacilityFeatures");
                }
                catch (Exception ex)
                {
                    Logger.Error("GeoJson.LoadFacilityFeatures", ex.ToString());
                }
            });
            return features;
        }
        catch (Exception ex)
        {
            Logger.Error("GeoJson.LoadFacilityFeatures", ex.ToString());
            return new Dictionary<string, List<ProcessedFeature>>();
        }
    }

    private static string ResolveFeatureType(string geometryType, JObject props)
    {
        if (geometryType == "LineString" || geometryType == "MultiLineString")
            return "Line";
        if (geometryType == "Point" && props.ContainsKey("text"))
            return "Text";
        if (geometryType == "Point")
            return "Symbol";
        return geometryType;
    }

    private static bool IsDefaultsFeature(JObject props)
    {
        if (props == null) return false;

        return props.Value<bool?>("isLineDefaults") == true || props.Value<bool?>("isSymbolDefaults") == true || props.Value<bool?>("isTextDefaults") == true;
    }
    private static Dictionary<string, object> MergeProperties(JObject featureProps, FeatureDefaults defaults, string geometryType)
    {
        var merged = new Dictionary<string, object>();

        // Normalize feature type
        string normalizedType = geometryType switch
        {
            "LineString" or "MultiLineString" => "Line",
            "Point" when featureProps?.ContainsKey("text") == true => "Text",
            "Point" => "Symbol",
            _ => geometryType
        };

        Dictionary<string, object> typeDefaults = normalizedType switch
        {
            "Line" => defaults.LineDefaults,
            "Symbol" => defaults.SymbolDefaults,
            "Text" => defaults.TextDefaults,
            _ => new Dictionary<string, object>()
        };

        if (featureProps == null || featureProps.Count == 0)
        {
            foreach (var kv in typeDefaults)
                merged[kv.Key] = kv.Value;

            ApplyCrcAutoDefaults(merged, normalizedType);
            return merged;
        }

        foreach (var kv in typeDefaults)
        {
            if (!featureProps.TryGetValue(kv.Key, out var value) || value == null || value.Type == JTokenType.Null)
                merged[kv.Key] = kv.Value;
            else
                merged[kv.Key] = value;
        }

        foreach (var kv in featureProps)
        {
            if (!merged.ContainsKey(kv.Key))
                merged[kv.Key] = kv.Value;
        }

        ApplyCrcAutoDefaults(merged, normalizedType);
        return merged;
    }

    private static void ApplyCrcAutoDefaults(Dictionary<string, object> props, string featureType)
    {
        switch (featureType)
        {
            case "Line":
                if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                if (!props.ContainsKey("style")) props["style"] = "solid";
                if (!props.ContainsKey("rgb")) props["rgb"] = "255,255,255";
                if (!props.ContainsKey("thickness")) props["thickness"] = 1;
                break;

            case "Symbol":
                if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                if (!props.ContainsKey("style")) props["style"] = "vor";
                if (!props.ContainsKey("rgb")) props["rgb"] = "255,255,255";
                if (!props.ContainsKey("size")) props["size"] = 1;
                break;

            case "Text":
                if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                if (!props.ContainsKey("size")) props["size"] = 1;
                if (!props.ContainsKey("underline")) props["underline"] = false;
                if (!props.ContainsKey("xOffset")) props["xOffset"] = 0;
                if (!props.ContainsKey("yOffset")) props["yOffset"] = 0;
                if (!props.ContainsKey("opaque")) props["opaque"] = false;
                break;
        }
    }

    private static FeatureDefaults ExtractLastIsDefaults(JObject geojson)
    {
        var defaults = new FeatureDefaults
        {
            LineDefaults = new Dictionary<string, object>(),
            SymbolDefaults = new Dictionary<string, object>(),
            TextDefaults = new Dictionary<string, object>()
        };

        if (geojson?["features"] is not JArray featuresArray)
            return defaults;

        foreach (var feature in featuresArray)
        {
            var props = feature["properties"] as JObject;
            if (props == null) continue;

            if (props.Value<bool?>("isLineDefaults") == true)
            {
                props.Remove("isLineDefaults");
                defaults.LineDefaults = props.ToObject<Dictionary<string, object>>();
            }

            if (props.Value<bool?>("isSymbolDefaults") == true)
            {
                props.Remove("isSymbolDefaults");
                defaults.SymbolDefaults = props.ToObject<Dictionary<string, object>>();
            }

            if (props.Value<bool?>("isTextDefaults") == true)
            {
                props.Remove("isTextDefaults");
                defaults.TextDefaults = props.ToObject<Dictionary<string, object>>();
            }
        }

        return defaults;
    }

    private static float SafeGetFloat(Dictionary<string, object> dict, string key, float fallback)
    {
        if (!dict.TryGetValue(key, out var val) || val == null)
            return fallback;

        try { return Convert.ToSingle(val); }
        catch { return fallback; }
    }
}
