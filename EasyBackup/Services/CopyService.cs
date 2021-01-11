using System;
using System.IO;
using EasyBackup.Models;

namespace EasyBackup.Services
{
    public class CopyService
    {
        public static void CopyFolderContent(BackupCase backupCase, out string status)
        {
            /*if (_isBackupRunning)
            {
                StatusText.Text = "A Backup is already Running";
                return;
            }*/

            status = $"{backupCase.BackupTitle} is running ...";
            //StatusText.UpdateLayout();

            // _isBackupRunning = true;

            //Check if the source Directory exists
            if (!Directory.Exists(backupCase.SourcePath))
            {
                //MessageBox.Show(_backupCase.sourcePath + " does not exist!");
                status = backupCase.SourcePath + " does not exist!";
                return;
            }

            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(backupCase.SourcePath, "*",
                SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory(dirPath.Replace(backupCase.SourcePath, backupCase.DestinationPath));
                    //ProgressBar.Value += 1; //TODO: Track Progress
                }
                catch (UnauthorizedAccessException e)
                {
                    status = e.ToString();
                    return;
                    //throw;
                }
            }


            //Copy all the files & Replaces any files with the same BackupTitle
            foreach (var newPath in Directory.GetFiles(backupCase.SourcePath, "*.*",
                SearchOption.AllDirectories))
            {
                try
                {
                    File.Copy(newPath, newPath.Replace(backupCase.SourcePath, backupCase.DestinationPath), true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e); //TODO: Write to a log file
                    //throw;
                }

                //ProgressBar.Value += 1;
            }

            backupCase.LastBackupDateTime = DateTime.Now;

            status = $"Backup: {backupCase.BackupTitle} finished!";

            // _isBackupRunning = false;
        }
    }
}