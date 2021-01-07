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
        public Guid BackupId { get; set; }
        public string BackupTitle { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public TimeSpan BackupTime { get; set; } = DateTime.Now.TimeOfDay;
        public IterationType Iteration { get; set; }
        public DateTime LastBackupDateTime { get; set; }

        public BackupCase()
        {
            BackupId = Guid.NewGuid();
        }

    }
}