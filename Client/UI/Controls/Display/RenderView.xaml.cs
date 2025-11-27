using System.Windows.Controls;
using vFalcon.Engines;
namespace Client.UI.Controls.Display;


public partial class RenderView : UserControl
{
    public SkiaEngine SkiaEngine { get; }
    public RenderView()
    {
        InitializeComponent();
        SkiaEngine = new(this);
    }
}
