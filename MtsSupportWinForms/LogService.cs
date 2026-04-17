using System;
using System.IO;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public static class LogService
    {
        private static string LogFilePath
        {
            get
            {
                var dir = Path.Combine(Application.StartupPath, "logs");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "activity.log");
            }
        }

        public static void Log(string action, string details)
        {
            try
            {
                File.AppendAllText(LogFilePath,
                    string.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2}{3}", DateTime.Now, action, details, Environment.NewLine));
            }
            catch
            {
            }
        }

        public static string[] ReadAll()
        {
            if (!File.Exists(LogFilePath)) return new string[0];
            return File.ReadAllLines(LogFilePath);
        }
    }
}
