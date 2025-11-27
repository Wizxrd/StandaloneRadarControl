using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models;

public class GeneralSettings
{
    public bool SimultaneousConnect { get; set; } = true;
    public ExportMethod ExportMethod { get; set; } = new();
    public MaxControllers MaxControllers { get; set; } = new();
    public Passwords Passwords { get; set; } = new();
    public Ports Ports { get; set; } = new();
}
