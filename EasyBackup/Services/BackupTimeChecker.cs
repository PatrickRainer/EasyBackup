using System;
using System.Collections.Generic;
using System.Threading;
using EasyBackup.Models;

namespace EasyBackup.Services
{
    public class BackupTimeChecker
    {
        public BackupTimeChecker(List<BackupCase> backupCases, BackupService backupService)
        {
            BackupService = backupService;
            BackupCases = backupCases;

            var timer = new Timer(state => CheckIsBackupDue());
            timer.Change(0, 60000);
        }

        public List<BackupCase> BackupsAreDue { get; private set; }
        public BackupService BackupService { get; }
        public List<BackupCase> BackupCases { get; }

        void CheckIsBackupDue()
        {
            BackupsAreDue = new List<BackupCase>();

            foreach (var backupCase in BackupCases)
            {
                var backupIsDue = DateTime.Now.TimeOfDay > backupCase.BackupTime;
                var isBackupAlreadyDoneToday = backupCase.LastBackupDateTime.Date.Equals(DateTime.Today);

                if (backupIsDue && !isBackupAlreadyDoneToday)
                {
                    BackupsAreDue.Add(backupCase);
                    BackupService.AddBackup(backupCase);
                }
            }
        }
    }
}