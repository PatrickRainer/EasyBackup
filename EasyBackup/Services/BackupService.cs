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
        Queue<Task<bool>> _runningBackups = new Queue<Task<bool>>();
        
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

        public void AddBackup(BackupCase backupCase)
        {
            _runningBackups.Enqueue(BackupAsync(backupCase));

            RunBackups();
        }

       async void RunBackups()
        {
            if (_runningBackups.Count <=0) return;

            IsTaskRunning = true;
            var t = await _runningBackups.Dequeue();
            IsTaskRunning + false;
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