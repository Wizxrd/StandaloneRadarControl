using AdonisUI.Controls;
using Client.SignalR;
using Client.UI.Displays.Tactical.CommandComposition;
using System.Windows.Input;
namespace Client.UI.Displays.Tactical;

public partial class TacticalView : AdonisWindow
{
    private CommandCompositionView commandCompositionView;

    public int Id { get; set; }

    public TacticalView()
    {
        InitializeComponent();
        DataContext = new TacticalViewModel(RenderDisplayView);
        commandCompositionView = CommandComposition as CommandCompositionView;
        KeyDown += TacticalView_KeyDown;
    }

    private void TacticalView_KeyDown(object sender, KeyEventArgs e)
    {
        var tb = commandCompositionView.CompositionTextBlock;
        if (tb == null) return;

        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

        if (e.Key == Key.Escape)
        {
            tb.Text = "_";
            commandCompositionView.FeedbackTextBlock.Text = "";
            commandCompositionView.AcceptedTextBox.Text = "";
            commandCompositionView.InvalidTextBox.Text = "";
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (SignalRClient.Connection?.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
            {
                commandCompositionView.FeedbackTextBlock.Text = "NOT CONNECTED";
                commandCompositionView.AcceptedTextBox.Text = "";
                commandCompositionView.InvalidTextBox.Text = "X";
                tb.Text = "_";
            }
            else
            {

            }
            return;
        }

        if (e.Key == Key.Back)
        {
            if (tb.Text == "_" || tb.Text.Length == 0) return;

            tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
            if (tb.Text.Length == 0) tb.Text = "_";
            return;
        }

        if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
            e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            return;

        if (tb.Text == "_")
            tb.Text = string.Empty;

        char? ch = null;

        if (e.Key >= Key.A && e.Key <= Key.Z)
        {
            ch = (char)('A' + (e.Key - Key.A));
        }
        else
        {
            switch (e.Key)
            {
                case Key.Space: ch = ' '; break;

                case Key.D0: ch = shift ? ')' : '0'; break;
                case Key.D1: ch = shift ? '!' : '1'; break;
                case Key.D2: ch = shift ? '@' : '2'; break;
                case Key.D3: ch = shift ? '#' : '3'; break;
                case Key.D4: ch = shift ? '$' : '4'; break;
                case Key.D5: ch = shift ? '%' : '5'; break;
                case Key.D6: ch = shift ? '^' : '6'; break;
                case Key.D7: ch = shift ? '&' : '7'; break;
                case Key.D8: ch = shift ? '*' : '8'; break;
                case Key.D9: ch = shift ? '(' : '9'; break;

                case Key.NumPad0: ch = '0'; break;
                case Key.NumPad1: ch = '1'; break;
                case Key.NumPad2: ch = '2'; break;
                case Key.NumPad3: ch = '3'; break;
                case Key.NumPad4: ch = '4'; break;
                case Key.NumPad5: ch = '5'; break;
                case Key.NumPad6: ch = '6'; break;
                case Key.NumPad7: ch = '7'; break;
                case Key.NumPad8: ch = '8'; break;
                case Key.NumPad9: ch = '9'; break;

                case Key.Multiply: ch = '*'; break;
                case Key.Divide: ch = '/'; break;
                case Key.Add: ch = '+'; break;
                case Key.Subtract: ch = '-'; break;
                case Key.Decimal: ch = '.'; break;

                case Key.OemPeriod: ch = '.'; break;
                case Key.OemMinus: ch = shift ? '_' : '-'; break;
                case Key.OemPlus: ch = shift ? '+' : '='; break;
                case Key.Oem1: ch = shift ? ':' : ';'; break;          // ; :
                case Key.Oem2: ch = '/'; break;                         // /
                //case Key.OemQuestion: ch = '?'; break;
                case Key.OemComma: /* no comma glyph */ break;

                case Key.OemQuotes:
                //case Key.Oem7:
                    //ch = shift ? '"' : '\'';                            // ' "
                    //break;

                case Key.OemOpenBrackets:
                    ch = shift ? '{' : '[';
                    break;
                case Key.Oem6:
                    ch = shift ? '}' : ']';
                    break;

                case Key.Up: ch = '↑'; break;
                case Key.Down: ch = '↓'; break;
            }
        }

        if (ch.HasValue)
        {
            const string allowed =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "0123456789" +
                " .:;'\"()?+-*/=!@#$%^&{}[]<>_↑↓";

            if (allowed.IndexOf(ch.Value) >= 0)
                tb.Text += ch.Value;
        }
    }
}
