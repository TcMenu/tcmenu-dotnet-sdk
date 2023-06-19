using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdkTests.ProtocolTests
{
    [TestClass]
    public class CorrelationIdTests
    {
        [TestMethod]
        public void TestCorrelationId()
        {
            var mockedTime = new MockedSystemClock();
            mockedTime.SpecifiedTime = 123456;
            CorrelationId.ResetCounter();

            var correlation1 = new CorrelationId(mockedTime);
            var correlation2 = new CorrelationId(mockedTime);

            mockedTime.SpecifiedTime = 123460;
            var correlation3 = new CorrelationId(mockedTime);

            Assert.AreEqual("0001E241", correlation1.ToString());
            Assert.AreEqual("0001E242", correlation2.ToString());
            Assert.AreEqual("0001E247", correlation3.ToString());

            Assert.AreEqual(0x0001E241, correlation1.GetUnderlyingCorrelation());

            var correlation4 = new CorrelationId("0001E247");
            Assert.AreEqual((long)0x1E247, correlation4.GetUnderlyingCorrelation());
        }

        [TestMethod]
        public void TestSystemClock()
        {
            var realClock = new SystemClock();
            Assert.IsTrue(realClock.SystemMillis() > 1000000000000L);
        }
    }

    class MockedSystemClock : SystemClock
    {
        public long SpecifiedTime { get; set; }

        public override long SystemMillis()
        {
            return SpecifiedTime;
        }
    }
}
