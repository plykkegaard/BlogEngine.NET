namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using BlogEngine.Core.Helpers;

    /// <summary>
    /// Provides comprehensive file upload validation including extension filtering and MIME type verification.
    /// </summary>
    /// <remarks>
    /// This class implements defense-in-depth security for file uploads by:
    /// 1. Validating file extensions against a whitelist (allowed) and blacklist (blocked)
    /// 2. Verifying actual file content matches the declared MIME type using magic bytes
    /// 3. Logging security events when uploads are blocked
    /// </remarks>
    public static class FileUploadValidator
    {
        private static FileUploadSecuritySection _securityConfig;
        private static readonly object _configLock = new object();

        /// <summary>
        /// Gets the file upload security configuration from Web.config.
        /// </summary>
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
                            }
                            catch (Exception ex)
                            {
                                Utils.Log("FileUploadValidator: Failed to load configuration section", ex);
                                // Return null - validation will fail closed (deny by default)
                            }
                        }
                    }
                }
                return _securityConfig;
            }
        }

        /// <summary>
        /// Validates whether a file upload is allowed based on extension and MIME type checks.
        /// </summary>
        /// <param name="fileStream">The uploaded file stream.</param>
        /// <param name="fileName">The name of the uploaded file.</param>
        /// <param name="contentType">The Content-Type header from the HTTP request.</param>
        /// <returns>True if the upload is allowed; otherwise, false.</returns>
        /// <remarks>
        /// This is the primary entry point for upload validation. It performs all security checks.
        /// </remarks>
        public static bool IsFileUploadAllowed(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", "Upload rejected: null stream or empty filename");
                return false;
            }

            // Step 1: Validate file extension against whitelist/blacklist
            if (!ValidateFileExtension(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", $"Upload rejected: invalid extension - {fileName}");
                return false;
            }

            // Step 2: Validate MIME type matches file content
            if (!ValidateMimeType(fileStream, fileName, contentType))
            {
                Utils.LogSecurityEvent("FileUploadValidation", $"Upload rejected: MIME type mismatch - {fileName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the file extension against allowed and blocked lists.
        /// </summary>
        /// <param name="fileName">The name of the file to validate.</param>
        /// <returns>True if the extension is allowed; otherwise, false.</returns>
        /// <remarks>
        /// Implements both whitelist (allowed) and blacklist (blocked) validation.
        /// The blacklist check takes precedence - if an extension is blocked, it cannot be uploaded
        /// even if it appears in the allowed list.
        /// </remarks>
        public static bool ValidateFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var config = SecurityConfig;
            if (config == null)
            {
                // Fail closed - if configuration is missing, deny uploads
                Utils.Log("FileUploadValidator: Configuration not available, denying upload");
                return false;
            }

            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                // Files without extensions are not allowed
                return false;
            }

            extension = extension.ToLowerInvariant();

            // Check blacklist first - blocked extensions are never allowed
            if (config.BlockedExtensions != null && IsExtensionInList(extension, config.BlockedExtensions))
            {
                return false;
            }

            // Check whitelist - extension must be explicitly allowed
            if (config.AllowedExtensions != null && IsExtensionInList(extension, config.AllowedExtensions))
            {
                return true;
            }

            // Extension not in whitelist - deny
            return false;
        }

        /// <summary>
        /// Validates that the file's actual MIME type matches its declared type and extension.
        /// </summary>
        /// <param name="fileStream">The file stream to analyze.</param>
        /// <param name="fileName">The file name with extension.</param>
        /// <param name="contentType">The Content-Type header from the HTTP request.</param>
        /// <returns>True if MIME type validation passes; otherwise, false.</returns>
        /// <remarks>
        /// Uses magic byte detection to verify the actual file type matches the extension.
        /// This prevents attacks where executables are renamed with safe extensions (e.g., virus.exe → virus.jpg).
        /// </remarks>
        public static bool ValidateMimeType(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || !fileStream.CanRead)
                return false;

            try
            {
                // Detect actual MIME type from file content
                string detectedMimeType = MimeTypeDetector.DetectMimeType(fileStream);

                if (string.IsNullOrEmpty(detectedMimeType))
                {
                    // Unknown file type - could be a text file or unsupported format
                    // For security, we'll allow certain text-based files without magic bytes
                    string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                    if (extension == ".txt" || extension == ".csv" || extension == ".json")
                    {
                        // These are safe text formats that may not have magic bytes
                        return true;
                    }

                    // Unknown binary format - reject
                    Utils.LogSecurityEvent("MimeTypeValidation", $"Unknown file format detected for {fileName}");
                    return false;
                }

                // Verify detected MIME type matches the file extension
                if (!MimeTypeDetector.ValidateMimeTypeMatchesExtension(fileStream, fileName))
                {
                    Utils.LogSecurityEvent("MimeTypeValidation", 
                        $"MIME type mismatch: {fileName} detected as {detectedMimeType}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Utils.Log("FileUploadValidator.ValidateMimeType", ex);
                // On error, fail closed (deny upload)
                return false;
            }
        }

        /// <summary>
        /// Checks if an extension exists in the specified configuration collection.
        /// </summary>
        /// <param name="extension">The extension to search for (case-insensitive).</param>
        /// <param name="collection">The configuration collection to search.</param>
        /// <returns>True if the extension is found; otherwise, false.</returns>
        private static bool IsExtensionInList(string extension, FileExtensionElementCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return false;

            extension = extension.ToLowerInvariant();

            for (int i = 0; i < collection.Count; i++)
            {
                var element = collection[i];
                if (element != null && element.Extension.ToLowerInvariant() == extension)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a user-friendly error message for upload validation failures.
        /// </summary>
        /// <returns>A generic error message that doesn't reveal security details.</returns>
        /// <remarks>
        /// Returns a generic message to prevent information disclosure about blocked file types.
        /// </remarks>
        public static string GetValidationErrorMessage()
        {
            return "The uploaded file type is not allowed. Please upload a valid file.";
        }

        /// <summary>
        /// Checks if a specific extension is explicitly blocked.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the extension is blocked; otherwise, false.</returns>
        public static bool IsExtensionBlocked(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return true;

            var config = SecurityConfig;
            if (config?.BlockedExtensions == null)
                return false;

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
                return true;

            return IsExtensionInList(extension, config.BlockedExtensions);
        }
    }
}
