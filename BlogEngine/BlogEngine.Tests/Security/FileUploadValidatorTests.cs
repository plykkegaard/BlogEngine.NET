namespace BlogEngine.Tests.Security
{
    using System.IO;
    using BlogEngine.Core.Services.Security;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileUploadValidatorTests
    {
        [TestMethod]
        public void DetectMimeType_JpegFile_ReturnsImageJpeg()
        {
            byte[] jpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            using (var stream = new MemoryStream(jpegBytes))
            {
                string mimeType = MimeTypeDetector.DetectMimeType(stream);
                Assert.AreEqual("image/jpeg", mimeType);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [TestMethod]
        public void DetectMimeType_PngFile_ReturnsImagePng()
        {
            byte[] pngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using (var stream = new MemoryStream(pngBytes))
            {
                string mimeType = MimeTypeDetector.DetectMimeType(stream);
                Assert.AreEqual("image/png", mimeType);
            }
        }

        [TestMethod]
        public void ValidateFileExtension_NoExtension_ReturnsFalse()
        {
            bool isValid = FileUploadValidator.ValidateFileExtension("filewithoutext");
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateFileExtension_NullFileName_ReturnsFalse()
        {
            bool isValid = FileUploadValidator.ValidateFileExtension(null);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetValidationErrorMessage_ReturnsGenericMessage()
        {
            string message = FileUploadValidator.GetValidationErrorMessage();
            Assert.IsFalse(string.IsNullOrEmpty(message));
            Assert.IsTrue(message.ToLower().Contains("not allowed"));
        }

        [TestMethod]
        public void IsFileUploadAllowed_NullStream_ReturnsFalse()
        {
            bool isAllowed = FileUploadValidator.IsFileUploadAllowed(null, "file.jpg", "image/jpeg");
            Assert.IsFalse(isAllowed);
        }

        [TestMethod]
        public void IsFileUploadAllowed_EmptyFileName_ReturnsFalse()
        {
            byte[] data = { 0xFF, 0xD8, 0xFF };
            using (var stream = new MemoryStream(data))
            {
                bool isAllowed = FileUploadValidator.IsFileUploadAllowed(stream, "", "image/jpeg");
                Assert.IsFalse(isAllowed);
            }
        }
    }
}
