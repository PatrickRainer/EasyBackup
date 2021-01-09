using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EasyBackup.Models;
using EasyBackup.Properties;

namespace EasyBackup.Services
{
    public class BackupService : INotifyPropertyChanged
    {
        const int TimerStartDelay = 1000;
        const int TimerInterval = 5000;
        Task _runningTask;
        string _status;

        public BackupService()
        {
            //Register Timer Callback and Start Timer
            var timer = new Timer(TimerCallback);
            timer.Change(TimerStartDelay, TimerInterval);
        }

        public ObservableCollection<BackupCase> QueuedBackups { get; } = new ObservableCollection<BackupCase>();

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

        public event PropertyChangedEventHandler PropertyChanged;

        async void TimerCallback(object state)
        {
            if (QueuedBackups.Count <= 0) return;

            if (_runningTask == null || _runningTask.IsCompleted)
            {
                _runningTask = Task.Run(() => BackupAsync(QueuedBackups[0]));
                await _runningTask;
                QueuedBackups.RemoveAt(0);
            }
        }

        public void AddBackup(BackupCase backupCase)
        {
            if (QueuedBackups.Contains(backupCase)) return;

            QueuedBackups.Add(backupCase);
        }

        async Task BackupAsync(BackupCase backupCase)
        {
            await Task.Run(() =>

                {
                    Status = $"Backing up {backupCase.BackupTitle}...";
                    Console.WriteLine(Status);

                    Task.Delay(TimeSpan.FromSeconds(5)).Wait(); //TODO: Make a real Backup here

                    Status = $"BackupAsync {backupCase.BackupTitle} finished.";
                    Console.WriteLine(Status);
                    return true;
                }
            );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}