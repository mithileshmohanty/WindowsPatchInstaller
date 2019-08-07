using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsPatchInstallerUI;

namespace WindowsPatchInstallerUIUnitTest
{
    [TestClass]
    public class UnitTestViewModel
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        /// <summary>
        /// Write the Unit Test method for the Get Current OS Info 
        /// Delete this after reading... The target method is private method read about how can you test iut. There is a way. Finally check if you cover all the possible scenarios.
        /// This is a mock class do this first later you can extend this for all the other methods.
        /// </summary>
        [TestMethod]
        public void TestMethodGetMacAddress()
        {
            PrivateObject target = new PrivateObject(typeof(MainWindow));
            string text = "D8F2CA8C502D";
            var retVal = target.Invoke("GetMacAddress");
            Assert.AreEqual(text, retVal);

        }
    }
}
