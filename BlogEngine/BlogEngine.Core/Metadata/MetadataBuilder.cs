namespace BlogEngine.Core.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Builder pattern implementation for constructing SEO/GEO metadata dictionaries.
    /// </summary>
    /// <remarks>
    /// This builder simplifies the process of creating metadata dictionaries from blog content
    /// (posts, pages) and settings. It provides a fluent API for building metadata incrementally
    /// and ensures proper encoding and formatting of metadata values.
    /// </remarks>
    public class MetadataBuilder
    {
        #region Fields

        private readonly Dictionary<string, string> _metadata;
        private readonly BlogSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataBuilder"/> class.
        /// </summary>
        /// <param name="settings">The blog settings for default values.</param>
        /// <remarks>
        /// Creates a new metadata builder with access to blog settings for fallback values
        /// and configuration-driven metadata generation.
        /// </remarks>
        public MetadataBuilder(BlogSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Fluent API Methods

        /// <summary>
        /// Sets the page title metadata.
        /// </summary>
        /// <param name="title">The page title.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// The title is sanitized and can be used for both the HTML title tag and Open Graph/Twitter Card metadata.
        /// </remarks>
        public MetadataBuilder SetTitle(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                _metadata["title"] = SanitizeText(title);
            }
            return this;
        }

        /// <summary>
        /// Sets the page description metadata.
        /// </summary>
        /// <param name="description">The page description.</param>
        /// <param name="maxLength">Maximum length for the description (default 160 characters).</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Description is truncated to the specified maximum length and sanitized.
        /// This is used for meta description, Open Graph description, and Twitter Card description.
        /// </remarks>
        public MetadataBuilder SetDescription(string description, int maxLength = 160)
        {
            if (!string.IsNullOrEmpty(description))
            {
                var sanitized = SanitizeText(description);
                _metadata["description"] = TruncateText(sanitized, maxLength);
            }
            return this;
        }

        /// <summary>
        /// Sets the keywords metadata from a collection of tags or keywords.
        /// </summary>
        /// <param name="keywords">Collection of keyword strings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Keywords are joined with commas and sanitized. Used for meta keywords tag
        /// and as a fallback for content categorization in structured data.
        /// </remarks>
        public MetadataBuilder SetKeywords(IEnumerable<string> keywords)
        {
            if (keywords != null && keywords.Any())
            {
                var sanitizedKeywords = keywords.Select(k => SanitizeText(k)).Where(k => !string.IsNullOrEmpty(k));
                _metadata["keywords"] = string.Join(", ", sanitizedKeywords);
            }
            return this;
        }

        /// <summary>
        /// Sets the author metadata.
        /// </summary>
        /// <param name="author">The author name.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Falls back to SeoDefaultAuthor from settings if no author is provided.
        /// Used in meta author tag, Schema.org author, and Open Graph article:author.
        /// </remarks>
        public MetadataBuilder SetAuthor(string author)
        {
            var authorName = !string.IsNullOrEmpty(author) ? author : _settings.SeoDefaultAuthor;
            if (!string.IsNullOrEmpty(authorName))
            {
                _metadata["author"] = SanitizeText(authorName);
            }
            return this;
        }

        /// <summary>
        /// Sets the canonical URL metadata.
        /// </summary>
        /// <param name="url">The canonical URL.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// The canonical URL should be the absolute, preferred URL for the page.
        /// Used in canonical link tag and Open Graph og:url.
        /// </remarks>
        public MetadataBuilder SetUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _metadata["url"] = url;
                _metadata["canonical"] = url;
            }
            return this;
        }

        /// <summary>
        /// Sets the featured image URL metadata.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Falls back to SeoDefaultImage from settings if no image is provided.
        /// Used in Open Graph og:image and Twitter Card twitter:image.
        /// Should be an absolute URL for proper social media display.
        /// </remarks>
        public MetadataBuilder SetImage(string imageUrl)
        {
            var image = !string.IsNullOrEmpty(imageUrl) ? imageUrl : _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(image))
            {
                _metadata["image"] = image;
            }
            return this;
        }

        /// <summary>
        /// Sets the publication date metadata.
        /// </summary>
        /// <param name="publishedDate">The publication date.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Date is formatted in ISO 8601 format for Open Graph article:published_time
        /// and Schema.org datePublished properties.
        /// </remarks>
        public MetadataBuilder SetPublishedDate(DateTime publishedDate)
        {
            _metadata["published"] = publishedDate.ToString("yyyy-MM-ddTHH:mm:sszzz");
            return this;
        }

        /// <summary>
        /// Sets the last modified date metadata.
        /// </summary>
        /// <param name="modifiedDate">The last modified date.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Date is formatted in ISO 8601 format for Open Graph article:modified_time
        /// and Schema.org dateModified properties.
        /// </remarks>
        public MetadataBuilder SetModifiedDate(DateTime modifiedDate)
        {
            _metadata["modified"] = modifiedDate.ToString("yyyy-MM-ddTHH:mm:sszzz");
            return this;
        }

        /// <summary>
        /// Sets the content category metadata.
        /// </summary>
        /// <param name="category">The primary category name.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Used in Open Graph article:section and Schema.org articleSection properties.
        /// Represents the primary topic or category of the content.
        /// </remarks>
        public MetadataBuilder SetCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                _metadata["category"] = SanitizeText(category);
            }
            return this;
        }

        /// <summary>
        /// Sets the tags metadata from a collection of tag strings.
        /// </summary>
        /// <param name="tags">Collection of tag strings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Tags are joined with commas and used in Open Graph article:tag properties.
        /// Multiple article:tag meta tags will be generated for each tag.
        /// </remarks>
        public MetadataBuilder SetTags(IEnumerable<string> tags)
        {
            if (tags != null && tags.Any())
            {
                var sanitizedTags = tags.Select(t => SanitizeText(t)).Where(t => !string.IsNullOrEmpty(t));
                _metadata["tags"] = string.Join(",", sanitizedTags);
            }
            return this;
        }

        /// <summary>
        /// Sets the Open Graph type metadata.
        /// </summary>
        /// <param name="type">The OG type (e.g., "article", "website", "blog").</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Defines the type of content for Open Graph. Common types for blogs:
        /// "article" (individual posts), "website" (homepage), "blog" (blog archive).
        /// </remarks>
        public MetadataBuilder SetOpenGraphType(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                _metadata["og:type"] = type;
            }
            return this;
        }

        /// <summary>
        /// Sets the Twitter creator handle metadata.
        /// </summary>
        /// <param name="handle">The Twitter handle (e.g., @username).</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Specifies the Twitter account of the content creator/author.
        /// Distinct from the site's main Twitter account (twitter:site).
        /// </remarks>
        public MetadataBuilder SetTwitterCreator(string handle)
        {
            if (!string.IsNullOrEmpty(handle))
            {
                _metadata["twitter:creator"] = handle;
            }
            return this;
        }

        /// <summary>
        /// Sets custom metadata key-value pairs.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <remarks>
        /// Allows setting arbitrary metadata for extensibility. Useful for custom
        /// metadata requirements or future metadata types not explicitly supported.
        /// </remarks>
        public MetadataBuilder SetCustom(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _metadata[key] = value;
            }
            return this;
        }

        /// <summary>
        /// Builds and returns the metadata dictionary.
        /// </summary>
        /// <returns>A dictionary containing all configured metadata key-value pairs.</returns>
        /// <remarks>
        /// Returns the completed metadata dictionary for use with SeoMetadataManager.
        /// The builder instance can be reused after calling Build() to create additional metadata sets.
        /// </remarks>
        public IDictionary<string, string> Build()
        {
            return new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Helper Methods - Post/Page Specific

        /// <summary>
        /// Builds metadata from a BlogEngine Post object.
        /// </summary>
        /// <param name="post">The post to extract metadata from.</param>
        /// <param name="settings">The blog settings for defaults.</param>
        /// <returns>A metadata dictionary suitable for the post page.</returns>
        /// <remarks>
        /// Convenience method that extracts all relevant metadata from a Post object
        /// and applies appropriate defaults from settings. Includes title, description,
        /// author, dates, categories, tags, and images.
        /// </remarks>
        public static IDictionary<string, string> FromPost(Post post, BlogSettings settings)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var builder = new MetadataBuilder(settings);

            return builder
                .SetTitle(post.Title)
                .SetDescription(post.Description ?? post.Content, 160)
                .SetAuthor(post.Author)
                .SetUrl(post.AbsoluteLink.ToString())
                .SetPublishedDate(post.DateCreated)
                .SetModifiedDate(post.DateModified)
                .SetCategory(post.Categories.FirstOrDefault()?.Title)
                .SetTags(post.Tags)
                .SetKeywords(post.Tags)
                .SetOpenGraphType("article")
                .Build();
        }

        /// <summary>
        /// Builds metadata from a BlogEngine Page object.
        /// </summary>
        /// <param name="page">The page to extract metadata from.</param>
        /// <param name="settings">The blog settings for defaults.</param>
        /// <returns>A metadata dictionary suitable for the page.</returns>
        /// <remarks>
        /// Convenience method that extracts metadata from a Page object.
        /// Pages use "website" as the Open Graph type since they're typically static content
        /// rather than time-based articles.
        /// </remarks>
        public static IDictionary<string, string> FromPage(Page page, BlogSettings settings)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var builder = new MetadataBuilder(settings);

            return builder
                .SetTitle(page.Title)
                .SetDescription(page.Description ?? page.Content, 160)
                .SetUrl(page.AbsoluteLink.ToString())
                .SetPublishedDate(page.DateCreated)
                .SetModifiedDate(page.DateModified)
                .SetOpenGraphType("website")
                .Build();
        }

        /// <summary>
        /// Builds metadata for the blog homepage or archive pages.
        /// </summary>
        /// <param name="settings">The blog settings.</param>
        /// <param name="pageTitle">Optional custom page title.</param>
        /// <param name="pageDescription">Optional custom page description.</param>
        /// <returns>A metadata dictionary suitable for listing pages.</returns>
        /// <remarks>
        /// Creates metadata for homepage, category pages, tag pages, and archive pages.
        /// Uses blog-level settings for title and description when custom values aren't provided.
        /// </remarks>
        public static IDictionary<string, string> ForHomepage(BlogSettings settings, string pageTitle = null, string pageDescription = null)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var builder = new MetadataBuilder(settings);

            var title = !string.IsNullOrEmpty(pageTitle) ? pageTitle : settings.Name;
            var description = !string.IsNullOrEmpty(pageDescription) ? pageDescription : settings.Description;

            return builder
                .SetTitle(title)
                .SetDescription(description, 160)
                .SetUrl(Utils.AbsoluteWebRoot.ToString())
                .SetOpenGraphType("website")
                .Build();
        }

        #endregion

        #region Text Processing Helpers

        /// <summary>
        /// Sanitizes text by removing HTML tags and decoding entities.
        /// </summary>
        /// <param name="text">The text to sanitize.</param>
        /// <returns>The sanitized text.</returns>
        /// <remarks>
        /// This method strips HTML tags and decodes HTML entities to produce plain text
        /// suitable for meta tags and structured data. Ensures metadata doesn't contain markup.
        /// </remarks>
        private static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Remove HTML tags
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);

            // Decode HTML entities
            var decoded = HttpUtility.HtmlDecode(withoutTags);

            // Remove multiple whitespace
            var normalized = System.Text.RegularExpressions.Regex.Replace(decoded, @"\s+", " ");

            return normalized.Trim();
        }

        /// <summary>
        /// Truncates text to a maximum length with ellipsis.
        /// </summary>
        /// <param name="text">The text to truncate.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The truncated text.</returns>
        /// <remarks>
        /// Truncates text at word boundaries when possible to avoid breaking words.
        /// Adds ellipsis (...) when text is truncated. Respects the specified maximum length.
        /// </remarks>
        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            // Try to truncate at word boundary
            var truncated = text.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > maxLength - 20) // Only truncate at word if not too far back
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated.TrimEnd() + "...";
        }

        #endregion
    }
}
