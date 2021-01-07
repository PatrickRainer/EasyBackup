using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
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

        public async Task AddBackup(BackupCase backupCase)
        {
            _backupQueue.Enqueue(backupCase);
          await RunBackupInQueue();
        }

        private async Task RunBackupInQueue()
        {
            IsQueueEmpty = _backupQueue.Count <= 0;

            if (!IsQueueEmpty)
            {
              var result = await BackupAsync(_backupQueue.Dequeue());
            }
        }
        
        async Task<bool> BackupAsync(BackupCase backupCase)
        {
            var t = await Task.Run(() =>

                {
                    Status = $"Backing up {backupCase.BackupTitle}...";
                    Console.WriteLine(Status);
                    Thread.Sleep(5000);
            
                    Status = $"BackupAsync {backupCase.BackupTitle} finished.";
                    Console.WriteLine(Status);
                    return true;
                }
            );

            return t;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}