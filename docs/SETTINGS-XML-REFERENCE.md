# BlogEngine.NET Settings.xml Configuration Reference

## Overview

This document provides a comprehensive reference for configuring SEO (Search Engine Optimization) and GEO (Generative Engine Optimization) settings in BlogEngine.NET through the `settings.xml` configuration file.

Settings are stored as XML elements in the blog's `settings.xml` file located in the `/App_Data/` directory. Each property in the `BlogSettings` class corresponds to an XML element with the same name.

## Quick Start

The most common SEO/GEO settings to configure:

```xml
<settings>
	<!-- SEO Basics -->
	<SeoTitleSuffix> - My Blog</SeoTitleSuffix>
	<SeoCanonicalDomain>https://example.com</SeoCanonicalDomain>
	<SeoDefaultAuthor>John Doe</SeoDefaultAuthor>
	<SeoDefaultImage>https://example.com/images/blog-logo.png</SeoDefaultImage>

	<!-- Social Integration -->
	<SeoTwitterHandle>@myblog</SeoTwitterHandle>
	<SeoFacebookAppId>1234567890</SeoFacebookAppId>

	<!-- Social & Structured Data -->
	<SeoEnableOpenGraph>true</SeoEnableOpenGraph>
	<SeoEnableTwitterCard>true</SeoEnableTwitterCard>
	<SeoEnableStructuredData>true</SeoEnableStructuredData>

	<!-- Organization Schema -->
	<SeoOrganizationName>My Company</SeoOrganizationName>
	<SeoOrganizationLogo>https://example.com/images/logo.png</SeoOrganizationLogo>

	<!-- GEO (Generative Engine Optimization) -->
	<GeoOptimizationEnabled>true</GeoOptimizationEnabled>
	<GeoOptimizationMode>Standard</GeoOptimizationMode>
	<GeoMetadataRichness>Rich</GeoMetadataRichness>
	<GeoEnableCitationOptimization>true</GeoEnableCitationOptimization>
</settings>
```

---

## Detailed Settings Reference

### SEO Configuration

#### SeoTitleSuffix
- **XML Element:** `<SeoTitleSuffix>`
- **Data Type:** String
- **Default Value:** Empty
- **Example:** `</Sr> - My Blog` or ` | Company Name`
- **Description:** This suffix is appended to page titles for consistent branding across all pages. Applied when `UseBlogNameInPageTitles` is false.
- **Recommendation:** Keep it concise (under 20 characters) to avoid truncation in search engine snippets.
- **SEO Impact:** High — Improves brand consistency in search results.

---

#### SeoCanonicalDomain
- **XML Element:** `<SeoCanonicalDomain>`
- **Data Type:** String (URL)
- **Default Value:** Empty
- **Example:** `https://example.com`
- **Description:** The canonical domain should be the preferred URL for the blog, including the protocol (http:// or https://). Used to generate canonical link tags that prevent duplicate content issues in search engines.
- **Format Requirements:** 
  - Must include protocol: `https://` or `http://`
  - Should match the primary domain where your blog is hosted
  - Do NOT include trailing slash
- **SEO Impact:** Critical — Prevents duplicate content penalties from search engines.
- **Use Cases:**
  - When your blog is accessible from multiple domains (with/without www, HTTP/HTTPS)
  - To specify the preferred version for search engines and social platforms

---

#### SeoDefaultAuthor
- **XML Element:** `<SeoDefaultAuthor>`
- **Data Type:** String
- **Default Value:** Empty
- **Example:** `John Doe`
- **Description:** Used as the default author value in meta tags and structured data when post-specific author information is not available. Appears in Schema.org Article metadata and Open Graph tags.
- **SEO Impact:** Medium — Helps establish content authority and authorship.
- **Use Cases:**
  - Single-author blogs
  - Default author for guest posts without assigned authors
  - Fallback when post author is not set

---

#### SeoDefaultImage
- **XML Element:** `<SeoDefaultImage>`
- **Data Type:** String (URL)
- **Default Value:** Empty
- **Example:** `https://example.com/images/blog-logo.png`
- **Description:** This image is used in Open Graph and Twitter Card tags when content doesn't have a specific featured image. Should be an absolute URL to a high-quality image representing the blog brand.
- **Image Specifications:**
  - Recommended size: 1200x630 pixels for optimal social media display
  - Minimum size: 600x315 pixels
  - Format: JPG, PNG, or GIF
  - Must be HTTPS if your blog uses HTTPS
- **SEO Impact:** Medium — Improves social media sharing and engagement.
- **Use Cases:**
  - Blog logo or brand image
  - Default cover image for posts without featured images
  - Fallback image for social platform previews

---

### Social Media Integration

#### SeoTwitterHandle
- **XML Element:** `<SeoTwitterHandle>`
- **Data Type:** String
- **Default Value:** Empty
- **Example:** `@myblog`
- **Format Requirements:** Should include the @ symbol
- **Description:** Used in Twitter Card metadata to attribute content to the blog's Twitter account. Enables Twitter to link mentions to your account.
- **SEO Impact:** Low-Medium — Enhances social media attribution and engagement.
- **Use Cases:**
  - Single blog account attribution
  - Unified social media identity

---

#### SeoFacebookAppId
- **XML Element:** `<SeoFacebookAppId>`
- **Data Type:** String (Numeric ID)
- **Default Value:** Empty
- **Example:** `1234567890`
- **Description:** Facebook App ID enables Facebook Insights for your blog's content shared on Facebook. Provides analytics on how your content is shared and engaged with on Facebook.
- **Format Requirements:** Must be a valid Facebook App ID (numeric string)
- **How to Get:** 
  1. Go to https://developers.facebook.com/
  2. Create or select your app
  3. Find the App ID in the app dashboard
- **SEO Impact:** Low — Primarily for analytics rather than direct SEO.
- **Use Cases:**
  - Tracking social engagement on Facebook
  - Analytics and insights for shared content

---

### Social Metadata Features

#### SeoEnableOpenGraph
- **XML Element:** `<SeoEnableOpenGraph>`
- **Data Type:** Boolean (`true` or `false`)
- **Default Value:** `false`
- **Description:** Open Graph tags provide rich preview information when content is shared on social platforms like Facebook, LinkedIn, and others. Recommended for all blogs to enhance social media presence.
- **Enabled By:** Setting to `true`
- **Impact When Enabled:**
  - Posts shared on Facebook, LinkedIn, and other platforms show rich previews
  - Blog title, description, and featured image appear in the preview
  - Increases click-through rates from social shares
- **SEO Impact:** High — Improves social media traffic and engagement.
- **Best Practice:** Enable this for maximum social media presence.

---

#### SeoEnableTwitterCard
- **XML Element:** `<SeoEnableTwitterCard>`
- **Data Type:** Boolean (`true` or `false`)
- **Default Value:** `false`
- **Description:** Twitter Card tags enable rich media previews when content is shared on Twitter. Supports both summary and large image card formats based on content.
- **Enabled By:** Setting to `true`
- **Impact When Enabled:**
  - Posts shared on Twitter show rich card previews with image and description
  - Increases click-through rates from Twitter shares
  - Improves content presentation on Twitter feed
- **Card Types Supported:**
  - Summary Card (text and small image)
  - Summary Card with Large Image (text and large featured image)
- **SEO Impact:** High — Improves Twitter engagement and traffic.
- **Requirement:** Requires Twitter Card approval (usually automatic for news/blog sites).

---

### Structured Data (Schema.org)

#### SeoEnableStructuredData
- **XML Element:** `<SeoEnableStructuredData>`
- **Data Type:** Boolean (`true` or `false`)
- **Default Value:** `false`
- **Description:** Structured data (JSON-LD) helps search engines and AI systems understand content semantics. Enables rich snippets in search results and better content understanding by generative AI engines. Critical for GEO (Generative Engine Optimization).
- **Enabled By:** Setting to `true`
- **Impact When Enabled:**
  - Search engines show rich snippets with ratings, dates, and content summaries
  - Content is better understood by AI systems
  - May enable special search result features (FAQ boxes, how-to snippets, etc.)
- **Schema Types Included:**
  - `Article` schema for blog posts
  - `BlogPosting` schema with author, date published, date modified
  - `Organization` schema for blog metadata
  - `BreadcrumbList` schema for navigation
- **SEO Impact:** Very High — Critical for modern SEO and AI visibility.
- **Best Practice:** Enable this for all new blogs and content.

---

#### SeoOrganizationName
- **XML Element:** `<SeoOrganizationName>`
- **Data Type:** String
- **Default Value:** Empty
- **Example:** `My Company Inc.` or `John's Tech Blog`
- **Description:** The legal or common name of the organization publishing the blog content. Used in Schema.org Organization and Publisher schema. Enhances trust signals for search engines.
- **Used In:**
  - Schema.org `Organization` schema
  - Publisher information in article structured data
  - Knowledge panel information (Google)
- **SEO Impact:** Medium-High — Establishes organizational identity and trust.
- **Best Practice:** Use your official organization or personal brand name.

---

#### SeoOrganizationLogo
- **XML Element:** `<SeoOrganizationLogo>`
- **Data Type:** String (URL)
- **Default Value:** Empty
- **Example:** `https://example.com/images/logo.png`
- **Image Specifications:**
  - Should be a square image, minimum 112x112 pixels
  - Recommended size: 512x512 pixels or larger
  - Format: JPG, PNG, or GIF
  - Must be HTTPS if your blog uses HTTPS
- **Used In:**
  - Schema.org `Organization` schema
  - Google Knowledge panel
  - Social media rich cards
- **SEO Impact:** Medium — Improves visual recognition in search results and knowledge panels.
- **Best Practice:** Use a high-quality logo image with transparent background (PNG recommended).

---

## GEO (Generative Engine Optimization) Settings

### What is GEO?

Generative Engine Optimization (GEO) is the practice of optimizing web content for AI-powered search engines and generative AI systems like ChatGPT, Claude, Gemini, and other large language models. GEO complements traditional SEO by ensuring your content is discoverable and properly attributed when used in AI-generated responses.

### GeoOptimizationEnabled
- **XML Element:** `<GeoOptimizationEnabled>`
- **Data Type:** Boolean (`true` or `false`)
- **Default Value:** `false`
- **Description:** Generative Engine Optimization targets AI-powered search engines and content discovery systems. When enabled, additional metadata and hints are provided to help AI systems better understand and utilize the blog's content in generated responses.
- **Enabled By:** Setting to `true`
- **Impact When Enabled:**
  - AI systems receive clear hints about content purpose and quality
  - Content is more likely to be cited and referenced in AI responses
  - Additional metadata improves content discoverability by generative AI
- **Ecosystem Impact:**
  - Makes content visible to AI search engines (e.g., Perplexity AI, SearchGPT)
  - Improves likelihood of content inclusion in AI-generated summaries
  - Enhances attribution in AI-powered applications
- **Importance:** High for future-proofing your blog against AI-powered search.
- **Best Practice:** Enable this alongside structured data for optimal AI visibility.

---

### GeoOptimizationMode
- **XML Element:** `<GeoOptimizationMode>`
- **Data Type:** String (enumeration)
- **Default Value:** Empty
- **Supported Values:**
  - `Standard` — Basic GEO optimization (default for balanced approach)
  - `AISearch` — Optimized for AI-powered search engines (e.g., Perplexity, SearchGPT)
  - `Conversational` — Optimized for conversational AI responses (e.g., ChatGPT, Claude)
  - `Citation` — Optimized for content attribution and citation in AI responses
- **Example:** `<GeoOptimizationMode>Standard</GeoOptimizationMode>`
- **Recommendation Guidance:**

  | Mode | Best For | Use Case |
  |------|----------|----------|
  | **Standard** | General blogs | Most blogs; provides balanced optimization |
  | **AISearch** | News, reference content | Content meant to be discovered by AI search engines |
  | **Conversational** | Tutorial, how-to, educational | Content designed for inclusion in conversational responses |
  | **Citation** | Academic, research, technical | Content emphasizing proper attribution and citations |

- **Description:** Defines the optimization strategy for generative AI search engines. Each mode adjusts metadata richness and structured data for different AI use cases.
- **Interaction with GeoMetadataRichness:** The mode affects which metadata fields are prioritized (e.g., Citation mode emphasizes author and publication date).
- **SEO Impact:** High for AI visibility.

---

### GeoMetadataRichness
- **XML Element:** `<GeoMetadataRichness>`
- **Data Type:** String (enumeration)
- **Default Value:** Empty
- **Supported Values:**
  - `Minimal` — Essential metadata only (smallest HTML footprint)
  - `Standard` — Recommended set of metadata (balanced)
  - `Rich` — Comprehensive metadata for better AI understanding (recommended)
  - `Maximum` — All available metadata (largest HTML footprint)
- **Example:** `<GeoMetadataRichness>Rich</GeoMetadataRichness>`
- **Description:** Controls how comprehensive the metadata output is for AI systems. Higher richness provides more context but increases page size.
- **Trade-offs:**

  | Richness | Page Size Impact | AI Understanding | Recommendation |
  |----------|-----------------|------------------|-----------------||
  | **Minimal** | +0-2 KB | Basic | Limited content; minimal AI visibility |
  | **Standard** | +2-5 KB | Good | Most blogs; good balance |
  | **Rich** | +5-10 KB | Excellent | Recommended for all blogs; best AI visibility |
  | **Maximum** | +10-20 KB | Comprehensive | Technical/academic blogs; maximum AI context |

- **Performance Considerations:**
  - Most modern browsers/CDNs won't notice a difference between levels
  - Focus on `Rich` for the best balance of AI visibility and performance
- **Best Practice:** Use `Rich` for most blogs; use `Maximum` only for technical/academic content.

---

### GeoEnableCitationOptimization
- **XML Element:** `<GeoEnableCitationOptimization>`
- **Data Type:** Boolean (`true` or `false`)
- **Default Value:** `false`
- **Description:** When enabled, additional citation metadata is included to help AI systems properly attribute and reference the blog's content when used in generated responses.
- **Enabled By:** Setting to `true`
- **Impact When Enabled:**
  - AI systems receive clear author and publication information
  - Content is more likely to be cited by name in AI responses
  - Proper attribution improves content discoverability and credibility
  - Helps establish content authority with AI systems
- **Metadata Included:**
  - Author name and biography
  - Publication date and modification date
  - Content source and URL
  - License information (if available)
  - Content version and history
- **SEO Impact:** Medium for AI visibility; High for content attribution.
- **Use Cases:**
  - News and journalism sites (critical for attribution)
  - Academic and research blogs
  - Technical documentation
  - Any blog that values proper citation of sources
- **Best Practice:** Enable this if attribution and content credibility are important.

---

## Complete Configuration Examples

### Example 1: Minimal SEO Setup
```xml
<settings>
	<SeoTitleSuffix> - My Blog</SeoTitleSuffix>
	<SeoCanonicalDomain>https://example.com</SeoCanonicalDomain>
	<SeoDefaultAuthor>Admin</SeoDefaultAuthor>
	<SeoEnableOpenGraph>true</SeoEnableOpenGraph>
	<SeoEnableStructuredData>true</SeoEnableStructuredData>
	<SeoOrganizationName>My Blog</SeoOrganizationName>
</settings>
```

### Example 2: Complete SEO + Social Media Setup
```xml
<settings>
	<SeoTitleSuffix> - My Blog</SeoTitleSuffix>
	<SeoCanonicalDomain>https://example.com</SeoCanonicalDomain>
	<SeoDefaultAuthor>John Doe</SeoDefaultAuthor>
	<SeoDefaultImage>https://example.com/images/blog-logo.png</SeoDefaultImage>
	<SeoTwitterHandle>@myblog</SeoTwitterHandle>
	<SeoFacebookAppId>1234567890</SeoFacebookAppId>
	<SeoEnableOpenGraph>true</SeoEnableOpenGraph>
	<SeoEnableTwitterCard>true</SeoEnableTwitterCard>
	<SeoEnableStructuredData>true</SeoEnableStructuredData>
	<SeoOrganizationName>My Blog</SeoOrganizationName>
	<SeoOrganizationLogo>https://example.com/images/logo.png</SeoOrganizationLogo>
</settings>
```

### Example 3: Complete SEO + GEO Setup (Recommended for Modern Blogs)
```xml
<settings>
	<!-- SEO Basics -->
	<SeoTitleSuffix> - My Tech Blog</SeoTitleSuffix>
	<SeoCanonicalDomain>https://mytechblog.com</SeoCanonicalDomain>
	<SeoDefaultAuthor>Jane Developer</SeoDefaultAuthor>
	<SeoDefaultImage>https://mytechblog.com/images/blog-logo.png</SeoDefaultImage>

	<!-- Social Media -->
	<SeoTwitterHandle>@mytechblog</SeoTwitterHandle>
	<SeoFacebookAppId>9876543210</SeoFacebookAppId>
	<SeoEnableOpenGraph>true</SeoEnableOpenGraph>
	<SeoEnableTwitterCard>true</SeoEnableTwitterCard>

	<!-- Structured Data -->
	<SeoEnableStructuredData>true</SeoEnableStructuredData>
	<SeoOrganizationName>My Tech Company</SeoOrganizationName>
	<SeoOrganizationLogo>https://mytechblog.com/images/logo.png</SeoOrganizationLogo>

	<!-- GEO (Generative Engine Optimization) -->
	<GeoOptimizationEnabled>true</GeoOptimizationEnabled>
	<GeoOptimizationMode>Standard</GeoOptimizationMode>
	<GeoMetadataRichness>Rich</GeoMetadataRichness>
	<GeoEnableCitationOptimization>true</GeoEnableCitationOptimization>
</settings>
```

### Example 4: Academic/Research Blog with Citation Focus
```xml
<settings>
	<!-- SEO Basics -->
	<SeoTitleSuffix> - Academic Research</SeoTitleSuffix>
	<SeoCanonicalDomain>https://research.university.edu</SeoCanonicalDomain>
	<SeoDefaultAuthor>Dr. Research Author</SeoDefaultAuthor>

	<!-- Social Media (minimal) -->
	<SeoTwitterHandle>@UniversityResearch</SeoTwitterHandle>
	<SeoEnableOpenGraph>true</SeoEnableOpenGraph>

	<!-- Structured Data -->
	<SeoEnableStructuredData>true</SeoEnableStructuredData>
	<SeoOrganizationName>University Name</SeoOrganizationName>
	<SeoOrganizationLogo>https://research.university.edu/images/university-logo.png</SeoOrganizationLogo>

	<!-- GEO with Citation Emphasis -->
	<GeoOptimizationEnabled>true</GeoOptimizationEnabled>
	<GeoOptimizationMode>Citation</GeoOptimizationMode>
	<GeoMetadataRichness>Maximum</GeoMetadataRichness>
	<GeoEnableCitationOptimization>true</GeoEnableCitationOptimization>
</settings>
```

---

## Best Practices & Troubleshooting

### Best Practices

1. **Always Use HTTPS URLs**
   - All image and domain URLs should use `https://` protocol
   - Mixed HTTP/HTTPS content may be blocked by browsers

2. **Test Your Settings**
   - Use [Schema.org Validator](https://validator.schema.org/) to verify structured data
   - Use [Facebook Sharing Debugger](https://developers.facebook.com/tools/debug/sharing) to test Open Graph
   - Use [Twitter Card Validator](https://cards-dev.twitter.com/validator) to test Twitter Cards

3. **Image Optimization**
   - Keep default images under 500KB
   - Use high-quality images (minimum 600x315 pixels)
   - Prefer PNG for logos, JPG for photos

4. **Consistency**
   - Keep `SeoOrganizationName` consistent across all settings
   - Use the same author name format across all posts
   - Maintain consistent canonical domain

5. **Regular Updates**
   - Review and update GEO settings quarterly as AI capabilities evolve
   - Monitor AI search engines (Perplexity AI, SearchGPT) for content visibility
   - Test structured data annually with Schema.org Validator

### Troubleshooting

#### Settings Not Saving
- **Problem:** Changes to `settings.xml` don't appear in the admin panel
- **Solution:** 
  1. Ensure the app has write permissions to `/App_Data/` directory
  2. Check that the XML syntax is valid
  3. Restart the application

#### Social Media Previews Not Appearing
- **Problem:** Posts shared on Facebook/Twitter don't show rich previews
- **Solution:**
  1. Verify `SeoEnableOpenGraph` and `SeoEnableTwitterCard` are set to `true`
  2. Check that `SeoDefaultImage` is a valid, accessible HTTPS URL
  3. Use Facebook/Twitter debugger tools to verify markup
  4. Clear cache in debugger tools and re-test

#### AI Search Engines Not Finding Content
- **Problem:** Content doesn't appear in Perplexity AI, SearchGPT, or other AI search results
- **Solution:**
  1. Enable `GeoOptimizationEnabled` and set `GeoOptimizationMode`
  2. Enable `SeoEnableStructuredData` for rich content understanding
  3. Set `GeoMetadataRichness` to `Rich` or higher
  4. Wait 1-2 weeks for AI search engines to re-crawl your content

#### Structured Data Validation Errors
- **Problem:** Schema.org Validator reports errors in structured data
- **Solution:**
  1. Verify all URLs are valid and HTTPS
  2. Check `SeoOrganizationName` and `SeoOrganizationLogo` are properly formatted
  3. Ensure author names don't contain special XML characters
  4. Use the MetadataBuilder class (in `BlogEngine.Core.Metadata`) to debug output

---

## Technical Implementation

### Property Mapping
All settings are mapped from `BlogSettings` class properties to XML elements via reflection in the `Load()` method:

```csharp
// In BlogSettings.cs Load() method:
foreach (DictionaryEntry entry in dic)
{
	string name = (string)entry.Key;  // XML element name
	PropertyInfo property = GetProperty(name);
	if (property?.CanWrite == true)
	{
		property.SetValue(this, Convert.ChangeType(value, property.PropertyType), null);
	}
}
```

### MetadataBuilder Usage
The `MetadataBuilder` class in `BlogEngine.Core.Metadata.MetadataBuilder` uses these settings to construct metadata:

```csharp
var builder = new MetadataBuilder(BlogSettings.Instance)
	.WithTitle("Post Title")
	.WithDescription("Post description")
	.WithImage("https://example.com/image.jpg")
	.WithAuthor("Author Name")
	.Build();
```

### Persistence
Settings are persisted via the `XmlBlogProvider` in `BlogEngine.Core.Providers.XmlProvider.Settings.cs`:

```csharp
// SaveSettings() method creates properly formatted XML
<settings>
	<SeoTitleSuffix> - My Blog</SeoTitleSuffix>
	<SeoCanonicalDomain>https://example.com</SeoCanonicalDomain>
	<!-- ... more settings ... -->
</settings>
```

---

## Related Documentation

- [MetadataBuilder Class](../BlogEngine/BlogEngine.Core/Metadata/MetadataBuilder.cs) — Implementation details
- [BlogSettings Class](../BlogEngine/BlogEngine.Core/BlogSettings.cs) — Full property definitions
- [Schema.org Documentation](https://schema.org/) — Official structured data schema
- [Open Graph Protocol](https://ogp.me/) — Social media metadata standard
- [Twitter Card Documentation](https://developer.twitter.com/en/docs/twitter-for-websites/cards/overview/abouts-cards) — Twitter Card implementation
- [Generative Engine Optimization Guide](https://www.searchenginejournal.com/generative-engine-optimization/) — GEO best practices

---

## Version History

- **v1.0** (2024) — Initial documentation for SEO/GEO settings
  - Documented 15 SEO/GEO configuration settings
  - Added complete examples and best practices
  - Included troubleshooting guide

---

## Questions or Feedback?

For issues or suggestions about these settings, please:
1. Check the [BlogEngine Support](https://blogengine.io/support/)
2. Visit the [GitHub Issues](https://github.com/plykkegaard/BlogEngine.NET/issues)
3. Review the [Admin Settings Panel](#) documentation for UI configuration alternatives
