using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogEngine.Core.Data.Services
{
    /// <summary>
    /// Generates a weighted tag cloud with HTML links for blog tags.
    /// </summary>
    /// <remarks>
    /// This service creates a tag cloud by analyzing all tags used in public blog posts and generating
    /// weighted HTML links based on tag frequency. Tags are classified into size categories (biggest, big, 
    /// medium, small, smallest) based on their usage frequency. The class is blog-instance aware and uses
    /// thread-safe caching to optimize performance across multiple blog instances.
    /// </remarks>
    public class TagCloud
    {
        /// <summary>
        /// HTML template for tag links.
        /// </summary>
        /// <remarks>
        /// Defines the format for rendering tag links with href, CSS class, title, and display text.
        /// </remarks>
        private const string Link = "<a href=\"{0}\" class=\"{1}\" title=\"{2}\">{3}</a> ";

        /// <summary>
        /// Synchronization object for thread-safe access to the weighted list cache.
        /// </summary>
        /// <remarks>
        /// Ensures thread safety when multiple threads access the static weighted list dictionary.
        /// </remarks>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Thread-safe cache of weighted tag lists per blog instance.
        /// </summary>
        /// <remarks>
        /// Dictionary keyed by blog ID (Guid) containing tag name to CSS weight class mappings.
        /// Cached for performance optimization across multiple requests.
        /// </remarks>
        private static Dictionary<Guid, Dictionary<string, string>> weightedList = new Dictionary<Guid, Dictionary<string, string>>();

        /// <summary>
        /// Gets or sets the minimum number of posts a tag must have to appear in the cloud.
        /// </summary>
        /// <remarks>
        /// Tags with fewer posts than this threshold will be excluded from the tag cloud.
        /// Default value is 1.
        /// </remarks>
        public int MinimumPosts { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tags to display in the cloud.
        /// </summary>
        /// <remarks>
        /// When set to a positive value, limits the number of tags shown. A value of -1 means no limit.
        /// Default value is -1 (unlimited).
        /// </remarks>
        public int TagCloudSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the TagCloud class.
        /// </summary>
        /// <remarks>
        /// Sets default values: MinimumPosts = 1 (tags with at least one post), TagCloudSize = -1 (unlimited).
        /// </remarks>
        public TagCloud()
        {
            MinimumPosts = 1;
            TagCloudSize = -1;
        }

        /// <summary>
        /// Generates a list of HTML-formatted tag cloud links.
        /// </summary>
        /// <remarks>
        /// Returns weighted tag links where each tag is classified by CSS size class (biggest, big, medium, small, smallest)
        /// based on its frequency relative to the most-used tag. Links are URL-encoded and include appropriate title attributes.
        /// </remarks>
        /// <returns>A list of HTML anchor elements representing the tag cloud with appropriate CSS weight classes.</returns>
        public List<string> Links()
        {
            var links = new List<string>();
            foreach (var key in WeightedList.Keys)
            {
                var link = string.Format(
                    Link,
                    $"{Utils.AbsoluteWebRoot}?tag={HttpUtility.UrlEncode(key)}",
                    WeightedList[key], $"Tag: {key}", key);
                links.Add(link);  
            }
            return links;
        }

        /// <summary>
        /// Gets the weighted tag list for the current blog instance.
        /// </summary>
        /// <remarks>
        /// Returns a lazy-loaded, thread-safe dictionary of tag names mapped to CSS weight classes.
        /// Uses double-checked locking pattern to ensure the cache is initialized only once per blog instance.
        /// If the cache doesn't exist for the current blog, it creates and initializes it automatically.
        /// </remarks>
        /// <returns>A dictionary mapping tag names to CSS weight classes (biggest, big, medium, small, smallest).</returns>
        private Dictionary<string, string> WeightedList
        {
            get
            {
                Dictionary<string, string> list = null;
                Guid blogId = Blog.CurrentInstance.Id;

                if (!weightedList.TryGetValue(blogId, out list))
                {
                    lock (SyncRoot)
                    {
                        if (!weightedList.TryGetValue(blogId, out list))
                        {
                            list = new Dictionary<string, string>();
                            weightedList.Add(blogId, list);

                            this.SortList();
                        }
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Creates an unweighted, sorted list of all tags with their frequency counts.
        /// </summary>
        /// <remarks>
        /// Scans all public blog posts and collects their tags, counting the frequency of each tag.
        /// Only tags from posts that are visible to the public are included. The results are returned
        /// in a SortedDictionary for consistent ordering.
        /// </remarks>
        /// <returns>A sorted dictionary mapping tag names to their usage frequency across all public posts.</returns>
        private static SortedDictionary<string, int> CreateRawList()
        {
            var dic = new SortedDictionary<string, int>();
            foreach (var tag in Post.Posts.Where(post => post.IsVisibleToPublic).SelectMany(post => post.Tags))
            {
                if (dic.ContainsKey(tag))
                {
                    dic[tag]++;
                }
                else
                {
                    dic[tag] = 1;
                }
            }

            return dic;
        }

        /// <summary>
        /// Generates and sorts the weighted tag list for the current blog instance.
        /// </summary>
        /// <remarks>
        /// Analyzes tag frequency data and applies weighting logic to classify tags into CSS weight classes.
        /// Weight calculation is based on relative tag frequency: tags with higher usage receive heavier weight classes.
        /// Classification thresholds are:
        /// - "biggest": weight >= 99%
        /// - "big": weight >= 70%
        /// - "medium": weight >= 40%
        /// - "small": weight >= 20%
        /// - "smallest": weight >= 3%
        /// 
        /// Tags below MinimumPosts threshold or beyond TagCloudSize limit are filtered out.
        /// </remarks>
        private void SortList()
        {
            var dic = CreateRawList();
            var max = dic.Values.Max();

            var currentTagCount = 0;

            int count = currentTagCount;
            foreach (var key in dic.Keys.Where(key => dic[key] >= MinimumPosts).Where(key => TagCloudSize <= 0 || count <= TagCloudSize))
            {
                currentTagCount++;

                var weight = ((double)dic[key] / max) * 100;
                if (weight >= 99)
                {
                    WeightedList.Add(key, "biggest");
                }
                else if (weight >= 70)
                {
                    WeightedList.Add(key, "big");
                }
                else if (weight >= 40)
                {
                    WeightedList.Add(key, "medium");
                }
                else if (weight >= 20)
                {
                    WeightedList.Add(key, "small");
                }
                else if (weight >= 3)
                {
                    WeightedList.Add(key, "smallest");
                }
            }
        }

    }
}
