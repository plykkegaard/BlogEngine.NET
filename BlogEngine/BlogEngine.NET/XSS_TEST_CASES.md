# XSS Test Cases for BlogEngine.NET PostList

## Overview
This document outlines manual test cases to verify that XSS vulnerabilities have been mitigated in the post rendering pipeline.

## Test Case 1: XSS in Post Title

**Setup:**
- Create a new post with the following title: `<script>alert('XSS in Title')</script>`

**Expected Behavior:**
- The title should be displayed as literal text: `<script>alert('XSS in Title')</script>`
- No JavaScript alert should appear
- The HTML should show: `&lt;script&gt;alert('XSS in Title')&lt;/script&gt;`

**Files Updated for Protection:**
- `BlogEngine.NET\Custom\Controls\Defaults\PostView.ascx` - Uses `Utils.HtmlEncode(Post.Title)`
- `BlogEngine.NET\Custom\Themes\Standard-2017\PostView.ascx` - Uses `Utils.HtmlEncode(Post.Title)`
- `BlogEngine.NET\Custom\Themes\Standard\PostView.ascx` - Uses `Utils.HtmlEncode(Post.Title)`

---

## Test Case 2: XSS in Post Author Name

**Setup:**
- Create a post with author name: `<img src=x onerror="alert('XSS in Author')">`

**Expected Behavior:**
- The author name should be displayed as literal text
- No image element should load or trigger an error handler
- The HTML should show the encoded version: `&lt;img src=x onerror="alert('XSS in Author')"&gt;`

**Files Updated for Protection:**
- `BlogEngine.NET\Custom\Themes\Standard-2017\PostView.ascx` - Uses `Utils.HtmlEncode()` for author display
- `BlogEngine.NET\Custom\Themes\Standard\PostView.ascx` - Uses `Utils.HtmlEncode()` for author display

---

## Test Case 3: XSS in Category Name

**Setup:**
- Create a category named: `javascript:alert('XSS in Category')`
- Add a post to this category

**Expected Behavior:**
- The category link should display as plain text
- No JavaScript URL should execute
- The category title should be HTML-encoded in the generated link

**Files Updated for Protection:**
- `BlogEngine.Core\Web\Controls\PostViewBase.cs` - `CategoryLinks()` method now encodes category titles with `Utils.HtmlEncode(c.Title)`

---

## Test Case 4: XSS via Query String in Pagination

**Setup:**
- Navigate to post list with URL: `?page=1"><script>alert('XSS')</script>`

**Expected Behavior:**
- The pagination links should not execute injected JavaScript
- The raw URL should be HTML-encoded before use in href attributes
- No script execution should occur

**Files Updated for Protection:**
- `BlogEngine.NET\Custom\Controls\PostList.ascx.cs` - `InitPaging()` method now encodes `Request.RawUrl` with `Utils.HtmlEncode()`

---

## Test Case 5: XSS in Page Number Parameter

**Setup:**
- Navigate to: `?page=<img src=x onerror="alert('XSS')">`

**Expected Behavior:**
- Page parameter should be safely parsed (int.TryParse will fail for non-numeric)
- No image should render or trigger error handler
- Safe fallback to default page index

**Files Updated for Protection:**
- `BlogEngine.NET\Custom\Controls\PostList.ascx.cs` - Uses `int.TryParse()` for validation

---

## Test Case 6: XSS in Theme Parameter

**Setup:**
- Navigate to: `?theme=<svg onload="alert('XSS')">`

**Expected Behavior:**
- Theme parameter is sanitized using `.SanitizePath()` extension method (existing protection)
- Invalid theme characters are stripped
- Falls back to default theme

**Files with Existing Protection:**
- `BlogEngine.NET\Custom\Controls\PostList.ascx.cs` - Line 126 uses `theme.SanitizePath()`

---

## Implementation Summary

### Core Utility Methods Added
- `Utils.HtmlEncode(string text)` - Wraps `HttpUtility.HtmlEncode()` with null safety
- `Utils.UrlEncode(string text)` - Wraps `HttpUtility.UrlEncode()` with null safety
- `Utils.AttributeEncode(string text)` - Wraps `HttpUtility.HtmlEncode()` for attribute encoding with null safety

### Files Modified for XSS Protection

1. **BlogEngine.Core\Helpers\Utils.cs**
   - Added three encoding utility methods for consistent XSS protection across the application

2. **BlogEngine.Core\Web\Controls\PostViewBase.cs**
   - Updated `CategoryLinks()` method to encode category titles
   - Added documentation clarifying XSS encoding strategy

3. **BlogEngine.NET\Custom\Controls\PostList.ascx.cs**
   - Updated `InitPaging()` method to HTML-encode `Request.RawUrl`
   - Added explanatory comments about XSS prevention

4. **BlogEngine.NET\Custom\Controls\Defaults\PostView.ascx**
   - Changed from `Server.HtmlEncode()` to `Utils.HtmlEncode()` for consistency
   - Added BlogEngine.Core namespace import

5. **BlogEngine.NET\Custom\Themes\Standard-2017\PostView.ascx**
   - Updated Post.Title encoding to use `Utils.HtmlEncode()`
   - Updated author display to use `Utils.HtmlEncode()` instead of just `RemoveIllegalCharacters()`

6. **BlogEngine.NET\Custom\Themes\Standard\PostView.ascx**
   - Updated Post.Title encoding to use `Utils.HtmlEncode()`
   - Updated author name encoding to use `Utils.HtmlEncode()`

---

## Encoding Strategy

- **HTML Content**: Post titles, author names, and category names are encoded at the template/display layer
- **Post Body Content**: Remains unencoded (templates may intentionally render formatted HTML)
- **URLs**: Pagination URLs are HTML-encoded to prevent XSS in href attributes
- **Defense in Depth**: Encoding applied at output layer (templates) ensures protection for all data sources

## Testing Instructions

1. Build the solution successfully (verified ✓)
2. Deploy to test environment or run locally
3. Follow each test case above
4. Verify that no JavaScript execution occurs
5. Verify that content displays correctly as literal text
6. Test with multiple post list scenarios (categories, tags, author pages)

---

## Notes

- `HttpUtility.HtmlEncode()` is the standard ASP.NET encoding mechanism for XSS protection in .NET Framework 4.8
- This implementation maintains backward compatibility with existing code patterns
- All encoding is null-safe with empty string fallback
- Existing `Server.HtmlEncode()` in PostViewBase and themes now replaced with `Utils.HtmlEncode()` for consistency

---

# Comment Rendering XSS Test Cases

## Test Case 6: XSS in Comment Author Name

**Setup:**
- Submit a comment with the following author name:
  ```
  <script>alert('XSS in Comment Author')</script>
  ```

**Expected Behavior:**
- The author name should be displayed as literal text
- No JavaScript alert should appear
- The HTML source should show: `&lt;script&gt;alert('XSS in Comment Author')&lt;/script&gt;`

**Files Updated for Protection:**
- `BlogEngine.Core\Web\Controls\CommentViewBase.cs` - Added `EncodedAuthor` property using `Utils.HtmlEncode()`
- `BlogEngine.NET\Custom\Themes\Standard-2017\CommentView.ascx` - Uses `<%=EncodedAuthor%>`
- `BlogEngine.NET\Custom\Themes\Standard\CommentView.ascx` - Uses `<%=EncodedAuthor%>`
- `BlogEngine.NET\Custom\Themes\SimpleBlue\CommentView.ascx` - Uses `<%=EncodedAuthor%>`
- `BlogEngine.NET\Custom\Controls\Defaults\CommentView.ascx` - Uses `<%=EncodedAuthor%>`

**Vulnerability Type:** Stored XSS (High Severity)

---

## Test Case 7: XSS via Comment Author Name - Attribute Injection

**Setup:**
- Submit a comment with author name:
  ```
  "><img src=x onerror="alert('XSS')">
  ```

**Expected Behavior:**
- The author name should be displayed as plain text with all characters encoded
- No image element should be created
- The HTML source should show: `&quot;&gt;&lt;img src=x onerror=&quot;alert('XSS')&quot;&gt;`

**Files Updated for Protection:**
- Same as Test Case 6

**Vulnerability Type:** Stored XSS (High Severity)

---

## Test Case 8: XSS via Comment Website - JavaScript Protocol

**Setup:**
- Submit a comment with the following website URL:
  ```
  javascript:alert('XSS in Website')
  ```

**Expected Behavior:**
- The comment author name should be displayed as plain text (no link)
- No `<a>` tag should be created for the website
- The `SafeWebsiteUrl` property should return empty string, causing the template to render a `<span>` instead of an `<a>` element

**Files Updated for Protection:**
- `BlogEngine.Core\Web\Controls\CommentViewBase.cs` - Added `SafeWebsiteUrl` property with protocol validation
- All CommentView.ascx templates updated to check `SafeWebsiteUrl` before rendering link

**Blocked Protocols:**
- `javascript:`
- `data:`
- `vbscript:`
- `file:`

**Allowed Protocols:**
- `http://`
- `https://`
- `/` (relative URLs)

**Vulnerability Type:** Stored XSS (High Severity)

---

## Test Case 9: XSS via Comment Website - Data URI

**Setup:**
- Submit a comment with website URL:
  ```
  data:text/html,<script>alert('XSS')</script>
  ```

**Expected Behavior:**
- The website URL should be rejected by `SafeWebsiteUrl`
- No link should be rendered
- Author name displayed as plain text without hyperlink

**Files Updated for Protection:**
- Same as Test Case 8

**Vulnerability Type:** Stored XSS (High Severity)

---

## Test Case 10: XSS via Comment Website - VBScript Protocol

**Setup:**
- Submit a comment with website URL:
  ```
  vbscript:msgbox("XSS")
  ```

**Expected Behavior:**
- The website URL should be rejected by `SafeWebsiteUrl`
- No link should be rendered
- Author name displayed as plain text without hyperlink

**Files Updated for Protection:**
- Same as Test Case 8

**Vulnerability Type:** Stored XSS (High Severity)

---

## Test Case 11: Valid Comment Website - HTTP

**Setup:**
- Submit a comment with a valid HTTP website:
  ```
  http://example.com
  ```

**Expected Behavior:**
- The author name should be displayed as a clickable hyperlink
- The URL should be HTML attribute-encoded in the href attribute
- Link should have `rel="nofollow"` attribute
- Link opens to `http://example.com`

**Verification:**
- Inspect HTML source to confirm proper encoding and attributes

---

## Test Case 12: Valid Comment Website - HTTPS

**Setup:**
- Submit a comment with a valid HTTPS website:
  ```
  https://secure-example.com
  ```

**Expected Behavior:**
- Same as Test Case 11, but with HTTPS URL
- Link should work correctly and securely

---

## Test Case 13: Comment Author Name - Unicode/Special Characters

**Setup:**
- Submit a comment with author name containing special characters:
  ```
  Author名前<>&"'
  ```

**Expected Behavior:**
- All special characters should be HTML-encoded
- `<` becomes `&lt;`
- `>` becomes `&gt;`
- `&` becomes `&amp;`
- `"` becomes `&quot;`
- Unicode characters display correctly
- No script execution or attribute injection

---

## Manual Testing Procedure

### Setup
1. Navigate to a blog post page that accepts comments
2. Fill out the comment form with test payloads
3. Submit the comment
4. Refresh the page to view the rendered comment

### Validation Steps
1. **Visual Inspection:** Verify malicious scripts are displayed as text, not executed
2. **Browser DevTools:** Inspect HTML source to confirm encoding is applied
3. **Browser Console:** Check for JavaScript errors or unexpected script execution
4. **Network Tab:** Verify no unexpected network requests from injected scripts

### Success Criteria
- ✅ No JavaScript alerts or dialogs appear
- ✅ HTML source shows encoded entities (`&lt;`, `&gt;`, `&quot;`, etc.)
- ✅ No script tags or event handlers execute
- ✅ Malicious URLs are blocked (no href created for dangerous protocols)
- ✅ Valid HTTP/HTTPS URLs work correctly
- ✅ User experience remains normal for legitimate comments

---

## Automated Testing Recommendations

Consider adding unit tests for:

1. **CommentViewBase.EncodedAuthor**
   - Null/empty comment author
   - Author with `<script>` tags
   - Author with HTML entities
   - Author with quotes and special characters

2. **CommentViewBase.SafeWebsiteUrl**
   - Null website (should return empty string)
   - `javascript:` protocol (should return empty string)
   - `data:` protocol (should return empty string)
   - `vbscript:` protocol (should return empty string)
   - `file:` protocol (should return empty string)
   - `http://example.com` (should return encoded URL)
   - `https://example.com` (should return encoded URL)
   - Relative URL `/path` (should return encoded URL)
   - URL with special characters needing attribute encoding

**Test Location:** `BlogEngine.Tests\Security\CommentViewSecurityTests.cs` (to be created)

---

## Remediation Summary

**Vulnerability:** Stored XSS in comment author names and website URLs  
**Severity:** High  
**CWE:** CWE-79 (Cross-Site Scripting)  

**Root Cause:**  
Comment templates rendered `Comment.Author` and `Comment.Website` directly in HTML without encoding, allowing malicious scripts stored in the database to execute in visitors' browsers.

**Remediation:**
1. Created `EncodedAuthor` property in `CommentViewBase.cs` with HTML encoding
2. Created `SafeWebsiteUrl` property with URL protocol validation
3. Updated all theme CommentView.ascx templates to use safe properties
4. Blocked dangerous protocols: `javascript:`, `data:`, `vbscript:`, `file:`
5. Applied HTML attribute encoding to URLs before output

**Impact:**  
All comment XSS attack vectors have been mitigated. Legitimate comments display correctly while malicious payloads are neutralized.

