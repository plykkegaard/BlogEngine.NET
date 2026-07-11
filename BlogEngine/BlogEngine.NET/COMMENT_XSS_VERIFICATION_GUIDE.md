# Comment XSS Remediation - Manual Verification Guide

**Date:** 2026-01-XX  
**Target:** BlogEngine.NET Comment Rendering  
**Vulnerability Fixed:** Stored XSS in Comment Author Names and Website URLs

---

## Quick Verification Checklist

Use this checklist to verify the XSS remediation is working correctly:

### ✅ Pre-Testing Verification

1. **Build Status:** ✓ Build successful
2. **Unit Tests:** ✓ All 66 tests passing
3. **Code Changes Applied:**
   - ✓ CommentViewBase.cs - Added EncodedAuthor and SafeWebsiteUrl properties
   - ✓ Standard-2017\CommentView.ascx - Updated to use safe properties
   - ✓ Standard\CommentView.ascx - Updated to use safe properties
   - ✓ SimpleBlue\CommentView.ascx - Updated to use safe properties
   - ✓ Defaults\CommentView.ascx - Updated to use safe properties

---

## Manual Testing Steps

### Test 1: Comment Author XSS - Script Tag

**Payload:**
```
Author Name: <script>alert('XSS')</script>
Email: test@example.com
Website: (leave empty)
Comment: Test comment
```

**Expected Result:**
- ✓ Author name displays as: `<script>alert('XSS')</script>` (literal text)
- ✓ No JavaScript alert appears
- ✓ View page source shows: `&lt;script&gt;alert('XSS')&lt;/script&gt;`
- ✓ Comment displays normally

**How to Verify:**
1. Submit the comment on any blog post
2. Refresh the page
3. Open browser DevTools (F12) → Console tab
4. Confirm no JavaScript errors or alerts
5. Right-click author name → Inspect Element
6. Verify HTML shows encoded entities

---

### Test 2: Comment Author XSS - Attribute Injection

**Payload:**
```
Author Name: "><img src=x onerror="alert('XSS')">
Email: test@example.com
Website: (leave empty)
Comment: Test comment 2
```

**Expected Result:**
- ✓ Author name displays as literal text with all characters visible
- ✓ No image element created
- ✓ No JavaScript alert appears
- ✓ HTML source shows proper encoding of quotes and angle brackets

---

### Test 3: Comment Website - JavaScript Protocol

**Payload:**
```
Author Name: John Doe
Email: test@example.com
Website: javascript:alert('XSS')
Comment: Test comment 3
```

**Expected Result:**
- ✓ Author name displays as plain text (NOT a clickable link)
- ✓ No `<a>` tag created for the author name
- ✓ Author name is wrapped in `<span>` instead of `<a>`
- ✓ No JavaScript executes when clicking the author name

**How to Verify:**
1. Submit the comment
2. Inspect the rendered HTML for the author name
3. Confirm it's a `<span>` element, not an `<a href="javascript:...">` element

---

### Test 4: Comment Website - Data URI

**Payload:**
```
Author Name: Jane Doe
Email: test@example.com
Website: data:text/html,<script>alert('XSS')</script>
Comment: Test comment 4
```

**Expected Result:**
- ✓ Author name displays as plain text (NOT a clickable link)
- ✓ No link created due to dangerous protocol
- ✓ SafeWebsiteUrl returns empty string, rendering `<span>` instead

---

### Test 5: Comment Website - Valid HTTP URL

**Payload:**
```
Author Name: Alice Smith
Email: alice@example.com
Website: http://example.com
Comment: Test comment 5
```

**Expected Result:**
- ✓ Author name displays as a clickable blue link
- ✓ Link has `rel="nofollow"` attribute
- ✓ Clicking link opens http://example.com in new browser context
- ✓ HTML shows: `<a href="http://example.com" rel="nofollow" class="url fn">Alice Smith</a>`

---

### Test 6: Comment Website - Valid HTTPS URL

**Payload:**
```
Author Name: Bob Johnson
Email: bob@example.com
Website: https://secure-example.com
Comment: Test comment 6
```

**Expected Result:**
- ✓ Author name displays as a clickable link
- ✓ Link works correctly and opens HTTPS site
- ✓ All security attributes present (`rel="nofollow"`)

---

### Test 7: Comment with Special Characters

**Payload:**
```
Author Name: Test<>&"'User名前
Email: test@example.com
Website: (leave empty)
Comment: Testing special characters
```

**Expected Result:**
- ✓ All special characters display correctly
- ✓ HTML entities properly encoded:
  - `<` → `&lt;`
  - `>` → `&gt;`
  - `&` → `&amp;`
  - `"` → `&quot;`
- ✓ Unicode characters (名前) display correctly
- ✓ No attribute injection or tag breaking

---

## Browser Testing Matrix

Test in multiple browsers to ensure cross-browser compatibility:

| Browser | Version | Test 1 | Test 2 | Test 3 | Test 4 | Test 5 | Test 6 |
|---------|---------|--------|--------|--------|--------|--------|--------|
| Chrome  | Latest  | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     |
| Firefox | Latest  | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     |
| Edge    | Latest  | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     |
| Safari  | Latest  | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     | ⬜     |

---

## Automated Verification (Optional)

For automated testing, consider using:

### Selenium WebDriver Test
```csharp
[Test]
public void Comment_WithScriptTag_ShouldNotExecuteJavaScript()
{
	// Navigate to post
	driver.Navigate().GoToUrl("http://localhost/post.aspx?id=test-post");

	// Submit comment with XSS payload
	driver.FindElement(By.Id("txtAuthor")).SendKeys("<script>alert('XSS')</script>");
	driver.FindElement(By.Id("txtEmail")).SendKeys("test@example.com");
	driver.FindElement(By.Id("txtContent")).SendKeys("Test comment");
	driver.FindElement(By.Id("btnSubmit")).Click();

	// Wait for page reload
	Thread.Sleep(1000);

	// Verify no alert present
	Assert.IsFalse(IsAlertPresent());

	// Verify author name is encoded
	var authorElement = driver.FindElement(By.ClassName("fn"));
	var authorHtml = authorElement.GetAttribute("innerHTML");
	Assert.IsTrue(authorHtml.Contains("&lt;script&gt;"));
}
```

### ZAP/OWASP Security Scanner
1. Configure OWASP ZAP to spider the blog
2. Run Active Scan with XSS rules enabled
3. Submit comments with XSS payloads via automated form submission
4. Verify no stored XSS vulnerabilities reported

---

## Verification Complete Criteria

Mark verification as complete when:

- ✅ All 7 manual test cases pass in at least 2 browsers
- ✅ No JavaScript alerts or errors appear
- ✅ HTML source inspection confirms proper encoding
- ✅ Valid HTTP/HTTPS URLs work as expected
- ✅ Dangerous protocols (javascript:, data:, vbscript:) are blocked
- ✅ User experience is normal for legitimate comments
- ✅ No regressions in comment display or functionality

---

## Troubleshooting

### Issue: JavaScript still executing
- Verify theme is using updated CommentView.ascx file
- Clear browser cache and application pool
- Rebuild solution and restart IIS/web server
- Check that `EncodedAuthor` property is being called in template

### Issue: Valid URLs not working
- Inspect HTML source to verify `SafeWebsiteUrl` is returning the URL
- Check that URL starts with `http://` or `https://`
- Verify protocol validation logic in CommentViewBase.cs

### Issue: HTML encoding visible as text
- This is expected and correct behavior
- Malicious scripts should display as literal text, not execute
- Use View Source to confirm encoding entities are present

---

## Sign-Off

**Tested By:** _________________  
**Date:** _________________  
**Result:** ⬜ PASS  ⬜ FAIL  
**Notes:**

---

## Related Documentation

- `BlogEngine\BlogEngine.NET\XSS_TEST_CASES.md` - Comprehensive test case documentation
- `BlogEngine\SECURITY_IMPLEMENTATION.md` - File upload security implementation
- `BlogEngine\BlogEngine.NET\XSS_IMPLEMENTATION_SUMMARY.md` - Post XSS protection summary

