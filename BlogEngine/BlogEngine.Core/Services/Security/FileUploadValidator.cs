namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Configuration;
    using System.IO;
    using BlogEngine.Core;

    /// <summary>
    /// Specifies the reason why a file upload was rejected.
    /// </summary>
    public enum FileUploadRejectionReason
    {
        /// <summary>
        /// No rejection - file upload is allowed.
        /// </summary>
        None = 0,

        /// <summary>
        /// File extension is not in the allowed list or is in the blocked list.
        /// </summary>
        ExtensionNotAllowed = 1,

        /// <summary>
        /// File content (MIME type) does not match the expected type for the extension.
        /// </summary>
        MimeTypeMismatch = 2,

        /// <summary>
        /// File is invalid (null stream, empty filename, etc.).
        /// </summary>
        InvalidFile = 3
    }

    /// <summary>
    /// Represents the result of a file upload validation.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the outcome of file upload validation, including success status,
    /// the specific reason for rejection (if any), and the filename being validated.
    /// Use the Success property for simple allow/deny logic, and the Reason property for
    /// providing detailed feedback to users.
    /// </remarks>
    public class FileUploadValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the file upload is allowed.
        /// </summary>
        /// <value>
        /// true if the file passed all validation checks; otherwise, false.
        /// </value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the reason for rejection if the upload was not allowed.
        /// </summary>
        /// <value>
        /// A FileUploadRejectionReason enumeration value indicating why the upload was rejected.
        /// If Success is true, this value is FileUploadRejectionReason.None.
        /// </value>
        public FileUploadRejectionReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the filename that was validated.
        /// </summary>
        /// <value>
        /// A string containing the filename passed to the validation method.
        /// May be an empty string if the filename was null.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadValidationResult"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the result with default values: Success is false,
        /// Reason is InvalidFile, and FileName is an empty string.
        /// </remarks>
        public FileUploadValidationResult()
        {
            Success = false;
            Reason = FileUploadRejectionReason.InvalidFile;
            FileName = string.Empty;
        }
    }

    /// <summary>
    /// Provides static methods for validating file uploads based on extension and MIME type security rules.
    /// </summary>
    /// <remarks>
    /// This class implements a two-layer security validation system:
    /// 1. Extension-based validation against configured allowed/blocked lists
    /// 2. MIME type detection and validation to prevent file type spoofing
    /// 
    /// Configuration is lazily loaded from the "BlogEngine/fileUploadSecurity" configuration section
    /// using thread-safe double-checked locking. Configuration errors are logged for diagnostics.
    /// </remarks>
    public static class FileUploadValidator
    {
        private static FileUploadSecuritySection _securityConfig;
        private static readonly object _configLock = new object();

        /// <summary>
        /// Gets the lazily-loaded file upload security configuration section.
        /// </summary>
        /// <value>
        /// The FileUploadSecuritySection from configuration, or null if not found or on error.
        /// </value>
        /// <remarks>
        /// Uses thread-safe double-checked locking to ensure configuration is loaded only once.
        /// Configuration errors are logged but do not throw exceptions to prevent denial of service.
        /// </remarks>
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

        /// <summary>
        /// Validates whether a file upload is allowed based on extension and MIME type validation.
        /// </summary>
        /// <param name="fileStream">The file stream to validate.</param>
        /// <param name="fileName">The name of the file to validate.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <returns>A FileUploadValidationResult containing validation status and details.</returns>
        /// <remarks>
        /// This method provides detailed validation results including the specific reason for rejection.
        /// Use this method when you need to provide contextual error messages to users.
        /// </remarks>
        public static FileUploadValidationResult ValidateFileUpload(Stream fileStream, string fileName, string contentType)
        {
            var result = new FileUploadValidationResult
            {
                FileName = fileName ?? string.Empty
            };

            if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", "Upload rejected: null stream or empty filename");
                result.Success = false;
                result.Reason = FileUploadRejectionReason.InvalidFile;
                return result;
            }

            if (!ValidateFileExtension(fileName))
            {
                Utils.LogSecurityEvent("FileUploadValidation", string.Format("Upload rejected: invalid extension - {0}", fileName));
                result.Success = false;
                result.Reason = FileUploadRejectionReason.ExtensionNotAllowed;
                return result;
            }

            if (!ValidateMimeType(fileStream, fileName, contentType))
            {
                Utils.LogSecurityEvent("FileUploadValidation", string.Format("Upload rejected: MIME type mismatch - {0}", fileName));
                result.Success = false;
                result.Reason = FileUploadRejectionReason.MimeTypeMismatch;
                return result;
            }

            result.Success = true;
            result.Reason = FileUploadRejectionReason.None;
            return result;
        }

        /// <summary>
        /// Determines whether a file upload is allowed without detailed reasoning.
        /// </summary>
        /// <param name="fileStream">The file stream to validate.</param>
        /// <param name="fileName">The name of the file to validate.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <returns>true if the upload is allowed; otherwise, false.</returns>
        /// <remarks>
        /// This is a convenience method for simple allow/deny decisions. For detailed validation feedback,
        /// use ValidateFileUpload(Stream, string, string) instead.
        /// Security events are logged for all rejections.
        /// </remarks>
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

        /// <summary>
        /// Validates whether a file extension is allowed based on security configuration.
        /// </summary>
        /// <param name="fileName">The name of the file whose extension will be validated.</param>
        /// <returns>true if the extension is allowed; otherwise, false.</returns>
        /// <remarks>
        /// This method checks if the extension is:
        /// 1. Not in the blocked extensions list
        /// 2. In the allowed extensions list
        /// 
        /// Blocked extensions are checked first and take precedence. Comparison is case-insensitive.
        /// If configuration is unavailable, the upload is denied for security.
        /// </remarks>
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

        /// <summary>
        /// Validates whether a file's content (MIME type) matches its extension.
        /// </summary>
        /// <param name="fileStream">The file stream whose content will be analyzed.</param>
        /// <param name="fileName">The name of the file being validated.</param>
        /// <param name="contentType">The provided content type header (may not be used if MIME detection is available).</param>
        /// <returns>true if the MIME type matches the extension; otherwise, false.</returns>
        /// <remarks>
        /// This method uses MimeTypeDetector to identify the actual file format and compares it
        /// against the extension. Some file types (txt, csv, json) may be allowed without MIME detection.
        /// If MIME type cannot be detected, the method logs a security event but may still allow certain
        /// text-based formats. Exceptions during detection are caught and logged, returning false to deny access.
        /// </remarks>
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

        /// <summary>
        /// Checks whether an extension is present in a collection of extensions.
        /// </summary>
        /// <param name="extension">The extension to search for (should include the dot, e.g., ".txt").</param>
        /// <param name="collection">The collection of FileExtensionElement items to search.</param>
        /// <returns>true if the extension is found in the collection; otherwise, false.</returns>
        /// <remarks>
        /// The comparison is case-insensitive using the invariant culture. If the collection is null or empty,
        /// returns false. This is a helper method used internally for both allowed and blocked extension checks.
        /// </remarks>
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

        /// <summary>
        /// Gets a validation error message based on the validation result and user authentication status.
        /// </summary>
        /// <param name="isAuthenticated">Whether the user is authenticated (admin/editor).</param>
        /// <param name="validationResult">The validation result containing rejection details.</param>
        /// <returns>A contextual error message appropriate for the user's authentication level.</returns>
        /// <remarks>
        /// Authenticated users receive detailed messages including filename and rejection reason.
        /// Anonymous users receive generic security messages to prevent information disclosure.
        /// </remarks>
        public static string GetValidationErrorMessage(bool isAuthenticated, FileUploadValidationResult validationResult)
        {
            if (validationResult == null || validationResult.Success)
            {
                return string.Empty;
            }

            // For anonymous/unauthenticated users, always return generic message
            if (!isAuthenticated)
            {
                return "The uploaded file type is not allowed. Please upload a valid file.";
            }

            // For authenticated users, provide detailed contextual messages
            string fileName = !string.IsNullOrWhiteSpace(validationResult.FileName) 
                ? validationResult.FileName 
                : "the file";

            string allowedTypesMessage = "Allowed file types: Images (jpg, png, gif, bmp), Documents (pdf, txt, csv, xml), Archives (zip, rar, 7z), Media (mp4, mp3).";

            switch (validationResult.Reason)
            {
                case FileUploadRejectionReason.ExtensionNotAllowed:
                    return string.Format("Upload rejected: '{0}' has a file extension that is not allowed. {1}", 
                        fileName, allowedTypesMessage);

                case FileUploadRejectionReason.MimeTypeMismatch:
                    return string.Format("Upload rejected: '{0}' file content does not match its extension. {1}", 
                        fileName, allowedTypesMessage);

                case FileUploadRejectionReason.InvalidFile:
                    return string.Format("Upload rejected: '{0}' is not a valid file.", fileName);

                default:
                    return "The uploaded file type is not allowed. Please upload a valid file.";
            }
        }

        /// <summary>
        /// Gets a generic validation error message.
        /// </summary>
        /// <returns>A generic error message for file upload rejection.</returns>
        /// <remarks>
        /// This method is maintained for backward compatibility.
        /// Consider using GetValidationErrorMessage(bool, FileUploadValidationResult) for contextual messages.
        /// </remarks>
        public static string GetValidationErrorMessage()
        {
            return "The uploaded file type is not allowed. Please upload a valid file.";
        }

        /// <summary>
        /// Determines whether a file extension is in the blocked extensions list.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        /// <returns>true if the file extension is blocked; otherwise, false.</returns>
        /// <remarks>
        /// This method only checks the blocked list, not the allowed list. It returns true for files
        /// with no extension or whitespace-only filenames as a safety precaution.
        /// </remarks>
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
