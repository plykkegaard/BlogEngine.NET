using App_Code;
using BlogEngine.Core;
using BlogEngine.Core.API.BlogML;
using BlogEngine.Core.Providers;
using System;
using BlogEngine.Core.Services.Security;

using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

public class UploadController : ApiController
{
    public HttpResponseMessage Post(string action, string dirPath = "")
    {
        try
        {
            WebUtils.CheckRightsForAdminPostPages(false);

            HttpPostedFile file = HttpContext.Current.Request.Files[0];
            action = action.ToLowerInvariant();

            Utils.Log($"UploadController.Post: Starting upload - Action: {action}, DirPath: {dirPath}");

            if (file != null && file.ContentLength > 0)
            {
                var dirName = string.Format("/{0}/{1}", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
                var fileName = new FileInfo(file.FileName).Name; // to work in IE and others

                Utils.Log($"UploadController.Post: File received - Name: {fileName}, Size: {file.ContentLength}, ContentType: {file.ContentType}");

                // iOS sends all images as "image.jpg" or "image.png"
                fileName = fileName.Replace("image.jpg", DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
                fileName = fileName.Replace("image.png", DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png");
                // Validate file upload security (extension whitelist/blacklist and MIME type)
                if (!FileUploadValidator.IsFileUploadAllowed(file.InputStream, fileName, file.ContentType))
                {
                    var userName = Security.CurrentUser != null ? Security.CurrentUser.Identity.Name : "Anonymous";
                    var ipAddress = HttpContext.Current.Request.UserHostAddress;
                    BlogEngine.Core.Utils.LogSecurityEvent("UploadBlocked", 
                        $"User: {userName}, File: {fileName}, IP: {ipAddress}");
                    Utils.Log($"UploadController.Post: Upload blocked by security validation - File: {fileName}");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, FileUploadValidator.GetValidationErrorMessage());
                }

                Utils.Log($"UploadController.Post: Security validation passed for file: {fileName}");

                // Rewind stream after validation for subsequent use
                if (file.InputStream.CanSeek)
                    file.InputStream.Position = 0;

                var root = Blog.CurrentInstance.StorageLocation + Utils.FilesFolder;

                dirPath = dirPath.SanitizePath(root);

            
            if (!string.IsNullOrEmpty(dirPath))
                dirName = dirPath;

            if (action == "filemgr" || action == "file")
            {
                string[] ImageExtensnios = { ".jpg", ".png", ".jpeg", ".tiff", ".gif", ".bmp" };

                if (ImageExtensnios.Any(x => fileName.ToLower().Contains(x.ToLower())))
                    action = "image";
                else
                    action = "file";
            }

            var dir = new BlogEngine.Core.FileSystem.Directory();
            var retUrl = "";

            if (action == "import")
            {
                if (Security.IsAdministrator)
                {
                    return ImportBlogML();
                }
            } 
            if (action == "profile")
            {
                if (Security.IsAuthorizedTo(Rights.EditOwnUser))
                {
                    // upload profile image
                    dir = BlogService.GetDirectory("/avatars");
                    var dot = fileName.LastIndexOf(".");
                    var ext = dot > 0 ? fileName.Substring(dot) : "";
                    if (User.Identity.Name.Contains("/") || User.Identity.Name.Contains(@"\"))
                        throw new ApplicationException("Invalid character detected in UserName");
                    var profileFileName = User.Identity.Name + ext;

                    var imgPath = HttpContext.Current.Server.MapPath(dir.FullPath + "/" + profileFileName);
                    var image = Image.FromStream(file.InputStream);
                    Image thumb = image.GetThumbnailImage(80, 80, () => false, IntPtr.Zero);
                    thumb.Save(imgPath);

                    return Request.CreateResponse(HttpStatusCode.Created, profileFileName);
                }
            }
            if (action == "image")
            {
                if (Security.IsAuthorizedTo(Rights.EditOwnPosts))
                {
                    Utils.Log($"UploadController.Post: Processing image upload - File: {fileName}, Directory: {dirName}");
                    dir = BlogService.GetDirectory(dirName);
                    Utils.Log($"UploadController.Post: Directory obtained - Path: {dir.FullPath}");
                    var uploaded = BlogService.UploadFile(file.InputStream, fileName, dir, true);
                    Utils.Log($"UploadController.Post: Image uploaded successfully - URL: {uploaded.AsImage.ImageUrl}");
                    return Request.CreateResponse(HttpStatusCode.Created, uploaded.AsImage.ImageUrl);
                }
                else
                {
                    Utils.Log($"UploadController.Post: User not authorized for image upload");
                }
            }
            if (action == "file")
            {
                if (Security.IsAuthorizedTo(Rights.EditOwnPosts)) 
                {
                    Utils.Log($"UploadController.Post: Processing file upload - File: {fileName}, Directory: {dirName}");
                    dir = BlogService.GetDirectory(dirName);
                    Utils.Log($"UploadController.Post: Directory obtained - Path: {dir.FullPath}");
                    var uploaded = BlogService.UploadFile(file.InputStream, fileName, dir, true);
                    retUrl = uploaded.FileDownloadPath + "|" + fileName + " (" + BytesToString(uploaded.FileSize) + ")";
                    Utils.Log($"UploadController.Post: File uploaded successfully - Path: {uploaded.FileDownloadPath}");
                    return Request.CreateResponse(HttpStatusCode.Created, retUrl);
                }
                else
                {
                    Utils.Log($"UploadController.Post: User not authorized for file upload");
                }
            }
            if (action == "video")
            {
                if (Security.IsAuthorizedTo(Rights.EditOwnPosts))
                {
                    // default media folder
                    var mediaFolder = "Custom/Media";

                    // get the mediaplayer extension and use it's folder
                    var mediaPlayerExtension = BlogEngine.Core.Web.Extensions.ExtensionManager.GetExtension("MediaElementPlayer");
                    mediaFolder = mediaPlayerExtension.Settings[0].GetSingleValue("folder");

                    var folder = Utils.ApplicationRelativeWebRoot + mediaFolder + "/";
                    //var fileName = file.FileName;

                    UploadVideo(folder, file, fileName);

                    return Request.CreateResponse(HttpStatusCode.Created, fileName);
                }
            }
        }
        Utils.Log($"UploadController.Post: Returning BadRequest - No valid action or file");
        return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            Utils.Log($"UploadController.Post: Exception occurred during upload", ex);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, 
                new { error = "Upload failed: " + ex.Message });
        }
    }

    #region Private methods

    HttpResponseMessage ImportBlogML()
    {
        HttpPostedFile file = HttpContext.Current.Request.Files[0];
        if (file != null && file.ContentLength > 0)
        {
            var reader = new BlogReader();
            var rdr = new StreamReader(file.InputStream);
            reader.XmlData = rdr.ReadToEnd();

            if (reader.Import())
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
        return Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    static String BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }

    private void UploadVideo(string virtualFolder, HttpPostedFile file, string fileName)
    {
        var folder = HttpContext.Current.Server.MapPath(virtualFolder);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        file.SaveAs(folder + fileName);
    }

    #endregion
}
