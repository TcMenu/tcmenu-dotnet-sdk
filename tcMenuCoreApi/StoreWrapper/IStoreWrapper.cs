using System;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Util;

namespace TcMenu.CoreSdk.StoreWrapper
{
    public interface IStoreWrapper
    {
        /// <summary>
        /// Loads all the elements back into memory after a restart.
        /// </summary>
        /// <returns>A task for tracking</returns>
        Task Load();

        /// <summary>
        /// Saves all the elements to disk ready for the next run.
        /// </summary>
        void Save();

        /// <summary>
        /// Check if the review request is appropriate, then actually make the appropriate system call
        /// to the review facilities if needed. 
        /// </summary>
        /// <returns>A task to track completion</returns>
        Task CheckAndPromptReviewIfNeeded();

        /// <summary>
        /// Get the time of the last attempted review
        /// </summary>
        DateTime TimeOfLastReviewRequest { get; }

        /// <summary>
        /// Checks if a review has already been requested for a given version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        bool HasRequestedReviewFor(LibraryVersion version);

        /// <summary>
        /// Check if the application has ever started before on the current version.
        /// </summary>
        bool HasStartedBeforeOnCurrentVersion { get; }
    }
}
