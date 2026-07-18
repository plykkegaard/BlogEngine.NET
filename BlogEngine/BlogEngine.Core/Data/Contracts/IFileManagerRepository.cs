using BlogEngine.Core.FileSystem;
using System.Collections.Generic;

namespace BlogEngine.Core.Data.Contracts
{
    /// <summary>
    /// Interface for FileManagerRepository
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for a file manager repository, which is responsible for managing file instances.
    /// </remarks>
    public interface IFileManagerRepository
    {
        /// <summary>
        /// Finds file instances based on the specified parameters.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="path"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <remarks>
        /// The method returns an enumerable collection of file instances that match the specified criteria.
        /// </remarks>
        IEnumerable<FileInstance> Find(int take = 10, int skip = 0, string path = "", string order = "");
    }
}
