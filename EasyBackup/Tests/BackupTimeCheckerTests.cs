using System.Collections.Generic;
using System.Threading;
using EasyBackup.Models;
using EasyBackup.Services;
using NUnit.Framework;

namespace EasyBackup.Tests
{
    [TestFixture]
    public class BackupTimeCheckerTests
    {
        [SetUp]
        public void SetUp()
        {
            _cases = new List<BackupCase> {MockBackupCases.B1, MockBackupCases.B2, MockBackupCases.B3};
        }

        readonly BackupService _backupService = new BackupService();
        BackupTimeChecker _backupTimeChecker;
        List<BackupCase> _cases;

        [Test]
        public void CheckBackupTimes()
        {
            _backupTimeChecker = new BackupTimeChecker(_cases, _backupService);

            Thread.Sleep(100); //The TimeChecker has a timer which need some time,
            //otherwise the test will fail
            Assert.IsNotEmpty(_backupTimeChecker.BackupsAreDue);
        }
    }
}