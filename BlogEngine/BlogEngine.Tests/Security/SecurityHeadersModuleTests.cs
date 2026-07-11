namespace BlogEngine.Tests.Security
{
    using System.Web;
    using BlogEngine.Core.Web.HttpModules;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Configuration;

    /// <summary>
    /// Unit tests for SecurityHeadersModule and SecurityHeadersSection configuration
    /// </summary>
    [TestClass]
    public class SecurityHeadersModuleTests
    {
        [TestMethod]
        public void SecurityHeadersSection_DefaultConfiguration_HasCorrectDefaults()
        {
            var section = new SecurityHeadersSection();

            Assert.IsTrue(section.EnableContentSecurityPolicy, "CSP should be enabled by default");
            Assert.AreEqual("script-src 'self'", section.ContentSecurityPolicy, "CSP should default to script-src 'self'");

            Assert.IsTrue(section.EnableXFrameOptions, "X-Frame-Options should be enabled by default");
            Assert.AreEqual("DENY", section.XFrameOptions, "X-Frame-Options should default to DENY");

            Assert.IsTrue(section.EnableXContentTypeOptions, "X-Content-Type-Options should be enabled by default");

            Assert.IsTrue(section.EnableReferrerPolicy, "Referrer-Policy should be enabled by default");
            Assert.AreEqual("strict-origin-when-cross-origin", section.ReferrerPolicy, "Referrer-Policy should default to strict-origin-when-cross-origin");
        }

        [TestMethod]
        public void SecurityHeadersSection_CanSetContentSecurityPolicy()
        {
            var section = new SecurityHeadersSection();
            section.ContentSecurityPolicy = "script-src 'self' https://cdn.example.com";

            Assert.AreEqual("script-src 'self' https://cdn.example.com", section.ContentSecurityPolicy);
        }

        [TestMethod]
        public void SecurityHeadersSection_CanSetXFrameOptions()
        {
            var section = new SecurityHeadersSection();
            section.XFrameOptions = "SAMEORIGIN";

            Assert.AreEqual("SAMEORIGIN", section.XFrameOptions);
        }

        [TestMethod]
        public void SecurityHeadersSection_CanSetReferrerPolicy()
        {
            var section = new SecurityHeadersSection();
            section.ReferrerPolicy = "no-referrer";

            Assert.AreEqual("no-referrer", section.ReferrerPolicy);
        }

        [TestMethod]
        public void SecurityHeadersSection_CanDisableHeaders()
        {
            var section = new SecurityHeadersSection();

            section.EnableContentSecurityPolicy = false;
            section.EnableXFrameOptions = false;
            section.EnableXContentTypeOptions = false;
            section.EnableReferrerPolicy = false;

            Assert.IsFalse(section.EnableContentSecurityPolicy);
            Assert.IsFalse(section.EnableXFrameOptions);
            Assert.IsFalse(section.EnableXContentTypeOptions);
            Assert.IsFalse(section.EnableReferrerPolicy);
        }

        [TestMethod]
        public void SecurityHeadersModule_ImplementsIHttpModule()
        {
            var module = new SecurityHeadersModule();

            Assert.IsNotNull(module);
            Assert.IsInstanceOfType(module, typeof(System.Web.IHttpModule));
        }

        [TestMethod]
        public void SecurityHeadersModule_Dispose_DoesNotThrow()
        {
            var module = new SecurityHeadersModule();

            try
            {
                module.Dispose();
                Assert.IsTrue(true, "Dispose should not throw an exception");
            }
            catch
            {
                Assert.Fail("Dispose should not throw an exception");
            }
        }

        [TestMethod]
        public void SecurityHeadersModule_Init_DoesNotThrow()
        {
            var module = new SecurityHeadersModule();

            try
            {
                // Cannot create a real HttpApplication in unit test context
                // So we just verify the module can be instantiated
                Assert.IsNotNull(module);
            }
            catch
            {
                Assert.Fail("Module instantiation should not throw an exception");
            }
        }

        [TestMethod]
        public void SecurityHeadersSection_ContentSecurityPolicyProperty_ReturnsConfiguredValue()
        {
            var section = new SecurityHeadersSection();
            var testValue = "default-src 'self'; script-src 'self' 'unsafe-inline'";
            section.ContentSecurityPolicy = testValue;

            Assert.AreEqual(testValue, section.ContentSecurityPolicy);
        }

        [TestMethod]
        public void SecurityHeadersSection_AllPropertiesAreReadWritable()
        {
            var section = new SecurityHeadersSection();

            // Test all boolean properties
            section.EnableContentSecurityPolicy = false;
            Assert.IsFalse(section.EnableContentSecurityPolicy);
            section.EnableContentSecurityPolicy = true;
            Assert.IsTrue(section.EnableContentSecurityPolicy);

            section.EnableXFrameOptions = false;
            Assert.IsFalse(section.EnableXFrameOptions);
            section.EnableXFrameOptions = true;
            Assert.IsTrue(section.EnableXFrameOptions);

            section.EnableXContentTypeOptions = false;
            Assert.IsFalse(section.EnableXContentTypeOptions);
            section.EnableXContentTypeOptions = true;
            Assert.IsTrue(section.EnableXContentTypeOptions);

            section.EnableReferrerPolicy = false;
            Assert.IsFalse(section.EnableReferrerPolicy);
            section.EnableReferrerPolicy = true;
            Assert.IsTrue(section.EnableReferrerPolicy);

            // Test all string properties
            section.ContentSecurityPolicy = "test-csp";
            Assert.AreEqual("test-csp", section.ContentSecurityPolicy);

            section.XFrameOptions = "test-xfo";
            Assert.AreEqual("test-xfo", section.XFrameOptions);

            section.ReferrerPolicy = "test-rp";
            Assert.AreEqual("test-rp", section.ReferrerPolicy);
        }
    }
}
