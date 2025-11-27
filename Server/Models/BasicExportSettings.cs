using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models;

public class BasicExportSettings
{
    public int UpdateRate { get; set; } = 1;
    public int MaxTargets { get; set; } = 500;
    public bool Airplanes { get; set; } = true;
    public bool Helicopters { get; set; } = true;
    public bool GroundVehicles { get; set; } = true;
    public bool Ships {  get; set; } = true;
    public bool AAMissiles { get; set; } = true;
}
