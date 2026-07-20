using BlogEngine.Core.Packaging;

namespace BlogEngine.Core.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Web.Configuration;

    using BlogEngine.Core.DataStore;
    using BlogEngine.Core.FileSystem;   

    /// <summary>
    /// Proxy service for communication between business objects and data providers.
    /// </summary>
    /// <remarks>
    /// This static service acts as a facade to abstract the underlying data persistence layer from business logic.
    /// It provides methods for CRUD operations on blog entities (posts, pages, categories, etc.) and configuration data.
    /// The service maintains thread-safe access to configured providers and file system providers.
    /// All data operations are delegated to the currently configured provider implementation, allowing for
    /// flexible storage backend switching without impacting the business layer.
    /// </remarks>
    public static class BlogService
    {
        #region Constants and Fields

        /// <summary>
        /// Synchronization object for thread-safe provider access.
        /// </summary>
        /// <remarks>
        /// Ensures that multiple threads can safely initialize and access provider instances.
        /// </remarks>
        private static readonly object TheLock = new object();

        /// <summary>
        /// The data persistence provider instance.
        /// </summary>
        /// <remarks>
        /// Lazy-initialized on first access through the Provider property.
        /// Do not access directly; use the Provider property instead.
        /// </remarks>
        private static BlogProvider _provider;

        /// <summary>
        /// The file system storage provider instance.
        /// </summary>
        /// <remarks>
        /// Handles file-based storage operations for blog content and attachments.
        /// Do not access directly; use the FileSystemProvider property instead.
        /// </remarks>
        private static BlogFileSystemProvider _fileStorageProvider;

        /// <summary>
        /// Collection of configured data persistence providers.
        /// </summary>
        /// <remarks>
        /// Loaded from configuration and cached after first access.
        /// </remarks>
        private static BlogProviderCollection _providers;


        /// <summary>
        /// Collection of configured file system providers.
        /// </summary>
        /// <remarks>
        /// Loaded from configuration and cached after first access.
        /// </remarks>
        private static BlogFileSystemProviderCollection _fileProviders;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently active file system provider.
        /// </summary>
        /// <remarks>
        /// Returns the default file system provider configured in Web.config.
        /// Lazy-initializes the provider collection on first access.
        /// </remarks>
        /// <value>The active BlogFileSystemProvider implementation.</value>
        public static BlogFileSystemProvider FileSystemProvider
        {
            get
            {
                LoadProviders();
                return _fileStorageProvider;
            }
        }

        /// <summary>
        /// Gets the collection of all configured file system providers.
        /// </summary>
        /// <remarks>
        /// Provides access to all file system provider instances defined in Web.config.
        /// Allows switching between different file storage backends if multiple are configured.
        /// </remarks>
        /// <value>A BlogFileSystemProviderCollection containing all configured file system providers.</value>
        public static BlogFileSystemProviderCollection FileSystemProviders
        {
            get
            {
                LoadProviders();
                return _fileProviders;
            }
        }

        /// <summary>
        /// Gets the currently active data persistence provider.
        /// </summary>
        /// <remarks>
        /// Returns the default provider configured in Web.config for blog data operations.
        /// Lazy-initializes the provider on first access with thread-safe synchronization.
        /// </remarks>
        /// <value>The active BlogProvider implementation.</value>
        public static BlogProvider Provider
        {
            get
            {
                LoadProviders();
                return _provider;
            }
        }

        /// <summary>
        /// Reloads the file system provider from configuration.
        /// </summary>
        /// <remarks>
        /// Clears the cached file system provider and forces reload on next access.
        /// Useful after changing provider configuration dynamically.
        /// </remarks>
        internal static void ReloadFileSystemProvider()
        {
            _fileStorageProvider = null;
            LoadProviders();
        }

        /// <summary>
        /// Gets the collection of all configured data persistence providers.
        /// </summary>
        /// <remarks>
        /// Provides access to all provider instances defined in Web.config.
        /// Enables switching between different storage backends if multiple are configured.
        /// </remarks>
        /// <value>A BlogProviderCollection containing all configured providers.</value>
        public static BlogProviderCollection Providers
        {
            get
            {
                LoadProviders();
                return _providers;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes the specified BlogRoll item from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes the blog roll entry from persistent storage. This operation is permanent and cannot be undone.
        /// </remarks>
        /// <param name="blogRoll">The BlogRollItem to delete.</param>
        public static void DeleteBlogRoll(BlogRollItem blogRoll)
        {
            Provider.DeleteBlogRollItem(blogRoll);
        }

        /// <summary>
        /// Deletes the specified blog instance from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes all blog configuration and metadata. This is a destructive operation and cannot be reversed.
        /// Use DeleteBlogStorageContainer to also remove associated files and content.
        /// </remarks>
        /// <param name="blog">The blog instance to delete.</param>
        public static void DeleteBlog(Blog blog)
        {
            Provider.DeleteBlog(blog);
        }

        /// <summary>
        /// Deletes the storage container and all associated files for the specified blog.
        /// </summary>
        /// <remarks>
        /// Removes the blog's file storage and associated content. This is a complete removal operation.
        /// Often used in conjunction with DeleteBlog to fully remove a blog instance.
        /// </remarks>
        /// <param name="blog">The blog instance for which to delete the storage container.</param>
        /// <returns>True if the storage container was successfully deleted; otherwise, false.</returns>
        public static bool DeleteBlogStorageContainer(Blog blog)
        {
            return Provider.DeleteBlogStorageContainer(blog);
        }

        /// <summary>
        /// Deletes the specified category from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes a category definition from the blog. Posts assigned to this category will retain their category
        /// associations, so consider reassigning posts before deletion.
        /// </remarks>
        /// <param name="category">The category to delete.</param>
        public static void DeleteCategory(Category category)
        {
            Provider.DeleteCategory(category);
        }

        /// <summary>
        /// Deletes the specified static page from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes a page and all its associated metadata. This operation is permanent.
        /// </remarks>
        /// <param name="page">The page instance to delete.</param>
        public static void DeletePage(Page page)
        {
            Provider.DeletePage(page);
        }

        /// <summary>
        /// Deletes the specified blog post from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes a post and all its associated metadata including comments, categories, and tags.
        /// This operation is permanent and cannot be reversed.
        /// </remarks>
        /// <param name="post">The post instance to delete.</param>
        public static void DeletePost(Post post)
        {
            Provider.DeletePost(post);
        }

        /// <summary>
        /// Deletes the specified author profile from the current provider.
        /// </summary>
        /// <remarks>
        /// Removes an author's profile information. Posts authored by this user will retain authorship attribution.
        /// </remarks>
        /// <param name="profile">The author profile to delete.</param>
        public static void DeleteProfile(AuthorProfile profile)
        {
            Provider.DeleteProfile(profile);
        }

        /// <summary>
        /// Returns a collection of all blog roll items from the current provider.
        /// </summary>
        /// <remarks>
        /// Retrieves the configured list of external blogs and links to display in the blog roll widget.
        /// </remarks>
        /// <returns>A list of BlogRollItem objects representing external blog links.</returns>
        public static List<BlogRollItem> FillBlogRolls()
        {
            return Provider.FillBlogRoll();
        }

        /// <summary>
        /// Returns a collection of all categories for the specified blog.
        /// </summary>
        /// <remarks>
        /// Retrieves all categories used to organize and classify blog posts.
        /// </remarks>
        /// <param name="blog">The blog instance to retrieve categories for.</param>
        /// <returns>A list of Category objects.</returns>
        public static List<Category> FillCategories(Blog blog)
        {
            return Provider.FillCategories(blog);
        }

        /// <summary>
        /// Returns a collection of all static pages from the current provider.
        /// </summary>
        /// <remarks>
        /// Retrieves all published and unpublished pages. Pages are static content separate from blog posts.
        /// </remarks>
        /// <returns>A list of Page objects.</returns>
        public static List<Page> FillPages()
        {
            return Provider.FillPages();
        }

        /// <summary>
        /// Returns a collection of all blog posts from the current provider.
        /// </summary>
        /// <remarks>
        /// Retrieves all posts including published and draft items. No filtering by visibility is applied.
        /// </remarks>
        /// <returns>A list of Post objects.</returns>
        public static List<Post> FillPosts()
        {
            return Provider.FillPosts();
        }

        /// <summary>
        /// Returns a collection of all blog instances from the current provider.
        /// </summary>
        /// <remarks>
        /// In multi-blog scenarios, retrieves all configured blog instances.
        /// </remarks>
        /// <returns>A list of Blog objects.</returns>
        public static List<Blog> FillBlogs()
        {
            return Provider.FillBlogs();
        }

        /// <summary>
        /// Returns a collection of all author profiles from the current provider.
        /// </summary>
        /// <remarks>
        /// Retrieves information about all authors who have contributed to the blog.
        /// </remarks>
        /// <returns>A list of AuthorProfile objects.</returns>
        public static List<AuthorProfile> FillProfiles()
        {
            return Provider.FillProfiles();
        }

        /// <summary>
        /// Returns a list of all Referrers in the current provider.
        /// </summary>
        /// <returns>
        /// A list of Referrer.
        /// </returns>
        public static List<Referrer> FillReferrers()
        {
            return Provider.FillReferrers();
        }

        /// <summary>
        /// Returns a dictionary representing rights and the roles that allow them.
        /// </summary>
        /// <returns>
        /// 
        /// The key must be a string of the name of the Rights enum of the represented Right.
        /// The value must be an IEnumerable of strings that includes only the role names of
        /// roles the right represents.
        /// 
        /// Inheritors do not need to worry about verifying that the keys and values are valid.
        /// This is handled in the Right class.
        /// 
        /// </returns>
        public static IDictionary<string, IEnumerable<string>> FillRights()
        {
            return Provider.FillRights();
        }

        /// <summary>
        /// Persists a new blog role item to the current provider.
        /// </summary>
        /// <remarks>
        /// Saves a new blog roll entry (link to external blog) to the data store.
        /// </remarks>
        /// <param name="blogRoll">The blog roll item to insert.</param>
        public static void InsertBlogRoll(BlogRollItem blogRoll)
        {
            Provider.InsertBlogRollItem(blogRoll);
        }

        /// <summary>
        /// Persists a new blog instance to the current provider.
        /// </summary>
        /// <remarks>
        /// Creates a new blog instance in the data store. Use only for multi-blog scenarios.
        /// </remarks>
        /// <param name="blog">The blog instance to insert.</param>
        public static void InsertBlog(Blog blog)
        {
            Provider.InsertBlog(blog);
        }

        /// <summary>
        /// Persists a new category to the current provider.
        /// </summary>
        /// <remarks>
        /// Creates a new category for organizing and classifying blog posts.
        /// </remarks>
        /// <param name="category">The category to insert.</param>
        public static void InsertCategory(Category category)
        {
            Provider.InsertCategory(category);
        }

        /// <summary>
        /// Persists a new static page to the current provider.
        /// </summary>
        /// <remarks>
        /// Saves a new page to the data store. Pages are static content separate from blog posts.
        /// </remarks>
        /// <param name="page">The page to insert.</param>
        public static void InsertPage(Page page)
        {
            Provider.InsertPage(page);
        }

        /// <summary>
        /// Persists a new blog post to the current provider.
        /// </summary>
        /// <remarks>
        /// Saves a new post to the data store. Publishing status is determined by the post's IsPublished property.
        /// </remarks>
        /// <param name="post">The post to insert.</param>
        public static void InsertPost(Post post)
        {
            Provider.InsertPost(post);
        }

        /// <summary>
        /// Persists a new author profile to the current provider.
        /// </summary>
        /// <remarks>
        /// Creates a new author profile for blog contributors and writers.
        /// </remarks>
        /// <param name="profile">The author profile to insert.</param>
        public static void InsertProfile(AuthorProfile profile)
        {
            Provider.InsertProfile(profile);
        }

        /// <summary>
        /// Persists a new referrer record to the current provider.
        /// </summary>
        /// <remarks>
        /// Saves information about sites referring traffic to blog posts.
        /// </remarks>
        /// <param name="referrer">The referrer record to insert.</param>
        public static void InsertReferrer(Referrer referrer)
        {
            Provider.InsertReferrer(referrer);
        }

        /// <summary>
        /// Loads settings from data storage
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension ID
        /// </param>
        /// <returns>
        /// Settings as stream
        /// </returns>
        public static object LoadFromDataStore(ExtensionType extensionType, string extensionId)
        {
            return Provider.LoadFromDataStore(extensionType, extensionId);
        }

        /// <summary>
        /// Loads the ping services.
        /// </summary>
        /// <returns>A StringCollection.</returns>
        public static StringCollection LoadPingServices()
        {
            return Provider.LoadPingServices();
        }

        /// <summary>
        /// Loads the settings from the provider and returns
        /// them in a StringDictionary for the BlogSettings class to use.
        /// </summary>
        /// <returns>A StringDictionary.</returns>
        public static StringDictionary LoadSettings(Blog blog)
        {
            return Provider.LoadSettings(blog);
        }

        /// <summary>
        /// Loads the stop words from the data store.
        /// </summary>
        /// <returns>A StringCollection.</returns>
        public static StringCollection LoadStopWords()
        {
            return Provider.LoadStopWords();
        }

        /// <summary>
        /// Removes object from data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension Id
        /// </param>
        public static void RemoveFromDataStore(ExtensionType extensionType, string extensionId)
        {
            Provider.RemoveFromDataStore(extensionType, extensionId);
        }

        /// <summary>
        /// Saves the ping services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public static void SavePingServices(StringCollection services)
        {
            Provider.SavePingServices(services);
        }

        /// <summary>
        /// Saves all of the current BlogEngine rights to the provider.
        /// </summary>
        public static void SaveRights()
        {
            Provider.SaveRights(Right.GetAllRights());

            // This needs to be called after rights are changed.
            Right.RefreshAllRights();
        }

        /// <summary>
        /// Save the settings to the current provider.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public static void SaveSettings(StringDictionary settings)
        {
            Provider.SaveSettings(settings);
        }

        /// <summary>
        /// Saves settings to data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extensio ID
        /// </param>
        /// <param name="settings">
        /// Settings object
        /// </param>
        public static void SaveToDataStore(ExtensionType extensionType, string extensionId, object settings)
        {
            Provider.SaveToDataStore(extensionType, extensionId, settings);
        }

        /// <summary>
        /// Returns a BlogRoll based on the specified id.
        /// </summary>
        /// <param name="id">The BlogRoll id.</param>
        /// <returns>A BlogRollItem.</returns>
        public static BlogRollItem SelectBlogRoll(Guid id)
        {
            return Provider.SelectBlogRollItem(id);
        }

        /// <summary>
        /// Returns a Blog based on the specified id.
        /// </summary>
        /// <param name="id">The Blog id.</param>
        /// <returns>A Blog.</returns>
        public static Blog SelectBlog(Guid id)
        {
            return Provider.SelectBlog(id);
        }

        /// <summary>
        /// Returns a Category based on the specified id.
        /// </summary>
        /// <param name="id">The Category id.</param>
        /// <returns>A Category.</returns>
        public static Category SelectCategory(Guid id)
        {
            return Provider.SelectCategory(id);
        }

        /// <summary>
        /// Returns a Page based on the specified id.
        /// </summary>
        /// <param name="id">The Page id.</param>
        /// <returns>A Page object.</returns>
        public static Page SelectPage(Guid id)
        {
            return Provider.SelectPage(id);
        }

        /// <summary>
        /// Returns a Post based on the specified id.
        /// </summary>
        /// <param name="id">The post id.</param>
        /// <returns>A Post object.</returns>
        public static Post SelectPost(Guid id)
        {
            return Provider.SelectPost(id);
        }

        /// <summary>
        /// Returns a Page based on the specified id.
        /// </summary>
        /// <param name="id">The AuthorProfile id.</param>
        /// <returns>An AuthorProfile.</returns>
        public static AuthorProfile SelectProfile(string id)
        {
            return Provider.SelectProfile(id);
        }

        /// <summary>
        /// Returns a Referrer based on the specified id.
        /// </summary>
        /// <param name="id">The Referrer Id.</param>
        /// <returns>A Referrer.</returns>
        public static Referrer SelectReferrer(Guid id)
        {
            return Provider.SelectReferrer(id);
        }

        /// <summary>
        /// Sets up the required storage files/tables for a new Blog instance, from an existing blog instance.
        /// </summary>
        /// <param name="existingBlog">The existing blog instance to base the new blog instance off of.</param>
        /// <param name="newBlog">The new blog instance.</param>
        /// <returns>A boolean indicating if the setup process was successful.</returns>
        public static bool SetupBlogFromExistingBlog(Blog existingBlog, Blog newBlog)
        {
            return Provider.SetupBlogFromExistingBlog(existingBlog, newBlog);
        }

        /// <summary>
        /// Setup new blog
        /// </summary>
        /// <param name="newBlog">New blog</param>
        /// <param name="userName">User name</param>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <returns>True if successful</returns>
        public static bool SetupNewBlog(Blog newBlog, string userName, string email, string password)
        {
            return Provider.SetupNewBlog(newBlog, userName, email, password);
        }

        /// <summary>
        /// Updates an existing blog roll item.
        /// </summary>
        /// <remarks>
        /// Persists changes to a blog roll entry back to the data store.
        /// </remarks>
        /// <param name="blogRoll">The updated blog roll item.</param>
        public static void UpdateBlogRoll(BlogRollItem blogRoll)
        {
            Provider.UpdateBlogRollItem(blogRoll);
        }

        /// <summary>
        /// Updates an existing blog instance.
        /// </summary>
        /// <remarks>
        /// Persists changes to blog configuration and metadata back to the data store.
        /// </remarks>
        /// <param name="blog">The updated blog instance.</param>
        public static void UpdateBlog(Blog blog)
        {
            Provider.UpdateBlog(blog);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <remarks>
        /// Persists changes to a category definition back to the data store.
        /// </remarks>
        /// <param name="category">The updated category.</param>
        public static void UpdateCategory(Category category)
        {
            Provider.UpdateCategory(category);
        }

        /// <summary>
        /// Updates an existing static page.
        /// </summary>
        /// <remarks>
        /// Persists changes to page content and metadata back to the data store.
        /// </remarks>
        /// <param name="page">The updated page instance.</param>
        public static void UpdatePage(Page page)
        {
            Provider.UpdatePage(page);
        }

        /// <summary>
        /// Updates an existing blog post.
        /// </summary>
        /// <remarks>
        /// Persists changes to post content, metadata, and publication status back to the data store.
        /// </remarks>
        /// <param name="post">The updated post instance.</param>
        public static void UpdatePost(Post post)
        {
            Provider.UpdatePost(post);
        }

        /// <summary>
        /// Updates an existing author profile.
        /// </summary>
        /// <remarks>
        /// Persists changes to an author's profile information back to the data store.
        /// </remarks>
        /// <param name="profile">The updated author profile.</param>
        public static void UpdateProfile(AuthorProfile profile)
        {
            Provider.UpdateProfile(profile);
        }

        /// <summary>
        /// Updates an existing referrer record.
        /// </summary>
        /// <remarks>
        /// Persists changes to referrer information back to the data store.
        /// </remarks>
        /// <param name="referrer">The updated referrer record.</param>
        public static void UpdateReferrer(Referrer referrer)
        {
            Provider.UpdateReferrer(referrer);
        }

        #region FileSystem Static Methods

        internal static void ClearFileSystem()
        {
            FileSystemProvider.ClearFileSystem();
        }

        /// <summary>
        /// Creates a directory at a specific path
        /// </summary>
        /// <param name="VirtualPath">The virtual path to be created</param>
        /// <returns>the new Directory object created</returns>
        /// <remarks>
        /// Virtual path is the path starting from the /files/ containers
        /// The entity is created against the current blog id
        /// </remarks>
        public static Directory CreateDirectory(string VirtualPath)
        {
            return FileSystemProvider.CreateDirectory(VirtualPath);
        }

        /// <summary>
        /// Deletes a spefic directory from a virtual path
        /// </summary>
        /// <param name="VirtualPath">The path to delete</param>
        /// <remarks>
        /// Virtual path is the path starting from the /files/ containers
        /// The entity is queried against to current blog id
        /// </remarks>
        public static void DeleteDirectory(string VirtualPath)
        {
            FileSystemProvider.DeleteDirectory(VirtualPath);
        }

        /// <summary>
        /// Deletes a directory by passing in the Directory object
        /// </summary>
        /// <param name="DirectoryObj">the DirectoryObj </param>
        public static void DeleteDirectory(Directory DirectoryObj)
        {
            DeleteDirectory(DirectoryObj.FullPath);
        }

        /// <summary>
        /// Returns wether or not the specific directory by virtual path exists
        /// </summary>
        /// <param name="VirtualPath">The virtual path to query</param>
        /// <returns>boolean</returns>
        public static bool DirectoryExists(string VirtualPath)
        {
            return FileSystemProvider.DirectoryExists(VirtualPath);
        }

        /// <summary>
        /// gets a directory by the virtual path
        /// </summary>
        /// <param name="VirtualPath">the virtual path</param>
        /// <returns>the directory object or null for no directory found</returns>
        public static Directory GetDirectory(string VirtualPath)
        {
            return FileSystemProvider.GetDirectory(VirtualPath);
        }

        /// <summary>
        /// gets a directory by a basedirectory and a string array of sub path tree
        /// </summary>
        /// <param name="BaseDirectory">the base directory object</param>
        /// <param name="SubPath">the params of sub path</param>
        /// <returns>the directory found, or null for no directory found</returns>
        public static Directory GetDirectory(Directory BaseDirectory, params string[] SubPath)
        {
            return FileSystemProvider.GetDirectory(BaseDirectory, SubPath);
        }

        /// <summary>
        /// gets all the directories underneath a base directory. Only searches one level.
        /// </summary>
        /// <param name="BaseDirectory">the base directory</param>
        /// <returns>collection of Directory objects</returns>
        internal static IEnumerable<Directory> GetDirectories(Directory BaseDirectory)
        {
            return FileSystemProvider.GetDirectories(BaseDirectory);
        }

        /// <summary>
        /// gets all the files in a directory, only searches one level
        /// </summary>
        /// <param name="BaseDirectory">the base directory</param>
        /// <returns>collection of File objects</returns>
        public static IEnumerable<File> GetFiles(Directory BaseDirectory)
        {
            return FileSystemProvider.GetFiles(BaseDirectory);
        }

        /// <summary>
        /// gets a specific file by virtual path
        /// </summary>
        /// <param name="VirtualPath">the virtual path of the file</param>
        /// <returns></returns>
        public static File GetFile(string VirtualPath)
        {
            return FileSystemProvider.GetFile(VirtualPath);
        }

        /// <summary>
        /// boolean wether a file exists by its virtual path
        /// </summary>
        /// <param name="VirtualPath">the virtual path</param>
        /// <returns>boolean</returns>
        public static bool FileExists(string VirtualPath)
        {
            return FileSystemProvider.FileExists(VirtualPath);
        }

        /// <summary>
        /// deletes a file by virtual path
        /// </summary>
        /// <param name="VirtualPath">virtual path</param>
        public static void DeleteFile(string VirtualPath)
        {
            FileSystemProvider.DeleteFile(VirtualPath);
        }

        /// <summary>
        /// uploads a file to the provider container
        /// </summary>
        /// <param name="FileBinary">file contents as byte array</param>
        /// <param name="FileName">the file name</param>
        /// <param name="BaseDirectory">directory object that is the owner</param>
        /// <returns>the new file object</returns>
        public static FileSystem.File UploadFile(byte[] FileBinary, string FileName, FileSystem.Directory BaseDirectory)
        {
            return FileSystemProvider.UploadFile(FileBinary, FileName, BaseDirectory);
        }

        /// <summary>
        /// uploads a file to the provider container
        /// </summary>
        /// <param name="FileBinary">the contents of the file as a byte array</param>
        /// <param name="FileName">the file name</param>
        /// <param name="BaseDirectory">the directory object that is the owner</param>
        /// <param name="Overwrite">boolean wether to overwrite the file if it exists.</param>
        /// <returns>the new file object</returns>
        public static FileSystem.File UploadFile(byte[] FileBinary, string FileName, FileSystem.Directory BaseDirectory, bool Overwrite)
        {
            return FileSystemProvider.UploadFile(FileBinary, FileName, BaseDirectory, Overwrite);
        }

        /// <summary>
        /// uploads a file to the provider container
        /// </summary>
        /// <param name="FileStream">the file stream of the file being uploaded</param>
        /// <param name="FileName">the file name</param>
        /// <param name="BaseDirectory">the directory object that is the owner</param>
        /// <returns>the new file object</returns>
        public static FileSystem.File UploadFile(System.IO.Stream FileStream, string FileName, FileSystem.Directory BaseDirectory)
        {
            return UploadFile(FileStream, FileName, BaseDirectory, false);
        }

        /// <summary>
        /// uploads a file to the provider container
        /// </summary>
        /// <param name="FileStream">the file stream of the file being uploaded</param>
        /// <param name="FileName">the file name</param>
        /// <param name="BaseDirectory">the directory object that is the owner</param>
        /// <param name="Overwrite">boolean wether to overwrite the file if it exists.</param>
        /// <returns>the new file object</returns>
        public static FileSystem.File UploadFile(System.IO.Stream FileStream, string FileName, FileSystem.Directory BaseDirectory, bool Overwrite)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            FileStream.CopyTo(ms);
            FileStream.Close();
            byte[] binary = ms.ToArray();
            ms.Close();
            return FileSystemProvider.UploadFile(binary, FileName, BaseDirectory, Overwrite);
        }

        /// <summary>
        /// gets the file contents via Lazy load, however in the DbProvider the Contents are loaded when the initial object is created to cut down on DbReads
        /// </summary>
        /// <param name="BaseFile">the baseFile object to fill</param>
        /// <returns>the original file object</returns>
        internal static File GetFileContents(File BaseFile)
        {
            return FileSystemProvider.GetFileContents(BaseFile);
        }
        #endregion

        #region Packaging

        /// <summary>
        /// Save installed gallery package
        /// </summary>
        /// <param name="package">Installed package</param>
        public static void InsertPackage(InstalledPackage package)
        {
            Provider.SavePackage(package);
        }
        /// <summary>
        /// Save package files
        /// </summary>
        /// <param name="packageFiles">List of package files</param>
        public static void InsertPackageFiles(List<PackageFile> packageFiles)
        {
            Provider.SavePackageFiles(packageFiles);
        }

        /// <summary>
        /// Packages installed from online gallery
        /// </summary>
        /// <returns></returns>
        public static List<InstalledPackage> InstalledFromGalleryPackages()
        {
            return Provider.FillPackages();
        }

        /// <summary>
        /// Log of files installed by gallery package
        /// </summary>
        /// <param name="packageId">Package ID</param>
        /// <returns>List of files</returns>
        public static List<PackageFile> InstalledFromGalleryPackageFiles(string packageId)
        {
            return Provider.FillPackageFiles(packageId);
        }

        /// <summary>
        /// Delete all installed by package files from application
        /// </summary>
        /// <param name="packageId">Package ID</param>
        public static void DeletePackage(string packageId)
        {
            Provider.DeletePackage(packageId);
        }

        #endregion

        #region Custom fields

        /// <summary>
        /// Saves custom field
        /// </summary>
        /// <param name="field">Object custom field</param>
        public static void SaveCustomField(BlogEngine.Core.Data.Models.CustomField field)
        {
            Provider.SaveCustomField(field);
        }

        /// <summary>
        /// Fills list of custom fields for a blog
        /// </summary>
        /// <remarks>
        /// This method retrieves all custom fields for the current blog.
        /// </remarks>
        /// <returns>List of custom fields</returns>
        public static List<BlogEngine.Core.Data.Models.CustomField> FillCustomFields()
        {
            return Provider.FillCustomFields();
        }

        /// <summary>
        /// Deletes custom field
        /// </summary>
        /// <param name="field">Object field</param>
        public static void DeleteCustomField(BlogEngine.Core.Data.Models.CustomField field)
        {
            Provider.DeleteCustomField(field);
        }

        /// <summary>
        /// Clear custom fields for a type (post, theme etc)
        /// </summary>
        /// <param name="blogId">Blog id</param>
        /// <param name="customType">Custom type</param>
        /// <param name="objectType">Custom object</param>
        public static void ClearCustomFields(string blogId, string customType, string objectType)
        {
            Provider.ClearCustomFields(blogId, customType, objectType);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Load the providers from the web.config.
        /// </summary>
        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (_provider == null)
            {
                lock (TheLock)
                {
                    // Do this again to make sure _provider is still null
                    if (_provider == null)
                    {
                        // Get a reference to the <blogProvider> section
                        var section =
                            (BlogProviderSection)WebConfigurationManager.GetSection("BlogEngine/blogProvider");

                        // Load registered providers and point _provider
                        // to the default provider
                        _providers = new BlogProviderCollection();
                        ProvidersHelper.InstantiateProviders(section.Providers, _providers, typeof(BlogProvider));
                        _provider = _providers[section.DefaultProvider];

                        if (_provider == null)
                        {
                            throw new ProviderException("Unable to load default BlogProvider");
                        }
                    }
                }
            }
            if (_fileStorageProvider == null)
            {
                lock (TheLock)
                {
                    if (_fileStorageProvider == null)
                    {
                        var section = (BlogFileSystemProviderSection)WebConfigurationManager.GetSection("BlogEngine/blogFileSystemProvider");
                        _fileProviders = new BlogFileSystemProviderCollection();
                        ProvidersHelper.InstantiateProviders(section.Providers, _fileProviders, typeof(BlogFileSystemProvider));
                        _fileStorageProvider = _fileProviders[section.DefaultProvider];
                        if (_fileStorageProvider == null)
                        {
                            throw new ProviderException("unable to load default file system Blog Provider");
                        }
                    }
                }
            }
        }

        #endregion
    }
}