# Troubleshooting: SEO & GEO Settings Not Showing in Admin UI

## Problem
When clicking on "SEO & GEO" in the Settings menu, the content area appears blank or empty.

## Root Causes & Solutions

### 1. Angular Routing Not Loading Template

**Symptoms:**
- Menu item highlights correctly
- URL changes to `#!/settings/seo`
- Content area is blank
- Browser console shows no errors

**Check:**
Open browser Developer Tools (F12) and look at the Console tab for the diagnostic messages:
```javascript
=== POST-BUNDLE DIAGNOSTIC ===
Angular loaded? true
blogAdmin module exists? true
Angular is bootstrapped!
Current $location.url(): /settings/seo
```

**If Angular is NOT bootstrapped:**
1. Check that `~/scripts/blogadmin` bundle is loading
2. Verify no JavaScript errors in console
3. Clear browser cache and try again

**Solution:**
Add additional console logging to verify the route and template:

```javascript
// Add to index.cshtml after line 30
console.log("Testing route resolution...");
setTimeout(function() {
	var injector = angular.element('[ng-app]').injector();
	if (injector) {
		var $route = injector.get('$route');
		console.log("Current route:", $route.current);
		console.log("Template URL:", $route.current ? $route.current.loadedTemplateUrl : 'NONE');
	}
}, 2000);
```

### 2. Template File Not Loading (404 Error)

**Check:**
1. Open Developer Tools → Network tab
2. Navigate to Settings → SEO & GEO
3. Look for a request to `app/settings/seoView.html`
4. Check if it returns 404 or 500 error

**Solution:**
- Verify file exists at: `BlogEngine.NET/admin/app/settings/seoView.html`
- Check IIS/web server has read permissions on the file
- Ensure no URL rewrite rules are blocking `.html` files

### 3. CSS Display Issue

**Symptoms:**
- Content loads but is not visible
- Inspecting DOM shows elements are present
- Elements have `display:none` or `opacity:0`

**Check:**
In Developer Tools:
1. Go to Elements/Inspector tab
2. Find the `<div ng-view>` element
3. Check its computed styles
4. Look for any `display:none`, `visibility:hidden`, or `height:0`

**Solution:**
Add temporary CSS override:

```css
/* Add to layout.cshtml or styles.css */
#ng-view {
	display: block !important;
	visibility: visible !important;
	opacity: 1 !important;
	min-height: 400px !important;
}

.settings-view {
	display: block !important;
	visibility: visible !important;
}
```

### 4. Z-Index / Layering Issue

**Check:**
- Other elements might be covering the content
- Check if sidebar or overlay is on top

**Solution:**
```css
#ng-view {
	position: relative;
	z-index: 1;
}
```

### 5. Angular Template Cache Issue

**Symptoms:**
- Works in other browsers but not current one
- Worked before but stopped working

**Solution:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Do a hard refresh (Ctrl+F5)
3. Try in incognito/private browsing mode
4. Disable browser extensions temporarily

### 6. Controller Not Initializing

**Check:**
Add console logging to settingController.js at the top:

```javascript
angular.module('blogAdmin').controller('SettingsController', [...function(...) {
	console.log("=== SettingsController initialized ===");
	console.log("Scope:", $scope);
	console.log("Location:", $location.url());

	// ... rest of controller code
```

**Solution:**
- Verify `settingController.js` is included in the bundle (BlogEngineConfig.cs line 155)
- Check for JavaScript syntax errors in the controller file
- Ensure no conflicting controller names

### 7. Data Loading Error

**Symptoms:**
- White screen appears
- Console shows API errors
- Network tab shows 401/403/500 on `/api/settings` or `/api/lookups`

**Check:**
1. Network tab → Filter by XHR
2. Look for requests to `/api/settings` and `/api/lookups`
3. Check response status and body

**Solution:**
- Verify user has `AccessAdminSettingsPages` permission
- Check `SettingsController.cs` Web API controller is working
- Test API endpoint directly: `http://yoursite/api/settings`
- Check server logs for exceptions

## Quick Diagnostic Checklist

Run through these steps in order:

1. ✅ **Menu item exists** - Check sidebar.cshtml line 90
2. ✅ **Route is configured** - Check app.js line 72
3. ✅ **Template file exists** - Check `admin/app/settings/seoView.html`
4. ✅ **Angular is loaded** - Check console for "Angular loaded? true"
5. ✅ **No 404 errors** - Check Network tab for seoView.html
6. ✅ **No API errors** - Check Network tab for /api/settings
7. ✅ **Controller bundled** - Check BlogEngineConfig.cs line 155
8. ✅ **ng-view is visible** - Inspect element in DOM

## Recommended Debugging Steps

### Step 1: Add Logging to seoView.html

Add this at the very top of `seoView.html`:

```html
<div style="background:yellow;padding:20px;border:3px solid red;">
	<h1>SEO VIEW LOADED!</h1>
	<p>If you see this, the template is loading correctly.</p>
</div>
```

### Step 2: Add Logging to Controller

Add this right after `$scope.load = function() {` in settingController.js:

```javascript
$scope.load = function () {
	console.log("=== LOAD FUNCTION CALLED ===");
	console.log("Scope:", $scope);
	spinOn();
	// ... rest of function
```

### Step 3: Check Route Resolution

Add this diagnostic script to index.cshtml:

```javascript
<script>
setTimeout(function() {
	// Check if route exists
	var injector = angular.element('[ng-app]').injector();
	if (injector) {
		var $route = injector.get('$route');
		var $location = injector.get('$location');

		console.log("Current location:", $location.path());
		console.log("Current route:", $route.current);

		if (!$route.current) {
			console.error("NO ROUTE MATCHED!");
			console.log("Available routes:", Object.keys($route.routes));
		} else if (!$route.current.loadedTemplateUrl) {
			console.error("TEMPLATE NOT LOADED!");
			console.log("Template URL:", $route.current.templateUrl);
		} else {
			console.log("✓ Route and template loaded successfully");
		}
	}
}, 3000);
</script>
```

## Most Likely Fix

Based on common issues with AngularJS routing in ASP.NET applications, the most likely cause is **template caching or URL resolution**.

### Immediate Fix to Try:

1. **Add cache buster to route** in `app.js`:

```javascript
// Change line 72 from:
.when("/settings/seo", { templateUrl: "app/settings/seoView.html" })

// To:
.when("/settings/seo", { templateUrl: "app/settings/seoView.html" + cacheBuster })
```

2. **Restart IIS/Application Pool** to clear server-side caching

3. **Clear browser cache** completely and test in incognito mode

4. **Check IIS MIME types** - Ensure `.html` files are served with `text/html` MIME type

## Still Not Working?

If none of the above solutions work, please provide:
1. Browser console output (including all diagnostic messages)
2. Network tab output (filtered for `seoView.html` and `/api/settings`)
3. Any red errors in the console
4. Screenshot of the blank area with DevTools open

This will help identify the specific root cause.
