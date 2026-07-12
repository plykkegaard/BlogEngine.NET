namespace BlogEngine.Core.Metadata.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// Generates Schema.org structured data in JSON-LD format for blog content.
    /// </summary>
    /// <remarks>
    /// This class creates structured data markup that helps search engines and AI systems
    /// understand the semantic meaning of blog content. Supports Article, BlogPosting,
    /// NewsArticle, and WebPage schemas based on content type and blog use case.
    /// Critical for GEO (Generative Engine Optimization).
    /// </remarks>
    public class SchemaOrgGenerator
    {
        #region Fields

        private readonly BlogSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaOrgGenerator"/> class.
        /// </summary>
        /// <param name="settings">The blog settings containing Schema.org configuration.</param>
        /// <remarks>
        /// Creates a schema generator with access to blog settings for organization data
        /// and configuration-driven schema generation.
        /// </remarks>
        public SchemaOrgGenerator(BlogSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion

        #region Public Methods - Post Schemas

        /// <summary>
        /// Generates Schema.org Article JSON-LD for a blog post.
        /// </summary>
        /// <param name="post">The post to generate schema for.</param>
        /// <returns>JSON-LD string representing the Article schema.</returns>
        /// <remarks>
        /// Creates a BlogPosting schema (subtype of Article) with all relevant properties
        /// including headline, description, author, dates, publisher, and image.
        /// Used for individual blog post pages.
        /// </remarks>
        public string GenerateArticleSchema(Post post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "BlogPosting",
                ["headline"] = TruncateHeadline(post.Title),
                ["description"] = GetPlainTextDescription(post.Description ?? post.Content, 200),
                ["datePublished"] = post.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = post.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["author"] = CreatePersonSchema(post.Author ?? _settings.SeoDefaultAuthor),
                ["publisher"] = CreateOrganizationSchema(),
                ["url"] = post.AbsoluteLink.ToString(),
                ["mainEntityOfPage"] = new Dictionary<string, object>
                {
                    ["@type"] = "WebPage",
                    ["@id"] = post.AbsoluteLink.ToString()
                }
            };

            // Add image if available
            var imageUrl = ExtractFirstImage(post.Content) ?? _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                schema["image"] = CreateImageSchema(imageUrl);
            }

            // Add keywords from tags
            if (post.Tags.Any())
            {
                schema["keywords"] = string.Join(", ", post.Tags);
            }

            // Add article section from first category
            if (post.Categories.Any())
            {
                schema["articleSection"] = post.Categories.First().Title;
            }

            // Add word count for GEO optimization
            if (_settings.GeoOptimizationEnabled)
            {
                schema["wordCount"] = EstimateWordCount(post.Content);
            }

            return JsonConvert.SerializeObject(schema, Formatting.None);
        }

        /// <summary>
        /// Generates Schema.org NewsArticle JSON-LD for news-style blog posts.
        /// </summary>
        /// <param name="post">The post to generate schema for.</param>
        /// <returns>JSON-LD string representing the NewsArticle schema.</returns>
        /// <remarks>
        /// Creates a NewsArticle schema (subtype of Article) with additional properties
        /// relevant to news content. Use this for time-sensitive, news-oriented blog posts.
        /// </remarks>
        public string GenerateNewsArticleSchema(Post post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "NewsArticle",
                ["headline"] = TruncateHeadline(post.Title),
                ["description"] = GetPlainTextDescription(post.Description ?? post.Content, 200),
                ["datePublished"] = post.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = post.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["author"] = CreatePersonSchema(post.Author ?? _settings.SeoDefaultAuthor),
                ["publisher"] = CreateOrganizationSchema(),
                ["url"] = post.AbsoluteLink.ToString()
            };

            // Add image if available
            var imageUrl = ExtractFirstImage(post.Content) ?? _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                schema["image"] = CreateImageSchema(imageUrl);
            }

            return JsonConvert.SerializeObject(schema, Formatting.None);
        }

        #endregion

        #region Public Methods - Page Schemas

        /// <summary>
        /// Generates Schema.org WebPage JSON-LD for static pages.
        /// </summary>
        /// <param name="page">The page to generate schema for.</param>
        /// <returns>JSON-LD string representing the WebPage schema.</returns>
        /// <remarks>
        /// Creates a WebPage schema for static content pages (About, Contact, etc.).
        /// Uses simpler schema structure than Article since pages are typically not time-based content.
        /// </remarks>
        public string GenerateWebPageSchema(Page page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "WebPage",
                ["name"] = page.Title,
                ["description"] = GetPlainTextDescription(page.Description ?? page.Content, 200),
                ["url"] = page.AbsoluteLink.ToString(),
                ["datePublished"] = page.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = page.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["publisher"] = CreateOrganizationSchema()
            };

            return JsonConvert.SerializeObject(schema, Formatting.None);
        }

        #endregion

        #region Public Methods - Blog/Website Schemas

        /// <summary>
        /// Generates Schema.org Blog JSON-LD for the blog homepage or archive.
        /// </summary>
        /// <returns>JSON-LD string representing the Blog schema.</returns>
        /// <remarks>
        /// Creates a Blog schema for the main blog page or blog archive listings.
        /// Identifies the blog as a whole and links to the publishing organization.
        /// </remarks>
        public string GenerateBlogSchema()
        {
            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Blog",
                ["name"] = _settings.Name,
                ["description"] = _settings.Description,
                ["url"] = Utils.AbsoluteWebRoot.ToString(),
                ["publisher"] = CreateOrganizationSchema()
            };

            if (!string.IsNullOrEmpty(_settings.SeoDefaultImage))
            {
                schema["image"] = _settings.SeoDefaultImage;
            }

            return JsonConvert.SerializeObject(schema, Formatting.None);
        }

        /// <summary>
        /// Generates Schema.org WebSite JSON-LD with search action for the blog.
        /// </summary>
        /// <returns>JSON-LD string representing the WebSite schema.</returns>
        /// <remarks>
        /// Creates a WebSite schema with a PotentialAction for site search.
        /// This enables the Google search box in search results (sitelinks searchbox).
        /// Should be included on the homepage.
        /// </remarks>
        public string GenerateWebSiteSchema()
        {
            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "WebSite",
                ["name"] = _settings.Name,
                ["url"] = Utils.AbsoluteWebRoot.ToString(),
                ["potentialAction"] = new Dictionary<string, object>
                {
                    ["@type"] = "SearchAction",
                    ["target"] = $"{Utils.AbsoluteWebRoot}search.aspx?q={{search_term_string}}",
                    ["query-input"] = "required name=search_term_string"
                }
            };

            return JsonConvert.SerializeObject(schema, Formatting.None);
        }

        #endregion

        #region Private Helper Methods - Schema Components

        /// <summary>
        /// Creates a Schema.org Person object for author representation.
        /// </summary>
        /// <param name="name">The person's name.</param>
        /// <returns>A dictionary representing a Person schema.</returns>
        /// <remarks>
        /// Person schema is used for author and creator properties in Article schemas.
        /// Can be extended with additional properties like email, URL, or image.
        /// </remarks>
        private Dictionary<string, object> CreatePersonSchema(string name)
        {
            var person = new Dictionary<string, object>
            {
                ["@type"] = "Person",
                ["name"] = name
            };

            return person;
        }

        /// <summary>
        /// Creates a Schema.org Organization object for publisher representation.
        /// </summary>
        /// <returns>A dictionary representing an Organization schema.</returns>
        /// <remarks>
        /// Organization schema is required for Article schemas to define the publisher.
        /// Uses blog settings for organization name and logo.
        /// </remarks>
        private Dictionary<string, object> CreateOrganizationSchema()
        {
            var org = new Dictionary<string, object>
            {
                ["@type"] = "Organization",
                ["name"] = !string.IsNullOrEmpty(_settings.SeoOrganizationName) 
                    ? _settings.SeoOrganizationName 
                    : _settings.Name,
                ["url"] = Utils.AbsoluteWebRoot.ToString()
            };

            if (!string.IsNullOrEmpty(_settings.SeoOrganizationLogo))
            {
                org["logo"] = CreateImageSchema(_settings.SeoOrganizationLogo);
            }

            return org;
        }

        /// <summary>
        /// Creates a Schema.org ImageObject for image representation.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns>A dictionary representing an ImageObject schema.</returns>
        /// <remarks>
        /// ImageObject schema provides structured data for images referenced in articles.
        /// Helps search engines display images correctly in rich results.
        /// </remarks>
        private Dictionary<string, object> CreateImageSchema(string imageUrl)
        {
            return new Dictionary<string, object>
            {
                ["@type"] = "ImageObject",
                ["url"] = imageUrl
            };
        }

        #endregion

        #region Private Helper Methods - Text Processing

        /// <summary>
        /// Truncates a headline to Schema.org recommended length (110 characters).
        /// </summary>
        /// <param name="headline">The headline text.</param>
        /// <returns>Truncated headline.</returns>
        /// <remarks>
        /// Schema.org recommends keeping headlines under 110 characters for optimal display.
        /// Truncation occurs at word boundaries when possible.
        /// </remarks>
        private string TruncateHeadline(string headline)
        {
            if (string.IsNullOrEmpty(headline))
                return string.Empty;

            const int maxLength = 110;
            if (headline.Length <= maxLength)
                return headline;

            var truncated = headline.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > maxLength - 20)
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated + "...";
        }

        /// <summary>
        /// Extracts plain text description from HTML content.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        /// <param name="maxLength">Maximum length for the description.</param>
        /// <returns>Plain text description.</returns>
        /// <remarks>
        /// Strips HTML tags and decodes entities to produce plain text suitable for structured data.
        /// Truncates to the specified maximum length.
        /// </remarks>
        private string GetPlainTextDescription(string htmlContent, int maxLength)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return string.Empty;

            // Remove HTML tags
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);

            // Decode HTML entities
            var decoded = HttpUtility.HtmlDecode(withoutTags);

            // Remove multiple whitespace
            var normalized = System.Text.RegularExpressions.Regex.Replace(decoded, @"\s+", " ").Trim();

            // Truncate
            if (normalized.Length <= maxLength)
                return normalized;

            var truncated = normalized.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > maxLength - 20)
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated + "...";
        }

        /// <summary>
        /// Extracts the first image URL from HTML content.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        /// <returns>The first image URL found, or null if none.</returns>
        /// <remarks>
        /// Uses regex to find img tags and extract the src attribute.
        /// Returns the first image found, which is often the featured or most relevant image.
        /// </remarks>
        private string ExtractFirstImage(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return null;

            var imgMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"<img[^>]+src=""([^""]+)""", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (imgMatch.Success && imgMatch.Groups.Count > 1)
            {
                var imageUrl = imgMatch.Groups[1].Value;

                // Convert relative URLs to absolute
                if (!imageUrl.StartsWith("http"))
                {
                    imageUrl = new Uri(Utils.AbsoluteWebRoot, imageUrl).ToString();
                }

                return imageUrl;
            }

            return null;
        }

        /// <summary>
        /// Estimates word count for content.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        /// <returns>Estimated word count.</returns>
        /// <remarks>
        /// Strips HTML tags and counts words by splitting on whitespace.
        /// Word count is useful for GEO optimization as it indicates content depth.
        /// </remarks>
        private int EstimateWordCount(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return 0;

            // Remove HTML tags
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);

            // Decode HTML entities
            var decoded = HttpUtility.HtmlDecode(withoutTags);

            // Count words
            var words = decoded.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        #endregion
    }
}
