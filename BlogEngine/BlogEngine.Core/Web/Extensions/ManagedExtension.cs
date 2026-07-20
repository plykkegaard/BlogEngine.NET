namespace BlogEngine.Core.Web.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Serializable object that holds extension metadata, attributes, and configuration methods.
    /// </summary>
    /// <remarks>
    /// This class manages a BlogEngine extension by storing its metadata (name, version, author, description),
    /// configuration settings, and per-blog enablement status. It supports both primary blog and sub-blog settings
    /// with proper inheritance and cloning of configurations. The class is designed to be serialized and deserialized
    /// for persistence in the BlogEngine configuration system.
    /// </remarks>
    [Serializable]
    public class ManagedExtension
    {
        #region Constants and Fields

        /// <summary>
        /// The extension settings collection for all blogs.
        /// </summary>
        /// <remarks>
        /// Contains ExtensionSettings objects that store configuration values for this extension,
        /// organized by blog. Sub-blogs inherit settings from the primary blog when SubBlogEnabled is true.
        /// </remarks>
        private List<ExtensionSettings> settings;

        /// <summary>
        /// List of blogs that have explicitly disabled this extension.
        /// </summary>
        /// <remarks>
        /// When a blog ID is present in this list, the extension is disabled for that blog instance,
        /// even if SubBlogEnabled allows sub-blog settings.
        /// </remarks>


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedExtension"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default parameterless constructor required for XML serialization. 
        /// It initializes all properties with default values (empty strings and true for enabled/showSettings).
        /// </remarks>
        public ManagedExtension()
        {
            this.Version = string.Empty;
            this.ShowSettings = true;
            this.Name = string.Empty;
            this.Enabled = true;
            this.Description = string.Empty;
            this.Author = string.Empty;
            this.AdminPage = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedExtension"/> class.
        /// </summary>
        /// <param name="name">The extension name.</param>
        /// <param name="version">The version.</param>
        /// <param name="desc">The description.</param>
        /// <param name="author">The author.</param>
        /// <remarks>
        /// Creates a new extension instance with the specified metadata. This constructor initializes
        /// the settings collection and sets the extension as enabled with settings visible to administrators.
        /// </remarks>
        public ManagedExtension(string name, string version, string desc, string author)
        {
            this.AdminPage = string.Empty;
            this.Name = name;
            this.Version = version;
            this.Description = desc;
            this.Author = author;
            this.settings = new List<ExtensionSettings>();
            this.Enabled = true;
            this.ShowSettings = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a custom admin page for extension configuration.
        /// </summary>
        /// <remarks>
        /// If defined, a custom admin page URL replaces the default settings page link in the admin extensions list.
        /// This allows extensions to provide a fully custom management interface. When empty or null, 
        /// the default settings page is used.
        /// </remarks>
        [XmlElement]
        public string AdminPage { get; set; }

        /// <summary>
        /// Gets or sets the extension author name.
        /// </summary>
        /// <remarks>
        /// Displayed in the admin settings page and can be formatted as a hyperlink to the author's home page
        /// or contact information. This field provides attribution and helps administrators identify the extension creator.
        /// </remarks>
        [XmlElement]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the extension description.
        /// </summary>
        /// <remarks>
        /// A human-readable explanation of what the extension does, displayed in the admin interface
        /// to help administrators understand the extension's purpose and functionality.
        /// </remarks>
        [XmlElement]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the extension is enabled.
        /// </summary>
        /// <remarks>
        /// When false, the extension is inactive and should not execute its functionality.
        /// This global flag controls the extension status across all blogs unless overridden by the Blogs collection.
        /// </remarks>
        [XmlElement]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the extension name.
        /// </summary>
        /// <remarks>
        /// This is the unique identifier for the extension, typically matching the class or assembly name.
        /// Used throughout the system to reference the extension and stored as an XML attribute for efficient serialization.
        /// </remarks>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the extension execution priority.
        /// </summary>
        /// <remarks>
        /// Used to determine the order in which extensions are loaded and executed. Higher priority values
        /// typically indicate earlier execution. This allows control over extension initialization sequence
        /// when multiple extensions interact or have dependencies.
        /// </remarks>
        [XmlElement]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether extension properties are enabled for sub-blogs.
        /// </summary>
        /// <remarks>
        /// When true, sub-blogs can have their own settings inherited from and based on the primary blog's settings.
        /// When false, sub-blogs use only the primary blog's extension settings.
        /// </remarks>
        [XmlElement]
        public bool SubBlogEnabled { get; set; }

        /// <summary>
        /// Gets or sets the extension configuration settings.
        /// </summary>
        /// <remarks>
        /// Contains all ExtensionSettings for this extension across all blogs. When accessed on a sub-blog with
        /// SubBlogEnabled enabled, the getter automatically clones and adapts primary blog settings for the sub-blog
        /// if no sub-blog-specific settings exist yet. The setter replaces the entire settings collection.
        /// </remarks>
        [XmlElement(IsNullable = true)]
        public List<ExtensionSettings> Settings
        {
            get
            {
                if (!Blog.CurrentInstance.IsPrimary && SubBlogEnabled)
                {
                    if (settings.All(xset => xset.BlogId != Blog.CurrentInstance.Id))
                    {
                        var primId = Blog.Blogs.FirstOrDefault(b => b.IsPrimary).BlogId;

                        List<ExtensionSettings> newSets = GenericHelper<List<ExtensionSettings>>.Copy(
                            settings.Where(setItem => setItem.BlogId == primId || setItem.BlogId == null).ToList());
                      
                        foreach (var setItem in newSets)
                        {
                            setItem.BlogId = Blog.CurrentInstance.Id;
                            settings.Add(setItem);
                        }
                    }
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        /// <summary>
        /// Gets settings specific to the current blog instance.
        /// </summary>
        /// <remarks>
        /// Returns only settings applicable to the current blog context. For sub-blogs with SubBlogEnabled enabled,
        /// this automatically creates and returns inherited settings from the primary blog if they don't exist yet.
        /// This property is not serialized and is computed at runtime.
        /// </remarks>
        [XmlIgnore]
        public List<ExtensionSettings> BlogSettings
        {
            get
            {
                var primId = Blog.Blogs.FirstOrDefault(b => b.IsPrimary).BlogId;

                if (!Blog.CurrentInstance.IsPrimary && SubBlogEnabled)
                {
                    if (settings.All(xset => xset.BlogId != Blog.CurrentInstance.Id))
                    {
                        List<ExtensionSettings> newSets = GenericHelper<List<ExtensionSettings>>.Copy(
                            settings.Where(setItem => setItem.BlogId == primId || setItem.BlogId == null).ToList());

                        foreach (var setItem in newSets)
                        {
                            setItem.BlogId = Blog.CurrentInstance.Id;
                            settings.Add(setItem);
                        }
                    }
                }
                return settings.Where(s => s.BlogId == primId || s.BlogId == null).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the list of blogs that have opted to disable this extension.
        /// </summary>
        /// <remarks>
        /// When a blog ID is in this collection, the extension is explicitly disabled for that blog instance,
        /// even if the extension is globally enabled. This provides per-blog extension control.
        /// </remarks>
        [XmlElement]
        public List<Guid> Blogs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show or hide settings in the admin extensions list.
        /// </summary>
        /// <remarks>
        /// When false, the extension configuration options are hidden from administrators in the admin UI,
        /// useful for internal system extensions that should not be manually configured.
        /// </remarks>
        [XmlElement]
        public bool ShowSettings { get; set; }

        /// <summary>
        /// Gets or sets the extension version.
        /// </summary>
        /// <remarks>
        /// A semantic version string identifying the extension release. Displayed in the admin interface
        /// to help administrators track which version is installed and manage compatibility.
        /// </remarks>
        [XmlElement]
        public string Version { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the extension has a setting with the specified name.
        /// </summary>
        /// <param name="settingName">The name of the setting to search for.</param>
        /// <returns>
        /// True if a setting with the specified name exists in the extension's settings collection; false otherwise.
        /// </returns>
        /// <remarks>
        /// This method performs a linear search through the settings collection to check for existence.
        /// It's useful for validating configuration before attempting to retrieve or modify a setting.
        /// </remarks>
        public bool ContainsSetting(string settingName)
        {
            return this.settings.Any(xset => xset.Name == settingName);
        }

        /// <summary>
        /// Initializes a new extension setting with default values.
        /// </summary>
        /// <param name="extensionSettings">The extension setting to initialize.</param>
        /// <remarks>
        /// This method assigns an index to the setting based on the current settings collection size
        /// and then saves the setting. It's typically called when first adding a new setting to the extension.
        /// </remarks>
        public void InitializeSettings(ExtensionSettings extensionSettings)
        {
            extensionSettings.Index = this.settings.Count;
            this.SaveSettings(extensionSettings);
        }

        /// <summary>
        /// Determines whether the extension settings have been initialized with default values.
        /// </summary>
        /// <param name="xs">The extension setting to check.</param>
        /// <returns>
        /// True if the setting exists in the extension's settings collection and the parameter count matches;
        /// false if the setting is null or not found in the collection.
        /// </returns>
        /// <remarks>
        /// This method is typically used to detect whether an extension has been loaded for the first time
        /// and its default settings have been initialized. It checks by matching the setting name and parameter count.
        /// </remarks>
        public bool Initialized(ExtensionSettings xs)
        {
            return xs != null && this.settings.Where(setItem => setItem.Name == xs.Name).Any(setItem => setItem.Parameters.Count == xs.Parameters.Count);
        }

        /// <summary>
        /// Caches and persists extension settings.
        /// </summary>
        /// <param name="extensionSettings">The extension setting to save.</param>
        /// <remarks>
        /// This method handles saving settings for both primary blogs and sub-blogs. For primary blogs,
        /// it replaces existing settings with the same name. For sub-blogs with SubBlogEnabled enabled,
        /// it updates or creates blog-specific settings. The settings collection is sorted by index after the operation.
        /// If the setting name is empty, it defaults to the extension name.
        /// </remarks>
        public void SaveSettings(ExtensionSettings extensionSettings)
        {
            if (string.IsNullOrEmpty(extensionSettings.Name))
            {
                extensionSettings.Name = this.Name;
            }

            if (!Blog.CurrentInstance.IsPrimary && SubBlogEnabled)
            {
                // update settings for sub-blog
                foreach (var setItem in this.settings.Where(
                    setItem => setItem.Name == extensionSettings.Name && 
                        setItem.BlogId == extensionSettings.BlogId))
                {
                    this.settings.Remove(setItem);
                    break;
                }
            }
            else
            {
                // update settings for primary blog
                var primId = Blog.Blogs.FirstOrDefault(b => b.IsPrimary).BlogId;
                extensionSettings.BlogId = primId;

                foreach (var setItem in this.settings.Where(
                    setItem => setItem.Name == extensionSettings.Name && 
                        (setItem.BlogId == primId || setItem.BlogId == null)))
                {
                    this.settings.Remove(setItem);
                    break;
                }
            }
            settings.Add(extensionSettings);
           
			this.settings.Sort((s1, s2) => string.Compare(s1.Index.ToString(), s2.Index.ToString()));
        }

        /// <summary>
        /// Returns the physical path and filename of this extension.
        /// </summary>
        /// <param name="checkExistence">
        /// If true, existence of the file is checked and if the file does not exist,
        /// an empty string is returned.
        /// </param>
        /// <returns></returns>
        //public string GetPathAndFilename(bool checkExistence)
        //{
        //    string filename = string.Empty;
        //    var appRoot = HostingEnvironment.MapPath("~/");
        //    var codeAssemblies = Utils.CodeAssemblies();
        //    foreach (Assembly a in codeAssemblies)
        //    {
        //        var types = a.GetTypes();
        //        foreach (var type in types.Where(type => type.Name == Name))
        //        {
        //            var assemblyName = type.Assembly.FullName.Split(".".ToCharArray())[0];
        //            assemblyName = assemblyName.Replace("App_SubCode_", "App_Code\\");
        //            var fileExt = assemblyName.Contains("VB_Code") ? ".vb" : ".cs";
        //            filename = appRoot + Path.Combine(Path.Combine(assemblyName, "Extensions"), Name + fileExt);
        //        }
        //    }

        //    if (checkExistence && !string.IsNullOrWhiteSpace(filename))
        //    {
        //        if (!File.Exists(filename))
        //            return string.Empty;
        //    }

        //    return filename;
        //}

        #endregion
    }

    /// <summary>
    /// Generic utility class for object cloning and deep copying.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into after cloning.</typeparam>
    /// <remarks>
    /// This class uses binary serialization to create deep copies of serializable objects.
    /// Objects are serialized to a memory stream and then deserialized to create a completely independent copy,
    /// including all nested objects and collections. This approach ensures true deep copying with no shared references.
    /// </remarks>
    public static class GenericHelper<T>
    {
        /// <summary>
        /// Creates a deep copy of the specified object using binary serialization.
        /// </summary>
        /// <param name="objectToCopy">The object to clone. Must be serializable.</param>
        /// <returns>
        /// A new object of type T that is a complete deep copy of the input object,
        /// with all nested objects and collections independently duplicated.
        /// </returns>
        /// <remarks>
        /// This method uses BinaryFormatter to serialize the object to a memory stream and then deserialize it
        /// to create a fully independent copy. All nested objects are also cloned, ensuring no shared references
        /// between the original and copy. The object type must be marked with the [Serializable] attribute.
        /// </remarks>
        public static T Copy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}