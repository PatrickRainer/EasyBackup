using System;
using System.Collections.Generic;
using System.Threading;
using EasyBackup.Models;

namespace EasyBackup.Services
{
    public class BackupTimeChecker
    {
        public List<BackupCase> BackupCases { get; }

        public BackupTimeChecker(List<BackupCase> backupCases)
        {
            BackupCases = backupCases;

            var timer = new Timer(state => CheckIsBackupDue());
            timer.Change(0, 60000);
        }

        void CheckIsBackupDue()
        {
            foreach (var backupCase in BackupCases)
            {
                //if IsBackupCase due
                //Add to Backup to backupQueue
            }
        }
    }
}