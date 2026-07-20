namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides methods for detecting MIME types based on file signatures (magic bytes) and validating MIME types against file extensions.
    /// </summary>
    /// <remarks>
    /// This class uses a collection of known file signatures to detect the actual MIME type of a file,
    /// independent of its extension. It supports detection of common image, video, audio, archive, and document formats.
    /// </remarks>
    public static class MimeTypeDetector
    {
        /// <summary>
        /// Signature database containing known file format identifiers.
        /// </summary>
        /// <remarks>
        /// Each signature entry defines the primary and optional secondary byte patterns that uniquely identify a file format,
        /// along with the corresponding MIME type and common file extensions.
        /// </remarks>
        private static readonly List<MimeTypeSignature> Signatures = new List<MimeTypeSignature>
        {
            new MimeTypeSignature(new byte[] { 0xFF, 0xD8, 0xFF }, "image/jpeg", new[] { ".jpg", ".jpeg" }),
            new MimeTypeSignature(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "image/png", new[] { ".png" }),
            new MimeTypeSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, "image/gif", new[] { ".gif" }),
            new MimeTypeSignature(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, "image/gif", new[] { ".gif" }),
            new MimeTypeSignature(new byte[] { 0x42, 0x4D }, "image/bmp", new[] { ".bmp" }),
            new MimeTypeSignature(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, "image/tiff", new[] { ".tiff", ".tif" }),
            new MimeTypeSignature(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, "image/tiff", new[] { ".tiff", ".tif" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x01, 0x00 }, "image/x-icon", new[] { ".ico" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x45, 0x42, 0x50 }, "image/webp", new[] { ".webp" }),
            new MimeTypeSignature(new byte[] { 0x25, 0x50, 0x44, 0x46 }, "application/pdf", new[] { ".pdf" }),
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, "application/zip", new[] { ".zip", ".docx", ".xlsx", ".pptx" }),
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x05, 0x06 }, "application/zip", new[] { ".zip" }),
            new MimeTypeSignature(new byte[] { 0x50, 0x4B, 0x07, 0x08 }, "application/zip", new[] { ".zip" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 }, "application/x-rar-compressed", new[] { ".rar" }),
            new MimeTypeSignature(new byte[] { 0x1F, 0x8B }, "application/gzip", new[] { ".gz" }),
            new MimeTypeSignature(new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, "application/x-7z-compressed", new[] { ".7z" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, "video/mp4", new[] { ".mp4" }),
            new MimeTypeSignature(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, "video/webm", new[] { ".webm", ".mkv" }),
            new MimeTypeSignature(new byte[] { 0x4F, 0x67, 0x67, 0x53 }, "video/ogg", new[] { ".ogg", ".ogv" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x41, 0x56, 0x49, 0x20 }, "video/x-msvideo", new[] { ".avi" }),
            new MimeTypeSignature(new byte[] { 0x49, 0x44, 0x33 }, "audio/mpeg", new[] { ".mp3" }),
            new MimeTypeSignature(new byte[] { 0xFF, 0xFB }, "audio/mpeg", new[] { ".mp3" }),
            new MimeTypeSignature(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x41, 0x56, 0x45 }, "audio/wav", new[] { ".wav" }),
            new MimeTypeSignature(new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, "text/xml", new[] { ".xml" }),
            new MimeTypeSignature(new byte[] { 0xEF, 0xBB, 0xBF, 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, "text/xml", new[] { ".xml" }),
            new MimeTypeSignature(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, "application/vnd.ms-office", new[] { ".doc", ".xls", ".ppt" })
        };

        /// <summary>
        /// Detects the MIME type of a file by reading its signature (magic bytes) from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the file data to analyze. The stream position is restored after reading.</param>
        /// <returns>
        /// The detected MIME type as a string (e.g., "image/jpeg", "application/pdf"), or null if the MIME type cannot be determined
        /// or if the stream is null or not readable.
        /// </returns>
        /// <remarks>
        /// This method reads up to 32 bytes from the beginning of the stream to match against known file signatures.
        /// If the stream is seekable, its original position is preserved after the operation.
        /// </remarks>
        public static string DetectMimeType(Stream stream)
        {
            if (stream == null || !stream.CanRead) return null;
            long originalPosition = stream.CanSeek ? stream.Position : 0;
            try
            {
                if (stream.CanSeek) stream.Position = 0;
                byte[] buffer = new byte[32];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) return null;
                foreach (var signature in Signatures.OrderByDescending(s => s.PrimarySignature.Length))
                {
                    if (signature.Matches(buffer, bytesRead)) return signature.MimeType;
                }
                return null;
            }
            finally { if (stream.CanSeek) stream.Position = originalPosition; }
        }

        /// <summary>
        /// Validates that the MIME type detected from a file stream matches the expected type based on the file extension.
        /// </summary>
        /// <param name="stream">The stream containing the file data to analyze.</param>
        /// <param name="fileName">The file name or path used to extract the file extension for validation.</param>
        /// <returns>
        /// true if the detected MIME type corresponds to the file's extension; false if the MIME type cannot be detected,
        /// the extension is invalid, the file name is empty, or the detected MIME type does not match the extension.
        /// </returns>
        /// <remarks>
        /// This method detects the actual MIME type from the file content and compares it against the expected extensions
        /// for that MIME type. This helps identify files that have incorrect extensions or potentially malicious content.
        /// </remarks>
        public static bool ValidateMimeTypeMatchesExtension(Stream stream, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            string detectedMimeType = DetectMimeType(stream);
            if (string.IsNullOrEmpty(detectedMimeType)) return false;
            string fileExtension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension)) return false;
            var matchingSignature = Signatures.FirstOrDefault(s => s.MimeType == detectedMimeType);
            if (matchingSignature == null) return false;
            return matchingSignature.Extensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Internal class representing a file format signature used for MIME type detection.
        /// </summary>
        /// <remarks>
        /// Each signature consists of a primary byte pattern (magic bytes) that identifies a file format.
        /// Some formats also use an optional secondary signature at a specific offset to ensure accurate detection.
        /// </remarks>
        private class MimeTypeSignature
        {
            /// <summary>
            /// Gets the primary signature bytes that identify the file format.
            /// </summary>
            /// <remarks>
            /// This contains the magic bytes at the beginning of a file that uniquely identify its format.
            /// </remarks>
            public byte[] PrimarySignature { get; }

            /// <summary>
            /// Gets the byte offset where the secondary signature is located, or -1 if no secondary signature is used.
            /// </summary>
            /// <remarks>
            /// Used for formats like WebP, WAV, and AVI that share a RIFF primary signature but differ in their secondary signatures.
            /// </remarks>
            public int SecondaryOffset { get; }

            /// <summary>
            /// Gets the secondary signature bytes, or null if not applicable.
            /// </summary>
            /// <remarks>
            /// When present, this provides additional bytes checked at the SecondaryOffset to disambiguate file formats.
            /// </remarks>
            public byte[] SecondarySignature { get; }

            /// <summary>
            /// Gets the MIME type identifier (e.g., "image/jpeg", "application/pdf").
            /// </summary>
            /// <remarks>
            /// This is the standard MIME type string that will be returned when this signature is matched.
            /// </remarks>
            public string MimeType { get; }

            /// <summary>
            /// Gets the array of file extensions commonly associated with this MIME type.
            /// </summary>
            /// <remarks>
            /// Used by ValidateMimeTypeMatchesExtension to verify that the detected MIME type corresponds to the file's extension.
            /// </remarks>
            public string[] Extensions { get; }

            /// <summary>
            /// Initializes a new instance of the MimeTypeSignature class with a primary signature only.
            /// </summary>
            /// <param name="signature">The byte pattern that identifies the file format.</param>
            /// <param name="mimeType">The MIME type associated with this signature.</param>
            /// <param name="extensions">The file extensions commonly associated with this MIME type.</param>
            public MimeTypeSignature(byte[] signature, string mimeType, string[] extensions)
            {
                PrimarySignature = signature; MimeType = mimeType; Extensions = extensions;
                SecondaryOffset = -1; SecondarySignature = null;
            }

            /// <summary>
            /// Initializes a new instance of the MimeTypeSignature class with both primary and secondary signatures.
            /// </summary>
            /// <param name="primarySignature">The primary byte pattern that identifies the file format.</param>
            /// <param name="secondaryOffset">The byte offset where the secondary signature begins.</param>
            /// <param name="secondarySignature">The secondary byte pattern that further refines format identification.</param>
            /// <param name="mimeType">The MIME type associated with these signatures.</param>
            /// <param name="extensions">The file extensions commonly associated with this MIME type.</param>
            /// <remarks>
            /// The secondary signature is checked at the specified offset to disambiguate between similar file formats
            /// that share the same primary signature (e.g., RIFF-based formats like WebP, WAV, and AVI).
            /// </remarks>
            public MimeTypeSignature(byte[] primarySignature, int secondaryOffset, byte[] secondarySignature, string mimeType, string[] extensions)
            {
                PrimarySignature = primarySignature; SecondaryOffset = secondaryOffset;
                SecondarySignature = secondarySignature; MimeType = mimeType; Extensions = extensions;
            }

            /// <summary>
            /// Determines whether the provided buffer matches this file signature.
            /// </summary>
            /// <param name="buffer">The byte buffer to check against the signatures.</param>
            /// <param name="bytesRead">The number of valid bytes in the buffer.</param>
            /// <returns>
            /// true if both the primary signature and any secondary signature (if defined) match the buffer; false otherwise.
            /// </returns>
            /// <remarks>
            /// This method first checks the primary signature at offset 0. If a secondary signature is defined,
            /// it is verified at the specified secondary offset.
            /// </remarks>
            public bool Matches(byte[] buffer, int bytesRead)
            {
                if (bytesRead < PrimarySignature.Length) return false;
                for (int i = 0; i < PrimarySignature.Length; i++)
                    if (buffer[i] != PrimarySignature[i]) return false;
                if (SecondarySignature != null && SecondaryOffset >= 0)
                {
                    int secondaryStart = SecondaryOffset;
                    if (bytesRead < secondaryStart + SecondarySignature.Length) return false;
                    for (int i = 0; i < SecondarySignature.Length; i++)
                        if (buffer[secondaryStart + i] != SecondarySignature[i]) return false;
                }
                return true;
            }
        }
    }
}
