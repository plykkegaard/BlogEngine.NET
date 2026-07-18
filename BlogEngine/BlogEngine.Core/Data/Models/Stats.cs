namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Represents blog statistics and content metrics.
    /// </summary>
    /// <remarks>
    /// This class provides a snapshot of various blog statistics including post/page counts,
    /// comment metrics, user counts, and category/tag statistics. It is typically used for
    /// dashboard displays and administrative overview pages.
    /// </remarks>
    public class Stats
    {
        /// <summary>
        /// Gets or sets the count of published posts.
        /// </summary>
        /// <remarks>
        /// Represents the number of posts that have been published and are visible to readers.
        /// </remarks>
        public int PublishedPostsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of draft posts.
        /// </summary>
        /// <remarks>
        /// Represents the number of unpublished posts that are saved as drafts by authors.
        /// </remarks>
        public int DraftPostsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of published pages.
        /// </summary>
        /// <remarks>
        /// Represents the number of static pages that have been published and are visible to readers.
        /// </remarks>
        public int PublishedPagesCount { get; set; }

        /// <summary>
        /// Gets or sets the count of draft pages.
        /// </summary>
        /// <remarks>
        /// Represents the number of unpublished static pages that are saved as drafts.
        /// </remarks>
        public int DraftPagesCount { get; set; }

        /// <summary>
        /// Gets or sets the count of published comments.
        /// </summary>
        /// <remarks>
        /// Represents the number of comments that have been approved and are displayed on the blog.
        /// </remarks>
        public int PublishedCommentsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of unapproved comments.
        /// </summary>
        /// <remarks>
        /// Represents the number of comments that are pending moderation and have not yet been approved for display.
        /// </remarks>
        public int UnapprovedCommentsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of spam comments.
        /// </summary>
        /// <remarks>
        /// Represents the number of comments that have been identified and flagged as spam.
        /// </remarks>
        public int SpamCommentsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of categories.
        /// </summary>
        /// <remarks>
        /// Represents the total number of post categories that have been created on the blog.
        /// </remarks>
        public int CategoriesCount { get; set; }

        /// <summary>
        /// Gets or sets the count of tags.
        /// </summary>
        /// <remarks>
        /// Represents the total number of unique tags that have been applied to posts.
        /// </remarks>
        public int TagsCount { get; set; }

        /// <summary>
        /// Gets or sets the count of users.
        /// </summary>
        /// <remarks>
        /// Represents the total number of user accounts registered on the blog.
        /// </remarks>
        public int UsersCount { get; set; }

        /// <summary>
        /// Gets or sets the subscriber count or status.
        /// </summary>
        /// <remarks>
        /// Represents the number of blog subscribers or subscription status information.
        /// This property is a string to allow for flexible representation of subscriber data.
        /// </remarks>
        public string SubscribersCount { get; set; }
    }
}
