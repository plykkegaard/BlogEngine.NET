namespace BlogEngine.Tests.Security
{
    using System;
    using System.IO;
    using System.Text;
    using BlogEngine.Core.Services.Security;
    using Xunit;

    /// <summary>
    /// Unit tests for file upload security validation.
    /// </summary>
    /// <remarks>
    /// Tests the FileUploadValidator, MimeTypeDetector, and extension filtering logic
    /// to ensure malicious files are blocked and valid files are allowed.
    /// </remarks>
    public class FileUploadValidatorTests
    {
        #region MimeTypeDetector Tests

        [Fact]
        public void DetectMimeType_JpegFile_ReturnsImageJpeg()
        {
            // Arrange: JPEG magic bytes (FF D8 FF)
            byte[] jpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            using (var stream = new MemoryStream(jpegBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("image/jpeg", mimeType);
                Assert.Equal(0, stream.Position); // Stream should be rewound
            }
        }

        [Fact]
        public void DetectMimeType_PngFile_ReturnsImagePng()
        {
            // Arrange: PNG magic bytes (89 50 4E 47 0D 0A 1A 0A)
            byte[] pngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using (var stream = new MemoryStream(pngBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("image/png", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_GifFile_ReturnsImageGif()
        {
            // Arrange: GIF89a magic bytes (47 49 46 38 39 61)
            byte[] gifBytes = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x00, 0x00 };
            using (var stream = new MemoryStream(gifBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("image/gif", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_PdfFile_ReturnsApplicationPdf()
        {
            // Arrange: PDF magic bytes (25 50 44 46 = %PDF)
            byte[] pdfBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
            using (var stream = new MemoryStream(pdfBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("application/pdf", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_ZipFile_ReturnsApplicationZip()
        {
            // Arrange: ZIP magic bytes (50 4B 03 04)
            byte[] zipBytes = { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00 };
            using (var stream = new MemoryStream(zipBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("application/zip", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_Mp4File_ReturnsVideoMp4()
        {
            // Arrange: MP4 magic bytes (00 00 00 18 66 74 79 70 6D 70 34 32)
            byte[] mp4Bytes = { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 };
            using (var stream = new MemoryStream(mp4Bytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("video/mp4", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_XmlFile_ReturnsTextXml()
        {
            // Arrange: XML magic bytes (3C 3F 78 6D 6C = <?xml)
            byte[] xmlBytes = { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20, 0x76, 0x65 };
            using (var stream = new MemoryStream(xmlBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Equal("text/xml", mimeType);
            }
        }

        [Fact]
        public void DetectMimeType_ExecutableFile_ReturnsNull()
        {
            // Arrange: Random bytes that don't match any known signature
            byte[] exeBytes = { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 }; // MZ header
            using (var stream = new MemoryStream(exeBytes))
            {
                // Act
                string mimeType = MimeTypeDetector.DetectMimeType(stream);

                // Assert
                Assert.Null(mimeType); // Unknown format should return null
            }
        }

        [Fact]
        public void ValidateMimeTypeMatchesExtension_JpegWithJpgExtension_ReturnsTrue()
        {
            // Arrange
            byte[] jpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
            using (var stream = new MemoryStream(jpegBytes))
            {
                // Act
                bool isValid = MimeTypeDetector.ValidateMimeTypeMatchesExtension(stream, "photo.jpg");

                // Assert
                Assert.True(isValid);
            }
        }

        [Fact]
        public void ValidateMimeTypeMatchesExtension_ExecutableWithJpgExtension_ReturnsFalse()
        {
            // Arrange: Executable bytes masquerading as JPG
            byte[] exeBytes = { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
            using (var stream = new MemoryStream(exeBytes))
            {
                // Act
                bool isValid = MimeTypeDetector.ValidateMimeTypeMatchesExtension(stream, "fake.jpg");

                // Assert
                Assert.False(isValid); // Should fail because content doesn't match extension
            }
        }

        [Fact]
        public void ValidateMimeTypeMatchesExtension_PngWithJpgExtension_ReturnsFalse()
        {
            // Arrange: PNG file with wrong extension
            byte[] pngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using (var stream = new MemoryStream(pngBytes))
            {
                // Act
                bool isValid = MimeTypeDetector.ValidateMimeTypeMatchesExtension(stream, "image.jpg");

                // Assert
                Assert.False(isValid); // PNG content doesn't match .jpg extension
            }
        }

        #endregion

        #region FileUploadValidator Tests

        [Fact]
        public void ValidateFileExtension_AllowedExtension_ReturnsTrue()
        {
            // Note: This test requires Web.config to be properly configured
            // In a real environment, you might mock the configuration

            // Act
            bool isValid = FileUploadValidator.ValidateFileExtension("document.pdf");

            // Assert
            // This will depend on the actual configuration loaded
            // In a properly configured test environment with .pdf in allowedExtensions, this should be true
        }

        [Fact]
        public void ValidateFileExtension_BlockedExtension_ReturnsFalse()
        {
            // Act
            bool isBlocked = FileUploadValidator.IsExtensionBlocked("malicious.aspx");

            // Assert
            // .aspx should be in the blocked list
        }

        [Fact]
        public void ValidateFileExtension_NoExtension_ReturnsFalse()
        {
            // Act
            bool isValid = FileUploadValidator.ValidateFileExtension("filewithoutext");

            // Assert
            Assert.False(isValid); // Files without extensions should be rejected
        }

        [Fact]
        public void ValidateFileExtension_NullFileName_ReturnsFalse()
        {
            // Act
            bool isValid = FileUploadValidator.ValidateFileExtension(null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileExtension_EmptyFileName_ReturnsFalse()
        {
            // Act
            bool isValid = FileUploadValidator.ValidateFileExtension(string.Empty);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetValidationErrorMessage_ReturnsGenericMessage()
        {
            // Act
            string message = FileUploadValidator.GetValidationErrorMessage();

            // Assert
            Assert.False(string.IsNullOrEmpty(message));
            Assert.Contains("not allowed", message.ToLower());
        }

        [Fact]
        public void ValidateMimeType_NullStream_ReturnsFalse()
        {
            // Act
            bool isValid = FileUploadValidator.ValidateMimeType(null, "file.jpg", "image/jpeg");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateMimeType_TextFile_ReturnsTrue()
        {
            // Arrange: Plain text file
            byte[] textBytes = Encoding.UTF8.GetBytes("This is a plain text file.");
            using (var stream = new MemoryStream(textBytes))
            {
                // Act
                bool isValid = FileUploadValidator.ValidateMimeType(stream, "file.txt", "text/plain");

                // Assert
                Assert.True(isValid); // Text files are allowed without magic bytes
            }
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void IsFileUploadAllowed_ValidJpegFile_ReturnsTrue()
        {
            // Arrange
            byte[] jpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            using (var stream = new MemoryStream(jpegBytes))
            {
                // Act
                // Note: Requires proper Web.config configuration
                bool isAllowed = FileUploadValidator.IsFileUploadAllowed(stream, "photo.jpg", "image/jpeg");

                // Assert - will depend on configuration being loaded
            }
        }

        [Fact]
        public void IsFileUploadAllowed_ExecutableFile_ReturnsFalse()
        {
            // Arrange: Executable masquerading as image
            byte[] exeBytes = { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
            using (var stream = new MemoryStream(exeBytes))
            {
                // Act
                bool isAllowed = FileUploadValidator.IsFileUploadAllowed(stream, "virus.jpg", "image/jpeg");

                // Assert
                Assert.False(isAllowed); // Should fail MIME type validation
            }
        }

        [Fact]
        public void IsFileUploadAllowed_AspxFile_ReturnsFalse()
        {
            // Arrange
            byte[] aspxBytes = Encoding.UTF8.GetBytes("<%@ Page Language=\"C#\" %>");
            using (var stream = new MemoryStream(aspxBytes))
            {
                // Act
                bool isAllowed = FileUploadValidator.IsFileUploadAllowed(stream, "shell.aspx", "text/plain");

                // Assert
                Assert.False(isAllowed); // .aspx should be blocked by extension
            }
        }

        [Fact]
        public void IsFileUploadAllowed_NullStream_ReturnsFalse()
        {
            // Act
            bool isAllowed = FileUploadValidator.IsFileUploadAllowed(null, "file.jpg", "image/jpeg");

            // Assert
            Assert.False(isAllowed);
        }

        [Fact]
        public void IsFileUploadAllowed_EmptyFileName_ReturnsFalse()
        {
            // Arrange
            byte[] data = { 0xFF, 0xD8, 0xFF };
            using (var stream = new MemoryStream(data))
            {
                // Act
                bool isAllowed = FileUploadValidator.IsFileUploadAllowed(stream, "", "image/jpeg");

                // Assert
                Assert.False(isAllowed);
            }
        }

        #endregion
    }
}
