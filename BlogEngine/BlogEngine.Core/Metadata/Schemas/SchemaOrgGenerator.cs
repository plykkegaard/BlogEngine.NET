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
        /// 
        /// GEO OPTIMIZATION: Includes critical fields for generative AI systems:
        /// - articleBody: Full content for AI extraction (CRITICAL)
        /// - inLanguage: Language classification for multilingual AI
        /// - isAccessibleForFree: Recommended by Google for AI Overview
        /// - keywords: Array format (preferred for GEO)
        /// - publisher.logo: Authority signal for AI systems
        /// - commentCount, identifier, interactionStatistic: Optional but recommended
        /// </remarks>
        public string GenerateArticleSchema(Post post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            // Determine schema type from Post.SchemaType or default to BlogPosting
            var schemaType = !string.IsNullOrEmpty(post.SchemaType) ? post.SchemaType : "BlogPosting";

            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = schemaType,
                ["headline"] = TruncateHeadline(post.Title),
                ["description"] = GetPlainTextDescription(post.Description ?? post.Content, 200),

                // GEO CRITICAL: articleBody - full content for AI extraction
                ["articleBody"] = GetPlainTextContent(post.Content),

                ["datePublished"] = post.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = post.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["author"] = CreatePersonSchema(post.Author ?? _settings.SeoDefaultAuthor, post),
                ["publisher"] = CreateOrganizationSchema(), // Now includes logo
                ["url"] = !string.IsNullOrEmpty(post.CanonicalUrl) ? post.CanonicalUrl : post.AbsoluteLink.ToString(),
                ["mainEntityOfPage"] = new Dictionary<string, object>
                {
                    ["@type"] = "WebPage",
                    ["@id"] = !string.IsNullOrEmpty(post.CanonicalUrl) ? post.CanonicalUrl : post.AbsoluteLink.ToString()
                },

                // GEO CRITICAL: inLanguage for multilingual generative summaries
                ["inLanguage"] = DetermineLanguage(post),

                // GEO RECOMMENDED: isAccessibleForFree (Google AI Overview)
                ["isAccessibleForFree"] = DetermineAccessibility(post)
            };

            // Add image if available
            var imageUrl = post.FirstImgSrc ?? ExtractFirstImage(post.Content) ?? _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                schema["image"] = CreateImageSchema(imageUrl);
            }

            // GEO PRIORITY: Keywords as array (preferred format for generative engines)
            var keywords = ExtractKeywordsArray(post);
            if (keywords != null && keywords.Count > 0)
            {
                schema["keywords"] = keywords; // Array format, not comma-separated string
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

            // GEO RECOMMENDED: identifier (DOI, ISBN, or unique ID)
            if (!string.IsNullOrEmpty(post.Id.ToString()))
            {
                schema["identifier"] = new Dictionary<string, object>
                {
                    ["@type"] = "PropertyValue",
                    ["propertyID"] = "BlogEngine_Post_ID",
                    ["value"] = post.Id.ToString()
                };
            }

            // GEO RECOMMENDED: commentCount for engagement signals
            if (post.ApprovedComments.Count > 0)
            {
                schema["commentCount"] = post.ApprovedComments.Count;
            }

            // GEO RECOMMENDED: interactionStatistic for engagement metrics
            if (_settings.GeoOptimizationEnabled && post.ApprovedComments.Count > 0)
            {
                schema["interactionStatistic"] = new Dictionary<string, object>
                {
                    ["@type"] = "InteractionCounter",
                    ["interactionType"] = "https://schema.org/CommentAction",
                    ["userInteractionCount"] = post.ApprovedComments.Count
                };
            }

            // GEO EXTENDED: Key entities for semantic understanding
            if (!string.IsNullOrEmpty(post.KeyEntities))
            {
                var entities = post.KeyEntities.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(e => e.Trim())
                                              .Where(e => !string.IsNullOrEmpty(e))
                                              .ToList();

                if (entities.Any())
                {
                    schema["about"] = entities.Select(e => new Dictionary<string, object>
                    {
                        ["@type"] = "Thing",
                        ["name"] = e
                    }).ToList();
                }
            }

            // GEO EXTENDED: Semantic summary (if provided)
            if (!string.IsNullOrEmpty(post.SemanticSummary))
            {
                schema["abstract"] = post.SemanticSummary;
            }

            // GEO EXTENDED: Main subject line for topic classification
            if (!string.IsNullOrEmpty(post.ContentMSL))
            {
                if (!schema.ContainsKey("about"))
                {
                    schema["about"] = new Dictionary<string, object>
                    {
                        ["@type"] = "Thing",
                        ["name"] = post.ContentMSL
                    };
                }
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
        /// 
        /// GEO OPTIMIZATION: Includes critical fields for generative AI systems:
        /// - articleBody: Full content for AI extraction (CRITICAL)
        /// - inLanguage: Language classification for multilingual AI
        /// - isAccessibleForFree: Recommended by Google for AI Overview
        /// - keywords: Array format (preferred for GEO)
        /// - publisher.logo: Authority signal for AI systems
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

                // GEO CRITICAL: articleBody - full content for AI extraction
                ["articleBody"] = GetPlainTextContent(post.Content),

                ["datePublished"] = post.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = post.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["author"] = CreatePersonSchema(post.Author ?? _settings.SeoDefaultAuthor, post),
                ["publisher"] = CreateOrganizationSchema(), // Now includes logo
                ["url"] = !string.IsNullOrEmpty(post.CanonicalUrl) ? post.CanonicalUrl : post.AbsoluteLink.ToString(),

                // GEO CRITICAL: inLanguage for multilingual generative summaries
                ["inLanguage"] = DetermineLanguage(post),

                // GEO RECOMMENDED: isAccessibleForFree (Google AI Overview)
                ["isAccessibleForFree"] = DetermineAccessibility(post)
            };

            // Add image if available
            var imageUrl = post.FirstImgSrc ?? ExtractFirstImage(post.Content) ?? _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                schema["image"] = CreateImageSchema(imageUrl);
            }

            // GEO PRIORITY: Keywords as array (preferred format for generative engines)
            var keywords = ExtractKeywordsArray(post);
            if (keywords != null && keywords.Count > 0)
            {
                schema["keywords"] = keywords;
            }

            // Add article section from first category
            if (post.Categories.Any())
            {
                schema["articleSection"] = post.Categories.First().Title;
            }

            // GEO RECOMMENDED: identifier
            if (!string.IsNullOrEmpty(post.Id.ToString()))
            {
                schema["identifier"] = new Dictionary<string, object>
                {
                    ["@type"] = "PropertyValue",
                    ["propertyID"] = "BlogEngine_Post_ID",
                    ["value"] = post.Id.ToString()
                };
            }

            // GEO RECOMMENDED: commentCount
            if (post.ApprovedComments.Count > 0)
            {
                schema["commentCount"] = post.ApprovedComments.Count;
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
        /// 
        /// GEO OPTIMIZATION: Includes critical fields for generative AI systems:
        /// - inLanguage: Language classification for multilingual AI
        /// - isAccessibleForFree: Recommended by Google for AI Overview
        /// - keywords: Array format (preferred for GEO)
        /// - publisher.logo: Authority signal for AI systems
        /// </remarks>
        public string GenerateWebPageSchema(Page page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            // Determine schema type from Page.SchemaType or default to WebPage
            var schemaType = !string.IsNullOrEmpty(page.SchemaType) ? page.SchemaType : "WebPage";

            var schema = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = schemaType,
                ["name"] = page.Title,
                ["description"] = GetPlainTextDescription(page.Description ?? page.Content, 200),
                ["url"] = !string.IsNullOrEmpty(page.CanonicalUrl) ? page.CanonicalUrl : page.AbsoluteLink.ToString(),
                ["datePublished"] = page.DateCreated.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["dateModified"] = page.DateModified.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["publisher"] = CreateOrganizationSchema(), // Now includes logo

                // GEO CRITICAL: inLanguage for multilingual generative summaries
                ["inLanguage"] = DetermineLanguagePage(page),

                // GEO RECOMMENDED: isAccessibleForFree (Google AI Overview)
                ["isAccessibleForFree"] = true // Pages are typically free access
            };

            // Add image if available
            var imageUrl = ExtractFirstImage(page.Content) ?? _settings.SeoDefaultImage;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                schema["image"] = CreateImageSchema(imageUrl);
            }

            // GEO PRIORITY: Keywords as array (preferred format for generative engines)
            if (!string.IsNullOrEmpty(page.Keywords))
            {
                var keywords = page.Keywords
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                if (keywords.Any())
                {
                    schema["keywords"] = keywords;
                }
            }

            // GEO EXTENDED: Key entities for semantic understanding
            if (!string.IsNullOrEmpty(page.KeyEntities))
            {
                var entities = page.KeyEntities.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(e => e.Trim())
                                              .Where(e => !string.IsNullOrEmpty(e))
                                              .ToList();

                if (entities.Any())
                {
                    schema["about"] = entities.Select(e => new Dictionary<string, object>
                    {
                        ["@type"] = "Thing",
                        ["name"] = e
                    }).ToList();
                }
            }

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

            // Add GEO extended settings
            if (!string.IsNullOrEmpty(_settings.InLanguage))
            {
                schema["inLanguage"] = _settings.InLanguage;
            }

            if (!string.IsNullOrEmpty(_settings.GEOImage))
            {
                schema["image"] = CreateImageSchema(_settings.GEOImage);
            }

            // Add Keywords with intelligent fallback strategy
            var keywords = ExtractKeywords();
            if (keywords != null && keywords.Count > 0)
            {
                schema["keywords"] = keywords;
            }

            // Add sameAs (social/profile links) if provided
            if (!string.IsNullOrEmpty(_settings.SameAs))
            {
                try
                {
                    var sameAsArray = JsonConvert.DeserializeObject<List<string>>(_settings.SameAs);
                    if (sameAsArray != null && sameAsArray.Count > 0)
                    {
                        schema["sameAs"] = sameAsArray;
                    }
                }
                catch
                {
                    // If JSON is malformed, skip this field
                }
            }

            // Add potential actions if provided (parsed JSON)
            if (!string.IsNullOrEmpty(_settings.GEOPotentialActions))
            {
                try
                {
                    var potentialActions = JsonConvert.DeserializeObject(_settings.GEOPotentialActions);
                    schema["potentialAction"] = potentialActions;
                }
                catch
                {
                    // If JSON is malformed, skip this field
                }
            }
            else
            {
                // Provide default ReadAction if not specified
                schema["potentialAction"] = new Dictionary<string, object>
                {
                    ["@type"] = "ReadAction",
                    ["target"] = Utils.AbsoluteWebRoot.ToString()
                };
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
        /// <param name="post">Optional post object for author-specific metadata.</param>
        /// <returns>A dictionary representing a Person schema.</returns>
        /// <remarks>
        /// Person schema is used for author and creator properties in Article schemas.
        /// GEO ENHANCEMENT: Can be extended with sameAs for author social profiles (authority signal).
        /// Currently uses basic person info from AuthorProfile (name, photo, URL).
        /// </remarks>
        private Dictionary<string, object> CreatePersonSchema(string name, Post post = null)
        {
            var person = new Dictionary<string, object>
            {
                ["@type"] = "Person",
                ["name"] = name
            };

            // GEO RECOMMENDED: Add author metadata if available
            // Try to get author profile for additional details
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    var authorProfile = AuthorProfile.GetProfile(name);
                    if (authorProfile != null)
                    {
                        // Add author photo if available
                        if (!string.IsNullOrEmpty(authorProfile.PhotoUrl))
                        {
                            person["image"] = authorProfile.PhotoUrl;
                        }

                        // Add author URL (their profile page or website)
                        if (!string.IsNullOrEmpty(authorProfile.RelativeLink))
                        {
                            person["url"] = Utils.AbsoluteWebRoot.ToString().TrimEnd('/') + authorProfile.RelativeLink;
                        }

                        // TODO: GEO RECOMMENDED - Add sameAs array for author social profiles
                        // This requires extending AuthorProfile with social media fields:
                        // - TwitterHandle or TwitterUrl
                        // - FacebookProfileUrl
                        // - LinkedInProfileUrl
                        // - GitHubUrl
                        // Example implementation when fields are available:
                        // var sameAsList = new List<string>();
                        // if (!string.IsNullOrEmpty(authorProfile.TwitterUrl)) sameAsList.Add(authorProfile.TwitterUrl);
                        // if (!string.IsNullOrEmpty(authorProfile.LinkedInUrl)) sameAsList.Add(authorProfile.LinkedInUrl);
                        // if (sameAsList.Any()) person["sameAs"] = sameAsList;
                    }
                }
                catch
                {
                    // If author profile not found, skip additional metadata
                }
            }

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
        /// Includes width, height, and name properties for optimal GEO performance.
        /// </remarks>
        private Dictionary<string, object> CreateImageSchema(string imageUrl)
        {
            var imageSchema = new Dictionary<string, object>
            {
                ["@type"] = "ImageObject",
                ["url"] = imageUrl
            };

            // Add standard image dimensions for better indexing
            // Using common responsive image dimensions
            imageSchema["width"] = 1200;
            imageSchema["height"] = 630;

            // Add image name derived from URL filename or generic name
            var imageName = ExtractImageName(imageUrl);
            if (!string.IsNullOrEmpty(imageName))
            {
                imageSchema["name"] = imageName;
            }

            return imageSchema;
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
        /// Extracts a friendly name from an image URL for use in structured data.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns>A friendly image name derived from the filename.</returns>
        /// <remarks>
        /// Extracts the filename from the URL, removes file extension, and converts
        /// hyphens/underscores to spaces for readability in structured data.
        /// </remarks>
        private string ExtractImageName(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            try
            {
                // Extract filename from URL
                var uri = new Uri(imageUrl);
                var filename = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(filename))
                    return null;

                // Convert hyphens and underscores to spaces and capitalize
                var friendlyName = System.Text.RegularExpressions.Regex.Replace(filename, @"[-_]", " ");

                // Capitalize first letter of each word
                var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
                return textInfo.ToTitleCase(friendlyName.ToLower());
            }
            catch
            {
                return null;
            }
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

        #region Private Methods - Keyword Extraction

        /// <summary>
        /// Extracts keywords for the blog schema from category titles (primary) and GEO Keywords (supplement).
        /// </summary>
        /// <returns>A list of keywords, or null if no keywords are available.</returns>
        /// <remarks>
        /// Keyword extraction strategy:
        /// 1. Category Keywords (PRIMARY): Extracts titles from blog categories
        /// 2. GEO Keywords (SUPPLEMENT): Adds explicit GEO keywords configured in admin settings
        /// 
        /// This ensures the schema reflects the actual organizational structure of the blog
        /// (via categories) while allowing for supplemental explicit keywords for GEO optimization.
        /// </remarks>
        private List<string> ExtractKeywords()
        {
            var keywords = new List<string>();

            // Priority 1: Extract keywords from blog categories (primary source)
            try
            {
                var categories = Category.Categories;
                if (categories != null && categories.Count > 0)
                {
                    var categoryKeywords = categories
                        .Where(c => !string.IsNullOrEmpty(c.Title))
                        .Select(c => c.Title.ToLower())
                        .Distinct(System.StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    keywords.AddRange(categoryKeywords);
                }
            }
            catch
            {
                // If category retrieval fails, continue with other sources
            }

            // Priority 2: GEO Keywords (supplement to category keywords)
            if (!string.IsNullOrEmpty(_settings.GEOKeywords))
            {
                var geoKeywords = _settings.GEOKeywords
                    .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k) && !keywords.Contains(k, System.StringComparer.OrdinalIgnoreCase))
                    .ToList();

                keywords.AddRange(geoKeywords);
            }

            return keywords.Count > 0 ? keywords : null;
        }

        /// <summary>
        /// Extracts keywords array from a post for GEO-optimized schema.
        /// </summary>
        /// <param name="post">The post to extract keywords from.</param>
        /// <returns>A list of keywords, or null if no keywords are available.</returns>
        /// <remarks>
        /// GEO PRIORITY: Keywords as array format (preferred by generative engines).
        /// Keyword extraction strategy for posts:
        /// 1. Post MetaKeywords (PRIMARY): Explicit SEO keywords set by author
        /// 2. Post Tags (SUPPLEMENT): Tags assigned to the post
        /// 3. Category Titles (FALLBACK): Categories assigned to the post
        /// </remarks>
        private List<string> ExtractKeywordsArray(Post post)
        {
            var keywords = new List<string>();

            // Priority 1: MetaKeywords (explicit SEO keywords)
            if (!string.IsNullOrEmpty(post.MetaKeywords))
            {
                var metaKeywords = post.MetaKeywords
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                keywords.AddRange(metaKeywords);
            }

            // Priority 2: Post Tags (supplement)
            if (post.Tags.Any())
            {
                var tagKeywords = post.Tags
                    .Where(t => !keywords.Contains(t, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                keywords.AddRange(tagKeywords);
            }

            // Priority 3: Post Categories (fallback)
            if (post.Categories.Any())
            {
                var categoryKeywords = post.Categories
                    .Select(c => c.Title)
                    .Where(t => !string.IsNullOrEmpty(t) && !keywords.Contains(t, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                keywords.AddRange(categoryKeywords);
            }

            return keywords.Count > 0 ? keywords : null;
        }

        /// <summary>
        /// Extracts full plain text content from HTML for articleBody schema property.
        /// </summary>
        /// <param name="htmlContent">The HTML content.</param>
        /// <returns>Full plain text content without HTML tags.</returns>
        /// <remarks>
        /// GEO CRITICAL: articleBody is the single most important field for generative engines.
        /// Without it, AI systems cannot extract summaries reliably.
        /// This method strips all HTML tags and entities to produce clean text for AI processing.
        /// </remarks>
        private string GetPlainTextContent(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return string.Empty;

            // Remove HTML tags
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);

            // Decode HTML entities
            var decoded = HttpUtility.HtmlDecode(withoutTags);

            // Remove multiple whitespace and normalize line breaks
            var normalized = System.Text.RegularExpressions.Regex.Replace(decoded, @"\s+", " ").Trim();

            return normalized;
        }

        /// <summary>
        /// Determines the language of the content for inLanguage schema property.
        /// </summary>
        /// <param name="post">The post to determine language for.</param>
        /// <returns>Language code (e.g., "en-US", "es", "fr").</returns>
        /// <remarks>
        /// GEO CRITICAL: Search engines use inLanguage to classify content for multilingual generative summaries.
        /// Priority: settings.InLanguage > system culture > default "en-US"
        /// </remarks>
        private string DetermineLanguage(Post post)
        {
            // Priority 1: Blog settings InLanguage
            if (!string.IsNullOrEmpty(_settings.InLanguage))
                return _settings.InLanguage;

            // Priority 2: System culture
            try
            {
                return System.Globalization.CultureInfo.CurrentCulture.Name;
            }
            catch
            {
                // Fallback to default
                return "en-US";
            }
        }

        /// <summary>
        /// Determines the language of the content for inLanguage schema property (Page version).
        /// </summary>
        /// <param name="page">The page to determine language for.</param>
        /// <returns>Language code (e.g., "en-US", "es", "fr").</returns>
        /// <remarks>
        /// GEO CRITICAL: Search engines use inLanguage to classify content for multilingual generative summaries.
        /// Priority: settings.InLanguage > system culture > default "en-US"
        /// </remarks>
        private string DetermineLanguagePage(Page page)
        {
            // Priority 1: Blog settings InLanguage
            if (!string.IsNullOrEmpty(_settings.InLanguage))
                return _settings.InLanguage;

            // Priority 2: System culture
            try
            {
                return System.Globalization.CultureInfo.CurrentCulture.Name;
            }
            catch
            {
                // Fallback to default
                return "en-US";
            }
        }

        /// <summary>
        /// Determines accessibility for isAccessibleForFree schema property.
        /// </summary>
        /// <param name="post">The post to determine accessibility for.</param>
        /// <returns>True if content is free, false if paywalled.</returns>
        /// <remarks>
        /// GEO RECOMMENDED: Google uses isAccessibleForFree for AI Overview.
        /// Currently defaults to true (free access) for all posts.
        /// Can be extended to check post metadata for paywall status.
        /// </remarks>
        private bool DetermineAccessibility(Post post)
        {
            // Check if post has a field indicating paywall status
            // For now, default to true (free access) for all posts
            // TODO: Add paywall detection based on post metadata or categories
            return true;
        }

        #endregion
    }
}
