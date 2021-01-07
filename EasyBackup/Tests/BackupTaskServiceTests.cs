﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyBackup.Services;
using NUnit.Framework;

namespace EasyBackup.Tests
{
    [TestFixture]
    public class BackupTaskServiceTests
    {
        BackupService backupService;
        BackupCase b1;
        BackupCase b2;
        BackupCase b3;

        [SetUp]
        public void Setup()
        {
            backupService = new BackupService();

            b1 = new BackupCase()
            {
                BackupTitle = "Backup 1"
            };
            b2 = new BackupCase()
            {
                BackupTitle = "Backup 2"
            };
            b3 = new BackupCase()
            {
                BackupTitle = "Backup 3"
            };

        }

        [Test]
        public void AddBackupTest()
        {
            backupService.AddBackup(b1);
            //Console.WriteLine(backupService.Status);
            backupService.AddBackup(b2);
            //Console.WriteLine(backupService.Status);
            backupService.AddBackup(b2);
            //Console.WriteLine(backupService.Status);
        }
    }
}