using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using EasyBackup.Annotations;
using EasyBackup.Converters;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;
using Timer = System.Windows.Forms.Timer;

//TODO: User Hangfire Framework for managing those tasks

namespace EasyBackup
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        readonly NotifyIcon _notifyIcon = new NotifyIcon();
        //private string mySourcePath = @"C:\Intel";
        //private string myDestinationPath = @"\\sulzer.com\dfs\ct\users\ch\rainpat\Documents\Backup";

        ObservableCollection<BackupCase> _caseList = new ObservableCollection<BackupCase>();

        public BackupCase SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                if (Equals(value, _selectedBackup)) return;
                _selectedBackup = value;
                OnPropertyChanged();
            }
        }

        bool _isBackupRunning;
        CollectionViewSource _itemCollectionViewSource;

        Timer _timer1;
        BackupCase _selectedBackup;

        public MainWindow()
        {
            InitializeComponent();
            CaseGrid.LoadingRow += CaseGridOnLoadingRow;
            CaseGrid.SelectionChanged += (sender, args) => { SelectedBackup = CaseGrid.SelectedItem as BackupCase;};

            // Load Backup List
            LoadBackupList();

            //TODO: Load Start with windows
            //StartWithWindows();

            // Bind Iteration Type Combobox
            cbIterationType.Items.Clear();
            var values = Enum.GetValues(typeof(IterationType));
            IterationTypeList = new List<IterationType>();
            foreach (IterationType value in values) IterationTypeList.Add(value);
            cbIterationType.ItemsSource = IterationTypeList;
            cbIterationType.SelectedIndex = 0;


            // Init Timer to Backup
            InitTimer();

            // DataGrid Binding
            DataGridBinding();
            CaseGrid.SelectedIndex = 0;

            //Version Label
            if (ApplicationDeployment.IsNetworkDeployed)
                lblVersion.Content = $"Version: {ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4)}";

            // Tray Icon
            _notifyIcon.Text = @"Easy Backup";
            //notifyIcon.Icon = this.Icon;
            _notifyIcon.Visible = true;
        }

        List<IterationType> IterationTypeList { get; }

        void CaseGridOnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var backupCase = e.Row.Item as BackupCase;
            if (!Directory.Exists(backupCase?.SourcePath)) e.Row.Background = new SolidColorBrush(Colors.IndianRed);
        }

        void StartWithWindows()
        {
            if ((bool) cboxStartWithWindows.IsChecked)
                AddApplicationToStartup();
            else if (!(bool) cboxStartWithWindows.IsChecked) RemoveApplicationFromStartup();
        }

        void DataGridBinding()
        {
            _itemCollectionViewSource = (CollectionViewSource) FindResource("ItemCollectionViewSource");
            _itemCollectionViewSource.Source = _caseList;
        }

        public static void AddApplicationToStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("Easy_Backup_Sync", "\"" + Application.ExecutablePath + "\"");
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("Easy_Backup_Sync", false);
            }
        }

        void SelectSourceFolderButton_Click(object sender, RoutedEventArgs e)
        {
            tbSourcePath.Text = string.IsNullOrEmpty(tbSourcePath.Text) ? GetPath() : GetPath(tbSourcePath.Text);
        }

        void SelectDestinationFolderButton_Click(object sender, RoutedEventArgs e)
        {
            tbDestinationPath.Text = string.IsNullOrEmpty(tbDestinationPath.Text) ? GetPath() : GetPath(tbDestinationPath.Text);
        }

        /// <summary>
        ///     Open a Folder Browser Dialog and return the path
        /// </summary>
        /// <returns></returns>
        string GetPath(string existingPath = null)
        {
            //TODO: Add last folder of source path to path and create this folder if necessary
            //TODO: If already a path, then open this path
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            if (existingPath!=null)
            {
                fbd.SelectedPath = existingPath;
            }

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = fbd.SelectedPath;
                return path;
            }

            return null;
        }

        void BtnBackupNow_Click(object sender, RoutedEventArgs e)
        {
            // Sanity Check
            if (IsBackupCaseSelected() == false) return;

            // Copy
            CopyFolderContent((BackupCase) CaseGrid.SelectedItem);
        }

        void CopyFolderContent(BackupCase _backupCase)
        {
            if (_isBackupRunning)
            {
                StatusText.Text = "A Backup is already Running";
                return;
            }

            StatusText.Text = $"{SelectedBackup.BackupTitle} is running ...";
            StatusText.UpdateLayout();

            _isBackupRunning = true;

            //Check if the source Directory exists
            if (!Directory.Exists(_backupCase.SourcePath))
            {
                //MessageBox.Show(_backupCase.sourcePath + " does not exist!");
                StatusText.Text = _backupCase.SourcePath + " does not exist!";
                return;
            }

            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(_backupCase.SourcePath, "*",
                SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory(dirPath.Replace(_backupCase.SourcePath, _backupCase.DestinationPath));
                    ProgressBar.Value += 1;
                }
                catch (UnauthorizedAccessException e)
                {
                    StatusText.Text = e.ToString();
                    return;
                    //throw;
                }
            }


            //Copy all the files & Replaces any files with the same BackupTitle
            foreach (var newPath in Directory.GetFiles(_backupCase.SourcePath, "*.*",
                SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(_backupCase.SourcePath, _backupCase.DestinationPath), true);
                ProgressBar.Value += 1;
            }

            _backupCase.LastBackupDateTime = DateTime.Now;

            StatusText.Text = "Backup finished!";

            _isBackupRunning = false;
        }

        void CboxStartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            AddApplicationToStartup();
        }

        void CboxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveApplicationFromStartup();
        }


        public void InitTimer()
        {
            _timer1 = new Timer();
            _timer1.Tick += timer1_Tick;
            _timer1.Interval = 5000; // in miliseconds
            _timer1.Start();
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            isBackupTimeReached();

            BackupDailyAt12();
        }

        void BackupDailyAt12()
        {
            // Get today 12 o'clock
            DateTime.TryParse(DateTime.Now.Date.ToShortDateString() + " 12:00 PM", out var _TodayAt12);

            // Thread List
            var threads = new List<Thread>();

            foreach (var bc in _caseList)
                if (DateTime.Now >= _TodayAt12 && bc.LastBackupDateTime.Date != DateTime.Now.Date)
                    CopyFolderContent(bc);
            //threads.Add(new Thread(() => CopyFolderContent(bc)));
            //threads.Last().Start();
        }

        void isBackupTimeReached()
        {
            //TODO: isBackupTimeReached
        }

        void btnAddClick(object sender, RoutedEventArgs e)
        {
            AddBackupCase();
            CaseGrid.SelectedIndex = CaseGrid.Items.Count - 1;
        }

        void AddBackupCase()
        {
            //var bc = new BackupCase();

            // Parse IterationType
            //Enum.TryParse(cbIterationType.Text, out IterationType iterationType);
            //TimeSpan.TryParse(tbTime.Text, out var time);

            _caseList.Add(new BackupCase
            {
                BackupTitle = "New Backup",
                BackupTime = DateTime.Now.TimeOfDay
            });
        }

        void DeleteBackupCase()
        {
            if (IsBackupCaseSelected() == false) return;

            _caseList.RemoveAt(CaseGrid.SelectedIndex);
        }

        bool IsBackupCaseSelected()
        {
            if (_caseList.Count - 1 < CaseGrid.SelectedIndex || CaseGrid.SelectedIndex == -1)
                return false;
            return true;
        }

        void SaveBackupList()
        {
            try
            {
                using (Stream stream = File.Open("BackupList.bin", FileMode.Create))
                {
                    var bin = new BinaryFormatter();
                    bin.Serialize(stream, _caseList);
                }
            }
            catch (IOException e)
            {
                StatusText.Text = e.Message;
            }
        }

        void LoadBackupList()
        {
            try
            {
                using (Stream stream = File.Open("BackupList.bin", FileMode.Open))
                {
                    var bin = new BinaryFormatter();

                    _caseList = (ObservableCollection<BackupCase>) bin.Deserialize(stream);
                }
            }
            catch (IOException e)
            {
                StatusText.Text = e.Message;
            }
        }

        void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            BackupDailyAt12();
        }

        void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteBackupCase();
        }

        void FrmMainWindow_Closing_1(object sender, CancelEventArgs e)
        {
            SaveBackupList();
            StartWithWindows();
        }

        // Run in Icon Tray
        // https://stackoverflow.com/questions/11027051/develop-a-program-that-runs-in-the-background-in-net
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}