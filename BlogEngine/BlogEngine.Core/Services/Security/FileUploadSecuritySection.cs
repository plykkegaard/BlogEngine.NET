namespace BlogEngine.Core.Services.Security
{
    using System.Configuration;

    /// <summary>
    /// Configuration section for file upload security settings.
    /// </summary>
    /// <remarks>
    /// This configuration section defines the allowed and blocked file extensions for file uploads.
    /// It is typically configured in the application's Web.config or App.config file to control which
    /// file types users are permitted to upload to the BlogEngine application. Both allowed and blocked
    /// extension collections are required.
    /// </remarks>
    public class FileUploadSecuritySection : ConfigurationSection
    {
        /// <summary>
        /// Gets the collection of file extensions that are allowed for upload.
        /// </summary>
        /// <value>
        /// A FileExtensionElementCollection containing the list of allowed file extensions.
        /// This property is required and must be configured in the configuration file.
        /// </value>
        /// <remarks>
        /// Extensions in this collection are permitted for file uploads. This collection is configured
        /// through the "allowedExtensions" configuration property and contains FileExtensionElement items.
        /// </remarks>
        [ConfigurationProperty("allowedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection AllowedExtensions
        {
            get { return (FileExtensionElementCollection)base["allowedExtensions"]; }
        }

        /// <summary>
        /// Gets the collection of file extensions that are blocked from upload.
        /// </summary>
        /// <value>
        /// A FileExtensionElementCollection containing the list of blocked file extensions.
        /// This property is required and must be configured in the configuration file.
        /// </value>
        /// <remarks>
        /// Extensions in this collection are explicitly forbidden from being uploaded. This collection
        /// is configured through the "blockedExtensions" configuration property and contains FileExtensionElement items.
        /// These extensions take precedence over the allowed extensions in security validation.
        /// </remarks>
        [ConfigurationProperty("blockedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection BlockedExtensions
        {
            get { return (FileExtensionElementCollection)base["blockedExtensions"]; }
        }
    }

    /// <summary>
    /// Represents a single file extension configuration element.
    /// </summary>
    /// <remarks>
    /// This class is used within the configuration system to define an individual file extension entry.
    /// Each extension is validated to be between 0 and 20 characters in length and must be unique within
    /// its collection. Instances of this class are typically created through the configuration system rather
    /// than directly in code.
    /// </remarks>
    public class FileExtensionElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the file extension value.
        /// </summary>
        /// <value>
        /// A string representing the file extension (e.g., "exe", "dll", "pdf").
        /// If null, returns an empty string.
        /// </value>
        /// <remarks>
        /// This is the key property for the element and must be unique within its collection.
        /// The extension value is validated to be between 0 and 20 characters in length.
        /// It is configured through the "extension" configuration property.
        /// </remarks>
        [ConfigurationProperty("extension", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 0, MaxLength = 20)]
        public string Extension
        {
            get 
            { 
                var value = (string)base["extension"];
                return value ?? string.Empty;
            }
            set { base["extension"] = value; }
        }
    }

    /// <summary>
    /// Represents a configuration collection of file extension elements.
    /// </summary>
    /// <remarks>
    /// This collection class manages a group of FileExtensionElement objects and provides methods for
    /// accessing and querying them. It is used by the FileUploadSecuritySection to store both allowed
    /// and blocked file extensions. The collection supports access by index and by extension name.
    /// </remarks>
    [ConfigurationCollection(typeof(FileExtensionElement))]
    public class FileExtensionElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Creates a new FileExtensionElement instance.
        /// </summary>
        /// <returns>
        /// A new FileExtensionElement object.
        /// </returns>
        /// <remarks>
        /// This method is called by the configuration system when a new element needs to be created
        /// for the collection.
        /// </remarks>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileExtensionElement();
        }

        /// <summary>
        /// Gets the key for a configuration element.
        /// </summary>
        /// <param name="element">The FileExtensionElement to get the key for.</param>
        /// <returns>
        /// A string representing the extension value which serves as the unique key for the element.
        /// </returns>
        /// <remarks>
        /// This method is used by the configuration system to identify and retrieve elements within the collection.
        /// The key is the Extension property of the FileExtensionElement.
        /// </remarks>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileExtensionElement)element).Extension;
        }

        /// <summary>
        /// Gets the file extension element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>
        /// The FileExtensionElement at the specified index.
        /// </returns>
        /// <value>
        /// A FileExtensionElement retrieved from the collection by its index position.
        /// </value>
        public FileExtensionElement this[int index]
        {
            get { return (FileExtensionElement)BaseGet(index); }
        }

        /// <summary>
        /// Gets the file extension element with the specified extension name.
        /// </summary>
        /// <param name="extension">The extension name to search for.</param>
        /// <returns>
        /// The FileExtensionElement with the matching extension, or null if not found.
        /// </returns>
        /// <value>
        /// A FileExtensionElement retrieved from the collection by its extension key.
        /// </value>
        public new FileExtensionElement this[string extension]
        {
            get { return (FileExtensionElement)BaseGet(extension); }
        }

        /// <summary>
        /// Determines whether the collection contains an extension with the specified name.
        /// </summary>
        /// <param name="extension">The extension name to check for.</param>
        /// <returns>
        /// true if an element with the specified extension is found in the collection; otherwise, false.
        /// </returns>
        /// <remarks>
        /// The comparison is case-insensitive using the invariant culture (ToLowerInvariant()).
        /// </remarks>
        public bool Contains(string extension)
        {
            return BaseGet(extension.ToLowerInvariant()) != null;
        }
    }
}
