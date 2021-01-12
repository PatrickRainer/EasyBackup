using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using EasyBackup.Helpers;
using EasyBackup.Models;
using EasyBackup.Properties;
using EasyBackup.Services;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

//TODO: Check using HangFire Framework for managing those tasks
//TODO: User SharpZipLibrary to backup as zip
//BUG: During a longer Backup the application is freezes
//BUG: Icon in TaskBar/TaskManager is missing
//BUG: StartWithWindowsCheckbox is not saved, needs to be checked on the program start
//BUG: Last Backup Time was not written on manual backup

namespace EasyBackup
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        readonly BackupService _backupService = new BackupService();

        ObservableCollection<BackupCase> _caseList = new ObservableCollection<BackupCase>();

        CollectionViewSource _itemCollectionViewSource;
        BackupCase _selectedBackup;

        NotifyIcon _mNotifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            CaseGrid.LoadingRow += CaseGridOnLoadingRow;
            CaseGrid.SelectionChanged += (sender, args) => { SelectedBackup = CaseGrid.SelectedItem as BackupCase; };

            // Load Backup List
            LoadBackupList();

           
            //StartWithWindows();

            InitNotifyIcon();
            
            // Bind Iteration Type Combobox
            cbIterationType.Items.Clear();
            var values = Enum.GetValues(typeof(IterationType));
            IterationTypeList = new List<IterationType>();
            foreach (IterationType value in values) IterationTypeList.Add(value);
            cbIterationType.ItemsSource = IterationTypeList;
            cbIterationType.SelectedIndex = 0;


            // Init Timer to Backup

            // DataGrid Binding
            DataGridBinding();
            CaseGrid.SelectedIndex = 0;

            //Version Label
            if (ApplicationDeployment.IsNetworkDeployed)
                lblVersion.Content = $"Version: {ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4)}";

            // Tray Icon
            // _notifyIcon.Text = @"Easy Backup";
            //notifyIcon.Icon = this.Icon;
            // _notifyIcon.Visible = true;


            TimeChecker = new BackupTimeChecker(_caseList.ToList(), _backupService);
        }

        public void InitNotifyIcon()
        {
           // initialise code here
           _mNotifyIcon = new System.Windows.Forms.NotifyIcon
           {
               BalloonTipText = @"The app has been minimized. Click the tray icon to show.",
               BalloonTipTitle = @"Easy Backup",
               Text = @"Easy Backup",
               Icon = new Icon(Application
                   .GetResourceStream(new Uri("pack://application:,,,/Resources/Images/Icon_Sync01.ico"))
                   ?.Stream ?? throw new InvalidOperationException())
           };

            ShowNotifyIcon();
           _mNotifyIcon.Click += _notifyIcon_Click;
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            _mNotifyIcon.Dispose();
        }

        public BackupTimeChecker TimeChecker { get; }

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

        List<IterationType> IterationTypeList { get; }

        // Run in Icon Tray
        // https://stackoverflow.com/questions/11027051/develop-a-program-that-runs-in-the-background-in-net
        public event PropertyChangedEventHandler PropertyChanged;

        void CaseGridOnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var backupCase = e.Row.Item as BackupCase;
            if (!Directory.Exists(backupCase?.SourcePath)) e.Row.Background = new SolidColorBrush(Colors.IndianRed);
        }

        void StartWithWindows()
        {
            var isChecked = cboxStartWithWindows.IsChecked != null && (bool) cboxStartWithWindows.IsChecked;

            if (isChecked)
                AddApplicationToStartup();
            else RemoveApplicationFromStartup();
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
                key?.SetValue("Easy_Backup_Sync", "\"" + System.Windows.Forms.Application.ExecutablePath + "\"");
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key?.DeleteValue("Easy_Backup_Sync", false);
            }
        }

        void SelectSourceFolderButton_Click(object sender, RoutedEventArgs e)
        {
            tbSourcePath.Text = string.IsNullOrEmpty(tbSourcePath.Text) ? GetPath() : GetPath(tbSourcePath.Text);
        }

        void SelectDestinationFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var lastSourceFolderName = StringHelpers.ExtractLastFolder(SelectedBackup.SourcePath);

            tbDestinationPath.Text = (string.IsNullOrEmpty(tbDestinationPath.Text)
                                         ? GetPath()
                                         : GetPath(tbDestinationPath.Text))
                                     + lastSourceFolderName;
        }

        /// <summary>
        ///     Open a Folder Browser Dialog and return the path
        /// </summary>
        /// <returns></returns>
        string GetPath(string existingPath = null)
        {
            //TODO: If already a path, then open this path
            var fbd = new FolderBrowserDialog {ShowNewFolderButton = true};
            if (existingPath != null)
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
            CopyService.CopyFolderContent((BackupCase) CaseGrid.SelectedItem, out _);
        }

        void CBoxStartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            AddApplicationToStartup();
        }

        void CBoxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveApplicationFromStartup();
        }

        void BtnAddClick(object sender, RoutedEventArgs e)
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
            catch (SerializationException e)
            {
                MessageBox.Show(e.Message);
            }
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveBackupList();
        }

        private WindowState _mStoredWindowState = WindowState.Normal;
        void OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _mNotifyIcon?.ShowBalloonTip(2000);
            }
            else
                _mStoredWindowState = WindowState;
        }
        
        void _notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = _mStoredWindowState;
        }

        void ShowNotifyIcon()
        {
            if (_mNotifyIcon != null) 
                _mNotifyIcon.Visible = true;
        }
    }
}