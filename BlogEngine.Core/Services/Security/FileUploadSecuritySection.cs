namespace BlogEngine.Core.Services.Security
{
    using System.Configuration;

    /// <summary>
    /// Configuration section for file upload security settings.
    /// </summary>
    /// <remarks>
    /// Provides whitelist and blacklist configuration for file upload validation.
    /// Used to prevent uploading of dangerous file types (.aspx, .ashx, .config, etc.).
    /// </remarks>
    public class FileUploadSecuritySection : ConfigurationSection
    {
        /// <summary>
        /// Gets the collection of allowed file extensions.
        /// </summary>
        /// <remarks>
        /// Whitelist approach - only extensions listed here are permitted for upload.
        /// Extensions must include the leading dot (e.g., ".jpg", ".png").
        /// </remarks>
        [ConfigurationProperty("allowedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection AllowedExtensions
        {
            get { return (FileExtensionElementCollection)base["allowedExtensions"]; }
        }

        /// <summary>
        /// Gets the collection of blocked file extensions.
        /// </summary>
        /// <remarks>
        /// Blacklist approach - extensions listed here are explicitly denied.
        /// This provides defense-in-depth protection against dangerous file types.
        /// Extensions must include the leading dot (e.g., ".aspx", ".ashx").
        /// </remarks>
        [ConfigurationProperty("blockedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection BlockedExtensions
        {
            get { return (FileExtensionElementCollection)base["blockedExtensions"]; }
        }
    }

    /// <summary>
    /// Represents a single file extension element in the configuration.
    /// </summary>
    public class FileExtensionElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the file extension (e.g., ".jpg", ".aspx").
        /// </summary>
        [ConfigurationProperty("extension", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 2, MaxLength = 10)]
        public string Extension
        {
            get { return (string)base["extension"]; }
            set { base["extension"] = value; }
        }
    }

    /// <summary>
    /// Collection of file extension configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(FileExtensionElement))]
    public class FileExtensionElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Creates a new ConfigurationElement.
        /// </summary>
        /// <returns>A new FileExtensionElement.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileExtensionElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element.
        /// </summary>
        /// <param name="element">The ConfigurationElement to return the key for.</param>
        /// <returns>The element key (file extension).</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileExtensionElement)element).Extension;
        }

        /// <summary>
        /// Gets the FileExtensionElement at the specified index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The FileExtensionElement at the specified index.</returns>
        public FileExtensionElement this[int index]
        {
            get { return (FileExtensionElement)BaseGet(index); }
        }

        /// <summary>
        /// Gets the FileExtensionElement with the specified extension.
        /// </summary>
        /// <param name="extension">The extension to retrieve.</param>
        /// <returns>The FileExtensionElement with the specified extension.</returns>
        public new FileExtensionElement this[string extension]
        {
            get { return (FileExtensionElement)BaseGet(extension); }
        }

        /// <summary>
        /// Determines whether the collection contains an element with the specified extension.
        /// </summary>
        /// <param name="extension">The extension to check (case-insensitive).</param>
        /// <returns>True if the extension exists in the collection; otherwise, false.</returns>
        public bool Contains(string extension)
        {
            return BaseGet(extension.ToLowerInvariant()) != null;
        }
    }
}
