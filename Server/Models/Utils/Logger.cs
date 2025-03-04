using System.IO;

namespace Server.Models.Utils
{
    public class Logger
    {
        public static void Wipe()
        {
            File.WriteAllText(LoadFile.Load("Logs", "log.txt"), string.Empty);
        }

        public static void Alert(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | ALERT | {function} | {message}");
                }
            }
            catch
            {

            }
        }

        public static void Error(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | ERROR | {function} | {message}");
                }
            }
            catch
            {

            }
        }

        public static void Warning(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | WARNING | {function} | {message}");
                }
            }
            catch
            {

            }
        }

        public static void Info(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | INFO | {function} | {message}");
                }
            }
            catch
            {

            }
        }

        public static void Debug(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | DEBUG | {function} | {message}");
                }
            }
            catch
            {

            }
        }
        public static void Trace(string function, string message)
        {
            try
            {
                string path = LoadFile.Load("Logs", "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now} | TRACE | {function} | {message}");
                }
            }
            catch
            {

            }
        }
    }
}
