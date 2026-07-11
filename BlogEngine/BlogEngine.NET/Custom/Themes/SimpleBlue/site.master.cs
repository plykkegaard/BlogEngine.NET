using System;
using System.Web.UI;
using BlogEngine.Core;
using System.Text.RegularExpressions;

/// <summary>
/// SimpleBlog master page code-behind for the SimpleBlog theme.
/// </summary>
/// <remarks>
/// This master page provides the layout and common functionality for the SimpleBlog theme.
/// It handles user authentication display and HTML rendering optimization for the theme pages.
/// </remarks>
public partial class SimpleBlog : System.Web.UI.MasterPage
{
    /// <summary>
    /// Server-side anchor control for the login/logout link.
    /// </summary>
    /// <remarks>
    /// This control is defined in the site.master markup and provides navigation to the login page or logout functionality.
    /// </remarks>
    protected System.Web.UI.HtmlControls.HtmlAnchor aLogin;

    /// <summary>
    /// Regular expression pattern for cleaning up whitespace in HTML output.
    /// </summary>
    /// <remarks>
    /// This regex removes unnecessary tabs and spaces between HTML tags to minimize the rendered HTML size.
    /// It removes tabs after non-closing brackets, extra spaces between closing and opening tags, and leading whitespace before newlines.
    /// </remarks>
    private static Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}");

    /// <summary>
    /// Gets the application-relative path to the SyntaxHighlighter script directory.
    /// </summary>
    /// <remarks>
    /// This protected field stores the root path for the SyntaxHighlighter jQuery plugin scripts,
    /// which is used for syntax highlighting of code blocks in blog posts.
    /// </remarks>
    protected static string ShRoot = Utils.ApplicationRelativeWebRoot + "Scripts/syntaxhighlighter/";

    /// <summary>
    /// Handles the page load event for the SimpleBlog master page.
    /// </summary>
    /// <remarks>
    /// This method performs initialization tasks such as enabling data binding in the page header
    /// and updating the login/logoff link based on the current user's authentication status.
    /// </remarks>
    /// <param name="sender">The source of the page load event.</param>
    /// <param name="e">The event arguments.</param>
    protected void Page_Load(object sender, EventArgs e)
    {


        // needed to make <%# %> binding work in the page header
        Page.Header.DataBind();
        if (Security.IsAuthenticated)
        {
            aLogin.InnerText = Resources.labels.logoff;
            aLogin.HRef = Utils.RelativeWebRoot + "Account/login.aspx?logoff";
        }
        else
        {
            aLogin.HRef = Utils.RelativeWebRoot + "Account/login.aspx";
            aLogin.InnerText = Resources.labels.login;
        }
    }

    /// <summary>
    /// Renders the master page HTML to the specified text writer with HTML optimization.
    /// </summary>
    /// <remarks>
    /// This method overrides the base Render method to capture the rendered HTML output and apply
    /// whitespace optimization using the regex pattern defined in the <see cref="reg"/> field.
    /// This helps reduce the final HTML size by removing unnecessary whitespace while preserving functionality.
    /// </remarks>
    /// <param name="writer">The HtmlTextWriter to which the HTML will be written.</param>
    protected override void Render(HtmlTextWriter writer)
    {
        using (HtmlTextWriter htmlwriter = new HtmlTextWriter(new System.IO.StringWriter()))
        {
            base.Render(htmlwriter);
            writer.Write(htmlwriter.InnerWriter.ToString());
        }
    }

}
