using BlogEngine.Core.Data.Models;
using System.Collections.Generic;

namespace BlogEngine.Core.Data.ViewModels
{
    /// <summary>
    /// View model for managing and displaying blog comments.
    /// </summary>
    /// <remarks>
    /// This view model provides a comprehensive data structure for comment management interfaces,
    /// including a list of comment items, the currently selected comment, and detailed information
    /// about the selected comment. It is typically used in administrative panels for comment moderation
    /// and management.
    /// </remarks>
    public class CommentsVM
    {
        /// <summary>
        /// Gets or sets the list of comment items.
        /// </summary>
        /// <remarks>
        /// Contains a collection of comment summaries or brief representations that can be displayed
        /// in a list view for browsing and selecting comments.
        /// </remarks>
        public List<CommentItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the currently selected comment item.
        /// </summary>
        /// <remarks>
        /// Represents the comment that is currently selected in the UI for viewing or editing.
        /// Used to track which comment the user is interacting with.
        /// </remarks>
        public CommentItem SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the detailed information of the selected comment.
        /// </summary>
        /// <remarks>
        /// Contains comprehensive details about the selected comment including full content,
        /// metadata, and moderation information. This is typically displayed when a user
        /// wants to view or edit a specific comment.
        /// </remarks>
        public CommentDetail Detail { get; set; }
    }
}
