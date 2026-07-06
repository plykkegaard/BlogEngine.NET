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
