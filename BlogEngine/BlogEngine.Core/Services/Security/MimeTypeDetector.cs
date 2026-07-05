namespace BlogEngine.Core.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class MimeTypeDetector
    {
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

        private class MimeTypeSignature
        {
            public byte[] PrimarySignature { get; }
            public int SecondaryOffset { get; }
            public byte[] SecondarySignature { get; }
            public string MimeType { get; }
            public string[] Extensions { get; }

            public MimeTypeSignature(byte[] signature, string mimeType, string[] extensions)
            {
                PrimarySignature = signature; MimeType = mimeType; Extensions = extensions;
                SecondaryOffset = -1; SecondarySignature = null;
            }

            public MimeTypeSignature(byte[] primarySignature, int secondaryOffset, byte[] secondarySignature, string mimeType, string[] extensions)
            {
                PrimarySignature = primarySignature; SecondaryOffset = secondaryOffset;
                SecondarySignature = secondarySignature; MimeType = mimeType; Extensions = extensions;
            }

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
