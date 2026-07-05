namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Utility class for detecting MIME types based on file magic bytes (file signatures).
    /// </summary>
    /// <remarks>
    /// This class reads the first bytes of a file stream and matches them against known
    /// file format signatures to determine the actual file type, regardless of the file extension.
    /// This prevents attacks where malicious files are renamed with safe extensions.
    /// </remarks>
    public static class MimeTypeDetector
    {
        /// <summary>
        /// Dictionary of magic byte signatures mapped to MIME types.
        /// </summary>
        /// <remarks>
        /// Each entry contains the file signature bytes and the corresponding MIME type.
        /// Signatures are checked in order, with longer signatures checked first to avoid false positives.
        /// </remarks>
        private static readonly List<MimeTypeSignature> Signatures = new List<MimeTypeSignature>
        {
            // Image formats
            new MimeTypeSignature(new byte[] { 0xFF, 0xD8, 0xFF }, "image/jpeg", new[] { ".jpg", ".jpeg" }),
            new MimeTypeSignature(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "image/png", new[] { ".png" }),
            new MimeTypeSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, "image/gif", new[] { ".gif" }),
            new MimeTypeSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, "image/gif", new[] { ".gif" }),
            new MimeTypeSignature(new byte[] { 0x42, 0x4D }, "image/bmp", new[] { ".bmp" }),
            new MimeTypeSignature(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, "image/tiff", new[] { ".tiff", ".tif" }),
            new MimeTypeSignature(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, "image/tiff", new[] { ".tiff", ".tif" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x01, 0x00 }, "image/x-icon", new[] { ".ico" }),

            // WebP image format
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x45, 0x42, 0x50 }, "image/webp", new[] { ".webp" }),

            // Document formats
            new MimeTypeSignature(new byte[] { 0x25, 0x50, 0x44, 0x46 }, "application/pdf", new[] { ".pdf" }),

            // Archive formats
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, "application/zip", new[] { ".zip", ".docx", ".xlsx", ".pptx" }),
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x05, 0x06 }, "application/zip", new[] { ".zip" }),
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x07, 0x08 }, "application/zip", new[] { ".zip" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 }, "application/x-rar-compressed", new[] { ".rar" }),
            new MimeTypeSignature(new byte[] { 0x1F, 0x8B }, "application/gzip", new[] { ".gz" }),
            new MimeTypeSignature(new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, "application/x-7z-compressed", new[] { ".7z" }),

            // Video formats
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, "video/webm", new[] { ".webm", ".mkv" }),
            new MimeTypeSignature(new byte[] { 0x4F, 0x67, 0x67, 0x53 }, "video/ogg", new[] { ".ogg", ".ogv" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x41, 0x56, 0x49, 0x20 }, "video/x-msvideo", new[] { ".avi" }),

            // Audio formats
            new MimeTypeSignature(new byte[] { 0x49, 0x44, 0x33 }, "audio/mpeg", new[] { ".mp3" }),
            new MimeTypeSignature(new byte[] { 0xFF, 0xFB }, "audio/mpeg", new[] { ".mp3" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x41, 0x56, 0x45 }, "audio/wav", new[] { ".wav" }),

            // Text/XML formats
            new MimeTypeSignature(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, "text/xml", new[] { ".xml" }),
            new MimeTypeSignature(new byte[] { 0xEF, 0xBB, 0xBF, 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, "text/xml", new[] { ".xml" }),

            // Office formats (legacy)
            new MimeTypeSignature(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, "application/vnd.ms-office", new[] { ".doc", ".xls", ".ppt" }),
        };

        /// <summary>
        /// Detects the MIME type of a file by reading its magic bytes.
        /// </summary>
        /// <param name="stream">The file stream to analyze.</param>
        /// <returns>The detected MIME type, or null if the format is not recognized.</returns>
        /// <remarks>
        /// The stream position is reset to its original position after detection.
        /// </remarks>
        public static string DetectMimeType(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return null;

            long originalPosition = stream.CanSeek ? stream.Position : 0;

            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                byte[] buffer = new byte[32]; // Read first 32 bytes for signature matching
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return null;

                // Check each signature in order (longest first for accuracy)
                foreach (var signature in Signatures.OrderByDescending(s => s.PrimarySignature.Length))
                {
                    if (signature.Matches(buffer, bytesRead))
                        return signature.MimeType;
                }

                return null; // Unknown file type
            }
            finally
            {
                if (stream.CanSeek)
                    stream.Position = originalPosition;
            }
        }

        /// <summary>
        /// Detects the MIME type and checks if it matches the expected extensions.
        /// </summary>
        /// <param name="stream">The file stream to analyze.</param>
        /// <param name="fileName">The file name with extension.</param>
        /// <returns>True if the detected MIME type matches the file extension; otherwise, false.</returns>
        public static bool ValidateMimeTypeMatchesExtension(Stream stream, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            string detectedMimeType = DetectMimeType(stream);
            if (string.IsNullOrEmpty(detectedMimeType))
                return false; // Unknown format - reject

            string fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension))
                return false;

            // Find the signature that matched
            var matchingSignature = Signatures.FirstOrDefault(s => s.MimeType == detectedMimeType);
            if (matchingSignature == null)
                return false;

            // Check if the file extension matches the detected type
            return matchingSignature.Extensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Represents a file signature (magic bytes) and its associated MIME type.
        /// </summary>
        private class MimeTypeSignature
        {
            public byte[] PrimarySignature { get; }
            public int SecondaryOffset { get; }
            public byte[] SecondarySignature { get; }
            public string MimeType { get; }
            public string[] Extensions { get; }

            /// <summary>
            /// Initializes a new instance with a single signature.
            /// </summary>
            public MimeTypeSignature(byte[] signature, string mimeType, string[] extensions)
            {
                PrimarySignature = signature;
                MimeType = mimeType;
                Extensions = extensions;
                SecondaryOffset = -1;
                SecondarySignature = null;
            }

            /// <summary>
            /// Initializes a new instance with primary and secondary signatures (e.g., RIFF formats).
            /// </summary>
            public MimeTypeSignature(byte[] primarySignature, int secondaryOffset, byte[] secondarySignature, string mimeType, string[] extensions)
            {
                PrimarySignature = primarySignature;
                SecondaryOffset = secondaryOffset;
                SecondarySignature = secondarySignature;
                MimeType = mimeType;
                Extensions = extensions;
            }

            /// <summary>
            /// Checks if the buffer matches this signature.
            /// </summary>
            public bool Matches(byte[] buffer, int bytesRead)
            {
                if (bytesRead < PrimarySignature.Length)
                    return false;

                // Check primary signature
                for (int i = 0; i < PrimarySignature.Length; i++)
                {
                    if (buffer[i] != PrimarySignature[i])
                        return false;
                }

                // Check secondary signature if present
                if (SecondarySignature != null && SecondaryOffset >= 0)
                {
                    int secondaryStart = SecondaryOffset;
                    if (bytesRead < secondaryStart + SecondarySignature.Length)
                        return false;

                    for (int i = 0; i < SecondarySignature.Length; i++)
                    {
                        if (buffer[secondaryStart + i] != SecondarySignature[i])
                            return false;
                    }
                }

                return true;
            }
        }
    }
}
