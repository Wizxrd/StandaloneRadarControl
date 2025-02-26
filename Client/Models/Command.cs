using System.Windows.Input;
using Client.Views;

namespace Client.Models;

public class Command
{
	private readonly CommandAreaViewModel commandAreaViewModel;

	public Command(CommandAreaViewModel commandAreaViewModel)
	{
		this.commandAreaViewModel = commandAreaViewModel;
	}

	public void Handle(bool ctrlPressed, bool shiftPressed, Key key)
	{
		var text = GetKeyCharacter(key, shiftPressed);
		if (!string.IsNullOrEmpty(text))
			AddText(text);
		else if (key == Key.Escape)
			ClearText();
		else if (key == Key.Enter)
			ClearText();
		else if (key == Key.Back && commandAreaViewModel.CommandText.Length > 0)
			commandAreaViewModel.CommandText =
				commandAreaViewModel.CommandText.Substring(0, commandAreaViewModel.CommandText.Length - 1);
	}

	public void ClearText()
	{
		commandAreaViewModel.CommandText = string.Empty;
	}

	public void AddText(string text)
	{
		commandAreaViewModel.CommandText += text;
	}

	private string GetKeyCharacter(Key key, bool isShiftPressed)
	{
		switch (key)
		{
			case Key.A: return "A";
			case Key.B: return "B";
			case Key.C: return "C";
			case Key.D: return "D";
			case Key.E: return "E";
			case Key.F: return "F";
			case Key.G: return "G";
			case Key.H: return "H";
			case Key.I: return "I";
			case Key.J: return "J";
			case Key.K: return "K";
			case Key.L: return "L";
			case Key.M: return "M";
			case Key.N: return "N";
			case Key.O: return "O";
			case Key.P: return "P";
			case Key.Q: return "Q";
			case Key.R: return "R";
			case Key.S: return "S";
			case Key.T: return "T";
			case Key.U: return "U";
			case Key.V: return "V";
			case Key.W: return "W";
			case Key.X: return "X";
			case Key.Y: return "Y";
			case Key.Z: return "Z";

			case Key.D1: return isShiftPressed ? "!" : "1";
			case Key.D2: return isShiftPressed ? "@" : "2";
			case Key.D3: return isShiftPressed ? "#" : "3";
			case Key.D4: return isShiftPressed ? "$" : "4";
			case Key.D5: return isShiftPressed ? "%" : "5";
			case Key.D6: return isShiftPressed ? "^" : "6";
			case Key.D7: return isShiftPressed ? "&" : "7";
			case Key.D8: return isShiftPressed ? "*" : "8";
			case Key.D9: return isShiftPressed ? "(" : "9";
			case Key.D0: return isShiftPressed ? ")" : "0";

			case Key.OemMinus: return isShiftPressed ? "_" : "-";
			case Key.OemPlus: return isShiftPressed ? "+" : "=";
			case Key.OemOpenBrackets: return isShiftPressed ? "{" : "[";
			case Key.OemCloseBrackets: return isShiftPressed ? "}" : "]";
			case Key.OemSemicolon: return isShiftPressed ? ":" : ";";
			case Key.OemQuotes: return isShiftPressed ? "\"" : "'";
			case Key.OemComma: return isShiftPressed ? "<" : ",";
			case Key.OemPeriod: return isShiftPressed ? ">" : ".";
			case Key.OemQuestion: return isShiftPressed ? "?" : "/";
			case Key.OemPipe: return isShiftPressed ? "|" : "\\";
			case Key.Space: return " ";
			default: return string.Empty;
		}
	}
}