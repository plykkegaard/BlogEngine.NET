<%@ Control Language="C#" EnableViewState="False" Inherits="BlogEngine.Core.Web.Controls.CommentViewBase" %>
<div>
	<h4>By <%=EncodedAuthor%> at <%= Comment.DateCreated %></h4>
	<p><%= Text %></p>
</div>