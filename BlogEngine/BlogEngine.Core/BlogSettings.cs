namespace BlogEngine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;

    using BlogEngine.Core.Providers;
    using System.Web.Hosting;
    using System.IO;
    using System.Web.Caching;

    /// <summary>
    /// Represents the configured settings for the blog engine.
    /// </summary>
    /// <remarks>
    /// This class holds configuration values loaded from the application
    /// settings and provides runtime access to themes, compression options,
    /// comment paging limits, and other behavioral flags used throughout the
    /// BlogEngine. It follows a singleton pattern per blog instance so that
    /// each hosted site can maintain its own independent configuration.
    /// </remarks>
    public class BlogSettings
    {
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS

        /// <summary>
        ///     Occurs when [changed].
        /// </summary>
        public static event EventHandler<EventArgs> Changed;

        /// <summary>
        ///     The blog settings singleton.
        /// </summary>
        /// <remarks>
        /// This should be created immediately instead of lazyloaded. It'll reduce the number of null checks that occur
        /// due to heavy reliance on calls to BlogSettings.Instance.
        /// </remarks>
        private static readonly Dictionary<Guid, BlogSettings> blogSettingsSingleton = new Dictionary<Guid, BlogSettings>();

        /// <summary>
        ///     The configured theme.
        /// </summary>
        private string configuredTheme = String.Empty;

        /// <summary>
        ///     The number of comments per page.
        /// </summary>
        private int commentsPerPage;

        /// <summary>
        ///     The enable http compression.
        /// </summary>
        private bool enableHttpCompression;

        /// <summary>
        ///     Whether passwords can be reset.
        /// </summary>
        private bool enablePasswordResets = true;

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Gets or sets the timeout in milliseconds for a remote download.
        /// </summary>
        /// <remarks>
        /// This field stores the configured timeout duration for remote file downloads.
        /// If set to a negative value, the default timeout of 30000 milliseconds (30 seconds) will be used instead.
        /// A value of 0 indicates unlimited time.
        /// </remarks>
        private int remoteDownloadTimeout = defaultRemoteDownloadTimeout;

        /// <summary>
        /// The default timeout in milliseconds for remote downloads (30 seconds).
        /// </summary>
        /// <remarks>
        /// This constant is used as the default timeout value when no explicit timeout is configured
        /// or when an invalid negative value is provided.
        /// </remarks>
        private const int defaultRemoteDownloadTimeout = 30000;

        /// <summary>
        /// The maximum size in bytes for remote files that can be downloaded by the server.
        /// </summary>
        /// <remarks>
        /// This field stores the configured maximum file size. If set to a negative value,
        /// the default maximum of 524288 bytes (512 KB) will be used instead.
        /// </remarks>
        private int maxRemoteFileSize = defaultMaxRemoteFileSize;

        /// <summary>
        /// The default maximum remote file size in bytes (512 KB).
        /// </summary>
        /// <remarks>
        /// This constant is used as the default value for remote file downloads when no explicit
        /// maximum is configured or when an invalid negative value is provided.
        /// </remarks>
        private const int defaultMaxRemoteFileSize = 524288;

        #endregion

        #region BlogSettings()

        /// <summary>
        ///     Prevents a default instance of the <see cref = "BlogSettings" /> class from being created. 
        ///     Initializes a new instance of the <see cref = "BlogSettings" /> class.
        /// </summary>
        /// <remarks>
        /// This private constructor ensures that BlogSettings instances can only be created
        /// internally through the static factory methods. It immediately loads the blog settings
        /// for the current blog instance.
        /// </remarks>
        private BlogSettings()
        {
            this.Load(Blog.CurrentInstance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogSettings"/> class for a specific blog.
        /// </summary>
        /// <param name="blog">The blog instance to load settings for.</param>
        /// <remarks>
        /// This private constructor is used internally by the singleton factory methods to create
        /// new BlogSettings instances. It immediately loads all configured settings from the blog's
        /// settings provider into this instance.
        /// </remarks>
        private BlogSettings(Blog blog)
        {
            this.Load(blog);
        }

        #endregion

        /// <summary>
        ///     Gets the singleton instance of the <see cref = "BlogSettings" /> class.
        /// </summary>
        /// <value>A singleton instance of the <see cref = "BlogSettings" /> class.</value>
        /// <remarks>
        /// Returns the settings singleton for the currently active blog instance. The singleton is created on first access
        /// and cached for reuse. This property provides the primary access point for retrieving blog settings in the application.
        /// </remarks>
        public static BlogSettings Instance
        {
            get
            {
                return GetInstanceSettings(Blog.CurrentInstance);
            }
        }

        /// <summary>
        /// Returns the settings for the requested blog instance.
        /// </summary>
        /// <param name="blog">The blog instance to retrieve settings for.</param>
        /// <remarks>
        /// This static factory method retrieves or creates the singleton settings instance for a specific blog.
        /// The method uses thread-safe double-checked locking to ensure that only one instance is created per blog,
        /// even in multi-threaded scenarios. Settings are loaded from the blog's settings provider upon creation.
        /// </remarks>
        /// <returns>
        /// The BlogSettings singleton instance for the specified blog. The same instance will be returned for
        /// subsequent calls with the same blog instance.
        /// </returns>
        public static BlogSettings GetInstanceSettings(Blog blog)
        {
            BlogSettings blogSettings;

            if (!blogSettingsSingleton.TryGetValue(blog.Id, out blogSettings))
            {
                lock (SyncRoot)
                {
                    if (!blogSettingsSingleton.TryGetValue(blog.Id, out blogSettings))
                    {
                        // settings will be loaded in constructor.
                        blogSettings = new BlogSettings(blog);

                        blogSettingsSingleton[blog.Id] = blogSettings;
                    }
                }
            }

            return blogSettings;
        }

        /// <summary>
        /// Cached indicator of whether the current theme is a Razor theme.
        /// </summary>
        /// <remarks>
        /// This nullable boolean field caches the result of theme type detection to avoid repeated
        /// file system checks. A null value indicates the check has not yet been performed.
        /// </remarks>
        private bool? _isRazorTheme;
        /// <summary>
        /// Gets whether the current theme is a Razor theme.
        /// </summary>
        /// <remarks>
        /// This property checks the configured theme to determine if it uses Razor view engine syntax
        /// (site.cshtml) rather than traditional ASPX syntax. The result is cached to minimize file system
        /// access for repeated checks. Use this property to determine which view engine to use when rendering
        /// theme content.
        /// </remarks>
        public bool IsRazorTheme
        {
            get
            {
                if (_isRazorTheme.HasValue) { return _isRazorTheme.Value; }

                _isRazorTheme = IsThemeRazor(this.Theme);
                return _isRazorTheme.Value;
            }
        }

        /// <summary>
        /// Determines if a theme with the specified name is a Razor theme.
        /// </summary>
        /// <param name="themeName">The name of the theme directory to check.</param>
        /// <remarks>
        /// This static method checks whether a theme uses the Razor view engine by looking for the presence
        /// of a site.cshtml file in the theme directory. This is useful for determining how to properly render
        /// the theme without requiring an instance of BlogSettings.
        /// </remarks>
        /// <returns>
        /// True if the theme contains a site.cshtml file indicating it is a Razor theme; otherwise, false.
        /// </returns>
        public static bool IsThemeRazor(string themeName)
        {
            string path = HostingEnvironment.MapPath($"~/Custom/Themes/{themeName}/site.cshtml");
            return File.Exists(path);
        }

        /// <summary>
        /// Gets the effective theme folder name accounting for theme overrides and Razor theme compatibility.
        /// </summary>
        /// <param name="themeOverride">An optional theme name that overrides the configured theme. If provided, this theme will be used instead.</param>
        /// <remarks>
        /// This method determines which theme folder should actually be used by taking into account:
        /// (1) Whether a theme override is specified for this request
        /// (2) Whether the effective theme is a Razor theme (which requires the RazorHost wrapper)
        /// (3) The configured default theme for the blog
        /// 
        /// When a theme is a Razor theme, the method returns "RazorHost" as a compatibility wrapper.
        /// Otherwise, it returns the actual theme folder name.
        /// </remarks>
        /// <returns>
        /// The actual theme folder to use, either "RazorHost" for Razor-based themes or the theme name for ASPX themes.
        /// </returns>
        public string GetThemeWithAdjustments(string themeOverride)
        {
            string theme = this.Theme;
            bool isRazorTheme = configuredTheme == theme ? IsRazorTheme : IsThemeRazor(theme);
            if (!string.IsNullOrWhiteSpace(themeOverride))
            {
                theme = themeOverride;
                isRazorTheme = IsThemeRazor(theme);
            }
            return isRazorTheme ? "RazorHost" : theme;
        }

        #region Description

        /// <summary>
        ///     Gets or sets the description of the blog.
        /// </summary>
        /// <value>A brief synopsis of the blog content.</value>
        /// <remarks>
        ///     This value is also used for the description meta tag in the HTML head section,
        ///     making it important for search engine optimization (SEO). The description should
        ///     be concise and accurately summarize the blog's purpose and content.
        /// </remarks>
        public string Description { get; set; }

        #endregion

        #region EnableHttpCompression

        /// <summary>
        ///     Gets or sets a value indicating if HTTP compression is enabled.
        /// </summary>
        /// <value><b>true</b> if compression is enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// HTTP compression reduces bandwidth usage by compressing response content using GZIP.
        /// This setting is automatically disabled on Mono platforms due to compatibility issues.
        /// Enabling compression can significantly improve page load times for end users.
        /// </remarks>
        public bool EnableHttpCompression
        {
            get
            {
                return this.enableHttpCompression && !Utils.IsMono;
            }

            set
            {
                this.enableHttpCompression = value;
            }
        }

        #endregion

        #region EnableReferrerTracking

        /// <summary>
        ///     Gets or sets a value indicating if referral tracking is enabled.
        /// </summary>
        /// <value><b>true</b> if referral tracking is enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the blog tracks and logs the referrer sources that bring visitors to the site.
        /// This information is used to understand traffic patterns and visitor sources. Combined with
        /// NumberOfReferrerDays, this setting controls how referrer data is collected and retained.
        /// </remarks>
        public bool EnableReferrerTracking { get; set; }

        #endregion

        #region NumberOfReferrerDays

        /// <summary>
        ///     Gets or sets a value indicating the number of days that referrer information should be stored.
        /// </summary>
        /// <remarks>
        /// This setting determines the retention period for referrer tracking data. Once a referrer record
        /// exceeds this age, it may be removed during cleanup operations. This helps manage storage space while
        /// still maintaining a reasonable historical view of traffic sources. Works in conjunction with
        /// EnableReferrerTracking to control referrer analytics.
        /// </remarks>
        public int NumberOfReferrerDays { get; set; }

        #endregion

        #region EnableRelatedPosts

        /// <summary>
        ///     Gets or sets a value indicating if related posts are displayed.
        /// </summary>
        /// <value><b>true</b> if related posts are displayed, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the blog displays posts related to the current post based on shared categories or tags.
        /// This feature helps increase engagement by suggesting relevant content to readers and can improve
        /// time spent on site and click-through rates.
        /// </remarks>
        public bool EnableRelatedPosts { get; set; }

        #endregion

        #region AlternateFeedUrl

        /// <summary>
        ///     Gets or sets the alternate feed URL.
        /// </summary>
        /// <remarks>
        /// This setting allows configuration of an alternate feed service URL, commonly used to integrate
        /// with feed distribution services like FeedBurner. When specified, feed requests may be redirected
        /// to this URL to provide additional analytics and feed management capabilities.
        /// </remarks>
        public string AlternateFeedUrl { get; set; }

        #endregion

        #region FeedAuthor

        /// <summary>
        /// Gets or sets the author name used in RSS feed entries.
        /// </summary>
        /// <remarks>
        /// This setting specifies the author name to be included in the RSS syndication feed metadata.
        /// It's displayed in feed readers to identify the blog's author. Leave empty to use the default
        /// blog author name configured elsewhere in the system.
        /// </remarks>
        public string FeedAuthor { get; set; }

        #endregion

        #region TimeStampPostLinks

        /// <summary>
        ///     Gets or sets whether or not to time stamp post links.
        /// </summary>
        /// <remarks>
        /// When enabled, post URLs will include timestamps or other temporal information,
        /// which can be useful for URL generation and organizational purposes. This affects
        /// how post links are constructed and displayed throughout the blog.
        /// </remarks>
        public bool TimeStampPostLinks { get; set; }

        #endregion

        #region Name

        /// <summary>
        ///     Gets or sets the name of the blog.
        /// </summary>
        /// <value>The title of the blog.</value>
        /// <remarks>
        /// This is the primary title/name of the blog, displayed in page headers and feed metadata.
        /// It serves as the blog's primary identifier and is used extensively throughout the user interface
        /// and in search engine optimization (SEO) elements.
        /// </remarks>
        public string Name { get; set; }

        #endregion

        #region PostsPerPage

        /// <summary>
        ///     Gets or sets the number of posts to show on each page.
        /// </summary>
        /// <value>The number of posts to show on each page.</value>
        /// <remarks>
        /// This setting controls pagination in post listings. It determines how many posts are displayed
        /// on archive pages, category pages, and tag pages. Affects both the blog homepage and filtered views.
        /// Lower values require more page navigation while higher values may impact page load performance.
        /// </remarks>
        public int PostsPerPage { get; set; }

        #endregion

        #region ShowLivePreview

        /// <summary>
        ///     Gets or sets a value indicating if live preview of post is displayed.
        /// </summary>
        /// <value><b>true</b> if live previews are displayed, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the post editor displays a live preview of post content as the author types,
        /// allowing real-time preview of formatting and markup rendering without needing to publish or save.
        /// This improves the authoring experience and helps catch formatting issues before publication.
        /// </remarks>
        public bool ShowLivePreview { get; set; }

        #endregion

        #region EnableRating

        /// <summary>
        ///     Gets or sets a value indicating if post and comment ratings are enabled.
        /// </summary>
        /// <value><b>true</b> if post/comment ratings are enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, visitors can rate posts and comments using a rating widget (typically 1-5 stars).
        /// This provides feedback on content quality and helps surface popular content. Rating data can be
        /// used for analytics and to identify the most valuable content.
        /// </remarks>
        public bool EnableRating { get; set; }

        #endregion

        #region ShowDescriptionInPostList

        /// <summary>
        ///     Gets or sets a value indicating if the full post is displayed in lists or only the description/excerpt.
        /// </summary>
        /// <remarks>
        /// When enabled, post listings display the complete post content. When disabled, only the post description
        /// or excerpt is shown, with a "Read more" link to the full post. This affects user experience and can impact
        /// page load times. The number of characters displayed can be controlled via DescriptionCharacters.
        /// </remarks>
        public bool ShowDescriptionInPostList { get; set; }

        #endregion

        #region DescriptionCharacters

        /// <summary>
        ///     Gets or sets a value indicating how many characters should be shown of the description.
        /// </summary>
        /// <remarks>
        /// This setting controls the maximum length of post descriptions displayed in list views.
        /// It only applies when ShowDescriptionInPostList is enabled. Text exceeding this character count
        /// will be truncated with an ellipsis (...) or a "Read more" link, depending on theme implementation.
        /// </remarks>
        public int DescriptionCharacters { get; set; }

        #endregion

        #region ShowDescriptionInPostListForPostsByTagOrCategory

        /// <summary>
        ///     Gets or sets a value indicating if the full post is displayed in lists by tag/category or only the description/excerpt.
        /// </summary>
        /// <remarks>
        /// This setting specifically controls description display behavior for post listings filtered by tag or category.
        /// When enabled, full post content is shown; when disabled, only excerpts are displayed. This may be configured
        /// differently from the general ShowDescriptionInPostList to provide different user experience for filtered views.
        /// </remarks>
        public bool ShowDescriptionInPostListForPostsByTagOrCategory { get; set; }

        #endregion

        #region DescriptionCharactersForPostsByTagOrCategory

        /// <summary>
        ///     Gets or sets a value indicating how many characters should be shown of the description when posts are shown by tag or category.
        /// </summary>
        /// <remarks>
        /// This setting controls excerpt length specifically for tag and category listing pages.
        /// It allows independent configuration from the general DescriptionCharacters setting,
        /// enabling different excerpt lengths for filtered views versus the main post listing.
        /// </remarks>
        public int DescriptionCharactersForPostsByTagOrCategory { get; set; }

        #endregion

        #region Enclosure support

        /// <summary>
        ///     Gets or sets a value indicating whether to enable enclosures for RSS feeds.
        /// </summary>
        /// <remarks>
        /// When enabled, RSS feed entries can include enclosures (typically audio or video files).
        /// Enclosures are used for podcast distribution and multimedia content syndication.
        /// This setting must be enabled for readers to properly handle podcast content in feeds.
        /// </remarks>
        public bool EnableEnclosures { get; set; }

        #endregion

        #region Tags Export

        /// <summary>
        ///     Gets or sets a value indicating whether to enable exporting of tags in RSS feeds.
        /// </summary>
        /// <remarks>
        /// When enabled, RSS feed entries include tag information using the category element.
        /// This allows feed readers and aggregators to understand and organize post content by topic.
        /// Disabling this can reduce feed size slightly but removes valuable metadata from syndication.
        /// </remarks>
        public bool EnableTagExport { get; set; }

        #endregion

        #region SyndicationFormat

        /// <summary>
        ///     Gets or sets the default syndication format used by the blog.
        /// </summary>
        /// <value>The default syndication format used by the blog.</value>
        /// <remarks>
        ///     If no value is specified, blog defaults to using RSS 2.0 format.
        /// </remarks>
        /// <seealso cref = "BlogEngine.Core.SyndicationFormat" />
        public string SyndicationFormat { get; set; }

        #endregion

        #region ThemeCookieName

        /// <summary>
        ///     The default theme cookie name.
        /// </summary>
        private const string DefaultThemeCookieName = "theme";

        /// <summary>
        ///     The theme cookie name.
        /// </summary>
        private string themeCookieName;

        /// <summary>
        ///     Gets or sets the name of the cookie that can override
        ///     the selected theme.
        /// </summary>
        /// <value>The name of the cookie that is checked while determining the theme.</value>
        /// <remarks>
        ///     The default value is "theme".
        /// </remarks>
        public string ThemeCookieName
        {
            get
            {
                return this.themeCookieName ?? DefaultThemeCookieName;
            }

            set
            {
                this.themeCookieName = value != DefaultThemeCookieName ? value : null;
            }
        }

        #endregion

        #region Theme

        /// <summary>
        ///     Gets or sets the current theme applied to the blog.
        ///     Default theme can be overridden in the query string
        ///     or cookie to let users select different theme
        /// </summary>
        /// <value>The configured theme for the blog.</value>
        public string Theme
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    var request = context.Request;
                    if (request.QueryString["theme"] != null)
                    {
                        return request.QueryString["theme"].SanitizePath();
                    }

                    var cookie = request.Cookies[this.ThemeCookieName];
                    if (cookie != null)
                    {
                        return cookie.Value.SanitizePath();
                    }

                    if (Utils.ShouldForceMainTheme(request))
                    {
                        return this.configuredTheme.SanitizePath();
                    }
                }

                return this.configuredTheme.SanitizePath();
            }

            set
            {
                this.configuredTheme = String.IsNullOrEmpty(value) ? String.Empty : value.SanitizePath();
            }
        }

        #endregion

        #region CompressWebResource

        /// <summary>
        ///     Gets or sets a value indicating whether to compress WebResource.axd responses.
        /// </summary>
        /// <value><c>true</c> if WebResource.axd responses should be compressed; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// WebResource.axd is an ASP.NET handler that serves embedded resources (scripts, styles). Compressing these
        /// responses can reduce bandwidth usage. This setting works in conjunction with EnableHttpCompression but allows
        /// independent control over WebResource compression specifically.
        /// </remarks>
        public bool CompressWebResource { get; set; }

        #endregion

        #region EnableOptimization

        /// <summary>
        ///     Gets or sets a value indicating whether general optimization features are enabled.
        /// </summary>
        /// <remarks>
        /// This property is deprecated and no longer actively used. It is maintained for backward compatibility
        /// with existing configurations but will be removed in a future version. Individual optimization features
        /// should be controlled through their specific settings instead.
        /// </remarks>
        public bool EnableOptimization { get; set; }

        #endregion 

        #region UseBlogNameInPageTitles

        /// <summary>
        ///     Gets or sets a value indicating if the blog name should be included in page titles.
        /// </summary>
        /// <value><b>true</b> if blog name is included in page titles, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the blog name (configured in the Name property) is automatically appended to all page titles
        /// throughout the site. This is useful for SEO and branding purposes, allowing visitors to immediately identify
        /// which blog they're visiting. For example, a post title becomes "Post Title - Blog Name".
        /// </remarks>
        public bool UseBlogNameInPageTitles { get; set; }

        #endregion

        #region RequireSSLMetaWeblogAPI;

        /// <summary>
        ///     Gets or sets a value indicating whether SSL is required for MetaWeblog API connections.
        /// </summary>
        /// <remarks>
        /// The MetaWeblog API allows remote clients to publish posts to the blog programmatically. When this setting is enabled,
        /// connections to the MetaWeblog API endpoint must use HTTPS instead of HTTP, improving security for remote publishing.
        /// This should be enabled when allowing external tools or services to manage blog content.
        /// </remarks>
        public bool RequireSslMetaWeblogApi { get; set; }

        #endregion

        #region EnableOpenSearch

        /// <summary>
        ///     Gets or sets a value indicating if OpenSearch support is enabled for the blog.
        /// </summary>
        /// <value><b>true</b> if OpenSearch is enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// OpenSearch is a standard that allows web browsers and clients to discover and use search plugins from websites.
        /// When enabled, the blog advertises its search capabilities via an OpenSearch description document, allowing users
        /// to add the blog as a search engine in compatible browsers.
        /// </remarks>
        public bool EnableOpenSearch { get; set; }

        #endregion

        #region TrackingScript

        /// <summary>
        ///     Gets or sets the tracking script code to be included in the blog's HTML pages.
        /// </summary>
        /// <remarks>
        /// This property contains custom tracking or analytics script code (typically JavaScript) that will be injected
        /// into blog pages. This is commonly used for services like Google Analytics or other web analytics platforms.
        /// The script content is inserted into the HTML page header or footer, depending on theme implementation.
        /// </remarks>
        public string TrackingScript { get; set; }

        #endregion

        #region ShowPostNavigation

        /// <summary>
        ///     Gets or sets a value indicating whether to display navigation links between posts.
        /// </summary>
        /// <value><c>true</c> if post navigation links should be displayed; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When enabled, navigation controls (previous/next post links) are displayed at the bottom of individual post pages,
        /// allowing readers to easily browse through posts. This improves user experience and can increase page views and engagement.
        /// </remarks>
        public bool ShowPostNavigation { get; set; }

        #endregion

        #region EnablePasswordReset

        /// <summary>
        ///     Gets or sets a value indicating whether users can reset forgotten passwords.
        /// </summary>
        /// <value><c>true</c> if password reset functionality is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When enabled, users who forget their passwords can use a password reset mechanism to regain access to their accounts.
        /// When disabled, administrators must manually reset user passwords. This is a security-critical feature that should be
        /// carefully considered based on your site's security requirements and user support capabilities.
        /// </remarks>
        public bool EnablePasswordReset
        {
            get { return enablePasswordResets; }
            set { enablePasswordResets = value; }
        }

        #endregion

        #region SelfRegistration

        /// <summary>
        ///     Gets or sets a value indicating whether users can register themselves for a blog account.
        /// </summary>
        /// <value><c>true</c> if self-registration is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When enabled, visitors can create their own user accounts without administrator intervention.
        /// The new user's initial role is determined by SelfRegistrationInitialRole, and whether a new blog
        /// is created is controlled by CreateBlogOnSelfRegistration. Disable this for private blogs requiring
        /// administrator approval for new users.
        /// </remarks>
        public bool EnableSelfRegistration { get; set; }

        /// <summary>
        ///     Gets or sets the initial role assigned to users who self-register.
        /// </summary>
        /// <value>The role name (e.g., "Editor", "Contributor").</value>
        /// <remarks>
        /// This property defines the permission level automatically granted to newly self-registered users.
        /// Common values include Editor, Contributor, or other custom roles defined in your system.
        /// Ensure the specified role exists and has appropriate permission levels for your security model.
        /// </remarks>
        public string SelfRegistrationInitialRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new blog should be created for self-registered users.
        /// </summary>
        /// <remarks>
        /// When true, each new self-registered user automatically receives their own blog in a multi-blog installation.
        /// When false, new users are added to the existing blog as a regular user or contributor. This setting is most
        /// relevant in multi-blog scenarios where each user may want their own publication space.
        /// </remarks>
        public bool CreateBlogOnSelfRegistration { get; set; }

        #endregion

        #region HandleWwwSubdomain

        /// <summary>
        ///     Gets or sets how to handle the www subdomain in blog URLs.
        /// </summary>
        /// <remarks>
        /// This setting controls URL canonicalization for SEO purposes. Typical values include:
        /// - "RemoveWWW": Redirect all www URLs to non-www URLs
        /// - "AddWWW": Redirect all non-www URLs to www URLs
        /// - "None": Allow both www and non-www URLs
        /// Consistent URL handling improves SEO by avoiding duplicate content penalties.
        /// </remarks>
        public string HandleWwwSubdomain { get; set; }

        #endregion

        #region EnablePingBackSend

        /// <summary>
        ///     Gets or sets a value indicating whether to send pingbacks when posts link to other blogs.
        /// </summary>
        /// <value><c>true</c> if pingback sending is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When enabled, the blog automatically sends pingbacks to other blogs when your posts contain links to them.
        /// This is a form of blog-to-blog communication that notifies target blogs about incoming links. Disabling this
        /// improves performance slightly but reduces discoverability in the blogging community.
        /// </remarks>
        public bool EnablePingBackSend { get; set; }

        #endregion

        #region EnablePingBackReceive;

        /// <summary>
        ///     Gets or sets a value indicating whether to accept pingbacks from other blogs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if pingback receiving is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, other blogs can send pingbacks to notify your blog about incoming links.
        /// Pingbacks appear as a form of trackback-like notification. Disabling this prevents unsolicited pingback spam.
        /// If enabled, you should enable comments moderation to handle potential spam pingbacks.
        /// </remarks>
        public bool EnablePingBackReceive { get; set; }

        #endregion

        #region EnableTrackBackSend;

        /// <summary>
        ///     Gets or sets a value indicating whether to send trackbacks when posts link to other blogs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if trackback sending is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Trackbacks are similar to pingbacks but use a different protocol. When enabled, trackbacks are sent to blogs
        /// that your posts link to. This feature is less common than pingbacks in modern blogging but may be needed for
        /// compatibility with older blog platforms.
        /// </remarks>
        public bool EnableTrackBackSend { get; set; }

        #endregion

        #region EnableTrackBackReceive;

        /// <summary>
        ///     Gets or sets a value indicating whether to accept trackbacks from other blogs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if trackback receiving is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, other blogs can send trackbacks to notify your blog about incoming links.
        /// Trackbacks appear as comments or special trackback entries on your posts. Disabling this prevents trackback spam.
        /// If enabled, enable comments moderation to screen incoming trackbacks for inappropriate content.
        /// </remarks>
        public bool EnableTrackBackReceive { get; set; }

        #endregion

        #region Email

        /// <summary>
        ///     Gets or sets the email address where blog notifications and alerts are sent.
        /// </summary>
        /// <value>The email address for blog notifications.</value>
        /// <remarks>
        /// This is the primary contact email address for the blog. Notifications about new comments, moderation alerts,
        /// contact form submissions, and other system events are sent to this address. This should be a valid, monitored
        /// email address to ensure important notifications are received.
        /// </remarks>
        public string Email { get; set; }

        #endregion

        #region SendMailOnComment

        /// <summary>
        ///     Gets or sets a value indicating whether email notifications are sent when a comment is added to a post.
        /// </summary>
        /// <value><b>true</b> if email notification of new comments is enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the blog sends an email to the configured Email address whenever a new comment is posted.
        /// This allows blog administrators to receive immediate notifications about new reader feedback. For high-traffic blogs,
        /// you may want to disable this to reduce email volume, or enable comments moderation to screen comments before notification.
        /// </remarks>
        public bool SendMailOnComment { get; set; }

        #endregion

        #region SmtpPassword

        /// <summary>
        ///     Gets or sets the password used to authenticate with the SMTP server.
        /// </summary>
        /// <value>The password for SMTP authentication.</value>
        /// <remarks>
        /// This is the password credential used when connecting to the SMTP server for sending emails.
        /// Leave empty if the SMTP server doesn't require authentication. Store this securely, as it controls
        /// access to your email sending capabilities. Consider using application-level password encryption.
        /// </remarks>
        public string SmtpPassword { get; set; }

        #endregion

        #region SmtpServer

        /// <summary>
        ///     Gets or sets the DNS name or IP address of the SMTP server used to send notification emails.
        /// </summary>
        /// <value>The DNS name or IP address of the SMTP server.</value>
        /// <remarks>
        /// This is the mail server that the blog uses to send email notifications and other automated emails.
        /// Common values include "smtp.gmail.com", "mail.yourdomain.com", or your hosting provider's mail server.
        /// The port number can be configured separately via SmtpServerPort. Ensure the server is reachable from your
        /// web server and firewall rules allow outbound SMTP connections.
        /// </remarks>
        public string SmtpServer { get; set; }

        #endregion

        #region SmtpServerPort

        /// <summary>
        ///     Gets or sets the port number on the SMTP server used to send notification emails.
        /// </summary>
        /// <value>The port number of the SMTP server.</value>
        /// <remarks>
        /// This specifies which port to connect to on the SMTP server. Common values are:
        /// - 25 (standard unencrypted SMTP)
        /// - 587 (SMTP with STARTTLS encryption)
        /// - 465 (SMTP SSL encryption)
        /// Port 587 with EnableSsl is the most common and recommended configuration for modern email servers.
        /// </remarks>
        public int SmtpServerPort { get; set; }

        #endregion

        #region SmtpUsername

        /// <summary>
        ///     Gets or sets the user name used to authenticate with the SMTP server.
        /// </summary>
        /// <value>The username for SMTP authentication.</value>
        /// <remarks>
        /// This is the username credential used when connecting to the SMTP server for authentication.
        /// Leave empty if the SMTP server doesn't require authentication. This is often an email address
        /// or service account name, depending on your SMTP server's authentication requirements.
        /// </remarks>
        public string SmtpUserName { get; set; }

        #endregion

        #region EnableSsl

        /// <summary>
        ///     Gets or sets a value indicating whether SSL/TLS encryption should be used for SMTP connections.
        /// </summary>
        /// <remarks>
        /// When enabled, SMTP communications are encrypted using SSL or STARTTLS protocols. This is strongly recommended
        /// for security, especially if the SMTP server is hosted on a remote service or the internet. Modern mail servers
        /// typically require this setting to be enabled. Set SmtpServerPort to 587 for STARTTLS or 465 for implicit SSL.
        /// </remarks>
        public bool EnableSsl { get; set; }

        #endregion

        #region EmailSubjectPrefix

        /// <summary>
        ///     Gets or sets a prefix that is prepended to the subject line of notification emails.
        /// </summary>
        /// <value>The email subject prefix.</value>
        /// <remarks>
        /// This prefix is automatically added to the beginning of email subjects for blog notifications.
        /// For example, if the prefix is "[MyBlog]", a new comment notification might have the subject "[MyBlog] New comment on Post Title".
        /// This helps recipients organize and filter emails by blog, which is useful in multi-blog scenarios or when multiple services send emails.
        /// </remarks>
        public string EmailSubjectPrefix { get; set; }

        #endregion

        #region DaysCommentsAreEnabled

        /// <summary>
        ///     Gets or sets the number of days after publication that a post accepts comments.
        /// </summary>
        /// <value>The number of days that a post accepts comments.</value>
        /// <remarks>
        ///     After this time period has elapsed since the post publication date, comments on the post are automatically
        ///     disabled, preventing new comments from being added. Set to 0 or a very high number to effectively allow
        ///     comments indefinitely. This helps manage comment spam and maintain control over older posts.
        /// </remarks>
        public int DaysCommentsAreEnabled { get; set; }

        #endregion

        #region EnableCountryInComments

        /// <summary>
        ///     Gets or sets a value indicating whether the commenter's country should be displayed.
        /// </summary>
        /// <value><b>true</b> if commenter country display is enabled, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, the blog attempts to determine and display the country of origin for commenters
        /// (typically via IP geolocation). This information may be displayed alongside the comment or in
        /// commenter profiles. Disable this for privacy-focused configurations.
        /// </remarks>
        public bool EnableCountryInComments { get; set; }

        #endregion

        #region EnableWebsiteInComments

        /// <summary>
        ///     Gets or sets a value indicating whether the commenter's website should be displayed.
        /// </summary>
        /// <remarks>
        /// When enabled, a commenter's website URL (if provided) is displayed alongside their comment,
        /// either as plain text or a clickable link. This encourages community engagement and allows
        /// commenters to promote their own content. Disable this to focus on comment content rather than
        /// commenter promotion or for privacy reasons.
        /// </remarks>
        public bool EnableWebsiteInComments { get; set; }

        #endregion

        #region IsCommentsEnabled

        /// <summary>
        ///     Gets or sets a value indicating whether comments are enabled for new posts.
        /// </summary>
        /// <value><b>true</b> if comments can be made against a post, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// This global setting controls the default comment permission for newly created posts. Individual posts
        /// can override this setting to enable or disable comments on a per-post basis. When disabled, all posts
        /// will not accept comments unless specifically configured otherwise.
        /// </remarks>
        public bool IsCommentsEnabled { get; set; }

        #endregion

        #region IsCoCommentEnabled

        /// <summary>
        ///     Gets or sets a value indicating whether CoComment support is enabled.
        /// </summary>
        /// <value>Always returns false.</value>
        /// <remarks>
        /// This property is maintained for backward compatibility with older themes and should not be used
        /// in new development. CoComment was a third-party service that is no longer active. This setting
        /// will be removed in a future version.
        /// </remarks>
        public bool IsCoCommentEnabled { get; set; }

        #endregion

        #region Avatar

        /// <summary>
        ///     Gets or sets the avatar/profile picture provider for commenters.
        /// </summary>
        /// <value>The avatar provider name (typically "Gravatar" or similar).</value>
        /// <remarks>
        /// This setting specifies which service is used to display avatar images for commenters.
        /// Common values include "Gravatar" for Gravatar avatars. When set, commenter avatars are automatically
        /// fetched from the configured service. Disable by leaving empty to show no avatars in comments.
        /// </remarks>
        public string Avatar { get; set; }

        #endregion

        #region IsCommentNestingEnabled

        /// <summary>
        ///     Gets or sets a value indicating whether comments should be displayed as nested/threaded.
        /// </summary>
        /// <value><b>true</b> if comments should be displayed as nested, <b>false</b> for flat comments.</value>
        /// <remarks>
        /// When enabled, comments are displayed in a threaded format where replies to comments are indented
        /// underneath their parent comment, creating a visual conversation thread. When disabled, all comments
        /// are displayed in a flat, chronological list. Nested comments improve readability for discussions but
        /// can make individual comments harder to find in large comment sections.
        /// </remarks>
        public bool IsCommentNestingEnabled { get; set; }

        #endregion

        #region Trust authenticated users

        /// <summary>
        ///     Gets or sets a value indicating whether comments from authenticated users are automatically approved.
        /// </summary>
        /// <remarks>
        /// When enabled, comments submitted by authenticated/registered users bypass the moderation queue and are
        /// automatically published. This streamlines the publishing process for trusted community members while still
        /// moderating comments from anonymous visitors. This assumes authenticated users are more trustworthy than
        /// anonymous commenters.
        /// </remarks>
        public bool TrustAuthenticatedUsers { get; set; }

        #endregion

        #region SecurityValidationKey

        /// <summary>
        ///     Gets or sets the security validation key used for protecting form submissions and CSRF protection.
        /// </summary>
        /// <value>The security validation key.</value>
        /// <remarks>
        /// This key is used for various security purposes including CSRF (Cross-Site Request Forgery) token validation
        /// and form submission verification. A random, unique value should be generated for each blog installation and
        /// kept secret. Keep this value secure and consistent across server restarts.
        /// </remarks>
        public string SecurityValidationKey { get; set; }

        #endregion

        #region Comments per page

        /// <summary>
        ///     Gets or sets the number of comments to display per page in the comments admin section.
        /// </summary>
        /// <remarks>
        /// This setting controls pagination in the administrative comments management interface. Affects how many
        /// comments are displayed per page when viewing/moderating comments. A minimum of 5 comments per page is enforced.
        /// Higher values show more comments at once but may slow page loading, while lower values require more navigation.
        /// </remarks>
        public int CommentsPerPage
        {
            get { return Math.Max(commentsPerPage, 5); }
            set { commentsPerPage = value; }
        }

        #endregion

        #region Comment providers and moderation

        /// <summary>
        /// Specifies which comment system provider is used for displaying and managing comments.
        /// </summary>
        /// <remarks>
        /// The blog supports multiple comment providers, allowing integration with third-party comment systems
        /// or using the built-in BlogEngine comment system. Each provider handles comment display, storage, and
        /// moderation differently.
        /// </remarks>
        public enum CommentsBy
        {
            /// <summary>
            ///     Comments are managed by the internal BlogEngine comment system.
            /// </summary>
            BlogEngine = 0,
            /// <summary>
            ///     Comments are managed by the external Disqus service.
            /// </summary>
            Disqus = 1,
            /// <summary>
            ///     Comments are managed by the external Facebook Comments plugin.
            /// </summary>
            Facebook = 2
        }

        /// <summary>
        ///     Gets or sets which comment provider system is used for the blog.
        /// </summary>
        /// <remarks>
        /// This setting determines whether the blog uses its built-in comment system (BlogEngine),
        /// or outsources comments to a third-party provider like Disqus or Facebook. When using external
        /// providers, additional configuration (API keys, site IDs) may be required.
        /// </remarks>
        public CommentsBy CommentProvider { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether comment moderation is enabled for posts.
        /// </summary>
        /// <value><b>true</b> if comments are moderated for posts, otherwise returns <b>false</b>.</value>
        /// <remarks>
        /// When enabled, new comments must be approved by an administrator before they appear on the blog.
        /// This helps prevent spam and inappropriate content from being published. Disable for immediate comment
        /// publication, but this may result in more spam. Consider enabling with TrustAuthenticatedUsers for a balance.
        /// </remarks>
        public bool EnableCommentsModeration { get; set; }

        /// <summary>
        ///     Gets or sets whether the blog should report detected spam/moderation issues to external services.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog may share information about detected spam or problematic comments with
        /// external moderation services to help improve spam detection. This helps the service learn about
        /// attack patterns and improve protection for all users.
        /// </remarks>
        public bool CommentReportMistakes { get; set; }

        /// <summary>
        ///     Gets or sets the short website name/shortname used to identify the Disqus account.
        /// </summary>
        /// <remarks>
        /// This property is only used when CommentProvider is set to Disqus. The shortname is the unique
        /// identifier for your Disqus forum, configured in your Disqus account settings. Leave empty
        /// if not using Disqus for comments.
        /// </remarks>
        public string DisqusWebsiteName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Disqus development mode is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, allows testing Disqus integration on localhost during development.
        /// Should be disabled on production sites. Only applies when using Disqus as the comment provider.
        /// </remarks>
        public bool DisqusDevMode { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Disqus comments are also added to static pages.
        /// </summary>
        /// <remarks>
        /// When enabled, Disqus comments are displayed on both blog posts and static pages.
        /// When disabled, comments appear only on blog posts. Only applies when using Disqus as the comment provider.
        /// </remarks>
        public bool DisqusAddCommentsToPages { get; set; }

        /// <summary>
        /// Gets or sets the Facebook Application ID for the Facebook Comments plugin.
        /// </summary>
        /// <remarks>
        /// This is the App ID for your Facebook application, which is required when using Facebook Comments
        /// as the comment provider. Obtain this from your Facebook App Dashboard. Leave empty if not using
        /// Facebook Comments.
        /// </remarks>
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the language code for the Facebook Comments plugin.
        /// </summary>
        /// <remarks>
        /// Specifies the language that the Facebook Comments interface should be displayed in.
        /// Use standard language codes like "en_US", "fr_FR", etc. Only applies when using Facebook Comments.
        /// </remarks>
        public string FacebookLanguage { get; set; }

        #endregion

        #region BlogrollMaxLength

        /// <summary>
        ///     Gets or sets the maximum number of characters displayed from a blogroll-sourced post.
        /// </summary>
        /// <value>The maximum number of characters to display.</value>
        /// <remarks>
        /// When aggregating blog content from external sources (blogroll), post excerpts are truncated to this length.
        /// This prevents very long posts from overwhelming the aggregation display. Longer values show more content
        /// context, while shorter values save screen space. Consider setting this relative to your blogroll display area.
        /// </remarks>
        public int BlogrollMaxLength { get; set; }

        #endregion

        #region BlogrollUpdateMinutes

        /// <summary>
        ///     Gets or sets the number of minutes to wait before polling blog-roll sources for changes.
        /// </summary>
        /// <value>The number of minutes to wait before polling blog-roll sources for changes.</value>
        public int BlogrollUpdateMinutes { get; set; }

        #endregion

        #region BlogrollVisiblePosts

        /// <summary>
        ///     Gets or sets the number of posts to display from a blog-roll source.
        /// </summary>
        /// <value>The number of posts to display from a blog-roll source.</value>
        public int BlogrollVisiblePosts { get; set; }

        #endregion

        #region EnableCommentSearch

        /// <summary>
        ///     Gets or sets a value indicating if search of post comments is enabled.
        /// </summary>
        /// <value><b>true</b> if post comments can be searched, otherwise returns <b>false</b>.</value>
        public bool EnableCommentSearch { get; set; }

        /// <summary>
        ///     If yes, checkbox for include comments in search added to UI
        /// </summary>
        public bool ShowIncludeCommentsOption { get; set; }

        #endregion

        #region SearchButtonText

        /// <summary>
        ///     Gets or sets the search button text to be displayed.
        /// </summary>
        /// <value>The search button text to be displayed.</value>
        public string SearchButtonText { get; set; }

        #endregion

        #region SearchCommentLabelText

        /// <summary>
        ///     Gets or sets the search comment label text to display.
        /// </summary>
        /// <value>The search comment label text to display.</value>
        public string SearchCommentLabelText { get; set; }

        #endregion

        #region SearchDefaultText

        /// <summary>
        ///     Gets or sets the default search text to display.
        /// </summary>
        /// <value>The default search text to display.</value>
        public string SearchDefaultText { get; set; }

        #endregion

        #region Endorsement

        /// <summary>
        ///     Gets or sets the URI of a web log that the author of this web log is promoting.
        /// </summary>
        /// <value>The <see cref = "Uri" /> of a web log that the author of this web log is promoting.</value>
        public string Endorsement { get; set; }

        #endregion

        #region PostsPerFeed

        /// <summary>
        ///     Gets or sets the maximum number of characters that are displayed from a blog-roll retrieved post.
        /// </summary>
        /// <value>The maximum number of characters to display.</value>
        public int PostsPerFeed { get; set; }

        #endregion

        #region AuthorName

        /// <summary>
        ///     Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string AuthorName { get; set; }

        #endregion

        #region Language

        /// <summary>
        ///     Gets or sets the language this blog is written in.
        /// </summary>
        /// <value>The language this blog is written in.</value>
        /// <remarks>
        ///     Recommended best practice for the values of the Language element is defined by RFC 1766 [RFC1766] which includes a two-letter Language Code (taken from the ISO 639 standard [ISO639]), 
        ///     followed optionally, by a two-letter Country Code (taken from the ISO 3166 standard [ISO3166]).
        /// </remarks>
        /// <example>
        ///     en-US
        /// </example>
        public string Language { get; set; }

        #endregion

        #region GeocodingLatitude

        /// <summary>
        ///     Gets or sets the latitude component of the geocoding position for this blog.
        /// </summary>
        /// <value>The latitude value.</value>
        public float GeocodingLatitude { get; set; }

        #endregion

        #region GeocodingLongitude

        /// <summary>
        ///     Gets or sets the longitude component of the geocoding position for this blog.
        /// </summary>
        /// <value>The longitude value.</value>
        public float GeocodingLongitude { get; set; }

        #endregion

        #region ContactFormMessage;

        /// <summary>
        ///     Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string ContactFormMessage { get; set; }

        #endregion

        #region ContactThankMessage

        /// <summary>
        ///     Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string ContactThankMessage { get; set; }

        #endregion

        #region ContactErrorMessage;
        /// <summary>
        ///     Gets or sets a custom error message for this blog.
        /// </summary>
        /// <value>The error messagge for this blog.</value>
        public string ContactErrorMessage { get; set; }
        
        #endregion

        #region HtmlHeader

        /// <summary>
        ///     Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string HtmlHeader { get; set; }

        #endregion

        #region Culture

        /// <summary>
        ///     Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string Culture { get; set; }

        #endregion

        #region Timezone

        /// <summary>
        /// Time zone id
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Converts a local date and time from the client into Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="localTime">The local time to convert. If omitted or empty, the current UTC time is returned.</param>
        /// <returns>The converted UTC date and time.</returns>
        /// <remarks>
        /// This method uses the blog's configured time zone identifier to translate client-local time values into UTC.
        /// If no value is supplied, the method returns the current UTC time. The input value is normalized to an
        /// unspecified kind before conversion so the operation is consistent across the application.
        /// </remarks>
        public DateTime ToUtc(DateTime ? localTime = null)
        {
            if(localTime == null || localTime == new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)) // no time sent in, use "now"
                return DateTime.UtcNow;

            var zone = string.IsNullOrEmpty(Instance.TimeZoneId) ? "UTC" : Instance.TimeZoneId;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(zone);
            localTime = DateTime.SpecifyKind(localTime.Value, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(localTime.Value, tz);
        }

        /// <summary>
        /// Converts a UTC date and time into the blog's configured local time zone.
        /// </summary>
        /// <param name="serverTime">The UTC time to convert. If omitted or empty, the current UTC time is used.</param>
        /// <returns>The converted local date and time.</returns>
        /// <remarks>
        /// This method converts storage values from UTC into the configured time zone for the current blog instance.
        /// If no value is supplied, the current UTC time is used. The input value is normalized to an unspecified kind
        /// before conversion so the operation is consistent with other time handling in the application.
        /// </remarks>
        public DateTime FromUtc(DateTime ? serverTime = null)
        {
            if (serverTime == null || serverTime == new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified))
                serverTime = DateTime.UtcNow;

            var zone = string.IsNullOrEmpty(Instance.TimeZoneId) ? "UTC" : Instance.TimeZoneId;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(zone);
            serverTime = DateTime.SpecifyKind(serverTime.Value, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeFromUtc(serverTime.Value, tz);
        }

        #endregion

        #region EnableContactAttachments

        /// <summary>
        ///     Gets or sets whether or not to allow visitors to send attachments via the contact form.
        /// </summary>
        public bool EnableContactAttachments { get; set; }

        #endregion

        #region EnableRecaptchaOnContactForm

        /// <summary>
        ///     Gets or sets whether or not to use Recaptcha on the contact form.
        /// </summary>
        public bool EnableRecaptchaOnContactForm { get; set; }

        #endregion

        #region RemoveExtensionsFromUrls

        /// <summary>
        ///     Gets or sets a value indicating if extensions (.aspx) should be removed from URLs
        ///     -- always returns true to prepare for transition to MVC style routing
        /// </summary>
        /// <value><b>true</b> if should be removed, otherwise returns <b>false</b>.</value>
        public bool RemoveExtensionsFromUrls { get { return true; } }

        #endregion

        #region RedirectToRemoveFileExtension

        /// <summary>
        ///     Gets or sets a value indicating if incoming requests containing extensions (.aspx) should be redirected to a URL with the extension removed.
        /// </summary>
        /// <value><b>true</b> if should be redirected, otherwise returns <b>false</b>.</value>
        public bool RedirectToRemoveFileExtension { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets whether this application's handlers should be able to download and cache files hosted on other servers.
        /// </summary>
        /// <remarks>
        /// 
        /// Allowing the server's various handlers(Such as JavaScriptHandler and CssHandler) could potentially allow a an attacker
        /// to tie up the server by continuously requesting files of excess file size, or from servers that take forever to time out.
        /// 
        /// This is false by default.
        /// 
        /// </remarks>
        public bool AllowServerToDownloadRemoteFiles { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of time in milliseconds the server should spend downloading remote files. The default value is 30000.
        /// </summary>
        /// <remarks>
        /// 
        /// If the limit is set to something below 0, the defaultRemoteDownloadTimeout will be used instead.
        /// 0 is an acceptable value, users should use this value to indicate "unlimited time".
        /// </remarks>
        public int RemoteFileDownloadTimeout
        {
            get
            {
                if (this.remoteDownloadTimeout < 0)
                {
                    this.remoteDownloadTimeout = defaultRemoteDownloadTimeout;
                }
                return this.remoteDownloadTimeout;
            }
            set
            {
                if (value < 0) { value = defaultRemoteDownloadTimeout; }
                this.remoteDownloadTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed file size in bytes that BlogEngine can download from a remote server. Defaults to 512k.
        /// </summary>
        /// <remarks>
        /// Set this value to 0 for unlimited file size.
        /// </remarks>
        public int RemoteMaxFileSize
        {
            get
            {
                if (this.maxRemoteFileSize < 0)
                {
                    this.maxRemoteFileSize = defaultMaxRemoteFileSize;
                }
                return this.maxRemoteFileSize;
            }
            set
            {
                if (value < 0) { value = defaultMaxRemoteFileSize; }
                this.maxRemoteFileSize = value;
            }
        }

        #region Version()

        /// <summary>
        ///     The version.
        /// </summary>
        private static string version { get; set; }

        /// <summary>
        /// Returns the BlogEngine.NET version information.
        /// </summary>
        /// <value>
        /// The BlogEngine.NET major, minor, revision, and build numbers.
        /// </value>
        /// <remarks>
        /// The current version is determined by extracting the build version of the BlogEngine.Core assembly.
        /// </remarks>
        /// <returns>
        /// The version.
        /// </returns>
        public string Version()
        {
            return version ?? (version = typeof(BlogSettings).Assembly.GetName().Version.ToString());
        }

        #endregion

        #region Custom front page
        /// <summary>
        /// Gets the custom front page file name for the current blog instance if one is configured.
        /// </summary>
        /// <value>The front page file name, such as "FrontPage.cshtml" or "FrontPage.aspx", when present.</value>
        /// <remarks>
        /// This property checks the blog root for a custom front page file and caches the resolved value for a short period.
        /// The value is resolved from either a Razor or ASPX front page file and is only considered for the primary blog instance.
        /// </remarks>
        public static string CustomFrontPage
        {
            get
            {
                // uncomment this line to disable caching for debugging
                string cacheKey = "Blog-Custom-Front-Page";
                Blog.CurrentInstance.Cache.Remove(cacheKey);

                if (Blog.CurrentInstance.Cache[cacheKey] == null)
                {
                    Blog.CurrentInstance.Cache.Add(
                        cacheKey,
                        GetCustomFrontPage(),
                        null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, 15, 0),
                        CacheItemPriority.Low,
                        null);
                }
                return Blog.CurrentInstance.Cache[cacheKey].ToString();
            }
        }

        /// <summary>
        /// Resolves the custom front page file name used by the primary blog instance, if any.
        /// </summary>
        /// <returns>The file name of the custom front page when it exists; otherwise, an empty string.</returns>
        /// <remarks>
        /// This helper checks the blog root for a Razor front page file first and then falls back to an ASPX file.
        /// The lookup is only performed for the primary blog instance, which is the only context where a custom front page is applied.
        /// </remarks>
        static string GetCustomFrontPage()
        {
            if (Blog.CurrentInstance.IsPrimary)
            {
                string razorPath = HostingEnvironment.MapPath("~/FrontPage.cshtml");
                string aspxPath = HostingEnvironment.MapPath("~/FrontPage.aspx");

                if (File.Exists(razorPath)) return "FrontPage.cshtml";
                if (File.Exists(aspxPath)) return "FrontPage.aspx";
            }
            return "";
        }
        #endregion

        #region SEO & GEO Settings

        /// <summary>
        /// Gets or sets the blog-wide SEO title suffix.
        /// </summary>
        /// <remarks>
        /// This suffix is appended to page titles for consistent branding across all pages.
        /// For example, " - My Blog" or " | Company Name". Applied when UseBlogNameInPageTitles is false.
        /// </remarks>
        public string SeoTitleSuffix { get; set; }

        /// <summary>
        /// Gets or sets the canonical domain for the blog.
        /// </summary>
        /// <remarks>
        /// The canonical domain should be the preferred URL for the blog, including the protocol (https://example.com).
        /// Used to generate canonical link tags that prevent duplicate content issues in search engines.
        /// </remarks>
        public string SeoCanonicalDomain { get; set; }

        /// <summary>
        /// Gets or sets the default author name for SEO metadata.
        /// </summary>
        /// <remarks>
        /// Used as the default author value in meta tags and structured data when post-specific author
        /// information is not available. Appears in Schema.org Article metadata and Open Graph tags.
        /// </remarks>
        public string SeoDefaultAuthor { get; set; }

        /// <summary>
        /// Gets or sets the default featured image URL for social sharing.
        /// </summary>
        /// <remarks>
        /// This image is used in Open Graph and Twitter Card tags when content doesn't have a specific
        /// featured image. Should be an absolute URL to a high-quality image representing the blog brand.
        /// Recommended size: 1200x630 pixels for optimal social media display.
        /// </remarks>
        public string SeoDefaultImage { get; set; }

        /// <summary>
        /// Gets or sets the Twitter handle for the blog (e.g., @username).
        /// </summary>
        /// <remarks>
        /// Used in Twitter Card metadata to attribute content to the blog's Twitter account.
        /// Should include the @ symbol. Enables Twitter to link mentions to your account.
        /// </remarks>
        public string SeoTwitterHandle { get; set; }

        /// <summary>
        /// Gets or sets the Facebook App ID for social integration.
        /// </summary>
        /// <remarks>
        /// Facebook App ID enables Facebook Insights for your blog's content shared on Facebook.
        /// Provides analytics on how your content is shared and engaged with on Facebook.
        /// </remarks>
        public string SeoFacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Open Graph meta tags are enabled.
        /// </summary>
        /// <remarks>
        /// Open Graph tags provide rich preview information when content is shared on social platforms
        /// like Facebook, LinkedIn, and others. Recommended for all blogs to enhance social media presence.
        /// </remarks>
        public bool SeoEnableOpenGraph { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Twitter Card meta tags are enabled.
        /// </summary>
        /// <remarks>
        /// Twitter Card tags enable rich media previews when content is shared on Twitter.
        /// Supports both summary and large image card formats based on content.
        /// </remarks>
        public bool SeoEnableTwitterCard { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Schema.org structured data is enabled.
        /// </summary>
        /// <remarks>
        /// Structured data (JSON-LD) helps search engines and AI systems understand content semantics.
        /// Enables rich snippets in search results and better content understanding by generative AI engines.
        /// Critical for GEO (Generative Engine Optimization).
        /// </remarks>
        public bool SeoEnableStructuredData { get; set; }

        /// <summary>
        /// Gets or sets the organization name for Schema.org structured data.
        /// </summary>
        /// <remarks>
        /// The legal or common name of the organization publishing the blog content.
        /// Used in Schema.org Organization and Publisher schema. Enhances trust signals for search engines.
        /// </remarks>
        public string SeoOrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the organization logo URL for Schema.org structured data.
        /// </summary>
        /// <remarks>
        /// URL to the organization's logo image. Should be a square image, minimum 112x112 pixels.
        /// Used in Schema.org Publisher schema and may appear in Google search results.
        /// </remarks>
        public string SeoOrganizationLogo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether GEO (Generative Engine Optimization) is enabled.
        /// </summary>
        /// <remarks>
        /// Generative Engine Optimization targets AI-powered search engines and content discovery systems.
        /// When enabled, additional metadata and hints are provided to help AI systems better understand
        /// and utilize the blog's content in generated responses.
        /// </remarks>
        public bool GeoOptimizationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the GEO optimization mode.
        /// </summary>
        /// <remarks>
        /// Defines the optimization strategy for generative AI search engines.
        /// Supported modes: "Standard", "AISearch", "Conversational", "Citation".
        /// Each mode adjusts metadata richness and structured data for different AI use cases.
        /// </remarks>
        public string GeoOptimizationMode { get; set; }

        /// <summary>
        /// Gets or sets the metadata richness level for GEO.
        /// </summary>
        /// <remarks>
        /// Controls how comprehensive the metadata output is for AI systems.
        /// Values: "Minimal", "Standard", "Rich", "Maximum".
        /// Higher richness provides more context but increases page size.
        /// </remarks>
        public string GeoMetadataRichness { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether content should be optimized for citation by AI systems.
        /// </summary>
        /// <remarks>
        /// When enabled, additional citation metadata is included to help AI systems properly attribute
        /// and reference the blog's content when used in generated responses.
        /// </remarks>
        public bool GeoEnableCitationOptimization { get; set; }

        #endregion

        #region "Methods"

        #region Load()

        /// <summary>
        /// Builds a dictionary mapping property names to PropertyInfo objects for all settings properties.
        /// </summary>
        /// <remarks>
        /// This helper method constructs a case-insensitive dictionary of all public properties on the BlogSettings class.
        /// This is used during settings loading and saving to efficiently look up properties by name without reflection overhead
        /// on each access. The dictionary is case-insensitive to accommodate different casing in stored settings.
        /// </remarks>
        /// <returns>
        /// A case-insensitive dictionary mapping property names to their corresponding PropertyInfo objects.
        /// </returns>
        private IDictionary<String, System.Reflection.PropertyInfo> GetSettingsTypePropertyDict()
        {
            var settingsType = this.GetType();

            var result = new System.Collections.Generic.Dictionary<String, System.Reflection.PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in settingsType.GetProperties())
            {
                result[prop.Name] = prop;
            }

            return result;

        }

        /// <summary>
        /// Initializes the singleton instance of the <see cref="BlogSettings"/> class with configuration values from storage.
        /// </summary>
        /// <param name="blog">The blog instance to load settings for.</param>
        /// <remarks>
        /// This private method is called during initialization to populate all settings properties with values
        /// from the blog's persistent settings storage. It uses reflection to dynamically map stored setting names to
        /// class properties, converting string values to appropriate types (including enums). Any errors during property
        /// conversion are logged but do not prevent the rest of the settings from loading.
        /// </remarks>
        private void Load(Blog blog)
        {

            // ------------------------------------------------------------
            // 	Enumerate through individual settings nodes
            // ------------------------------------------------------------
            var dic = BlogService.LoadSettings(blog);
            var settingsProps = GetSettingsTypePropertyDict();

            foreach (System.Collections.DictionaryEntry entry in dic)
            {
                string name = (string)entry.Key;
                System.Reflection.PropertyInfo property = null;

                if (settingsProps.TryGetValue(name, out property))
                {
                    // ------------------------------------------------------------
                    // 	Attempt to apply configured setting
                    // ------------------------------------------------------------
                    try
                    {
                        if (property.CanWrite)
                        {
                            string value = (string)entry.Value;
                            var propType = property.PropertyType;
                            if (propType.IsEnum)
                            {
                                property.SetValue(this, Enum.Parse(propType, value), null);
                            }
                            else
                            {
                                property.SetValue(this, Convert.ChangeType(value, propType, CultureInfo.CurrentCulture), null);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.Log($"Error loading blog settings: {e.Message}");
                    }
                }

            }

        }

        #endregion

        #region OnChanged()

        /// <summary>
        /// Occurs when the settings have been changed and notifies all registered event handlers.
        /// </summary>
        /// <remarks>
        /// This private method is called after settings have been saved to disk. It triggers the Changed event,
        /// which allows the rest of the application to respond to configuration changes. Event handlers can use this
        /// to invalidate caches, update runtime behavior, or perform other necessary updates when settings change.
        /// </remarks>
        private static void OnChanged()
        {
            // Execute event handler
            if (Changed != null)
            {
                Changed(null, EventArgs.Empty);
            }
        }

        #endregion

        #region Save()

        /// <summary>
        /// Persists the current settings to the blog's settings storage.
        /// </summary>
        /// <remarks>
        /// This method saves all public property values from this BlogSettings instance to persistent storage
        /// using the blog's configured settings provider. String representations of all property values are
        /// stored, with null and default values being saved as empty strings. After saving, the theme cache
        /// is invalidated and the Changed event is raised to notify other components of the configuration update.
        /// </remarks>
        public void Save()
        {
            var dic = new StringDictionary();
            var settingsType = this.GetType();

            // ------------------------------------------------------------
            // 	Enumerate through settings properties
            // ------------------------------------------------------------
            foreach (var propertyInformation in settingsType.GetProperties())
            {
                if (propertyInformation.Name != "Instance")
                {
                    // ------------------------------------------------------------
                    // 	Extract property value and its string representation
                    // ------------------------------------------------------------
                    var propertyValue = propertyInformation.GetValue(this, null);

                    string valueAsString;

                    // ------------------------------------------------------------
                    // 	Format null/default property values as empty strings
                    // ------------------------------------------------------------
                    if (propertyValue == null || propertyValue.Equals(Int32.MinValue) ||
                        propertyValue.Equals(Single.MinValue))
                    {
                        valueAsString = String.Empty;
                    }
                    else
                    {
                        valueAsString = propertyValue.ToString();
                    }

                    // ------------------------------------------------------------
                    // 	Write property name/value pair
                    // ------------------------------------------------------------
                    dic.Add(propertyInformation.Name, valueAsString);
                }
            }

            BlogService.SaveSettings(dic);
            _isRazorTheme = null;
            OnChanged();
        }

        #endregion

        #endregion


        #region "ErrorPage Title"
        /// <summary>
        /// Gets or sets the title shown on the error page.
        /// </summary>
        /// <value>The title text for the error page.</value>
        /// <remarks>
        /// This value is displayed to users when an application error occurs and should provide a clear, user-friendly description
        /// of the failure state. It can be customized to match the branding or messaging style of the blog.
        /// </remarks>
        public string ErrorTitle { get; set; }

        #endregion

        #region "ErrorPage Body"
        /// <summary>
        /// Gets or sets the body text shown on the error page.
        /// </summary>
        /// <value>The body content for the error page.</value>
        /// <remarks>
        /// This value is displayed as the main content of the error page and can be used to explain the problem or provide
        /// guidance to visitors when an unexpected error occurs.
        /// </remarks>
        public string ErrorText { get; set; }

        #endregion

        #region EditorOptions

        /// <summary>
        /// Gets or sets a value indicating whether the post editor shows the slug option.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes the slug field so authors can customize the URL-friendly post identifier.
        /// </remarks>
        public bool PostOptionsSlug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the post editor shows the description option.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes the post description field for additional metadata and excerpt content.
        /// </remarks>
        public bool PostOptionsDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the post editor shows custom field options.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes support for custom metadata fields that can be attached to a post.
        /// </remarks>
        public bool PostOptionsCustomFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page editor shows the slug option.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes the URL slug field for custom pages so authors can tailor the page address.
        /// </remarks>
        public bool PageOptionsSlug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page editor shows the description option.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes the description field for page metadata and summary text.
        /// </remarks>
        public bool PageOptionsDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page editor shows custom field options.
        /// </summary>
        /// <remarks>
        /// When enabled, the editor exposes custom field support for pages so additional metadata can be managed.
        /// </remarks>
        public bool PageOptionsCustomFields { get; set; }

        #endregion

        #region SEO & GEO (Generative Engine Optimization)

        /// <summary>
        /// Gets or sets a value indicating whether Generative Engine Optimization (GEO) is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog will include enhanced metadata and structured data optimized for
        /// Generative AI systems (LLMs, RAG systems, AI search engines) in addition to traditional search engines.
        /// </remarks>
        public bool EnableGEO { get; set; }

        /// <summary>
        /// Gets or sets the Schema.org organization name for structured data.
        /// </summary>
        /// <remarks>
        /// This is the official name of the blog organization used in Schema.org markup for rich snippets
        /// and structured data recognition by search engines and AI systems. Used as a default for all posts
        /// unless overridden at the individual post level.
        /// </remarks>
        public string SchemaOrgName { get; set; }

        /// <summary>
        /// Gets or sets the Schema.org organization type.
        /// </summary>
        /// <remarks>
        /// Specifies the organization type in Schema.org taxonomy (e.g., "Blog", "NewsMediaOrganization", "Organization").
        /// Used in structured data markup to help AI systems categorize and understand the blog's purpose.
        /// </remarks>
        public string SchemaOrgType { get; set; }

        /// <summary>
        /// Gets or sets the Schema.org organization URL.
        /// </summary>
        /// <remarks>
        /// The canonical URL of the blog organization for Schema.org markup. Used to establish organizational
        /// identity in structured data for search engines and AI indexing systems.
        /// </remarks>
        public string SchemaOrgUrl { get; set; }

        /// <summary>
        /// Gets or sets the blog's canonical URL base.
        /// </summary>
        /// <remarks>
        /// The canonical base URL for the blog domain (e.g., "https://example.com"). Used to prevent duplicate
        /// content penalties and establish primary URL identity for search engines and AI crawlers.
        /// </remarks>
        public string CanonicalUrlBase { get; set; }

        /// <summary>
        /// Gets or sets comma-separated generative AI keywords for blog-wide optimization.
        /// </summary>
        /// <remarks>
        /// Default keywords (comma-separated) to be associated with all posts for Generative AI indexing.
        /// Individual posts can override or extend these keywords. Helps AI systems understand core topics covered by the blog.
        /// </remarks>
        public string GenerativeAIKeywords { get; set; }

        /// <summary>
        /// Gets or sets comma-separated entity hints for AI entity extraction.
        /// </summary>
        /// <remarks>
        /// Comma-separated list of key entities (people, places, concepts) relevant to the blog
        /// that should be recognized by AI systems. Examples: "machine learning, digital marketing, software development".
        /// Helps train AI extractors to properly identify relevant entities in blog content.
        /// </remarks>
        public string EntityHints { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether breadcrumb navigation structured data is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, the blog will include Schema.org BreadcrumbList markup in post and archive pages.
        /// Helps search engines and AI systems understand page hierarchy and navigation structure.
        /// </remarks>
        public bool BreadcrumbEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Google AI domain verification token.
        /// </summary>
        /// <remarks>
        /// Optional verification token for Google's generative AI indexing system.
        /// Used to establish domain ownership and control how the blog's content is indexed by Google's AI systems.
        /// </remarks>
        public string GoogleAIDomainVerification { get; set; }

        /// <summary>
        /// Gets or sets the sitemap generation policy.
        /// </summary>
        /// <remarks>
        /// Specifies sitemap generation strategy: "auto" (automatic), "manual" (require manual generation), or "disabled".
        /// Sitemaps help search engines and AI crawlers discover and index all content efficiently.
        /// </remarks>
        public string SitemapPolicy { get; set; }

        /// <summary>
        /// Gets or sets the language code for Schema.org markup (inLanguage property).
        /// </summary>
        /// <remarks>
        /// ISO 639-1 language code (e.g., "en", "es", "fr", "de") used in Schema.org structured data.
        /// Defaults to "en" if not specified.
        /// </remarks>
        private string inLanguage;

        /// <summary>
        /// Gets or sets the language code for Schema.org markup (inLanguage property).
        /// </summary>
        /// <remarks>
        /// ISO 639-1 language code (e.g., "en", "es", "fr", "de") used in Schema.org structured data.
        /// Defaults to "en" if not specified.
        /// </remarks>
        public string InLanguage
        {
            get { return string.IsNullOrEmpty(this.inLanguage) ? "en" : this.inLanguage; }
            set { this.inLanguage = value; }
        }

        /// <summary>
        /// Gets or sets comma-separated keywords for GEO discovery.
        /// </summary>
        /// <remarks>
        /// Keywords for GEO optimization, separate from GenerativeAIKeywords.
        /// </remarks>
        public string GEOKeywords { get; set; }

        /// <summary>
        /// Gets the computed path to the theme-specific image for GEO applications.
        /// </summary>
        /// <remarks>
        /// Automatically computed from the current theme name.
        /// Returns ~/Custom/Themes/{themeName}/img/{theme-name}.png
        /// </remarks>
        public string GEOImage
        {
            get
            {
                try
                {
                    string themeName = this.Theme ?? "Default";
                    string themeImageName = themeName.ToLower().Replace(" ", "-");
                    return $"~/Custom/Themes/{themeName}/img/{themeImageName}.png";
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the hardcoded GEO potential actions for Schema.org ReadAction.
        /// </summary>
        /// <remarks>
        /// JSON-formatted ReadAction metadata for AI systems.
        /// </remarks>
        public string GEOPotentialActions { get; set; }

        /// <summary>
        /// Gets or sets the social media and profile URLs (sameAs) for schema.org markup.
        /// </summary>
        /// <remarks>
        /// JSON array of URLs representing the blog's social media profiles and related URLs.
        /// Example: ["https://github.com/username", "https://linkedin.com/company/name"]
        /// Used in Schema.org Blog schema for discoverability and verification.
        /// </remarks>
        public string SameAs { get; set; }

        #endregion

        #region Legacy

        /// <summary>
        /// Specifies the moderation mode used by the blog.
        /// </summary>
        /// <remarks>
        /// This enum defines the available moderation strategies for comments and other review workflows.
        /// </remarks>
        public enum Moderation 
        { 
            /// <summary>Manual moderation mode.</summary>
            Manual, 
            /// <summary>Automatic moderation mode.</summary>
            Auto, 
            /// <summary>Disqus moderation platform.</summary>
            Disqus 
        }

        /// <summary>
        /// Gets the effective moderation mode for the blog.
        /// </summary>
        /// <remarks>
        /// The current implementation always returns the automatic moderation mode for backward compatibility.
        /// </remarks>
        public Moderation ModerationType { get { return Moderation.Auto; } }

        #endregion

    }
}