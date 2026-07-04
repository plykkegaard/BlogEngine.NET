<%@ Application Language="C#" %>
<%@ Import Namespace="BlogEngine.NET.App_Start" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script RunAt="server">
    void Application_BeginRequest(object sender, EventArgs e)
    {
        var app = (HttpApplication)sender;
        BlogEngineConfig.Initialize(app.Context);
    }
        
    void Application_PreRequestHandlerExecute(object sender, EventArgs e)
    {
        BlogEngineConfig.SetCulture(sender, e);
    }

    protected void Application_EndRequest(object sender, EventArgs e)
    {
        // Properly set SameSite attribute on cookies for .NET Framework 4.8
        var httpContext = HttpContext.Current;
        if (httpContext != null && httpContext.Response != null)
        {
            var cookies = httpContext.Response.Headers.GetValues("Set-Cookie");
            if (cookies != null)
            {
                var modifiedCookies = new List<string>();
                foreach (var cookie in cookies)
                {
                    // Only add SameSite if not already present
                    if (!cookie.Contains("SameSite="))
                    {
                        modifiedCookies.Add(cookie + "; SameSite=Lax");
                    }
                    else
                    {
                        modifiedCookies.Add(cookie);
                    }
                }

                httpContext.Response.Headers.Remove("Set-Cookie");
                foreach (var modifiedCookie in modifiedCookies)
                {
                    httpContext.Response.Headers.Add("Set-Cookie", modifiedCookie);
                }
            }
        }
    }
</script>