namespace BlogEngine.Core.API.MetaWeblog
{
    /// <summary>
    /// MetaWeblog Category struct
    ///     returned as an array from GetCategories
    /// </summary>
#pragma warning disable CS0649 // Field is never assigned, will always have default value this is a false positive, as the fields are populated via reflection from the XML-RPC call
#pragma warning disable S101 // Types should be named in PascalCase
    internal struct MWACategory
    {
        #region Constants and Fields

        /// <summary>
        ///     Category title
        /// </summary>
        public string description;

        /// <summary>
        ///     Url to thml display of category
        /// </summary>
        public string htmlUrl;

        /// <summary>
        ///     The guid of the category
        /// </summary>
        public string id;

        /// <summary>
        ///     Url to RSS for category
        /// </summary>
        public string rssUrl;

        /// <summary>
        ///     The title/name of the category
        /// </summary>
        public string title;

        #endregion
    }
#pragma warning restore S101
#pragma warning restore CS0649
}