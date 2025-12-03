using Client.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Client.Services;

public class SurveillanceService : ISurveillanceService
{
    public static List<JObject> Airplanes = new();
}
