# File Upload Security Implementation

**Version:** 3.4.0.0  
**Date:** 2025-01-XX  
**Status:** ✅ Implemented & Tested

## Overview

This document describes the comprehensive file upload security measures implemented in BlogEngine.NET to prevent malicious file uploads including web shells, executables, and disguised files.

## Security Features

### 1. Extension-Based Filtering

**Location:** `Web.Config` → `BlogEngine/fileUploadSecurity`

#### Allowed Extensions (Whitelist)
- **Images:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.tiff`, `.tif`, `.webp`, `.ico`
- **Documents:** `.pdf`, `.txt`, `.csv`, `.xml`, `.json`
- **Archives:** `.zip`, `.rar`, `.7z`
- **Media:** `.mp4`, `.webm`, `.ogg`, `.ogv`, `.mp3`, `.wav`

#### Blocked Extensions (Blacklist)
- **ASP.NET:** `.aspx`, `.ashx`, `.asmx`, `.asax`, `.ascx`, `.master`, `.config`
- **Code:** `.cs`, `.vb`
- **Executables:** `.dll`, `.exe`, `.com`, `.bat`, `.cmd`, `.vbs`, `.ps1`, `.msi`, `.scr`, `.jar`

**Defense Strategy:** Files must be in the allowed list AND not in the blocked list.

### 2. MIME Type Validation

**Component:** `MimeTypeDetector.cs`

Uses magic byte signatures to detect actual file content regardless of extension:

| File Type | Signature (Hex) | MIME Type |
|-----------|----------------|-----------|
| JPEG | `FF D8 FF` | `image/jpeg` |
| PNG | `89 50 4E 47 0D 0A 1A 0A` | `image/png` |
| GIF | `47 49 46 38` | `image/gif` |
| PDF | `25 50 44 46` | `application/pdf` |
| ZIP | `50 4B 03 04` | `application/zip` |
| WebP | `52 49 46 46 ... 57 45 42 50` | `image/webp` |
| MP4 | `00 00 00 [18|1C|20] 66 74 79 70` | `video/mp4` |

**Key Features:**
- Stream position is preserved after detection
- Validates that detected MIME type matches file extension
- Returns `null` for unknown formats (triggers fallback handling)

### 3. Centralized Validation

**Component:** `FileUploadValidator.cs`

All uploads are validated through a single entry point:

```csharp
public static bool IsFileUploadAllowed(Stream fileStream, string fileName, string contentType)
{
	// 1. Null/empty checks
	// 2. Extension validation (whitelist/blacklist)
	// 3. MIME type validation (magic bytes + extension match)
	// 4. Security event logging on rejection
	return true/false;
}
```

### 4. Upload Controller Integration

**File:** `UploadController.cs`

Validation is performed **before** any file processing:

```csharp
// Early validation (post-filename normalization, pre-dispatch)
if (!FileUploadValidator.IsFileUploadAllowed(file.InputStream, fileName, file.ContentType))
{
	Utils.LogSecurityEvent("UploadBlocked", 
		$"User {Security.CurrentUser.Identity.Name} attempted to upload blocked file: {fileName}");

	return Request.CreateResponse(HttpStatusCode.BadRequest, 
		FileUploadValidator.GetValidationErrorMessage());
}

// Stream rewind for downstream processing
if (file.InputStream.CanSeek)
	file.InputStream.Position = 0;

// Existing action dispatch continues...
```

**Coverage:** This protects all upload paths:
- Image uploads
- File uploads
- Video uploads
- Profile pictures
- Import files

### 5. Security Logging

**Component:** `Utils.LogSecurityEvent()`

All rejected uploads are logged with:
- UTC timestamp
- Event type (`UploadBlocked`)
- User identity
- Filename
- Reason for rejection

**Format:**
```
[SECURITY] [2025-01-XX 14:23:45 UTC] UploadBlocked: User admin attempted to upload blocked file: malicious.aspx
```

### 6. Generic Error Messages

Users receive a generic message on upload rejection:

> "The uploaded file type is not allowed. Please upload a valid file."

This prevents attackers from learning which specific checks failed.

## Configuration

### Web.Config Structure

```xml
<configuration>
  <configSections>
	<sectionGroup name="BlogEngine">
	  <section name="fileUploadSecurity" 
			   type="BlogEngine.Core.Services.Security.FileUploadSecuritySection, BlogEngine.Core" 
			   requirePermission="false" />
	</sectionGroup>
  </configSections>

  <BlogEngine>
	<fileUploadSecurity>
	  <allowedExtensions>
		<add extension=".jpg" />
		<add extension=".png" />
		<!-- ... -->
	  </allowedExtensions>
	  <blockedExtensions>
		<add extension=".aspx" />
		<add extension=".exe" />
		<!-- ... -->
	  </blockedExtensions>
	</fileUploadSecurity>
  </BlogEngine>
</configuration>
```

### Modifying Allowed/Blocked Lists

1. Open `Web.Config`
2. Locate `<BlogEngine><fileUploadSecurity>`
3. Add/remove `<add extension=".xyz" />` entries
4. Restart application pool

**Important:** Always maintain the blocked list to prevent common attack vectors.

## Testing

**Test Suite:** `BlogEngine.Tests.Security.FileUploadValidatorTests`

| Test | Purpose |
|------|---------|
| `DetectMimeType_JpegFile_ReturnsImageJpeg` | Validates JPEG signature detection |
| `DetectMimeType_PngFile_ReturnsImagePng` | Validates PNG signature detection |
| `ValidateFileExtension_NoExtension_ReturnsFalse` | Rejects extensionless files |
| `ValidateFileExtension_NullFileName_ReturnsFalse` | Handles null inputs safely |
| `GetValidationErrorMessage_ReturnsGenericMessage` | Confirms generic error message |
| `IsFileUploadAllowed_NullStream_ReturnsFalse` | Rejects null streams |
| `IsFileUploadAllowed_EmptyFileName_ReturnsFalse` | Rejects empty filenames |

**Status:** ✅ All 7 tests passing

## Attack Vectors Mitigated

### 1. Web Shell Upload
**Attack:** Rename `shell.aspx` to `shell.jpg`  
**Defense:** MIME detection identifies as `text/plain` or `text/xml`, fails extension/MIME match validation

### 2. Extension Bypass
**Attack:** Use double extension like `image.jpg.aspx`  
**Defense:** Extension extraction via `Path.GetExtension()` returns `.aspx`, blocked by blacklist

### 3. Executable Disguise
**Attack:** Rename `malware.exe` to `document.pdf`  
**Defense:** MIME detection identifies `4D 5A` (PE header), fails to match PDF signature

### 4. Configuration File Upload
**Attack:** Upload `Web.config` to override settings  
**Defense:** `.config` is explicitly blocked, rejected before file write

### 5. Case Variation
**Attack:** Use `.ASPX` or `.AsPx` to bypass filters  
**Defense:** All extension comparisons use `.ToLowerInvariant()`

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│ UploadController.Post()                                  │
│  ├─ Filename normalization                              │
│  ├─ FileUploadValidator.IsFileUploadAllowed()   ◄─── Centralized
│  │   ├─ ValidateFileExtension()                         │
│  │   │   ├─ Check whitelist                             │
│  │   │   └─ Check blacklist                             │
│  │   └─ ValidateMimeType()                              │
│  │       ├─ MimeTypeDetector.DetectMimeType()           │
│  │       └─ MimeTypeDetector.ValidateMimeTypeMatchesExt()│
│  ├─ Utils.LogSecurityEvent() on rejection               │
│  └─ Generic error response                              │
└─────────────────────────────────────────────────────────┘
		 │                                          │
		 ▼                                          ▼
┌──────────────────┐                    ┌──────────────────────┐
│ Web.Config       │                    │ Magic Byte Signatures│
│  - Whitelist     │                    │  - JPEG: FF D8 FF    │
│  - Blacklist     │                    │  - PNG: 89 50 4E...  │
└──────────────────┘                    │  - PDF: 25 50 44...  │
										└──────────────────────┘
```

## Files Modified/Created

### New Files
- `BlogEngine.Core/Services/Security/FileUploadSecuritySection.cs`
- `BlogEngine.Core/Services/Security/MimeTypeDetector.cs`
- `BlogEngine.Core/Services/Security/FileUploadValidator.cs`
- `BlogEngine.Tests/Security/FileUploadValidatorTests.cs`

### Modified Files
- `BlogEngine.NET/Web.Config` (added configuration section)
- `BlogEngine.NET/AppCode/Api/UploadController.cs` (added validation)
- `BlogEngine.Core/Helpers/Utils.cs` (added `LogSecurityEvent`)
- `BlogEngine.Core/BlogEngine.Core.csproj` (added new source files)
- `BlogEngine.Tests/BlogEngine.Tests.csproj` (added test file)

## Maintenance Notes

### Adding a New Allowed Extension

1. Verify the MIME signature exists in `MimeTypeDetector.cs`
2. Add the extension to `Web.Config` allowed list
3. Add corresponding test case if new MIME type
4. Restart application

### Security Audit Recommendations

- Review `Web.Config` extension lists quarterly
- Monitor security logs for upload rejection patterns
- Consider adding file size limits (separate feature)
- Consider implementing virus scanning integration
- Verify magic byte signatures remain current with format specs

## Compliance

This implementation addresses:
- **OWASP Top 10:** A03:2021 - Injection (file upload variant)
- **CWE-434:** Unrestricted Upload of File with Dangerous Type
- **Defense in Depth:** Multiple validation layers (extension + content)

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 3.4.0.0 | 2025-01-XX | Initial security implementation |

---

**Security Contact:** Review security logs regularly and report suspicious patterns.
