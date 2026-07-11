namespace BlogEngine.Core.Web.HttpModules
{
    using System.Configuration;

    /// <summary>
    /// Configuration section for security headers settings.
    /// </summary>
    /// <remarks>
    /// This configuration section allows administrators to enable/disable and customize
    /// security headers that are added to HTTP responses. Configuration is read from
    /// web.config under BlogEngine/securityHeaders section.
    /// </remarks>
    public class SecurityHeadersSection : ConfigurationSection
    {
        #region Content-Security-Policy

        /// <summary>
        /// Gets or sets a value indicating whether Content-Security-Policy header should be added.
        /// </summary>
        /// <remarks>
        /// Content-Security-Policy (CSP) helps prevent XSS attacks by controlling which scripts
        /// can be executed on the page.
        /// </remarks>
        [ConfigurationProperty("enableContentSecurityPolicy", DefaultValue = true, IsRequired = false)]
        public bool EnableContentSecurityPolicy
        {
            get { return (bool)base["enableContentSecurityPolicy"]; }
            set { base["enableContentSecurityPolicy"] = value; }
        }

        /// <summary>
        /// Gets or sets the Content-Security-Policy directive value.
        /// </summary>
        /// <remarks>
        /// Default is "script-src 'self'" which only allows scripts from the same origin.
        /// To allow inline scripts, add 'unsafe-inline' (not recommended).
        /// To allow specific CDNs, add their domains (e.g., "script-src 'self' https://cdn.example.com").
        /// </remarks>
        [ConfigurationProperty("contentSecurityPolicy", DefaultValue = "script-src 'self'", IsRequired = false)]
        [StringValidator(MinLength = 1, MaxLength = 500)]
        public string ContentSecurityPolicy
        {
            get { return (string)base["contentSecurityPolicy"]; }
            set { base["contentSecurityPolicy"] = value; }
        }

        #endregion

        #region X-Frame-Options

        /// <summary>
        /// Gets or sets a value indicating whether X-Frame-Options header should be added.
        /// </summary>
        /// <remarks>
        /// X-Frame-Options helps prevent clickjacking attacks by controlling whether the page
        /// can be embedded in an iframe.
        /// </remarks>
        [ConfigurationProperty("enableXFrameOptions", DefaultValue = true, IsRequired = false)]
        public bool EnableXFrameOptions
        {
            get { return (bool)base["enableXFrameOptions"]; }
            set { base["enableXFrameOptions"] = value; }
        }

        /// <summary>
        /// Gets or sets the X-Frame-Options directive value.
        /// </summary>
        /// <remarks>
        /// Valid values: DENY, SAMEORIGIN.
        /// DENY prevents any domain from framing the content.
        /// SAMEORIGIN allows only the same origin to frame the content.
        /// </remarks>
        [ConfigurationProperty("xFrameOptions", DefaultValue = "DENY", IsRequired = false)]
        [StringValidator(MinLength = 1, MaxLength = 50)]
        public string XFrameOptions
        {
            get { return (string)base["xFrameOptions"]; }
            set { base["xFrameOptions"] = value; }
        }

        #endregion

        #region X-Content-Type-Options

        /// <summary>
        /// Gets or sets a value indicating whether X-Content-Type-Options header should be added.
        /// </summary>
        /// <remarks>
        /// X-Content-Type-Options: nosniff prevents browsers from MIME-sniffing a response away
        /// from the declared content-type, reducing exposure to drive-by download attacks.
        /// </remarks>
        [ConfigurationProperty("enableXContentTypeOptions", DefaultValue = true, IsRequired = false)]
        public bool EnableXContentTypeOptions
        {
            get { return (bool)base["enableXContentTypeOptions"]; }
            set { base["enableXContentTypeOptions"] = value; }
        }

        #endregion

        #region Referrer-Policy

        /// <summary>
        /// Gets or sets a value indicating whether Referrer-Policy header should be added.
        /// </summary>
        /// <remarks>
        /// Referrer-Policy controls how much referrer information should be included with requests.
        /// </remarks>
        [ConfigurationProperty("enableReferrerPolicy", DefaultValue = true, IsRequired = false)]
        public bool EnableReferrerPolicy
        {
            get { return (bool)base["enableReferrerPolicy"]; }
            set { base["enableReferrerPolicy"] = value; }
        }

        /// <summary>
        /// Gets or sets the Referrer-Policy directive value.
        /// </summary>
        /// <remarks>
        /// Valid values include:
        /// - no-referrer: Never send referrer information
        /// - no-referrer-when-downgrade: Send full referrer for same-protocol, no referrer for HTTPS to HTTP
        /// - same-origin: Send referrer for same-origin requests only
        /// - origin: Send only the origin (no path/query string)
        /// - strict-origin: Send origin for same-protocol, no referrer for HTTPS to HTTP
        /// - origin-when-cross-origin: Send full referrer for same-origin, origin for cross-origin
        /// - strict-origin-when-cross-origin: Recommended - sends full URL for same-origin, origin for cross-origin HTTPS, nothing for HTTPS to HTTP
        /// - unsafe-url: Always send full referrer (not recommended)
        /// </remarks>
        [ConfigurationProperty("referrerPolicy", DefaultValue = "strict-origin-when-cross-origin", IsRequired = false)]
        [StringValidator(MinLength = 1, MaxLength = 50)]
        public string ReferrerPolicy
        {
            get { return (string)base["referrerPolicy"]; }
            set { base["referrerPolicy"] = value; }
        }

        #endregion
    }
}
