namespace BlogEngine.Core.Services.Security
{
    using System.Configuration;

    /// <summary>
    /// Configuration section for file upload security settings.
    /// </summary>
    public class FileUploadSecuritySection : ConfigurationSection
    {
        [ConfigurationProperty("allowedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection AllowedExtensions
        {
            get { return (FileExtensionElementCollection)base["allowedExtensions"]; }
        }

        [ConfigurationProperty("blockedExtensions", IsRequired = true)]
        [ConfigurationCollection(typeof(FileExtensionElementCollection), AddItemName = "add")]
        public FileExtensionElementCollection BlockedExtensions
        {
            get { return (FileExtensionElementCollection)base["blockedExtensions"]; }
        }
    }

    public class FileExtensionElement : ConfigurationElement
    {
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

    [ConfigurationCollection(typeof(FileExtensionElement))]
    public class FileExtensionElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileExtensionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileExtensionElement)element).Extension;
        }

        public FileExtensionElement this[int index]
        {
            get { return (FileExtensionElement)BaseGet(index); }
        }

        public new FileExtensionElement this[string extension]
        {
            get { return (FileExtensionElement)BaseGet(extension); }
        }

        public bool Contains(string extension)
        {
            return BaseGet(extension.ToLowerInvariant()) != null;
        }
    }
}
