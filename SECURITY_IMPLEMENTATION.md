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

---

# Security Headers Implementation

**Version:** 3.4.0.0  
**Date:** 2025-01-XX  
**Status:** ✅ Implemented & Tested

## Overview

BlogEngine.NET now includes modern HTTP security headers to protect against common web vulnerabilities including Cross-Site Scripting (XSS), clickjacking, MIME-sniffing attacks, and information leakage through referrer headers.

## Security Headers

### 1. Content-Security-Policy (CSP)

**Purpose:** Prevents XSS attacks by controlling which scripts can be executed on the page.

**Default Value:** `script-src 'self'`

**Protection:**
- Only allows scripts from the same origin (blocks inline scripts and external CDNs by default)
- Prevents execution of injected malicious scripts
- Mitigates XSS attack surface significantly

**Configuration Example:**
```xml
<securityHeaders 
    enableContentSecurityPolicy="true" 
    contentSecurityPolicy="script-src 'self'" />
```

**Advanced Configuration:**
```xml
<!-- Allow specific CDNs -->
<securityHeaders 
    contentSecurityPolicy="script-src 'self' https://cdn.jsdelivr.net https://ajax.microsoft.com" />

<!-- Allow inline scripts (not recommended) -->
<securityHeaders 
    contentSecurityPolicy="script-src 'self' 'unsafe-inline'" />
```

### 2. X-Frame-Options

**Purpose:** Prevents clickjacking attacks by controlling whether the page can be embedded in iframes.

**Default Value:** `DENY`

**Protection:**
- Prevents any domain from framing the content
- Blocks UI redress attacks where attackers trick users into clicking hidden elements
- Protects against clickjacking, likejacking, and cursorjacking

**Configuration Example:**
```xml
<securityHeaders 
    enableXFrameOptions="true" 
    xFrameOptions="DENY" />
```

**Alternative Values:**
- `DENY` - No domain can frame the content (recommended)
- `SAMEORIGIN` - Only same origin can frame the content

### 3. X-Content-Type-Options

**Purpose:** Prevents browsers from MIME-sniffing a response away from the declared content-type.

**Default Value:** `nosniff` (always on when enabled)

**Protection:**
- Prevents drive-by download attacks
- Stops browsers from interpreting files as a different MIME type than declared
- Reduces risk of malicious file execution

**Configuration Example:**
```xml
<securityHeaders 
    enableXContentTypeOptions="true" />
```

### 4. Referrer-Policy

**Purpose:** Controls how much referrer information should be included with requests.

**Default Value:** `strict-origin-when-cross-origin`

**Protection:**
- Prevents sensitive information leakage through URL parameters
- Protects user privacy by limiting referrer data sent to third parties
- Balances security with functionality (maintains analytics capabilities)

**Behavior:**
- **Same-origin requests:** Full URL sent (including path and query string)
- **Cross-origin HTTPS → HTTPS:** Only origin sent (no path/query)
- **HTTPS → HTTP:** No referrer sent (prevents downgrade leakage)

**Configuration Example:**
```xml
<securityHeaders 
    enableReferrerPolicy="true" 
    referrerPolicy="strict-origin-when-cross-origin" />
```

**Alternative Values:**
- `no-referrer` - Never send referrer (most private)
- `same-origin` - Send referrer only for same-origin requests
- `strict-origin` - Send only origin, no referrer on HTTPS → HTTP downgrade
- `no-referrer-when-downgrade` - Send full referrer unless downgrade occurs

## Implementation Architecture

### HttpModule Pattern

**Component:** `SecurityHeadersModule.cs`

All security headers are applied via a single HttpModule that hooks into the ASP.NET request pipeline:

```csharp
public sealed class SecurityHeadersModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        context.EndRequest += OnEndRequest;
    }

    private static void OnEndRequest(object sender, EventArgs e)
    {
        // Add security headers to response based on configuration
    }
}
```

**Pipeline Integration:**
- Registered in `web.config` under `<httpModules>`
- Executes on every request via `EndRequest` event
- Headers added before response is sent to client
- Safe header addition (checks for existing headers)

### Configuration Section

**Component:** `SecurityHeadersSection.cs`

Type-safe configuration class reads settings from `web.config`:

```csharp
public class SecurityHeadersSection : ConfigurationSection
{
    [ConfigurationProperty("enableContentSecurityPolicy", DefaultValue = true)]
    public bool EnableContentSecurityPolicy { get; set; }

    [ConfigurationProperty("contentSecurityPolicy", DefaultValue = "script-src 'self'")]
    public string ContentSecurityPolicy { get; set; }

    // Additional properties for other headers...
}
```

**Location:** `Web.Config` → `BlogEngine/securityHeaders`

### Default Configuration

```xml
<BlogEngine>
  <securityHeaders 
    enableContentSecurityPolicy="true" 
    contentSecurityPolicy="script-src 'self'" 
    enableXFrameOptions="true" 
    xFrameOptions="DENY" 
    enableXContentTypeOptions="true" 
    enableReferrerPolicy="true" 
    referrerPolicy="strict-origin-when-cross-origin" />
</BlogEngine>
```

## Testing & Validation

### Unit Tests

**Location:** `BlogEngine.Tests/Security/SecurityHeadersModuleTests.cs`

Comprehensive test coverage includes:
- Default configuration values validation
- Property setters for all headers
- Enable/disable toggle functionality
- IHttpModule interface implementation
- Configuration read/write operations

### Manual Verification

**Using Browser Developer Tools:**

1. Open Developer Tools (F12)
2. Navigate to the Network tab
3. Load any page on the blog
4. Click the request and view Response Headers
5. Verify presence of:
   - `Content-Security-Policy`
   - `X-Frame-Options`
   - `X-Content-Type-Options`
   - `Referrer-Policy`

**Using curl:**

```bash
curl -I https://yourblog.com

# Expected output includes:
# Content-Security-Policy: script-src 'self'
# X-Frame-Options: DENY
# X-Content-Type-Options: nosniff
# Referrer-Policy: strict-origin-when-cross-origin
```

### Security Scanning Tools

- **Mozilla Observatory:** https://observatory.mozilla.org
- **Security Headers:** https://securityheaders.com
- **CSP Evaluator:** https://csp-evaluator.withgoogle.com

## Troubleshooting

### CSP Blocking Legitimate Scripts

**Symptom:** Console shows CSP violations for scripts that should work.

**Solutions:**
1. Add specific CDN domains to `contentSecurityPolicy`:
   ```xml
   contentSecurityPolicy="script-src 'self' https://trusted-cdn.com"
   ```
2. Temporarily use report-only mode to identify issues without blocking
3. Review and whitelist necessary external scripts

### Embedded Content Not Loading

**Symptom:** Videos, social media embeds, or iframes don't display.

**Solution:** Adjust X-Frame-Options if embedding is required:
```xml
xFrameOptions="SAMEORIGIN"
```
Or disable for specific routes (requires custom logic).

### Analytics/Tracking Issues

**Symptom:** Referrer data not reaching analytics platforms.

**Solution:** Use a less restrictive referrer policy:
```xml
referrerPolicy="no-referrer-when-downgrade"
```

## Maintenance & Customization

### Disabling Specific Headers

To disable any header, set its `enable` property to `false`:

```xml
<securityHeaders 
    enableContentSecurityPolicy="false" 
    enableXFrameOptions="true" 
    enableXContentTypeOptions="true" 
    enableReferrerPolicy="true" />
```

### Environment-Specific Configuration

Use web.config transforms for different environments:

**Web.Debug.config:**
```xml
<securityHeaders 
    contentSecurityPolicy="script-src 'self' 'unsafe-inline'" 
    xdt:Transform="SetAttributes" />
```

**Web.Release.config:**
```xml
<securityHeaders 
    contentSecurityPolicy="script-src 'self'" 
    xdt:Transform="SetAttributes" />
```

### Adding Additional Security Headers

To add more headers (e.g., `Strict-Transport-Security`):

1. Add properties to `SecurityHeadersSection.cs`
2. Update `SecurityHeadersModule.OnEndRequest` method
3. Add configuration attributes in `web.config`
4. Add corresponding unit tests

## Compliance & Standards

This implementation addresses:

- **OWASP Top 10:** A05:2021 - Security Misconfiguration
- **OWASP ASVS:** V14.4 - HTTP Security Headers
- **CWE-693:** Protection Mechanism Failure
- **CWE-1021:** Improper Restriction of Rendered UI Layers (Clickjacking)
- **CWE-79:** Cross-site Scripting (XSS) mitigation through CSP
- **Mozilla Web Security Guidelines**
- **NIST Cybersecurity Framework**

---

## Files Modified/Created

### File Upload Security - New Files
- `BlogEngine.Core/Services/Security/FileUploadSecuritySection.cs`
- `BlogEngine.Core/Services/Security/MimeTypeDetector.cs`
- `BlogEngine.Core/Services/Security/FileUploadValidator.cs`
- `BlogEngine.Tests/Security/FileUploadValidatorTests.cs`

### File Upload Security - Modified Files
- `BlogEngine.NET/Web.Config` (added fileUploadSecurity configuration section)
- `BlogEngine.NET/AppCode/Api/UploadController.cs` (added validation)
- `BlogEngine.Core/Helpers/Utils.cs` (added `LogSecurityEvent`)
- `BlogEngine.Core/BlogEngine.Core.csproj` (added new source files)
- `BlogEngine.Tests/BlogEngine.Tests.csproj` (added test file)

### Security Headers - New Files
- `BlogEngine.Core/Web/HttpModules/SecurityHeadersModule.cs`
- `BlogEngine.Core/Web/HttpModules/SecurityHeadersSection.cs`
- `BlogEngine.Tests/Security/SecurityHeadersModuleTests.cs`

### Security Headers - Modified Files
- `BlogEngine.NET/Web.Config` (added securityHeaders configuration section and module registration)
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
| 3.4.0.0 | 2025-01-XX | File upload security implementation + Security headers (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy) |

---

**Security Contact:** Review security logs regularly and report suspicious patterns.
