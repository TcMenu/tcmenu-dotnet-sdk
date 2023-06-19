using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Serilog;
using TcMenu.CoreSdk.Util;

namespace TcMenu.CoreSdk.StoreWrapper
{
    public abstract class BaseStoreWrapper : IStoreWrapper
    {
        protected ILogger logger = Log.ForContext<IStoreWrapper>();

        private readonly LibraryVersion _currentVersion;
        private readonly IDirectory _homeDirectory;
        private readonly int _delayTime;
        protected readonly DeploymentType _deploymentType;
        protected int _startCountOnThisVersion;
        protected LibraryVersion _lastVersionReviewed = LibraryVersion.ERROR_VERSION;

        public DateTime TimeOfLastReviewRequest { get; private set; }
        public bool HasStartedBeforeOnCurrentVersion { get; private set; }

        protected BaseStoreWrapper(IDirectory homeDirectory, LibraryVersion currentVersion, DeploymentType deploymentType, int delayTime = 5000)
        {
            _currentVersion = currentVersion;
            _homeDirectory = homeDirectory;
            _deploymentType = deploymentType;
            _delayTime = delayTime;
        }

        public async Task Load()
        {
            try
            {
                var data = await _homeDirectory.GetSourceFileWithName("StoreData.xml").ConfigureAwait(true);
                var doc = XDocument.Parse(data);
                if (doc.Root == null || doc.Root.Name != "StoreSettings") return ;
                
                var lastVer = (string)doc.Root.Element("LastVersionReviewed");
                _lastVersionReviewed = lastVer == "Unknown" ?  LibraryVersion.ERROR_VERSION : new LibraryVersion(lastVer);

                lastVer = (string)doc.Root.Element("LastVersionStarted");
                var lastVersionStarted = lastVer == "Unknown" ? LibraryVersion.ERROR_VERSION : new LibraryVersion(lastVer);

                _startCountOnThisVersion = lastVersionStarted.Equals(_currentVersion) ? (int)doc.Root.Element("StartCountForVer") : 1;

                TimeOfLastReviewRequest = (DateTime)doc.Root.Element("LastReviewDate");

                HasStartedBeforeOnCurrentVersion = lastVersionStarted.Equals(_currentVersion);
            }
            catch (Exception ex)
            {
                HasStartedBeforeOnCurrentVersion = false;
            }
        }

        public void Save()
        {
            var doc = new XDocument(
                new XElement("StoreSettings",
                    new XElement("LastReviewDate", TimeOfLastReviewRequest),
                    new XElement("StartCountForVer", _startCountOnThisVersion + 1),
                    new XElement("LastVersionReviewed", _lastVersionReviewed.ToString()),
                    new XElement("LastVersionStarted", _currentVersion.ToString())
                )
            );
            _homeDirectory.WriteImmediately("StoreData.xml", doc.ToString());
        }

        public abstract Task<bool> InternalPromptUserToRateApp();

        public virtual async Task CheckAndPromptReviewIfNeeded()
        {
            if (_deploymentType != DeploymentType.AppStore) return;
            var daysSinceLastRequest = (DateTime.Now - TimeOfLastReviewRequest).TotalDays;
            if (_startCountOnThisVersion > 2 && !HasRequestedReviewFor(_currentVersion) && daysSinceLastRequest > 30)
            {
                await Task.Delay(_delayTime).ConfigureAwait(true);
                var successful = await InternalPromptUserToRateApp().ConfigureAwait(true);
                if (successful)
                {
                    MarkAsAttemptedToReview();
                }
            }
        }

        protected void MarkAsAttemptedToReview()
        {
            TimeOfLastReviewRequest = DateTime.Now;
            _lastVersionReviewed = _currentVersion;
        }

        public bool HasRequestedReviewFor(LibraryVersion version)
        {
            if (_deploymentType != DeploymentType.AppStore) return true;
            return _lastVersionReviewed?.IsSameOrNewerThan(version) ?? false;
        }
    }
}