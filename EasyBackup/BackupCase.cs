using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyBackup
{
    public enum IterationType { Daily, Weekly, Monthly }

    [System.Serializable]
    public class BackupCase
    {
        public Guid BackupID { get; set; }
        public string name { get; set; }
        public string sourcePath { get; set; }
        public string destinationPath { get; set; }
        public DateTime backupTime { get; set; }
        public IterationType iteration { get; set; }
        public DateTime lastBackupTime { get; set; }

        public BackupCase()
        {
            BackupID = Guid.NewGuid();

        }
    }
}