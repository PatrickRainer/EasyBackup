using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyBackup.Annotations;

namespace EasyBackup.Services
{
    public class BackupService : INotifyPropertyChanged
    {
        Queue<BackupCase> _backupQueue = new Queue<BackupCase>();
        bool IsQueueEmpty;
        string _status;

        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public BackupService()
        {
            
        }

        private void RunBackupInQueue()
        {
            IsQueueEmpty = _backupQueue.Count <= 0;

            if (!IsQueueEmpty)
            {
                Backup(_backupQueue.Dequeue());
            }
        }

        public void AddBackup(BackupCase backupCase)
        {
            _backupQueue.Enqueue(backupCase);
            RunBackupInQueue();
        }

        bool Backup(BackupCase backupCase)
        {
            var t = Task<bool>.Factory.StartNew(() =>

                {
                    Status = $"Backing up {backupCase.BackupTitle}...";
                    Thread.Sleep(2000);
                    Status = $"Backup {backupCase.BackupTitle} finished.";
                    return true;
                }
            );

            return t.Result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}