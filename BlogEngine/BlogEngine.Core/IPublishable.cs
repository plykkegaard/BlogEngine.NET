namespace BlogEngine.Core
{
    using System;

    /// <summary>
    /// Defines the contract for publishable content items in the blog.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by classes that represent publishable content such as posts and pages.
    /// Implementers of this interface support search functionality, RSS/ATOM syndication, and other publishing
    /// features. The interface provides essential properties for accessing content metadata, identifiers,
    /// publication status, and links.
    /// </remarks>
    public interface IPublishable
    {
        #region Properties

        /// <summary>
        /// Gets the absolute web link to the publishable item.
        /// </summary>
        /// <remarks>
        /// Provides a full URI that can be used to access the item from any location, including external sites.
        /// </remarks>
        /// <value>A Uri object representing the absolute link to the item.</value>
        Uri AbsoluteLink { get; }

        /// <summary>
        /// Gets the author of the publishable item.
        /// </summary>
        /// <remarks>
        /// Returns the name or identifier of the user who created or is credited with the item.
        /// </remarks>
        /// <value>The author name as a string.</value>
        string Author { get; }

        /// <summary>
        /// Gets the collection of categories assigned to the item.
        /// </summary>
        /// <remarks>
        /// Categories provide a way to organize and classify publishable content for navigation and filtering.
        /// May be empty if no categories are assigned.
        /// </remarks>
        /// <value>A StateList of Category objects containing all assigned categories.</value>
        StateList<Category> Categories { get; }

        /// <summary>
        /// Gets the collection of tags assigned to the item.
        /// </summary>
        /// <remarks>
        /// Tags provide additional metadata for content organization, search, and tag cloud generation.
        /// May be empty if no tags are assigned.
        /// </remarks>
        /// <value>A StateList of tag strings containing all assigned tags.</value>
        StateList<string> Tags { get; }

        /// <summary>
        /// Gets the main content body of the publishable item.
        /// </summary>
        /// <remarks>
        /// Contains the full or formatted content of the item, typically displayed on the post/page view.
        /// May include HTML markup depending on how the item is stored and rendered.
        /// </remarks>
        /// <value>The content as a string.</value>
        string Content { get; }

        /// <summary>
        /// Gets the date and time when the item was initially created.
        /// </summary>
        /// <remarks>
        /// This timestamp is set once when the item is first saved and typically does not change.
        /// </remarks>
        /// <value>A DateTime object representing the creation date and time.</value>
        DateTime DateCreated { get; }

        /// <summary>
        /// Gets the date and time of the most recent modification to the item.
        /// </summary>
        /// <remarks>
        /// Updated whenever the item is edited or its properties are changed.
        /// </remarks>
        /// <value>A DateTime object representing the last modification date and time.</value>
        DateTime DateModified { get; }

        /// <summary>
        /// Gets the description or excerpt of the publishable item.
        /// </summary>
        /// <remarks>
        /// A brief summary of the item's content, typically used in feeds, lists, and meta descriptions.
        /// </remarks>
        /// <value>The description as a string.</value>
        string Description { get; }

        /// <summary>
        /// Gets the unique identifier of the publishable item.
        /// </summary>
        /// <remarks>
        /// A GUID that uniquely identifies this item across the entire blog system,
        /// even if the item is moved or migrated between instances.
        /// </remarks>
        /// <value>A Guid representing the unique item identifier.</value>
        Guid Id { get; }

        /// <summary>
        /// Gets the unique identifier of the blog instance containing this item.
        /// </summary>
        /// <remarks>
        /// In multi-blog scenarios, this identifies which blog instance the item belongs to.
        /// </remarks>
        /// <value>A Guid representing the blog instance identifier.</value>
        Guid BlogId { get; }

        /// <summary>
        /// Gets the blog instance containing this publishable item.
        /// </summary>
        /// <remarks>
        /// Provides access to the blog configuration and other blog-specific settings.
        /// </remarks>
        /// <value>A Blog object representing the blog instance.</value>
        Blog Blog { get; }

        /// <summary>
        /// Gets a value indicating whether the item is published and visible in the blog.
        /// </summary>
        /// <remarks>
        /// Published items are visible according to their visibility settings. Unpublished items
        /// are typically only visible to administrators.
        /// </remarks>
        /// <value>True if the item is published; otherwise, false.</value>
        bool IsPublished { get; }

        /// <summary>
        /// Gets the relative website link to the publishable item.
        /// </summary>
        /// <remarks>
        /// Provides a link relative to the web root, useful for on-site references and navigation.
        /// Example: "/posts/my-post" instead of "http://example.com/posts/my-post".
        /// </remarks>
        /// <value>The relative link as a string.</value>
        string RelativeLink { get; }

        /// <summary>
        /// Gets the relative link if the blog is on the same host, otherwise returns the absolute link.
        /// </summary>
        /// <remarks>
        /// Returns a relative link if possible when the blog's hostname matches the current site's hostname.
        /// If the hostnames differ (in multi-blog aggregation scenarios), the absolute link is returned instead.
        /// This is useful for proper link handling in aggregated blog environments.
        /// </remarks>
        /// <value>The relative link if on the same host; otherwise, the absolute link as a string.</value>
        string RelativeOrAbsoluteLink { get; }

        /// <summary>
        /// Gets the title of the publishable item.
        /// </summary>
        /// <remarks>
        /// The primary heading or name of the item, displayed in page titles, lists, and feeds.
        /// </remarks>
        /// <value>The title as a string.</value>
        string Title { get; }

        /// <summary>
        /// Gets a value indicating whether the item should be displayed in listings and views.
        /// </summary>
        /// <remarks>
        /// Controls basic visibility of the item, separate from publication status.
        /// An item might be published but not visible, or vice versa, depending on business logic.
        /// </remarks>
        /// <value>True if the item should be shown; otherwise, false.</value>
        bool IsVisible { get; }

        /// <summary>
        /// Gets a value indicating whether the item is visible to the general public.
        /// </summary>
        /// <remarks>
        /// Distinguishes between items visible only to authenticated administrators or content creators
        /// versus items visible to all site visitors. This is a stricter visibility check than IsVisible.
        /// </remarks>
        /// <value>True if the item is visible to the public; otherwise, false.</value>
        bool IsVisibleToPublic { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Raises the Serving event for this publishable item.
        /// </summary>
        /// <remarks>
        /// This method is called to trigger pre-rendering or pre-serving event handlers.
        /// Allows subscribers to intercept and modify the item before it is served to the user.
        /// </remarks>
        /// <param name="eventArgs">The ServingEventArgs instance containing the event data and context.</param>
        void OnServing(ServingEventArgs eventArgs);

        #endregion
    }
}