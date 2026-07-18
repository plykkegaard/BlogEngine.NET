namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Blog settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Settings() { }

        /// <summary>
        /// Gets or sets the blog name.
        /// </summary>
        /// <remarks>
        /// This is the primary title or identifier for the blog.
        /// </remarks>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the blog description.
        /// </summary>
        /// <remarks>
        /// A short description or tagline for the blog, often used in feeds and metadata.
        /// </remarks>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the number of posts displayed per page.
        /// </summary>
        /// <remarks>
        /// Controls pagination of blog posts on the main page and archive listings.
        /// </remarks>
        public int PostsPerPage { get; set; }
        /// <summary>
        /// Gets or sets the cookie name used to override the default theme for a session.
        /// </summary>
        /// <remarks>
        /// Allows temporarily switching themes without changing the blog-wide default.
        /// </remarks>
        public string ThemeCookieName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the blog name is included in page titles.
        /// </summary>
        /// <remarks>
        /// When enabled, page titles will include the blog name (e.g., "Post Title - Blog Name").
        /// </remarks>
        public bool UseBlogNameInPageTitles { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether related posts functionality is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, displays related or similar posts alongside each post.
        /// </remarks>
        public bool EnableRelatedPosts { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether post ratings are enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, readers can rate posts using a rating control.
        /// </remarks>
        public bool EnableRating { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to show post descriptions in post lists.
        /// </summary>
        /// <remarks>
        /// When enabled, displays the post description or excerpt instead of the full content in lists.
        /// </remarks>
        public bool ShowDescriptionInPostList { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of characters to display in post descriptions.
        /// </summary>
        /// <remarks>
        /// Truncates post descriptions to this length in list views when ShowDescriptionInPostList is enabled.
        /// </remarks>
        public int DescriptionCharacters { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether descriptions are shown only for tag and category listings.
        /// </summary>
        /// <remarks>
        /// When enabled, post descriptions are displayed only on tag and category archive pages.
        /// </remarks>
        public bool ShowDescriptionInPostListForPostsByTagOrCategory { get; set; }
        /// <summary>
        /// Gets or sets the maximum character count for descriptions in tag and category lists.
        /// </summary>
        /// <remarks>
        /// Truncates post descriptions to this length when shown in tag and category archive listings.
        /// </remarks>
        public int DescriptionCharactersForPostsByTagOrCategory { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether post timestamps are displayed in links.
        /// </summary>
        /// <remarks>
        /// When enabled, shows the date/time information on post links.
        /// </remarks>
        public bool TimeStampPostLinks { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether post navigation controls are displayed.
        /// </summary>
        /// <remarks>
        /// When enabled, shows "Next" and "Previous" post navigation links.
        /// </remarks>
        public bool ShowPostNavigation { get; set; }
        /// <summary>
        /// Gets or sets the culture/language code for the blog.
        /// </summary>
        /// <remarks>
        /// Follows ISO 639-1 language codes, used for localization and globalization settings.
        /// </remarks>
        public string Culture { get; set; }
        /// <summary>
        /// Gets or sets the time zone identifier for the blog.
        /// </summary>
        /// <remarks>
        /// Uses IANA time zone database IDs or Windows time zone names (e.g., "America/New_York" or "Eastern Standard Time").
        /// </remarks>
        public string TimeZoneId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether file extensions are removed from URLs.
        /// </summary>
        /// <remarks>
        /// When enabled, URLs appear without file extensions (e.g., /post instead of /post.html).
        /// </remarks>
        public bool RemoveExtensionsFromUrls { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to redirect URLs to versions without file extensions.
        /// </summary>
        /// <remarks>
        /// Useful when migrating from an older blog that used extensions; automatically redirects old URLs.
        /// </remarks>
        public bool RedirectToRemoveFileExtension { get; set; }
        /// <summary>
        /// Gets or sets how the www subdomain is handled in URLs.
        /// </summary>
        /// <remarks>
        /// Values may include "Remove", "Add", or "None" to control www subdomain handling.
        /// </remarks>
        public string HandleWwwSubdomain { get; set; }
        /// <summary>
        /// Gets or sets the default desktop theme for the blog.
        /// </summary>
        /// <remarks>
        /// Specifies the theme name used when displaying the blog to desktop browsers.
        /// </remarks>
        public string DesktopTheme { get; set; }

        // advanced settings
        /// <summary>
        /// Gets or sets a value indicating whether HTTP compression is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, compresses HTTP responses using gzip to reduce bandwidth usage.
        /// </remarks>
        public bool EnableHttpCompression { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether web resources are compressed.
        /// </summary>
        /// <remarks>
        /// When enabled, CSS, JavaScript, and other resources are minified or compressed.
        /// </remarks>
        public bool CompressWebResource { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether Open Search is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, provides an OpenSearch description document for search engine autodiscovery.
        /// </remarks>
        public bool EnableOpenSearch { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether SSL/TLS is required for the MetaWeblog API.
        /// </summary>
        /// <remarks>
        /// When enabled, API clients must use HTTPS connections for secure communication.
        /// </remarks>
        public bool RequireSslForMetaWeblogApi { get; set; }

        /// <summary>
        /// Enables password reset functionality.
        /// </summary>
        /// <remarks>
        /// When enabled, users can request password resets via email.
        /// </remarks>
        public bool EnablePasswordReset { get; set; }

        /// <summary>
        /// Enables self-registration for new users.
        /// </summary>
        /// <remarks>
        /// When enabled, visitors can create accounts without administrator approval.
        /// </remarks>
        public bool EnableSelfRegistration { get; set; }
        /// <summary>
        /// Automatically creates a blog for newly self-registered users.
        /// </summary>
        /// <remarks>
        /// Only effective when self-registration is enabled.
        /// </remarks>
        public bool CreateBlogOnSelfRegistration { get; set; }
        /// <summary>
        /// Allows the server to download remote files.
        /// </summary>
        /// <remarks>
        /// When enabled, the application can fetch files from external URLs.
        /// </remarks>
        public bool AllowServerToDownloadRemoteFiles { get; set; }
        /// <summary>
        /// The timeout in milliseconds for remote file downloads.
        /// </summary>
        /// <remarks>
        /// Specifies how long the application will wait for a remote file to download before timing out.
        /// </remarks>
        public int RemoteFileDownloadTimeout { get; set; }
        /// <summary>
        /// The maximum file size in bytes for remote file downloads.
        /// </summary>
        /// <remarks>
        /// Prevents downloading excessively large files from remote sources.
        /// </remarks>
        public int RemoteMaxFileSize { get; set; }
        /// <summary>
        /// The initial role assigned to self-registered users.
        /// </summary>
        /// <remarks>
        /// Specifies the default user role for newly self-registered user accounts.
        /// </remarks>
        public string SelfRegistrationInitialRole { get; set; }

        // feed
        /// <summary>
        /// The author name for the feed.
        /// </summary>
        /// <remarks>
        /// Used in feed metadata and author attribution.
        /// </remarks>
        public string AuthorName { get; set; }
        /// <summary>
        /// The feed author information.
        /// </summary>
        /// <remarks>
        /// Represents the author details displayed in the feed.
        /// </remarks>
        public string FeedAuthor { get; set; }
        /// <summary>
        /// Endorsement or certification information.
        /// </summary>
        /// <remarks>
        /// May contain certification details or endorsement information.
        /// </remarks>
        public string Endorsement { get; set; }
        /// <summary>
        /// An alternate feed URL for syndication.
        /// </summary>
        /// <remarks>
        /// Allows specifying an alternative feed URL instead of the default.
        /// </remarks>
        public string AlternateFeedUrl { get; set; }
        /// <summary>
        /// The language code for the blog content.
        /// </summary>
        /// <remarks>
        /// Follows ISO 639-1 language codes (e.g., "en", "en-US").
        /// </remarks>
        public string Language { get; set; }
        /// <summary>
        /// The number of posts per feed page.
        /// </summary>
        /// <remarks>
        /// Controls how many posts are included in each feed page.
        /// </remarks>
        public int PostsPerFeed { get; set; }
        /// <summary>
        /// Enables enclosures in feed items.
        /// </summary>
        /// <remarks>
        /// When enabled, allows media files to be included in feed entries.
        /// </remarks>
        public bool EnableEnclosures { get; set; }
        /// <summary>
        /// Enables tag export functionality.
        /// </summary>
        /// <remarks>
        /// When enabled, tags can be exported from the blog.
        /// </remarks>
        public bool EnableTagExport { get; set; }
        /// <summary>
        /// The syndication feed format (e.g., "RSS", "ATOM").
        /// </summary>
        /// <remarks>
        /// Specifies the format used for RSS/ATOM feed generation.
        /// </remarks>
        public string SyndicationFormat { get; set; }

        // email
        /// <summary>
        /// The blog administrator email address.
        /// </summary>
        /// <remarks>
        /// Used for notifications and contact communications.
        /// </remarks>
        public string Email { get; set; }
        /// <summary>
        /// The SMTP server address for email delivery.
        /// </summary>
        /// <remarks>
        /// Required for the application to send emails.
        /// </remarks>
        public string SmtpServer { get; set; }
        /// <summary>
        /// The port number for the SMTP server.
        /// </summary>
        /// <remarks>
        /// Common values are 25 (unencrypted), 587 (TLS), and 465 (SSL).
        /// </remarks>
        public int SmtpServerPort { get; set; }
        /// <summary>
        /// The username for SMTP authentication.
        /// </summary>
        /// <remarks>
        /// Required if the SMTP server requires authentication.
        /// </remarks>
        public string SmtpUserName { get; set; }
        /// <summary>
        /// The password for SMTP authentication.
        /// </summary>
        /// <remarks>
        /// Required if the SMTP server requires authentication. Should be securely stored and encrypted.
        /// </remarks>
        public string SmtpPassword { get; set; }
        /// <summary>
        /// A prefix to add to the subject line of outgoing emails.
        /// </summary>
        /// <remarks>
        /// Useful for filtering and organizing email messages.
        /// </remarks>
        public string EmailSubjectPrefix { get; set; }
        /// <summary>
        /// Enables SSL/TLS encryption for SMTP connections.
        /// </summary>
        /// <remarks>
        /// When enabled, secures the SMTP connection with SSL/TLS encryption.
        /// </remarks>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// Sends an email notification when a new comment is posted.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog administrator receives email notifications for new comments.
        /// </remarks>
        public bool SendMailOnComment { get; set; }

        // controls
        /// <summary>
        /// The text displayed on the search button.
        /// </summary>
        /// <remarks>
        /// Customizes the label for the search functionality button.
        /// </remarks>
        public string SearchButtonText { get; set; }
        /// <summary>
        /// The label text for the search comments option.
        /// </summary>
        /// <remarks>
        /// Displays text next to the checkbox for including comments in search.
        /// </remarks>
        public string SearchCommentLabelText { get; set; }
        /// <summary>
        /// The default placeholder text for the search input field.
        /// </summary>
        /// <remarks>
        /// Shown as placeholder when the search field is empty.
        /// </remarks>
        public string SearchDefaultText { get; set; }
        /// <summary>
        /// Enables searching within comments.
        /// </summary>
        /// <remarks>
        /// When enabled, users can search for text within post comments.
        /// </remarks>
        public bool EnableCommentSearch { get; set; }
        /// <summary>
        /// Shows an option to include comments in search results.
        /// </summary>
        /// <remarks>
        /// When enabled, users can opt to include comments when performing searches.
        /// </remarks>
        public bool ShowIncludeCommentsOption { get; set; }
        /// <summary>
        /// The message displayed on the contact form.
        /// </summary>
        /// <remarks>
        /// Customizable message shown to users on the contact form page.
        /// </remarks>
        public string ContactFormMessage { get; set; }
        /// <summary>
        /// The thank you message displayed after form submission.
        /// </summary>
        /// <remarks>
        /// Shown to users after they successfully submit the contact form.
        /// </remarks>
        public string ContactThankMessage { get; set; }
        /// <summary>
        /// The error message displayed when form submission fails.
        /// </summary>
        /// <remarks>
        /// Shown to users if there is an error submitting the contact form.
        /// </remarks>
        public string ContactErrorMessage { get; set; }
        /// <summary>
        /// Enables file attachments in the contact form.
        /// </summary>
        /// <remarks>
        /// When enabled, users can attach files to their contact form submissions.
        /// </remarks>
        public bool EnableContactAttachments { get; set; }
        /// <summary>
        /// Enables reCAPTCHA verification on the contact form.
        /// </summary>
        /// <remarks>
        /// When enabled, adds reCAPTCHA validation to prevent spam submissions.
        /// </remarks>
        public bool EnableRecaptchaOnContactForm { get; set; }
        /// <summary>
        /// The title displayed on error pages.
        /// </summary>
        /// <remarks>
        /// Customizable title for error notification pages.
        /// </remarks>
        public string ErrorTitle { get; set; }
        /// <summary>
        /// The text displayed on error pages.
        /// </summary>
        /// <remarks>
        /// Customizable text content for error notification pages.
        /// </remarks>
        public string ErrorText { get; set; }

        // custom code
        /// <summary>
        /// Custom HTML to be inserted in the page header.
        /// </summary>
        /// <remarks>
        /// Allows adding custom HTML or meta tags to the page header.
        /// </remarks>
        public string HtmlHeader { get; set; }
        /// <summary>
        /// Custom tracking script code (e.g., Google Analytics).
        /// </summary>
        /// <remarks>
        /// Allows inserting analytics or tracking scripts into the page.
        /// </remarks>
        public string TrackingScript { get; set; }

        // comments
        /// <summary>
        /// The number of days after posting that comments remain enabled.
        /// </summary>
        /// <remarks>
        /// After this period, comments are automatically closed for the post.
        /// </remarks>
        public int DaysCommentsAreEnabled { get; set; }
        /// <summary>
        /// Indicates whether comments are globally enabled on the blog.
        /// </summary>
        /// <remarks>
        /// When disabled, comments are closed for all posts.
        /// </remarks>
        public bool IsCommentsEnabled { get; set; }
        /// <summary>
        /// Requires comments to be moderated before publication.
        /// </summary>
        /// <remarks>
        /// When enabled, new comments must be approved by an administrator before appearing.
        /// </remarks>
        public bool EnableCommentsModeration { get; set; }
        /// <summary>
        /// Enables nested/threaded comment replies.
        /// </summary>
        /// <remarks>
        /// When enabled, users can reply directly to other comments, creating comment threads.
        /// </remarks>
        public bool IsCommentNestingEnabled { get; set; }
        /// <summary>
        /// The avatar provider service (e.g., "Gravatar", "Local").
        /// </summary>
        /// <remarks>
        /// Specifies which service to use for displaying user avatars.
        /// </remarks>
        public string Avatar { get; set; }
        /// <summary>
        /// Enables sending pingbacks to other blogs.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog automatically sends pingbacks when linking to other blogs.
        /// </remarks>
        public bool EnablePingBackSend { get; set; }
        /// <summary>
        /// Enables receiving pingbacks from other blogs.
        /// </summary>
        /// <remarks>
        /// When enabled, other blogs can notify this blog of incoming links.
        /// </remarks>
        public bool EnablePingBackReceive { get; set; }
        /// <summary>
        /// Enables sending trackbacks to other blogs.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog can send trackback notifications to other blogs.
        /// </remarks>
        public bool EnableTrackBackSend { get; set; }
        /// <summary>
        /// Enables receiving trackbacks from other blogs.
        /// </summary>
        /// <remarks>
        /// When enabled, other blogs can send trackback notifications to this blog.
        /// </remarks>
        public bool EnableTrackBackReceive { get; set; }
        /// <summary>
        /// The number of comments displayed per page.
        /// </summary>
        /// <remarks>
        /// Controls pagination of comments on post pages.
        /// </remarks>
        public int CommentsPerPage { get; set; }
        /// <summary>
        /// Displays the commenter's country in comment information.
        /// </summary>
        /// <remarks>
        /// When enabled, shows country information for commenters based on their IP address.
        /// </remarks>
        public bool EnableCountryInComments { get; set; }
        /// <summary>
        /// Displays the commenter's website URL in comment information.
        /// </summary>
        /// <remarks>
        /// When enabled, shows website information for commenters who provide it.
        /// </remarks>
        public bool EnableWebsiteInComments { get; set; }
        /// <summary>
        /// Enables live preview of comments as the user types.
        /// </summary>
        /// <remarks>
        /// When enabled, users can see a real-time preview of their comment before submission.
        /// </remarks>
        public bool ShowLivePreview { get; set; }

        /// <summary>
        /// The comment provider service (e.g., "Internal", "Disqus", "Facebook").
        /// </summary>
        /// <remarks>
        /// Specifies which service is used for managing blog comments.
        /// </remarks>
        public BlogSettings.CommentsBy CommentProvider { get; set; }
        /// <summary>
        /// Enables Disqus development mode.
        /// </summary>
        /// <remarks>
        /// When enabled, Disqus operates in development mode for testing purposes.
        /// </remarks>
        public bool DisqusDevMode { get; set; }
        /// <summary>
        /// Enables Disqus comments on static pages.
        /// </summary>
        /// <remarks>
        /// When enabled, Disqus comments are added to static pages in addition to blog posts.
        /// </remarks>
        public bool DisqusAddCommentsToPages { get; set; }
        /// <summary>
        /// The Disqus website shortname.
        /// </summary>
        /// <remarks>
        /// Required for Disqus integration; obtained from your Disqus account settings.
        /// </remarks>
        public string DisqusWebsiteName { get; set; }
        /// <summary>
        /// The Facebook App ID for Facebook comments integration.
        /// </summary>
        /// <remarks>
        /// Required for Facebook comment plugin; obtained from your Facebook Developer account.
        /// </remarks>
        public string FacebookAppId { get; set; }
        /// <summary>
        /// The language code for Facebook comments.
        /// </summary>
        /// <remarks>
        /// Specifies the language in which Facebook comments will be displayed.
        /// </remarks>
        public string FacebookLanguage { get; set; }

        // custom filters
        /// <summary>
        /// Trusts content from authenticated users without filtering.
        /// </summary>
        /// <remarks>
        /// When enabled, content from authenticated users bypasses certain security filters.
        /// </remarks>
        public bool TrustAuthenticatedUsers { get; set; }

        // SEO & GEO Settings
        /// <summary>
        /// Gets or sets the blog-wide SEO title suffix.
        /// </summary>
        /// <remarks>
        /// Appended to page titles for search engine optimization (e.g., " - My Blog").
        /// </remarks>
        public string SeoTitleSuffix { get; set; }
        /// <summary>
        /// Gets or sets the canonical domain for SEO purposes.
        /// </summary>
        /// <remarks>
        /// Used in canonical URLs to avoid duplicate content issues in search engines.
        /// </remarks>
        public string SeoCanonicalDomain { get; set; }
        /// <summary>
        /// Gets or sets the default author name for SEO metadata.
        /// </summary>
        /// <remarks>
        /// Used as the default author in posts when no specific author is assigned.
        /// </remarks>
        public string SeoDefaultAuthor { get; set; }
        /// <summary>
        /// Gets or sets the default image URL for social sharing and SEO.
        /// </summary>
        /// <remarks>
        /// Used when sharing blog posts on social media if no post-specific image is available.
        /// </remarks>
        public string SeoDefaultImage { get; set; }
        /// <summary>
        /// Gets or sets the Twitter handle for social cards.
        /// </summary>
        /// <remarks>
        /// Used in Twitter Card metadata for attribution when posts are shared on Twitter.
        /// </remarks>
        public string SeoTwitterHandle { get; set; }
        /// <summary>
        /// Gets or sets the Facebook App ID for Open Graph integration.
        /// </summary>
        /// <remarks>
        /// Required for proper Open Graph metadata handling when posts are shared on Facebook.
        /// </remarks>
        public string SeoFacebookAppId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether Open Graph metadata is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, generates Open Graph meta tags for better social media integration.
        /// </remarks>
        public bool SeoEnableOpenGraph { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether Twitter Card metadata is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, generates Twitter Card meta tags for rich previews when posts are shared on Twitter.
        /// </remarks>
        public bool SeoEnableTwitterCard { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether structured data (Schema.org) is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, generates structured data (JSON-LD) markup for enhanced search engine understanding.
        /// </remarks>
        public bool SeoEnableStructuredData { get; set; }
        /// <summary>
        /// Gets or sets the organization name for structured data.
        /// </summary>
        /// <remarks>
        /// Used in JSON-LD structured data to identify the organization or blog.
        /// </remarks>
        public string SeoOrganizationName { get; set; }
        /// <summary>
        /// Gets or sets the organization logo URL for structured data.
        /// </summary>
        /// <remarks>
        /// Used in JSON-LD structured data and search engine branding.
        /// </remarks>
        public string SeoOrganizationLogo { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether GEO (Generative Engine Optimization) is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, optimizes content for generative AI search engines and systems like ChatGPT.
        /// </remarks>
        public bool GeoOptimizationEnabled { get; set; }
        /// <summary>
        /// Gets or sets the GEO optimization mode (e.g., "Basic", "Advanced").
        /// </summary>
        /// <remarks>
        /// Controls the level of optimization applied for generative AI search engines.
        /// </remarks>
        public string GeoOptimizationMode { get; set; }
        /// <summary>
        /// Gets or sets the metadata richness level for GEO (e.g., "Minimal", "Standard", "Rich").
        /// </summary>
        /// <remarks>
        /// Determines how much metadata and schema information is included for AI systems.
        /// </remarks>
        public string GeoMetadataRichness { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether citation optimization is enabled for GEO.
        /// </summary>
        /// <remarks>
        /// When enabled, optimizes content for proper citation in AI-generated summaries.
        /// </remarks>
        public bool GeoEnableCitationOptimization { get; set; }

        /// <summary>
        /// Gets or sets the language code for Schema.org markup (inLanguage).
        /// </summary>
        /// <remarks>
        /// Specifies the primary language of the content using ISO 639-1 codes for structured data.
        /// </remarks>
        public string InLanguage { get; set; }

        /// <summary>
        /// Gets or sets GEO keywords for content discovery.
        /// </summary>
        /// <remarks>
        /// Keywords optimized for generative AI systems to enhance content discoverability.
        /// </remarks>
        public string GEOKeywords { get; set; }

        /// <summary>
        /// Gets or sets the GEO potential actions JSON.
        /// </summary>
        /// <remarks>
        /// JSON structured data defining potential actions related to the content for AI interpretation.
        /// </remarks>
        public string GEOPotentialActions { get; set; }

        /// <summary>
        /// Gets or sets the social media and profile URLs (sameAs) JSON array.
        /// </summary>
        /// <remarks>
        /// JSON array of social media and profile URLs for structured data identity consolidation.
        /// </remarks>
        public string SameAs { get; set; }
    }
}
