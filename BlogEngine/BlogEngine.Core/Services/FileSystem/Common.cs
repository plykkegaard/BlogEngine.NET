using System.Collections.Generic;

namespace BlogEngine.Core.FileSystem
{
    /// <summary>
    /// Represents a response containing file system data for a specific directory.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate the results of file system operations, including
    /// a collection of files/directories and their parent path. It is typically used when
    /// fetching directory contents or performing file browser operations.
    /// </remarks>
    public class FileResponse
    {
        /// <summary>
        /// Initializes a new instance of the FileResponse class.
        /// </summary>
        /// <remarks>
        /// Creates an empty FileResponse with an empty file collection and path.
        /// </remarks>
        public FileResponse()
        {
            Files = new List<FileInstance>();
            Path = string.Empty;
        }

        /// <summary>
        /// Gets or sets the collection of files and directories in the response.
        /// </summary>
        /// <value>
        /// An IEnumerable collection of FileInstance objects representing files or directories.
        /// </value>
        public IEnumerable<FileInstance> Files { get; set; }

        /// <summary>
        /// Gets or sets the directory path associated with the file collection.
        /// </summary>
        /// <value>
        /// A string representing the full or relative path of the directory containing the files.
        /// </value>
        public string Path { get; set; }
    }

    /// <summary>
    /// Represents metadata and properties of a single file or directory in the file system.
    /// </summary>
    /// <remarks>
    /// This class encapsulates information about a file or directory including its name, size,
    /// creation date, type, and associated UI properties. It is used primarily in file browser
    /// operations and file listing scenarios. The ImgPlaceholder property automatically generates
    /// Font Awesome icon class names based on file extensions.
    /// </remarks>
    public class FileInstance
    {
        /// <summary>
        /// Gets or sets a value indicating whether this file instance is selected or checked.
        /// </summary>
        /// <value>
        /// true if the file instance is selected; otherwise, false.
        /// </value>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the sort order for this file instance in a collection.
        /// </summary>
        /// <value>
        /// An integer representing the sort order, typically used for displaying files in a specific sequence.
        /// </value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the file or directory.
        /// </summary>
        /// <value>
        /// A string representation of the creation date and time.
        /// </value>
        public string Created { get; set; }

        /// <summary>
        /// Gets or sets the name of the file or directory.
        /// </summary>
        /// <value>
        /// A string representing the name (without full path) of the file or directory.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in a human-readable format.
        /// </summary>
        /// <value>
        /// A string representing the file size (e.g., "1.5 MB", "256 KB"). Empty for directories.
        /// </value>
        public string FileSize { get; set; }

        /// <summary>
        /// Gets or sets the type of this file system item.
        /// </summary>
        /// <value>
        /// A FileType enumeration value indicating whether this is a Directory, File, Image, or None.
        /// </value>
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the full path to the file or directory.
        /// </summary>
        /// <value>
        /// A string representing the complete path to the file or directory.
        /// </value>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets the Font Awesome icon class associated with the file type or extension.
        /// </summary>
        /// <value>
        /// A string containing a Font Awesome icon class name (e.g., "fa fa-file-pdf-o" for PDF files).
        /// Returns an empty string for directories.
        /// </value>
        /// <remarks>
        /// This property returns an appropriate Font Awesome icon class based on the file extension.
        /// Supported extensions include archive files (.zip, .gzip, .7zip, .rar), documents (.doc, .docx),
        /// spreadsheets (.xls, .xlsx), PDFs (.pdf), and text files (.txt). For other file types,
        /// a generic file icon is returned.
        /// </remarks>
        public string ImgPlaceholder
        {
            get
            {
                if(FileType == FileType.File)
                {
                    return getPlaceholder(Name);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the Font Awesome icon class for a file based on its name and extension.
        /// </summary>
        /// <param name="name">The filename or full path to evaluate.</param>
        /// <returns>
        /// A Font Awesome icon class string appropriate for the file type.
        /// Common returns include "fa fa-file-archive-o" for archives, "fa fa-file-word-o" for Word documents,
        /// "fa fa-file-excel-o" for Excel spreadsheets, "fa fa-file-pdf-o" for PDFs, "fa fa-file-text-o" for text files,
        /// and "fa fa-file-o" as the default generic file icon.
        /// </returns>
        /// <remarks>
        /// This static method examines the file extension (case-insensitive) and returns the appropriate
        /// Font Awesome icon class. The method performs a case-insensitive comparison by converting the
        /// filename to lowercase before checking extensions.
        /// </remarks>
        static string getPlaceholder(string name)
        {
            var file = name.ToLower().Trim();

            if (file.EndsWith(".zip") || file.EndsWith(".gzip") || file.EndsWith(".7zip") || file.EndsWith(".rar"))
            {
                return "fa fa-file-archive-o";
            }
            if (file.EndsWith(".doc") || file.EndsWith(".docx"))
            {
                return "fa fa-file-word-o";
            }
            if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
            {
                return "fa fa-file-excel-o";
            }
            if (file.EndsWith(".pdf"))
            {
                return "fa fa-file-pdf-o";
            }
            if (file.EndsWith(".txt"))
            {
                return "fa fa-file-text-o";
            }

            return "fa fa-file-o";
        }
    }

    /// <summary>
    /// Specifies the type of a file system item.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to categorize file system items as directories, files, images,
    /// or unknown types. It allows the application to handle different item types appropriately
    /// in UI rendering and file operations.
    /// </remarks>
    public enum FileType
    {
        /// <summary>
        /// Represents a directory or folder.
        /// </summary>
        Directory,

        /// <summary>
        /// Represents a regular file (non-image).
        /// </summary>
        File,

        /// <summary>
        /// Represents an image file.
        /// </summary>
        Image,

        /// <summary>
        /// Represents an unknown or undefined file system item type.
        /// </summary>
        None
    }
}
