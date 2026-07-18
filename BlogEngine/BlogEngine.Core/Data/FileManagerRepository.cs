using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.FileSystem;
using BlogEngine.Core.Providers;
using System;
using System.Collections.Generic;

namespace BlogEngine.Core.Data
{
    /// <summary>
    /// Repository for managing file instances in the file manager.
    /// </summary>
    /// <remarks>
    /// This repository provides methods to interact with the file system, allowing for the retrieval
    /// and management of file instances within the file manager. It ensures that only authorized users
    /// can access and manipulate the files.
    /// </remarks>
    public class FileManagerRepository : IFileManagerRepository
    {
        /// <summary>
        /// Finds file instances based on the specified parameters.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="path"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <remarks>
        /// The method returns an enumerable collection of file instances that match the specified criteria.
        /// </remarks>
        public IEnumerable<FileInstance> Find(int take = 10, int skip = 0, string path = "", string order = "")
        {
            if (!Security.IsAuthorizedTo(Rights.EditOwnPosts))
                throw new UnauthorizedAccessException();

            var list = new List<FileInstance>();
            var rwr = Utils.RelativeWebRoot;
            // var responsePath = "root";

            path = path.SanitizePath();

            if(string.IsNullOrEmpty(path))
                path = Blog.CurrentInstance.StorageLocation + Utils.FilesFolder;

            var directory = BlogService.GetDirectory(path);

            if (!directory.IsRoot)
            {
                list.Add(new FileInstance()
                {
                    FileSize = "",
                    FileType = FileType.Directory,
                    Created = DateTime.Now.ToString(),
                    FullPath = directory.Parent.FullPath,
                    Name = "..."
                });
                // responsePath = "root" + directory.FullPath;
            }

            foreach (var dir in directory.Directories)
                list.Add(new FileInstance()
                {
                    FileSize = "",
                    FileType = FileType.Directory,
                    Created = dir.DateCreated.ToString(),
                    FullPath = dir.FullPath,
                    Name = dir.Name.Replace("/", "")
                });


            foreach (var file in directory.Files)
                list.Add(new FileInstance()
                {
                    FileSize = file.FileSizeFormat,
                    Created = file.DateCreated.ToString(),
                    FileType = file.IsImage ? FileType.Image : FileType.File,
                    FullPath = file.FilePath,
                    Name = file.Name
                });

            for (int i = 0; i < list.Count; i++)
            {
                list[i].SortOrder = i;
            }

            return list;
        }
    }
}
