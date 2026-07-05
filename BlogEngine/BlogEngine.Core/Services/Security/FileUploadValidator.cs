namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Configuration;
    using System.IO;
    using BlogEngine.Core;

    public static class FileUploadValidator
    {
        private static FileUploadSecuritySection _securityConfig;
        private static readonly object _configLock = new object();

        private static FileUploadSecuritySection SecurityConfig
        {
            get
            {
                if (_securityConfig == null)
                {
                    lock (_configLock)
                    {
                        if (_securityConfig == null)
                        {
                            try 
                            { 
                                _securityConfig = (FileUploadSecuritySection)ConfigurationManager.GetSection("BlogEngine/fileUploadSecurity"); 
                                if (_securityConfig != null)
                                {
                                    Utils.Log("FileUploadValidator: Configuration loaded successfully");
                                }
                            }
                            catch (ConfigurationErrorsException configEx) 
                            { 
                                Utils.Log($"FileUploadValidator: Configuration error at {configEx.Filename} line {configEx.Line}: {configEx.Message}");
                                Utils.Log("FileUploadValidator: Please check web.config for invalid extension values. Extensions must be at least 1 character (e.g., '.c', '.jpg')");
                            }
                            catch (Exception ex) 
                            { 
                                Utils.Log("FileUploadValidator: Failed to load configuration", ex); 
                            }
                        }
                    }
                }
                return _securityConfig;
            }
        }

        public static bool IsFileUploadAllowed(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", "Upload rejected: null stream or empty filename");
                return false;
            }
            if (!ValidateFileExtension(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", string.Format("Upload rejected: invalid extension - {0}", fileName));
                return false;
            }
            if (!ValidateMimeType(fileStream, fileName, contentType))
            {
                Utils.LogSecurityEvent("FileUploadValidation", string.Format("Upload rejected: MIME type mismatch - {0}", fileName));
                return false;
            }
            return true;
        }

        public static bool ValidateFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            var config = SecurityConfig;
            if (config == null)
            {
                Utils.Log("FileUploadValidator: Configuration not available, denying upload");
                return false;
            }
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension)) return false;
            extension = extension.ToLowerInvariant();
            if (config.BlockedExtensions != null && IsExtensionInList(extension, config.BlockedExtensions))
                return false;
            if (config.AllowedExtensions != null && IsExtensionInList(extension, config.AllowedExtensions))
                return true;
            return false;
        }

        public static bool ValidateMimeType(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || !fileStream.CanRead) return false;
            try
            {
                string detectedMimeType = MimeTypeDetector.DetectMimeType(fileStream);
                if (string.IsNullOrEmpty(detectedMimeType))
                {
                    string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                    if (extension == ".txt" || extension == ".csv" || extension == ".json") return true;
                    Utils.LogSecurityEvent("MimeTypeValidation", string.Format("Unknown file format detected for {0}", fileName));
                    return false;
                }
                if (!MimeTypeDetector.ValidateMimeTypeMatchesExtension(fileStream, fileName))
                {
                    Utils.LogSecurityEvent("MimeTypeValidation", string.Format("MIME type mismatch: {0} detected as {1}", fileName, detectedMimeType));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utils.Log("FileUploadValidator.ValidateMimeType", ex);
                return false;
            }
        }

        private static bool IsExtensionInList(string extension, FileExtensionElementCollection collection)
        {
            if (collection == null || collection.Count == 0) return false;
            extension = extension.ToLowerInvariant();
            for (int i = 0; i < collection.Count; i++)
            {
                var element = collection[i];
                if (element != null && !string.IsNullOrWhiteSpace(element.Extension) && element.Extension.ToLowerInvariant() == extension) return true;
            }
            return false;
        }

        public static string GetValidationErrorMessage()
        {
            return "The uploaded file type is not allowed. Please upload a valid file.";
        }

        public static bool IsExtensionBlocked(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return true;
            var config = SecurityConfig;
            if (config?.BlockedExtensions == null) return false;
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension)) return true;
            return IsExtensionInList(extension, config.BlockedExtensions);
        }
    }
}
