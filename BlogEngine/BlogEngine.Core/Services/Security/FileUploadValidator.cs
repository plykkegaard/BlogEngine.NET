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
    public class FileUploadValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the file upload is allowed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the reason for rejection if the upload was not allowed.
        /// </summary>
        public FileUploadRejectionReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the filename that was validated.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadValidationResult"/> class.
        /// </summary>
        public FileUploadValidationResult()
        {
            Success = false;
            Reason = FileUploadRejectionReason.InvalidFile;
            FileName = string.Empty;
        }
    }

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

        /// <summary>
        /// Gets a validation error message based on the validation result and user authentication status.
        /// </summary>
        /// <param name="isAuthenticated">Whether the user is authenticated (admin/editor).</param>
        /// <param name="validationResult">The validation result containing rejection details.</param>
        /// <returns>A contextual error message appropriate for the user's authentication level.</returns>
        /// <remarks>
        /// Authenticated users receive detailed messages including filename and rejection reason.
        /// Anonymous users receive generic security messages.
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
