using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyBackup.Services;
using NUnit.Framework;

namespace EasyBackup.Tests
{
    [TestFixture]
    public class ApplicationAutoStartTests
    {
        [Test]
        public void SaveAutoStartToRegistry()
        {
            Assert.Fail();
        }

        [Test]
        public void LoadAutoStartFromRegistry()
        {
            var result = RegistryHelper.IsApplicationRegisteredForStartUp();
            Assert.Fail();
        }
    }
}
