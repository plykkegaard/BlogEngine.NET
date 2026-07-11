<%@ Control Language="C#" EnableViewState="False" Inherits="BlogEngine.Core.Web.Controls.CommentViewBase" %>
<div id="id_<%=Comment.Id %>" class=" clearfix vcard comment<%= Post.Author.Equals(Comment.Author, StringComparison.OrdinalIgnoreCase) ? " self" : "" %>">
    <div class="comment-contents clearfix">
        <div class="float-left gravatar"><%= Gravatar(72)%></div>
        <div class="float-left comment-content">
            <div class="comment-header clearfix">
                <% if (!string.IsNullOrEmpty(SafeWebsiteUrl)) { %>
                    <a href="<%=SafeWebsiteUrl%>" rel="nofollow" class="url fn"><%=EncodedAuthor%></a>
                <% } else { %>
                    <span class="fn"><%=EncodedAuthor%></span>
                <% } %>
                <a class="comment-datetime float-right"><%= Comment.DateCreated  %></a>
            </div>
            <p><%= Text %></p>
            <div class="text-right reply-to">
                <%=ReplyToLink%>
                <div>
                    <%= AdminLinks.Length > 2 ? AdminLinks.Substring(2) : AdminLinks %>
                </div>
            </div>
        </div>
    </div>
    <div class="comment-replies" id="replies_<%=Comment.Id %>" <%= (Comment.Comments.Count == 0 || Comment.Email == "pingback" || Comment.Email == "trackback") ? " style=\"display:none;\"" : "" %>>
        <asp:PlaceHolder ID="phSubComments" runat="server" />
    </div>
</div>
