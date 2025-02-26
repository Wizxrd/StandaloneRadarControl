using System.IO;

namespace Client.Models;

public class LoadFile
{
	public static bool DEBUG = true;

	public static string Load(string folderPath, string fileName)
	{
		try
		{
			string folderDir;
			string filePath;
			if (DEBUG)
			{
				var binDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
				var solutionDir = Directory.GetParent(binDir).FullName;
				folderDir = Path.Combine(solutionDir, folderPath);
				filePath = Path.Combine(folderDir, fileName);
			}
			else
			{
				var exeDir = AppDomain.CurrentDomain.BaseDirectory;
				folderDir = Path.Combine(exeDir, folderPath);
				filePath = Path.Combine(folderDir, fileName);
			}

			if (!Directory.Exists(folderDir)) Directory.CreateDirectory(folderDir);
			if (!File.Exists(filePath)) File.WriteAllText(filePath, "");
			return filePath;
		}
		catch (Exception ex)
		{
			Logger.Error("Load", ex.ToString());
			return string.Empty;
		}
	}

	public static string LoadFolder(string folderPath)
	{
		try
		{
			string folderDir;
			if (DEBUG)
			{
				var binDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
				var solutionDir = Directory.GetParent(binDir).FullName;
				folderDir = Path.Combine(solutionDir, folderPath);
			}
			else
			{
				var exeDir = AppDomain.CurrentDomain.BaseDirectory;
				folderDir = Path.Combine(exeDir, folderPath);
			}

			if (!Directory.Exists(folderDir)) Directory.CreateDirectory(folderDir);
			return folderDir;
		}
		catch (Exception ex)
		{
			Logger.Error("Load", ex.ToString());
			return string.Empty;
		}
	}
}