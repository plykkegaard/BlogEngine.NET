using System;

namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Represents a comment filter item
    /// </summary>
    /// <remarks>
    /// This class is used to define filters for comments, such as blocking or deleting comments based on certain criteria.
    /// </remarks>
    public class CommentFilterItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        /// <remarks>
        /// This property indicates whether the filter item is selected in the user interface.
        /// </remarks>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the Comment Id
        /// </summary>
        /// <remarks>
        /// This property uniquely identifies the comment filter item.
        /// </remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Block, delete etc.
        /// </summary>
        /// <remarks>
        /// This property specifies the action to be taken when the filter criteria are met, such as blocking or deleting a comment.
        /// </remarks>
        public string Action { get; set; }

        /// <summary>
        /// Email, IP etc.
        /// </summary>
        /// <remarks>
        /// This property specifies the subject of the filter, such as an email address or IP address.
        /// </remarks>
        public string Subject { get; set; }

        /// <summary>
        /// Equals, contains etc.
        /// </summary>
        /// <remarks>
        /// This property specifies the operation to be performed for the filter, such as "equals" or "contains".
        /// </remarks>
        public string Operation { get; set; }

        /// <summary>
        /// Content of the filter, like email or IP address
        /// </summary>
        /// <remarks>
        /// This property contains the actual value to be filtered, such as an email address or IP address.
        /// </remarks>
        public string Filter { get; set; }
    }
}
