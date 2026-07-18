using System.Collections.Generic;

namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Represents a view model for the trash/recycle bin feature.
    /// </summary>
    /// <remarks>
    /// This view model provides data for displaying deleted items in a trash/recycle bin interface.
    /// It includes a collection of trash items, the total count of items in trash, and configuration
    /// settings for trash management such as retention limits.
    /// </remarks>
    public class TrashVM
    {
        /// <summary>
        /// Gets or sets the list of trash items.
        /// </summary>
        /// <remarks>
        /// Contains the collection of deleted items currently in the trash. Each item represents
        /// a deleted post, page, or other deletable content that can potentially be restored.
        /// </remarks>
        public List<TrashItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the total count of items in the trash.
        /// </summary>
        /// <remarks>
        /// Provides a quick reference to the total number of items currently in the trash without
        /// needing to count the Items collection.
        /// </remarks>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items to retain in the trash.
        /// </summary>
        /// <remarks>
        /// Specifies the retention threshold for trash items. When the trash exceeds this limit,
        /// older items may be permanently deleted to maintain the specified threshold.
        /// </remarks>
        public int Threshold { get; set; }
    }
}
