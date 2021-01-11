using System;

namespace EasyBackup.Models
{
    public enum IterationType
    {
        Daily,
        Weekly,
        Monthly
    }

    [System.Serializable]
    public class BackupCase
    {
        public BackupCase()
        {
            BackupId = Guid.NewGuid();
        }

        public Guid BackupId { get; set; }
        public string BackupTitle { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public TimeSpan BackupTime { get; set; } = DateTime.Now.TimeOfDay;
        public IterationType Iteration { get; set; }
        public DateTime LastBackupDateTime { get; set; }
    }
}