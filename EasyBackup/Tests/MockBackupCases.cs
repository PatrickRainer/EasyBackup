using System;
using EasyBackup.Models;

namespace EasyBackup.Tests
{
    public class MockBackupCases
    {
        public static readonly BackupCase B1 = new BackupCase
        {
            BackupTitle = "Backup 1",
            BackupTime = TimeSpan.FromHours(8)
        };

        public static readonly BackupCase B2 = new BackupCase
        {
            BackupTitle = "Backup 2",
            BackupTime = TimeSpan.FromHours(14)
        };

        public static BackupCase B3 = new BackupCase
        {
            BackupTitle = "Backup 3",
            BackupTime = TimeSpan.FromHours(20)
        };
    }
}