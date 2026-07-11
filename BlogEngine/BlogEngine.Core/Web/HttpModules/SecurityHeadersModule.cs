namespace BlogEngine.Core.Web.HttpModules
{
    using System;
    using System.Configuration;
    using System.Web;

    /// <summary>
    /// HTTP module that adds modern security headers to all responses.
    /// </summary>
    /// <remarks>
    /// This module adds the following security headers to protect against common web vulnerabilities:
    /// - Content-Security-Policy: Prevents XSS attacks by controlling script sources
    /// - X-Frame-Options: Prevents clickjacking attacks
    /// - X-Content-Type-Options: Prevents MIME-type sniffing
    /// - Referrer-Policy: Controls referrer information leakage
    /// 
    /// Configuration is read from web.config under BlogEngine/securityHeaders section.
    /// </remarks>
    public sealed class SecurityHeadersModule : IHttpModule
    {
        #region Constants and Fields

        /// <summary>
        /// Configuration section for security headers
        /// </summary>
        private static SecurityHeadersSection _config;

        /// <summary>
        /// Lock object for thread-safe configuration loading
        /// </summary>
        private static readonly object _syncRoot = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the security headers configuration from web.config
        /// </summary>
        private static SecurityHeadersSection Config
        {
            get
            {
                if (_config == null)
                {
                    lock (_syncRoot)
                    {
                        if (_config == null)
                        {
                            try
                            {
                                _config = ConfigurationManager.GetSection("BlogEngine/securityHeaders") as SecurityHeadersSection;
                            }
                            catch (Exception)
                            {
                                // If configuration fails, return null and headers won't be added
                                _config = null;
                            }
                        }
                    }
                }
                return _config;
            }
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpModule

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module 
        ///     that implements <see cref="T:System.Web.IHttpModule"></see>.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpApplication"></see> 
        ///     that provides access to the methods, properties, and events common to 
        ///     all application objects within an ASP.NET application.
        /// </param>
        public void Init(HttpApplication context)
        {
            context.EndRequest += OnEndRequest;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Handles the EndRequest event to add security headers to the response.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void OnEndRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;
            if (application == null || application.Context == null)
            {
                return;
            }

            var response = application.Context.Response;
            if (response == null)
            {
                return;
            }

            var config = Config;
            if (config == null)
            {
                // If no configuration is available, apply default security headers
                AddDefaultSecurityHeaders(response);
                return;
            }

            // Add Content-Security-Policy header
            if (config.EnableContentSecurityPolicy && !string.IsNullOrWhiteSpace(config.ContentSecurityPolicy))
            {
                AddHeaderSafely(response, "Content-Security-Policy", config.ContentSecurityPolicy);
            }

            // Add X-Frame-Options header
            if (config.EnableXFrameOptions && !string.IsNullOrWhiteSpace(config.XFrameOptions))
            {
                AddHeaderSafely(response, "X-Frame-Options", config.XFrameOptions);
            }

            // Add X-Content-Type-Options header
            if (config.EnableXContentTypeOptions)
            {
                AddHeaderSafely(response, "X-Content-Type-Options", "nosniff");
            }

            // Add Referrer-Policy header
            if (config.EnableReferrerPolicy && !string.IsNullOrWhiteSpace(config.ReferrerPolicy))
            {
                AddHeaderSafely(response, "Referrer-Policy", config.ReferrerPolicy);
            }
        }

        /// <summary>
        /// Adds default security headers when no configuration is available.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        private static void AddDefaultSecurityHeaders(HttpResponse response)
        {
            AddHeaderSafely(response, "Content-Security-Policy", "script-src 'self'");
            AddHeaderSafely(response, "X-Frame-Options", "DENY");
            AddHeaderSafely(response, "X-Content-Type-Options", "nosniff");
            AddHeaderSafely(response, "Referrer-Policy", "strict-origin-when-cross-origin");
        }

        /// <summary>
        /// Adds a header to the response only if it doesn't already exist.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="headerName">The name of the header.</param>
        /// <param name="headerValue">The value of the header.</param>
        private static void AddHeaderSafely(HttpResponse response, string headerName, string headerValue)
        {
            try
            {
                // Check if header already exists
                var existingHeader = response.Headers[headerName];
                if (string.IsNullOrEmpty(existingHeader))
                {
                    response.Headers.Add(headerName, headerValue);
                }
            }
            catch (Exception)
            {
                // Silently fail if header cannot be added (e.g., headers already sent)
            }
        }

        #endregion
    }
}
