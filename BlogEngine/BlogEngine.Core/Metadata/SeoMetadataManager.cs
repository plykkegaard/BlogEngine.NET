namespace BlogEngine.Core.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// Central service for rendering SEO and Generative Engine Optimization (GEO) metadata tags.
    /// </summary>
    /// <remarks>
    /// This service generates appropriate meta tags, Open Graph tags, Twitter Card metadata,
    /// and structured data for blog content. It supports both traditional search engine optimization
    /// and optimization for AI-powered generative search engines.
    /// </remarks>
    public class SeoMetadataManager
    {
        #region Fields

        private readonly Page _page;
        private readonly BlogSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SeoMetadataManager"/> class.
        /// </summary>
        /// <param name="page">The page instance to render metadata into.</param>
        /// <param name="settings">The blog settings containing SEO configuration.</param>
        /// <remarks>
        /// Creates a metadata manager bound to a specific page and blog settings context.
        /// The manager will render all metadata tags into the page's header section.
        /// </remarks>
        public SeoMetadataManager(Page page, BlogSettings settings)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Renders all appropriate SEO/GEO metadata tags for the current page.
        /// </summary>
        /// <param name="metadata">The metadata dictionary containing key-value pairs for meta tags.</param>
        /// <remarks>
        /// This method orchestrates the rendering of all metadata types: basic meta tags,
        /// Open Graph tags, Twitter Card tags, and structured data. It determines which
        /// metadata to render based on the blog settings and the provided metadata dictionary.
        /// </remarks>
        public void RenderMetadata(IDictionary<string, string> metadata)
        {
            if (metadata == null || metadata.Count == 0)
                return;

            // Render basic meta tags
            RenderBasicMetaTags(metadata);

            // Render Open Graph tags
            if (_settings.SeoEnableOpenGraph)
            {
                RenderOpenGraphTags(metadata);
            }

            // Render Twitter Card tags
            if (_settings.SeoEnableTwitterCard)
            {
                RenderTwitterCardTags(metadata);
            }

            // Render canonical URL
            if (!string.IsNullOrEmpty(_settings.SeoCanonicalDomain))
            {
                RenderCanonicalLink(metadata);
            }
        }

        /// <summary>
        /// Renders Schema.org structured data as JSON-LD in the page header.
        /// </summary>
        /// <param name="jsonLd">The JSON-LD structured data string.</param>
        /// <remarks>
        /// Structured data helps search engines and AI systems understand content semantics.
        /// This method injects a script tag containing JSON-LD formatted schema markup.
        /// </remarks>
        public void RenderStructuredData(string jsonLd)
        {
            if (string.IsNullOrEmpty(jsonLd) || !_settings.SeoEnableStructuredData)
                return;

            var script = new HtmlGenericControl("script");
            script.Attributes["type"] = "application/ld+json";
            script.InnerHtml = jsonLd;
            _page.Header.Controls.Add(script);
        }

        #endregion

        #region Private Methods - Basic Meta Tags

        /// <summary>
        /// Renders basic HTML meta tags (description, keywords, author, robots).
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <remarks>
        /// Basic meta tags are the foundation of SEO. This method renders standard HTML meta tags
        /// that are recognized by all search engines and web crawlers.
        /// </remarks>
        private void RenderBasicMetaTags(IDictionary<string, string> metadata)
        {
            AddMetaTag("description", GetMetadataValue(metadata, "description"));
            AddMetaTag("keywords", GetMetadataValue(metadata, "keywords"));
            AddMetaTag("author", GetMetadataValue(metadata, "author", _settings.SeoDefaultAuthor));
            AddMetaTag("robots", GetMetadataValue(metadata, "robots", "index, follow"));

            // GEO-specific: Add AI crawler hints if GEO optimization is enabled
            if (_settings.GeoOptimizationEnabled)
            {
                AddMetaTag("ai-content-declaration", "true");
            }
        }

        /// <summary>
        /// Adds a standard HTML meta tag to the page header.
        /// </summary>
        /// <param name="name">The meta tag name attribute.</param>
        /// <param name="content">The meta tag content attribute.</param>
        /// <remarks>
        /// This helper method creates and adds a meta tag only if the content is not empty.
        /// It follows the HTML standard format: &lt;meta name="..." content="..." /&gt;
        /// </remarks>
        private void AddMetaTag(string name, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            var meta = new HtmlMeta
            {
                Name = name,
                Content = HttpUtility.HtmlEncode(content)
            };
            _page.Header.Controls.Add(meta);
        }

        #endregion

        #region Private Methods - Open Graph

        /// <summary>
        /// Renders Open Graph meta tags for social media sharing.
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <remarks>
        /// Open Graph tags enable rich previews when content is shared on social platforms
        /// like Facebook, LinkedIn, and others that support the Open Graph protocol.
        /// </remarks>
        private void RenderOpenGraphTags(IDictionary<string, string> metadata)
        {
            AddOpenGraphTag("og:site_name", _settings.Name);
            AddOpenGraphTag("og:type", GetMetadataValue(metadata, "og:type", "article"));
            AddOpenGraphTag("og:title", GetMetadataValue(metadata, "title", _page.Title));
            AddOpenGraphTag("og:description", GetMetadataValue(metadata, "description"));
            AddOpenGraphTag("og:url", GetMetadataValue(metadata, "url"));
            AddOpenGraphTag("og:image", GetMetadataValue(metadata, "image", _settings.SeoDefaultImage));
            AddOpenGraphTag("og:locale", System.Globalization.CultureInfo.CurrentCulture.Name);

            // Article-specific OG tags
            if (GetMetadataValue(metadata, "og:type", "article") == "article")
            {
                AddOpenGraphTag("article:published_time", GetMetadataValue(metadata, "published"));
                AddOpenGraphTag("article:modified_time", GetMetadataValue(metadata, "modified"));
                AddOpenGraphTag("article:author", GetMetadataValue(metadata, "author", _settings.SeoDefaultAuthor));
                AddOpenGraphTag("article:section", GetMetadataValue(metadata, "category"));

                // Add tags
                var tags = GetMetadataValue(metadata, "tags");
                if (!string.IsNullOrEmpty(tags))
                {
                    foreach (var tag in tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        AddOpenGraphTag("article:tag", tag.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Adds an Open Graph meta tag to the page header.
        /// </summary>
        /// <param name="property">The OG property attribute value.</param>
        /// <param name="content">The OG content attribute value.</param>
        /// <remarks>
        /// Open Graph tags use the "property" attribute instead of "name" to distinguish
        /// them from standard HTML meta tags. Format: &lt;meta property="og:..." content="..." /&gt;
        /// </remarks>
        private void AddOpenGraphTag(string property, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            var meta = new HtmlMeta();
            meta.Attributes["property"] = property;
            meta.Content = HttpUtility.HtmlEncode(content);
            _page.Header.Controls.Add(meta);
        }

        #endregion

        #region Private Methods - Twitter Card

        /// <summary>
        /// Renders Twitter Card meta tags for Twitter sharing.
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <remarks>
        /// Twitter Card tags enable rich media previews when content is shared on Twitter.
        /// Supports summary and summary_large_image card types.
        /// </remarks>
        private void RenderTwitterCardTags(IDictionary<string, string> metadata)
        {
            var hasImage = !string.IsNullOrEmpty(GetMetadataValue(metadata, "image", _settings.SeoDefaultImage));
            var cardType = hasImage ? "summary_large_image" : "summary";

            AddTwitterTag("twitter:card", cardType);
            AddTwitterTag("twitter:site", _settings.SeoTwitterHandle);
            AddTwitterTag("twitter:creator", GetMetadataValue(metadata, "twitter:creator", _settings.SeoTwitterHandle));
            AddTwitterTag("twitter:title", GetMetadataValue(metadata, "title", _page.Title));
            AddTwitterTag("twitter:description", GetMetadataValue(metadata, "description"));
            AddTwitterTag("twitter:image", GetMetadataValue(metadata, "image", _settings.SeoDefaultImage));
        }

        /// <summary>
        /// Adds a Twitter Card meta tag to the page header.
        /// </summary>
        /// <param name="name">The Twitter Card name attribute value.</param>
        /// <param name="content">The Twitter Card content attribute value.</param>
        /// <remarks>
        /// Twitter Card tags follow the standard HTML meta tag format with a "twitter:" prefix
        /// in the name attribute. Format: &lt;meta name="twitter:..." content="..." /&gt;
        /// </remarks>
        private void AddTwitterTag(string name, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            var meta = new HtmlMeta
            {
                Name = name,
                Content = HttpUtility.HtmlEncode(content)
            };
            _page.Header.Controls.Add(meta);
        }

        #endregion

        #region Private Methods - Canonical Link

        /// <summary>
        /// Renders a canonical link tag to specify the preferred URL for the page.
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <remarks>
        /// Canonical links help search engines understand which URL is the primary version
        /// of a page, preventing duplicate content issues. This is critical for SEO.
        /// </remarks>
        private void RenderCanonicalLink(IDictionary<string, string> metadata)
        {
            var canonicalUrl = GetMetadataValue(metadata, "canonical");
            if (string.IsNullOrEmpty(canonicalUrl))
            {
                // Build canonical URL from current request
                var currentUrl = HttpContext.Current.Request.Url;
                var path = currentUrl.AbsolutePath;
                canonicalUrl = new Uri(new Uri(_settings.SeoCanonicalDomain), path).ToString();
            }

            var link = new HtmlLink
            {
                Href = canonicalUrl
            };
            link.Attributes["rel"] = "canonical";
            _page.Header.Controls.Add(link);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves a metadata value from the dictionary with optional fallback.
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <param name="key">The key to retrieve.</param>
        /// <param name="defaultValue">Optional default value if key is not found.</param>
        /// <returns>The metadata value, default value, or empty string.</returns>
        /// <remarks>
        /// This helper provides safe dictionary access with fallback support, ensuring
        /// that missing metadata keys don't cause exceptions during rendering.
        /// </remarks>
        private string GetMetadataValue(IDictionary<string, string> metadata, string key, string defaultValue = "")
        {
            if (metadata.ContainsKey(key) && !string.IsNullOrEmpty(metadata[key]))
                return metadata[key];

            return defaultValue;
        }

        #endregion
    }
}
