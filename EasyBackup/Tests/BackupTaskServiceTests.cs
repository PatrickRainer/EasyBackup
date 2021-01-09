using EasyBackup.Services;
using NUnit.Framework;

namespace EasyBackup.Tests
{
    [TestFixture]
    public class BackupTaskServiceTests
    {
        [SetUp]
        public void Setup()
        {
            _backupService = new BackupService();
        }

        BackupService _backupService;
        readonly MockBackupCases _mockBackupCases = new MockBackupCases();

        [Test]
        public void AddBackupTest()
        {
            _backupService.AddBackup(MockBackupCases.B1);
            //Console.WriteLine(backupService.Status);
            _backupService.AddBackup(MockBackupCases.B2);
            //Console.WriteLine(backupService.Status);
            _backupService.AddBackup(MockBackupCases.B2);
            //Console.WriteLine(backupService.Status);
        }

        [Test]
        public void AddSameBackupTwiceTest()
        {
            _backupService.AddBackup(MockBackupCases.B1);
            //Console.WriteLine(backupService.Status);
            _backupService.AddBackup(MockBackupCases.B2);
            //Console.WriteLine(backupService.Status);
            _backupService.AddBackup(MockBackupCases.B2);
            //Console.WriteLine(backupService.Status);


            Assert.LessOrEqual(_backupService.QueuedBackups.Count, 2);
        }
    }
}