using Microsoft.Win32;

namespace EasyBackup.Services
{
    public class RegistryHelper
    {
        const string EasyBackupRegistryName = "Easy_Backup";

        public static void AddApplicationToStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key?.SetValue(EasyBackupRegistryName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"");
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key?.DeleteValue(EasyBackupRegistryName, false);
            }
        }

        public static bool IsApplicationRegisteredForStartUp()
        {
            throw new System.NotImplementedException();
        }
    }
}