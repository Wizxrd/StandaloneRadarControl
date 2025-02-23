using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using Client.Views;
using Client.ViewModels;
using System.Windows.Annotations;
using System.Numerics;

namespace Client.Network
{
    public class TcpCallbackHandler
    {
        public static readonly Dictionary<string, Action<TcpClientHandler, MainWindowView, JObject>> CommandMap = new()
        {
            { "OnConnectionEstablished", OnConnectionEstablished },
            { "OnGlobalContactExport", OnGlobalContactExport},
        };

        public static void OnConnectionEstablished(TcpClientHandler handler, MainWindowView mainWindowView, JObject jObject)
        {
            Logger.Debug("TcpCallbackHandler.OnConnectionEstablished", "Connection established");
            handler.connectionEstablished = true;
        }

        public static void OnGlobalContactExport(TcpClientHandler handler, MainWindowView mainWindowView, JObject jObject)
        {
            foreach (JObject contact in (JArray)jObject["contacts"])
            {
                string name = (string)contact["name"];
                bool player = (bool)contact["player"];
                string side = (string)contact["side"];
                double lat = (double)contact["lat"];
                double lon = (double)contact["lon"];
                double alt = (double)contact["alt"];
                JObject wind = (JObject)contact["wind"];
                double heading = (double)contact["heading"];
                JObject velocity = (JObject)contact["velocity"];
                string type = (string)contact["type"];
                bool airborne = (bool)contact["airborne"];
                if (!Radar.Contacts.ContainsKey(name))
                {
                    Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", $"adding new: {name}");
                    Contact newContact = new()
                    {
                        Latitude = lat,
                        Longitude = lon,
                        Heading = heading,
                        Wind = new Vector3((float)wind["x"], (float)wind["y"], (float)wind["z"]),
                        Velocity = new Vector3((float)velocity["x"], (float)velocity["y"], (float)velocity["z"]),
                    };
                    Radar.Contacts[name] = newContact;
                }
                else
                {
                    Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", $"updating existing: {name}");
                    Contact existingContact = Radar.Contacts[name];
                    existingContact.Latitude = lat;
                    existingContact.Longitude = lon;
                    existingContact.Heading = heading;
                    existingContact.Wind = new Vector3((float)wind["x"], (float)wind["y"], (float)wind["z"]);
                    existingContact.Velocity = new Vector3((float)velocity["x"], (float)velocity["y"], (float)velocity["z"]);
                }
            }
            mainWindowView.InvalidateCanvas();
            Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", "Contact export recieved!");
        }
    }
}
