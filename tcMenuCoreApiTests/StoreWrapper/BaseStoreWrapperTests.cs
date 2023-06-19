using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.StoreWrapper;
using TcMenu.CoreSdk.Util;

namespace TcMenu.CoreSdkTests.StoreWrapper
{
    [TestClass]
    public class BaseStoreWrapperTests
    {
        [TestMethod]
        public async Task CheckWhenStoreReviewIsDue()
        {
            var homeDir = PrepareHomeDirectory(3, DateTime.Now.Subtract(TimeSpan.FromDays(50)), new LibraryVersion("1.9.4"));
            var wrapper = new UnitTestStoreWrapper(homeDir, new LibraryVersion(2,0,1), DeploymentType.AppStore);
            await wrapper.Load();
            wrapper.PrepareForUse(true);

            Assert.IsTrue(wrapper.HasStartedBeforeOnCurrentVersion);
            Assert.IsFalse(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.1")));
            await wrapper.CheckAndPromptReviewIfNeeded();

            Assert.IsTrue(wrapper.WasPromptCalled);
            Assert.IsTrue(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.1")));

            CheckSave(wrapper, homeDir, new LibraryVersion("2.0.1"), true);
        }

        [TestMethod]
        public async Task CheckNotDueWhenVersionChanges()
        {
            var homeDir = PrepareHomeDirectory(3, DateTime.Now.Subtract(TimeSpan.FromDays(50)), new LibraryVersion("1.9.4"));
            var wrapper = new UnitTestStoreWrapper(homeDir, new LibraryVersion(2, 0, 2), DeploymentType.AppStore);
            await wrapper.Load();
            wrapper.PrepareForUse(true);

            // here we have started up on a different version, last was 2.0.1, this run 2.0.2, so will not request review
            Assert.IsFalse(wrapper.HasStartedBeforeOnCurrentVersion);
            Assert.IsFalse(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.2")));
            await wrapper.CheckAndPromptReviewIfNeeded();

            // make sure the review was not requested.
            Assert.IsFalse(wrapper.WasPromptCalled);
            Assert.IsFalse(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.2")));

            CheckSave(wrapper, homeDir, new LibraryVersion("2.0.2"), false);
        }

        private void CheckSave(UnitTestStoreWrapper wrapper, UnitTestDirectory dir, LibraryVersion versionStarted, bool wasReviewed)
        {
            wrapper.Save();

            var xml = XDocument.Parse(dir.FileToContent["StoreData.xml"]);
            
            Assert.IsNotNull(xml.Root);

            Assert.AreEqual(wrapper.NumberOfTimesRun + 1, (int)xml.Root.Element("StartCountForVer"));
            Assert.AreEqual(wrapper.LastReviewed.ToString(), (string)xml.Root.Element("LastVersionReviewed"));
            Assert.AreEqual(versionStarted.ToString(), (string)xml.Root.Element("LastVersionStarted"));
            
            if (!wasReviewed) return;
            var dt = (DateTime) xml.Root.Element("LastReviewDate");
            Assert.IsTrue(dt.Date.Equals(DateTime.Now.Date));
        }

        [TestMethod]
        public async Task CheckThatReviewNeverRequestedNonStore()
        {
            var homeDir = PrepareHomeDirectory(3, DateTime.Now.Subtract(TimeSpan.FromDays(50)), new LibraryVersion("1.9.4"));
            var wrapper = new UnitTestStoreWrapper(homeDir, new LibraryVersion(2, 0, 1), DeploymentType.Deployed);
            await wrapper.Load();
            wrapper.PrepareForUse(true);

            Assert.IsTrue(wrapper.HasStartedBeforeOnCurrentVersion);
            Assert.IsTrue(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.1"))); // non store version always returns
            Assert.IsTrue(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.2"))); // true to prevent a popup from ever
            Assert.IsTrue(wrapper.HasRequestedReviewFor(new LibraryVersion("1.0.3"))); // even being requested.
            await wrapper.CheckAndPromptReviewIfNeeded();

            Assert.IsFalse(wrapper.WasPromptCalled); // should not have been called, even though conditions met.
            CheckSave(wrapper, homeDir, new LibraryVersion("2.0.1"), false);
        }

        [TestMethod]
        public async Task CheckWhenTooRecentReviewToRequestAgain()
        {
            var homeDir = PrepareHomeDirectory(3, DateTime.Now.Subtract(TimeSpan.FromDays(10)), new LibraryVersion("1.9.4"));
            var wrapper = new UnitTestStoreWrapper(homeDir, new LibraryVersion(2, 0, 1), DeploymentType.AppStore);
            await wrapper.Load();
            wrapper.PrepareForUse(true);

            // in this case we have not yet reviewed the version, started enough times, but the review was requested too recently
            Assert.IsTrue(wrapper.HasStartedBeforeOnCurrentVersion);
            Assert.IsFalse(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.1")));
            await wrapper.CheckAndPromptReviewIfNeeded();

            Assert.IsFalse(wrapper.WasPromptCalled);
            Assert.IsFalse(wrapper.HasRequestedReviewFor(new LibraryVersion("2.0.1")));
        }

        UnitTestDirectory PrepareHomeDirectory(int startsOnVer, DateTime lastReviewDate, LibraryVersion lastReviewVersion)
        {
            return new UnitTestDirectory("home", new List<IDirectory>(), new Dictionary<string, string>
            {
                ["StoreData.xml"] = new XDocument(
                    new XElement("StoreSettings",
                        new XElement("LastReviewDate", lastReviewDate),
                        new XElement("StartCountForVer", startsOnVer),
                        new XElement("LastVersionReviewed", lastReviewVersion),
                        new XElement("LastVersionStarted", "2.0.1")
                    )
                ).ToString()
            });
        }
    }

    class UnitTestStoreWrapper : BaseStoreWrapper
    {
        private bool _responseToServe;
        private CountdownEvent _countdown = new CountdownEvent(1);

        public bool WasPromptCalled { get; private set; }

        public int NumberOfTimesRun => _startCountOnThisVersion;

        public LibraryVersion LastReviewed => _lastVersionReviewed;

        public UnitTestStoreWrapper(IDirectory homeDirectory, LibraryVersion currentVersion,
            DeploymentType deploymentType)
            : base(homeDirectory, currentVersion, deploymentType, 50)
        {
        }

        public override Task<bool> InternalPromptUserToRateApp()
        {
            WasPromptCalled = true;
            _countdown.Signal();
            return Task.FromResult(_responseToServe);
        }

        public void PrepareForUse(bool response)
        {
            _responseToServe = response;
            _countdown.Reset();
            WasPromptCalled = false;
        }

        public bool WaitForSignal()
        {
            return _countdown.Wait(250);
        }
    }
}