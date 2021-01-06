using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Application = System.Windows.Forms.Application;
using Timer = System.Windows.Forms.Timer;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using MessageBox = System.Windows.MessageBox;

namespace EasyBackup
{
    public partial class MainWindow : Window
    {
        //private string mySourcePath = @"C:\Intel";
        //private string myDestinationPath = @"\\sulzer.com\dfs\ct\users\ch\rainpat\Documents\Backup";

        private ObservableCollection<BackupCase> CaseList = new ObservableCollection<BackupCase>();
        private CollectionViewSource itemCollectionViewSource;

        private List<IterationType> IterationTypeList { get; set; }
               
        private Timer timer1;

        private bool isBackupRunning = false;

        private NotifyIcon notifyIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // Load Backup List
            LoadBackupList();

            //TOOD: Load Start with windows
            //StartWithWindows();

            // Bind Iteration Type Combobox
            cbIterationType.Items.Clear();
            Array values = Enum.GetValues((typeof(IterationType)));
            IterationTypeList = new List<IterationType>();
            foreach (IterationType value in values)
            {
                IterationTypeList.Add(value);
            }
            cbIterationType.ItemsSource = IterationTypeList;
            cbIterationType.SelectedIndex = 0;

            // Default dtPicker Value
            dpDatePicker.SelectedDate = DateTime.Now.Date;

            // Init Timer to Backup
            InitTimer();

            // DataGrid Binding
            //CaseGrid.ItemsSource = CaseList;
            DataGridBinding();

            //Version Label
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                lblVersion.Content = string.Format("Version: {0}",
                    ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4));
            }

            // Tray Icon
            notifyIcon.Text = "Easy Backup";
            //notifyIcon.Icon = this.Icon;
            notifyIcon.Visible = true;

        }

        private void StartWithWindows()
        {
            if ((bool)cboxStartWithWindows.IsChecked)
            {
                AddApplicationToStartup();
            }
            else if (!(bool)cboxStartWithWindows.IsChecked)
            {
                RemoveApplicationFromStartup();
            }
        }

        private void DataGridBinding()
        {
            
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = CaseList;
        }

        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("Easy_Backup_Sync", "\"" + Application.ExecutablePath + "\"");
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("Easy_Backup_Sync", false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbSourcePath.Text = GetPath();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            tbDestinationPath.Text = GetPath();
        }

        /// <summary>
        /// Open a Folder Browser Dialog and return the path
        /// </summary>
        /// <returns></returns>
        private string GetPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            string path;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
                return path;
            }
            else
            {
                return null;
            }
        }

        private void BtnBackupNow_Click(object sender, RoutedEventArgs e)
        {
            // Sanity Check
            if (IsBackupCaseSelected()==false)
            {
                return;
            }

            // Copy
            CopyFolderContent((BackupCase)CaseGrid.SelectedItem);
        }

        private void CopyFolderContent(BackupCase _backupCase)
        {
            if (isBackupRunning)
            {
                StatusText.Text="A Backup is already Running";
                return;
            }

            StatusText.Text="running ...";
            StatusText.UpdateLayout();

            isBackupRunning = true;

            //Check if the source Directory exists
            if (!Directory.Exists(_backupCase.sourcePath))
            {
                MessageBox.Show(_backupCase.sourcePath + " does not exist!");
                return;
            }
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(_backupCase.sourcePath, "*",
                    SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(_backupCase.sourcePath, _backupCase.destinationPath));
                    ProgressBar.Value += 1;
                }
            

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(_backupCase.sourcePath, "*.*",
                SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(_backupCase.sourcePath, _backupCase.destinationPath), true);
                ProgressBar.Value += 1;
            }

            _backupCase.lastBackupTime = DateTime.Now;

            StatusText.Text = "Backup finished!";

            isBackupRunning = false;
        }

        private void CboxStartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            AddApplicationToStartup();
        }

        private void CboxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveApplicationFromStartup();
        }


        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 5000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            isBackupTimeReached();

            BackupDailyAt12();
        }

        private void BackupDailyAt12()
        {
            // Get today 12 o'clock
            DateTime.TryParse(DateTime.Now.Date.ToShortDateString() + " 12:00 PM", out DateTime _TodayAt12);

            // Thread List
            List<Thread> threads = new List<Thread>();

            foreach (BackupCase bc in CaseList)
            {
                if (DateTime.Now >= _TodayAt12 && bc.lastBackupTime.Date != DateTime.Now.Date)
                {
                    CopyFolderContent(bc);
                        //threads.Add(new Thread(() => CopyFolderContent(bc)));
                        //threads.Last().Start();
                }
            }
        }

        private void isBackupTimeReached()
        {
            //TODO:
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            AddBackupCase();
        }

        private void AddBackupCase()
        {
            BackupCase bc = new BackupCase();

            // Parse IterationType
            Enum.TryParse(cbIterationType.Text, out IterationType _iterationType);

            // Create DateTime
            DateTime.TryParse(dpDatePicker.Text + " " + tbTime.Text, out DateTime _DateTimeResult);

            CaseList.Add(new BackupCase()
            {
                name = tbBackupName.Text,
                sourcePath = tbSourcePath.Text,
                destinationPath = tbDestinationPath.Text,
                iteration = _iterationType,
                backupTime = _DateTimeResult
            });
        }

        private void DeleteBackupCase()
        {
            if (IsBackupCaseSelected()==false)
            {
                return;
            }

            CaseList.RemoveAt(CaseGrid.SelectedIndex);

        }

        private bool IsBackupCaseSelected()
        {
            if (CaseList.Count - 1 < CaseGrid.SelectedIndex || CaseGrid.SelectedIndex == -1)
            {
               return false;
            }
            else
            {
                return true;
            }
        }

        private void SaveBackupList()
        {
            try
            {
                using (Stream stream = File.Open("BackupList.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, CaseList);
                }
            }
            catch (IOException)
            {
            }
        }

        private void LoadBackupList()
        {
            try
            {
                using (Stream stream = File.Open("BackupList.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    CaseList = (ObservableCollection<BackupCase>)bin.Deserialize(stream);
                }
            }
            catch (IOException)
            {
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            BackupDailyAt12();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteBackupCase();
        }

        private void FrmMainWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveBackupList();
            StartWithWindows();
        }

        // Run in Icon Tray
        // https://stackoverflow.com/questions/11027051/develop-a-program-that-runs-in-the-background-in-net
    }
}
