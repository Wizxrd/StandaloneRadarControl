using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
namespace Server.Models;

public class Settings
{
    public WindowSettings WindowSettings { get; set; } = new(50,50,900,600);
    public GeneralSettings GeneralSettings { get; set; } = new();
    public BasicExportSettings BasicExportSettings { get; set; } = new();
    public AdvancedExportSettings AdvancedExportSettings { get; set; } = new();
    public RealisticExportSettings RealisticExportSettings { get; set; } = new();
}
