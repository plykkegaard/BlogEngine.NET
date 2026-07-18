namespace BlogEngine.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using BlogEngine.Core.Data.Models;
    using BlogEngine.Core.Providers;

    /// <summary>
    /// A post is an entry on the blog - a blog post.
    /// </summary>
    [Serializable]
    public class Post : BusinessBase<Post, Guid>, IComparable<Post>, IPublishable
    {
        #region Constants and Fields

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The categories.
        /// </summary>
        private readonly StateList<Category> categories;

        /// <summary>
        /// All comments, including deleted
        /// </summary>
        private readonly List<Comment> allcomments;

        /// <summary>
        /// The notification emails.
        /// </summary>
        private readonly StateList<string> notificationEmails;

        /// <summary>
        /// The post tags.
        /// </summary>
        private readonly StateList<string> tags;

        /// <summary>
        /// The posts.
        /// </summary>
        private static Dictionary<Guid, List<Post>> posts = new Dictionary<Guid, List<Post>>();

        /// <summary>
        /// The deleted posts.
        /// </summary>
        private static Dictionary<Guid, List<Post>> deletedposts = new Dictionary<Guid, List<Post>>();

        /// <summary>
        ///     The author.
        /// </summary>
        private string author;

        /// <summary>
        ///     The content.
        /// </summary>
        private string content;

        /// <summary>
        ///     The description - UNIFIED METADATA: This serves as the primary meta description.
        ///     Part of the unified SEO/GEO metadata model. Used for meta description tags,
        ///     Open Graph og:description, Twitter Card description, and Schema.org description.
        /// </summary>
        /// <remarks>
        ///     BACKWARD COMPATIBILITY: Legacy property maintained for existing code.
        ///     In the unified model, this is the authoritative source for all description metadata.
        ///     SemanticSummary provides an alternative AI-optimized description when set.
        /// </remarks>
        private string description;

        /// <summary>
        ///     Whether the post is comments enabled.
        /// </summary>
        private bool hasCommentsEnabled;

        /// <summary>
        ///     The nested comments.
        /// </summary>
        private List<Comment> nestedComments;

        /// <summary>
        ///     Whether the post is published.
        /// </summary>
        private bool isPublished;

        /// <summary>
        ///     Whether the post is deleted.
        /// </summary>
        private bool isDeleted;

        /// <summary>
        ///     The raters.
        /// </summary>
        private int raters;

        /// <summary>
        ///     The rating.
        /// </summary>
        private float rating;

        /// <summary>
        ///     The slug of the post.
        /// </summary>
        private string slug;

        /// <summary>
        ///     The title.
        /// </summary>
        private string title;

        #region Unified SEO/GEO Metadata Fields

        /// <summary>
        ///     UNIFIED METADATA: Canonical URL for the post.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Prevents duplicate content penalties, establishes primary URL identity
        ///     - GEO: Helps AI systems identify the authoritative source for this content
        ///     Used in: canonical link tag, og:url, Schema.org url property
        /// </remarks>
        private string canonicalUrl;

        /// <summary>
        ///     UNIFIED METADATA: Schema.org type (e.g., "BlogPosting", "Article", "NewsArticle").
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Helps search engines understand content type for rich snippets
        ///     - GEO: Critical for AI systems to properly classify and understand content
        ///     Defaults to "BlogPosting" if not specified.
        /// </remarks>
        private string schemaType;

        /// <summary>
        ///     UNIFIED METADATA: Comma-separated key entities mentioned in the post.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model (primarily GEO-focused).
        ///     - GEO: Named entities (people, organizations, locations, concepts) for AI extraction
        ///     - SEO: Can enhance semantic understanding in advanced search systems
        ///     Example: "Machine Learning, Neural Networks, TensorFlow"
        ///     Used in: Custom metadata tags, Schema.org mentions/about properties
        /// </remarks>
        private string keyEntities;

        /// <summary>
        ///     UNIFIED METADATA: Semantic summary optimized for AI systems.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model (primarily GEO-focused).
        ///     - GEO: Concise, semantically rich summary emphasizing key concepts and relationships
        ///     - SEO: Alternative to traditional description for AI-powered search
        ///     Unlike Description, this focuses on semantic meaning and entity relationships.
        ///     Falls back to Description if not set. Used in: AI-specific meta tags, structured data
        /// </remarks>
        private string semanticSummary;

        /// <summary>
        ///     UNIFIED METADATA: Main Subject Line (MSL) for content classification.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model (primarily GEO-focused).
        ///     - GEO: Primary subject/topic for AI topic modeling and categorization
        ///     - SEO: Can be used for content clustering and site organization
        ///     Should be a single, clear phrase (e.g., "Cloud Computing Best Practices")
        ///     Used in: Custom classification tags, Schema.org about property
        /// </remarks>
        private string contentMSL;

        /// <summary>
        ///     UNIFIED METADATA: Meta keywords for search engines and AI systems.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Traditional keyword meta tag (lower weight in modern search)
        ///     - GEO: Still valuable for AI systems and specialized search
        ///     CONSOLIDATED: This field unifies all keyword storage. Legacy 'keywords' usage
        ///     should map to this field for backward compatibility.
        ///     Used in: meta keywords tag, Schema.org keywords property
        /// </remarks>
        private string metaKeywords;

        /// <summary>
        ///     UNIFIED METADATA: Meta robots directives (e.g., "index, follow", "noindex").
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Controls search engine crawling and indexing behavior
        ///     - GEO: Can include/exclude content from AI training data and generative results
        ///     Common values: "index, follow", "noindex, follow", "index, nofollow", "noindex, nofollow"
        ///     Extended for GEO: "noai", "noimageai" directives may be added
        ///     Used in: meta robots tag, HTTP headers, robots.txt references
        /// </remarks>
        private string metaRobots;

        /// <summary>
        ///     UNIFIED METADATA: Open Graph data (JSON serialized).
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Rich social media previews on Facebook, LinkedIn, etc.
        ///     - GEO: Provides structured context for AI systems analyzing social shares
        ///     USAGE PATTERN: This field is for CUSTOM OVERRIDES only.
        ///     By default, SeoMetadataManager generates OG tags from other metadata properties.
        ///     Store JSON here only when you need to override the default OG tag generation.
        ///     Example: {"og:video": "https://...", "og:video:type": "video/mp4"}
        /// </remarks>
        private string openGraphData;

        /// <summary>
        ///     UNIFIED METADATA: Breadcrumb label for navigation structure.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Used in Schema.org BreadcrumbList for rich snippets
        ///     - GEO: Helps AI understand page hierarchy and site structure
        ///     Falls back to post title if not set.
        ///     Used in: Schema.org BreadcrumbList structured data
        /// </remarks>
        private string breadcrumbLabel;

        /// <summary>
        ///     UNIFIED METADATA: Schema.org organization name for this post.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Organization attribution in structured data for rich snippets
        ///     - GEO: Helps AI systems understand organizational context and authority
        ///     Can override blog-level organization for posts from guest authors or partners.
        ///     Used in: Schema.org publisher/organization properties
        /// </remarks>
        private string schemaOrganization;

        #endregion

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Post" /> class. 
        ///     The default contstructor assign default values.
        /// </summary>
        public Post()
        {
            this.Id = Guid.NewGuid();
            this.allcomments = new List<Comment>();
            this.categories = new StateList<Category>();
            this.tags = new StateList<string>();
            this.notificationEmails = new StateList<string>();
            this.DateCreated = new DateTime();
            this.isPublished = true;
            this.hasCommentsEnabled = true;
        }

        static Post()
        {
            Blog.Saved += (s, e) =>
            {
                if (e.Action == SaveAction.Delete)
                {
                    Blog blog = s as Blog;
                    if (blog != null)
                    {
                        RefreshPostLists(blog);
                    }
                }
            };
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs before a new comment is added.
        /// </summary>
        public static event EventHandler<CancelEventArgs> AddingComment;

        /// <summary>
        ///     Occurs when a comment is added.
        /// </summary>
        public static event EventHandler<EventArgs> CommentAdded;

        /// <summary>
        ///     Occurs when a comment has been removed.
        /// </summary>
        public static event EventHandler<EventArgs> CommentRemoved;

        /// <summary>
        ///     Occurs when a comment has been purged.
        /// </summary>
        public static event EventHandler<EventArgs> CommentPurged;

        /// <summary>
        ///     Occurs when a comment has been restored.
        /// </summary>
        public static event EventHandler<EventArgs> CommentRestored;

        /// <summary>
        ///     Occurs when a comment is updated.
        /// </summary>
        public static event EventHandler<EventArgs> CommentUpdated;

        /// <summary>
        ///     Occurs when a visitor rates the post.
        /// </summary>
        public static event EventHandler<EventArgs> Rated;

        /// <summary>
        ///     Occurs before comment is removed.
        /// </summary>
        public static event EventHandler<CancelEventArgs> RemovingComment;

        /// <summary>
        ///     Occurs before comment is purged.
        /// </summary>
        public static event EventHandler<CancelEventArgs> PurgingComment;

        /// <summary>
        ///     Occurs before comment is restored.
        /// </summary>
        public static event EventHandler<CancelEventArgs> RestoringComment;

        /// <summary>
        ///     Occurs when the post is being served to the output stream.
        /// </summary>
        public static event EventHandler<ServingEventArgs> Serving;

        /// <summary>
        ///     Occurs when the post is being published.
        /// </summary>
        public static event EventHandler<CancelEventArgs> Publishing;

        /// <summary>
        ///     Occurs when a post is published.
        /// </summary>
        public static event EventHandler<EventArgs> Published;

        /// <summary>
        ///     Occurs before a new comment is updated.
        /// </summary>
        public static event EventHandler<CancelEventArgs> UpdatingComment;

        #endregion

        #region Post Properties

        /// <summary>
        ///     Gets a sorted collection of all undeleted posts in the blog.
        ///     Sorted by date.
        /// </summary>
        public static List<Post> Posts
        {
            get
            {
                Blog blog = Blog.CurrentInstance;
                List<Post> blogPosts;

                if (!posts.TryGetValue(blog.BlogId, out blogPosts))
                {
                    lock (SyncRoot)
                    {
                        if (!posts.TryGetValue(blog.BlogId, out blogPosts))
                        {
                            posts[blog.Id] = blogPosts = BlogService.FillPosts().Where(p => p.IsDeleted == false).ToList();
                            blogPosts.TrimExcess();
                            AddRelations(blogPosts);
                        }
                    }
                }

                return blogPosts;
            }
        }

        /// <summary>
        ///     Gets a sorted collection of all undeleted posts across all blogs.
        ///     Sorted by date.
        /// </summary>
        public static List<Post> AllBlogPosts
        {
            get
            {
                List<Blog> blogs = Blog.Blogs.Where(b => b.IsActive).ToList();
                Guid originalBlogInstanceIdOverride = Blog.InstanceIdOverride;
                List<Post> postsAcrossAllBlogs = new List<Post>();

                // Posts are not loaded for a blog instance until that blog
                // instance is first accessed.  For blog instances where the
                // posts have not yet been loaded, using InstanceIdOverride to
                // temporarily switch the blog CurrentInstance blog so the Posts
                // for that blog instance can be loaded.
                //
                for (int i = 0; i < blogs.Count; i++)
                {
                    List<Post> blogPosts;
                    if (!posts.TryGetValue(blogs[i].Id, out blogPosts))
                    {
                        // temporarily override the Current BlogId to the
                        // blog Id we need posts to be loaded for.
                        Blog.InstanceIdOverride = blogs[i].Id;
                        blogPosts = Posts;
                        Blog.InstanceIdOverride = originalBlogInstanceIdOverride;
                    }

                    postsAcrossAllBlogs.AddRange(blogPosts);
                }

                postsAcrossAllBlogs.Sort();
                
                // do not call AddRelations(). that will change the Next/Previous properties
                // to point to posts in other blogs, which leads to the Next / Previous
                // posts pointing to posts in other blog instances when viewing a single post
                // (in post.aspx).  If Next/Previous is needed for the posts returned
                // here in AllBlogPosts, would be better to create new properties
                // (e.g. AllBlogsNextPost, AllBlogsPreviousPost).
                // AddRelations(postsAcrossAllBlogs);

                return postsAcrossAllBlogs;
            }
        }

        /// <summary>
        ///     Gets a sorted collection of all undeleted posts, taking into account the
        ///     current blog instance's Site Aggregation status in determining if posts
        ///     from just the current instance or all instances should be returned.
        ///     Sorted by date.
        /// </summary>
        /// <remarks>
        ///     This logic could be put into the normal 'Posts' property, however
        ///     there are times when a Site Aggregation blog instance may just need
        ///     its own posts.  So ApplicablePosts can be called when data across
        ///     all blog instances may be needed, and Posts can be called when data
        ///     for just the current blog instance is needed.
        /// </remarks>
        public static List<Post> ApplicablePosts
        {
            get
            {
                if (Blog.CurrentInstance.IsSiteAggregation)
                    return AllBlogPosts;
                else
                    return Posts;
            }
        }

        /// <summary>
        ///     Gets a sorted collection of all deleted posts in the blog.
        ///     Sorted by date.
        /// </summary>
        public static List<Post> DeletedPosts
        {
            get
            {
                Blog blog = Blog.CurrentInstance;
                List<Post> blogPosts;

                if (!deletedposts.TryGetValue(blog.Id, out blogPosts))
                {
                    lock (SyncRoot)
                    {
                        if (!deletedposts.TryGetValue(blog.Id, out blogPosts))
                        {
                            blogPosts = BlogService.FillPosts().Where(p => p.IsDeleted == true).ToList();
                            deletedposts[blog.Id] = blogPosts;
                        }
                    }
                }

                return blogPosts;
            }
        }

        /// <summary>
        ///     Gets or sets the Author or the post.
        /// </summary>
        public string Author
        {
            get
            {
                return this.author;
            }

            set
            {
                base.SetValue("Author", value, ref this.author);
            }
        }

        /// <summary>
        ///     Gets AuthorProfile.
        /// </summary>
        public AuthorProfile AuthorProfile
        {
            get
            {
                return AuthorProfile.GetProfile(this.Author);
            }
        }

        /// <summary>
        ///     Gets an unsorted List of categories.
        /// </summary>
        public StateList<Category> Categories
        {
            get
            {
                return this.categories;
            }
        }

        /// <summary>
        ///     Gets or sets the Content or the post.
        /// </summary>
        public string Content
        {
            get
            {
                return this.content;
            }

            set
            {

                base.SetValue("Content", value, ref this.content);

                // This is commented out only because I can't find any reference to
                // this cache item anywhere in the project. So it seems pretty obscure
                // if it's supposed to be used by plugins or something else.

                //if (base.SetValue("Content", value, ref this.content))
                //{
                //    Blog.CurrentInstance.Cache.Remove("content_" + this.Id);
                //}
            }
        }

        /// <summary>
        ///     Gets or sets the Description of the post.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: This is the primary meta description in the unified SEO/GEO model.
        ///     - SEO: Used in meta description tags for search engine results
        ///     - GEO: Provides context for AI summarization and content understanding
        ///     BACKWARD COMPATIBILITY: Legacy property maintained. This is the authoritative source
        ///     for all description metadata unless SemanticSummary provides an AI-optimized alternative.
        ///     Used by MetadataBuilder and SeoMetadataManager for og:description, twitter:description,
        ///     and Schema.org description properties.
        /// </remarks>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                base.SetValue("Description", value, ref this.description);
            }
        }

        /// <summary>
        ///     Gets if the Post have been changed.
        /// </summary>
        public override bool IsChanged
        {
            get
            {
                if (base.IsChanged)
                {
                    return true;
                }

                if (this.Categories.IsChanged || this.Tags.IsChanged || this.NotificationEmails.IsChanged)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets the next post relative to this one based on time.
        ///     <remarks>
        ///         If this post is the newest, then it returns null.
        ///     </remarks>
        /// </summary>
        public Post Next { get; private set; }

        /// <summary>
        ///     Gets a collection of email addresses that is signed up for 
        ///     comment notification on the specific post.
        /// </summary>
        public StateList<string> NotificationEmails
        {
            get
            {
                return this.notificationEmails;
            }
        }

        /// <summary>
        ///     Gets the absolute permanent link to the post.
        /// </summary>
        public Uri PermaLink
        {
            get
            {
                return new Uri($"{Blog.AbsoluteWebRoot}post.aspx?id={Id}");
                //return new Uri(string.Format("{0}post/{1}", this.Blog.AbsoluteWebRoot, this.Slug));
            }
        }

        /// <summary>
        ///     Gets the previous post relative to this one based on time.
        ///     <remarks>
        ///         If this post is the oldest, then it returns null.
        ///     </remarks>
        /// </summary>
        public Post Previous { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the post is published.
        /// </summary>
        public bool IsPublished
        {
            get
            {
                return this.isPublished;
            }

            set
            {
                base.SetValue("IsPublished", value, ref this.isPublished);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the post is deleted.
        /// </summary>
        public bool IsDeleted
        {
            get
            {
                return this.isDeleted;
            }

            set
            {
                base.SetValue("IsDeleted", value, ref this.isDeleted);
            }
        }

        /// <summary>
        ///     Gets or sets the number of raters or the object.
        /// </summary>
        public int Raters
        {
            get
            {
                return this.raters;
            }

            set
            {
                base.SetValue("Raters", value, ref this.raters);
            }
        }

        /// <summary>
        ///     Gets or sets the rating or the post.
        /// </summary>
        public float Rating
        {
            get
            {
                return this.rating;
            }

            set
            {
                base.SetValue("Rating", value, ref this.rating);
            }
        }

        /// <summary>
        ///     Gets the absolute link to the post.
        /// </summary>
        public Uri AbsoluteLink
        {
            get
            {
                return new Uri(this.Blog.AbsoluteWebRootAuthority + this.RelativeLink);
            }
        }

        /// <summary>
        ///     Gets a relative-to-the-site-root path to the post.
        ///     Only for in-site use.
        /// </summary>
        public string RelativeLink
        {
            get
            {
                // taking into account aggregated posts
                var settings = BlogSettings.GetInstanceSettings(Blog);
                var ext = string.IsNullOrEmpty(BlogConfig.FileExtension) ? ".aspx" : BlogConfig.FileExtension;

                var theslug = Utils.RemoveIllegalCharacters(this.Slug);
                if (!settings.RemoveExtensionsFromUrls)
                    theslug += ext;

                var BlogUrl = "";
                if (this.BlogId != Blog.CurrentInstance.Id)
                {
                    // point it to child blog
                    BlogUrl = this.Blog.Name + "/";
                }

                return settings.TimeStampPostLinks
                    ? string.Format("{0}{1}post/{2}{3}", Blog.RelativeWebRoot, BlogUrl, DateCreated.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture), theslug)
                    : string.Format("{0}{1}post/{2}", Utils.RelativeWebRoot, BlogUrl, theslug);
            }
        }

        /// <summary>
        ///     Returns a relative link if possible if the hostname of this blog instance matches the
        ///     hostname of the site aggregation blog.  If the hostname is different, then the
        ///     absolute link is returned.
        /// </summary>
        public string RelativeOrAbsoluteLink
        {
            get
            {
                return Blog.DoesHostnameDifferFromSiteAggregationBlog ? AbsoluteLink.ToString() : RelativeLink;
            }
        }

        /// <summary>
        ///     Gets or sets the Slug of the Post.
        ///     A Slug is the relative URL used by the posts.
        /// </summary>
        public string Slug
        {
            get
            {
                return string.IsNullOrEmpty(this.slug) ? GetUniqueSlug(this.title, this.Id) : this.slug;
            }

            set
            {
                base.SetValue("Slug", value, ref this.slug);
            }
        }

        /// <summary>
        ///     Gets an unsorted collection of tags.
        /// </summary>
        public StateList<string> Tags
        {
            get
            {
                return this.tags;
            }
        }

        /// <summary>
        ///     Gets or sets the Title or the post.
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                base.SetValue("Title", value, ref this.title);
            }
        }

        /// <summary>
        ///     Gets the trackback link to the post.
        /// </summary>
        public Uri TrackbackLink
        {
            get
            {
                return new Uri($"{Blog.AbsoluteWebRoot}trackback.axd?id={Id}");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not the post is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (this.IsDeleted)
                    return false;
                else if (this.IsPublished && this.DateCreated <= BlogSettings.Instance.FromUtc())
                    return true;
                else if (Security.IsAuthorizedTo(Rights.ViewUnpublishedPosts))
                    return true;

                return false;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether a post is available to visitors not logged into the blog.
        /// </summary>
        public bool IsVisibleToPublic
        {
            get
            {
                return (this.IsPublished && this.IsDeleted == false &&
                    this.DateCreated <= BlogSettings.Instance.FromUtc());
            }
        }

        /// <summary>
        /// URL of the first image in the post, if any.
        /// If there's no first image, returns an empty string.
        /// </summary>
        public string FirstImgSrc
        {
            get
            {
                string srcValue = null;
                if (!string.IsNullOrEmpty(content))
                {
                    Match match = Regex.Match(content, @"<img\s+?.*?src=('|"")(.*?)\1.*?>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        srcValue = match.Groups[2].Value;
                    }
                }
                return srcValue ?? string.Empty;
            }
        }

        #endregion

        #region SEO & GEO (Generative Engine Optimization) Properties

        /// <summary>
        ///     Gets or sets the canonical URL for this post.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     The canonical URL is used to prevent duplicate content penalties in search engines
        ///     and to establish the primary URL identity for this specific post when duplicated elsewhere.
        ///     Also helps AI systems identify the authoritative source.
        /// </remarks>
        public string CanonicalUrl
        {
            get { return this.canonicalUrl; }
            set { base.SetValue("CanonicalUrl", value, ref this.canonicalUrl); }
        }

        /// <summary>
        ///     Gets or sets the Schema.org type for structured data markup.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     Specifies the schema type for this post (e.g., "BlogPosting", "Article", "NewsArticle").
        ///     Used in structured data to help search engines and AI systems understand the content type.
        ///     Defaults to "BlogPosting" if not specified.
        /// </remarks>
        public string SchemaType
        {
            get { return this.schemaType; }
            set { base.SetValue("SchemaType", value, ref this.schemaType); }
        }

        /// <summary>
        ///     Gets or sets comma-separated key entities mentioned in the post.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model (GEO-focused).
        ///     A comma-separated list of named entities (people, organizations, locations, concepts)
        ///     mentioned in the post content. Used by AI systems for entity extraction and semantic understanding.
        ///     Example: "Machine Learning, Neural Networks, TensorFlow"
        /// </remarks>
        public string KeyEntities
        {
            get { return this.keyEntities; }
            set { base.SetValue("KeyEntities", value, ref this.keyEntities); }
        }

        /// <summary>
        ///     Gets or sets a semantic summary optimized for AI systems.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model (GEO-focused).
        ///     A concise, semantically rich summary that emphasizes key concepts and relationships
        ///     for better understanding by generative AI systems. Unlike traditional meta descriptions,
        ///     this should highlight semantic meaning and entity relationships.
        /// </remarks>
        public string SemanticSummary
        {
            get { return this.semanticSummary; }
            set { base.SetValue("SemanticSummary", value, ref this.semanticSummary); }
        }

        /// <summary>
        ///     Gets or sets the Main Subject Line (MSL) for content classification.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model (GEO-focused).
        ///     The primary subject or main topic of the post content, used for topic modeling
        ///     and content categorization by AI systems. Should be a single, clear phrase representing
        ///     the post's main subject (e.g., "Cloud Computing Best Practices").
        /// </remarks>
        public string ContentMSL
        {
            get { return this.contentMSL; }
            set { base.SetValue("ContentMSL", value, ref this.contentMSL); }
        }

        /// <summary>
        ///     Gets or sets comma-separated meta keywords for search engines and AI systems.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     Keywords that describe the post content for search engines and AI-powered search systems.
        ///     Use comma-separated values. Modern search engines may give less weight to this field,
        ///     but it remains useful for AI systems and specialized search applications.
        ///     
        ///     CONSOLIDATED FIELD: This is the unified storage for all keyword metadata in the Post model.
        ///     
        ///     BACKWARD COMPATIBILITY: This property provides explicit keyword metadata control.
        ///     Falls back to Tags collection if not explicitly set. MetadataBuilder automatically
        ///     uses this field when set, or falls back to Tags when empty.
        ///     
        ///     Usage: Set this when you need fine-grained control over SEO keywords that differs
        ///     from your taxonomy Tags. Leave empty to use Tags as the default keyword source.
        /// </remarks>
        public string MetaKeywords
        {
            get 
            { 
                // If MetaKeywords not explicitly set, fall back to Tags for backward compatibility
                if (string.IsNullOrEmpty(this.metaKeywords) && this.Tags != null && this.Tags.Count > 0)
                {
                    return string.Join(", ", this.Tags);
                }
                return this.metaKeywords; 
            }
            set { base.SetValue("MetaKeywords", value, ref this.metaKeywords); }
        }

        /// <summary>
        ///     Gets or sets meta robots directives for search engine crawling behavior.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     Controls how search engine crawlers and AI indexers should treat this post.
        ///     Common values: "index, follow", "noindex, follow", "index, nofollow", "noindex, nofollow".
        ///     Extended GEO directives: "noai", "noimageai" to opt-out of AI training/generation.
        ///     Used to include/exclude content from search results and AI training data.
        /// </remarks>
        public string MetaRobots
        {
            get { return this.metaRobots; }
            set { base.SetValue("MetaRobots", value, ref this.metaRobots); }
        }

        /// <summary>
        ///     Gets or sets Open Graph metadata (JSON serialized).
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     JSON-formatted Open Graph metadata for social media sharing and rich previews.
        ///     Includes properties like og:title, og:description, og:image, og:type, etc.
        ///     Improves how posts appear when shared on social media and messaging platforms.
        ///     USAGE PATTERN: This field is for CUSTOM OVERRIDES only. By default, SeoMetadataManager
        ///     generates OG tags from other metadata properties. Use this only when you need to override
        ///     default OG tag generation with custom values.
        /// </remarks>
        public string OpenGraphData
        {
            get { return this.openGraphData; }
            set { base.SetValue("OpenGraphData", value, ref this.openGraphData); }
        }

        /// <summary>
        ///     Gets or sets the breadcrumb label for navigation structure.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     A label for this post in breadcrumb navigation and structural hierarchy.
        ///     Used in Schema.org BreadcrumbList markup to establish navigation paths that
        ///     help AI systems understand page hierarchy and relationships.
        /// </remarks>
        public string BreadcrumbLabel
        {
            get { return this.breadcrumbLabel; }
            set { base.SetValue("BreadcrumbLabel", value, ref this.breadcrumbLabel); }
        }

        /// <summary>
        ///     Gets or sets the Schema.org organization name for this post.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     The organization responsible for or associated with this post content in Schema.org markup.
        ///     Can override the blog-level organization if the post is associated with a specific organization.
        ///     Used in structured data for proper attribution and organizational identification by AI systems.
        /// </remarks>
        public string SchemaOrganization
        {
            get { return this.schemaOrganization; }
            set { base.SetValue("SchemaOrganization", value, ref this.schemaOrganization); }
        }

        #endregion

        #region Comment Properties

        /// <summary>
        ///     Gets a Collection of All Comments for the post
        /// </summary>
        public List<Comment> Comments
        {
            get
            {
                return this.AllComments.FindAll(c => !c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a Collection of All Comments for the post
        /// </summary>
        public List<Comment> AllComments
        {
            get
            {
                return this.allcomments;
            }
        }

        /// <summary>
        ///     Gets a collection of Approved comments for the post sorted by date.
        ///     When moderation is enabled, unapproved comments go to pending.
        ///     Whith moderation off, they shown as approved.
        /// </summary>
        public List<Comment> ApprovedComments
        {
            get
            {
                if (BlogSettings.Instance.EnableCommentsModeration)
                {
                    return this.Comments.FindAll(c => c.IsApproved && !c.IsSpam && !c.IsPingbackOrTrackback);
                }
                else
                {
                    return this.Comments.FindAll(c => !c.IsSpam && !c.IsPingbackOrTrackback);
                }
            }
        }

        /// <summary>
        ///     Gets a collection of comments waiting for approval for the post, sorted by date
        ///     excluding comments rejected as spam
        /// </summary>
        public List<Comment> NotApprovedComments
        {
            get
            {
                return this.Comments.FindAll(c => !c.IsApproved && !c.IsSpam && !c.IsPingbackOrTrackback);
            }
        }

        /// <summary>
        ///     Gets a collection of pingbacks and trackbacks for the post, sorted by date
        /// </summary>
        public List<Comment> Pingbacks
        {
            get
            {
                return this.Comments.FindAll(c => c.IsApproved && !c.IsSpam && c.IsPingbackOrTrackback);
            }
        }

        /// <summary>
        ///     Gets a collection of comments marked as spam for the post, sorted by date.
        /// </summary>
        public List<Comment> SpamComments
        {
            get
            {
                return this.Comments.FindAll(c => c.IsSpam && !c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a collection of comments marked as deleted for the post, sorted by date.
        /// </summary>
        public List<Comment> DeletedComments
        {
            get
            {
                return this.allcomments.FindAll(c => c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a collection of the comments that are nested as replies
        /// </summary>
        public List<Comment> NestedComments
        {
            get
            {
                if (this.nestedComments == null)
                {
                    this.CreateNestedComments();
                }

                return this.nestedComments;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has comments enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has comments enabled; otherwise, <c>false</c>.
        /// </value>
        public bool HasCommentsEnabled
        {
            get
            {
                return this.hasCommentsEnabled;
            }

            set
            {
                base.SetValue("HasCommentsEnabled", value, ref this.hasCommentsEnabled);

            }
        }

        #endregion

        #region Post Public Methods

        /// <summary>
        /// Gets whether the current user can publish this post.
        /// </summary>
        /// <param name="author">The author of the post without needing to assign it to the Author property.</param>
        /// <returns></returns>
        public bool CanPublish(string author)
        {
            bool isOwnPost = Security.CurrentUser.Identity.Name.Equals(author, StringComparison.OrdinalIgnoreCase);

            if (isOwnPost && !Security.IsAuthorizedTo(Rights.PublishOwnPosts))
            {
                return false;
            }
            else if (!isOwnPost && !Security.IsAuthorizedTo(Rights.PublishOtherUsersPosts))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets whether or not the current user owns this post.
        /// </summary>
        /// <returns></returns>
        public override bool CurrentUserOwns
        {
            get
            {
                return Security.CurrentUser.Identity.Name.Equals(this.Author, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets whether the current user can delete this post.
        /// </summary>
        /// <returns></returns>
        public override bool CanUserDelete
        {
            get
            {
                if (CurrentUserOwns && Security.IsAuthorizedTo(Rights.DeleteOwnPosts))
                    return true;
                else if (!CurrentUserOwns && Security.IsAuthorizedTo(Rights.DeleteOtherUsersPosts))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Gets whether the current user can edit this post.
        /// </summary>
        /// <returns></returns>
        public override bool CanUserEdit
        {
            get
            {
                if (CurrentUserOwns && Security.IsAuthorizedTo(Rights.EditOwnPosts))
                    return true;
                else if (!CurrentUserOwns && Security.IsAuthorizedTo(Rights.EditOtherUsersPosts))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Returns a post based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The post id.
        /// </param>
        /// <returns>
        /// The selected post.
        /// </returns>
        public static Post GetPost(Guid id)
        {
            return Posts.Find(p => p.Id == id);
        }

        /// <summary>
        /// Returns all posts written by the specified author.
        /// </summary>
        /// <param name="author">
        /// The author.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByAuthor(string author)
        {
            var legalAuthor = Utils.RemoveIllegalCharacters(author);
            var list = ApplicablePosts.FindAll(
                p =>
                {
                    var legalTitle = Utils.RemoveIllegalCharacters(p.Author);
                    return legalAuthor.Equals(legalTitle, StringComparison.OrdinalIgnoreCase);
                });

            return list;
        }

        /// <summary>
        /// Get blog by author
        /// </summary>
        /// <param name="author">Post author</param>
        /// <returns>Blog if author wrote any posts there</returns>
        public static Blog GetBlogByAuthor(string author)
        {
            var legalAuthor = Utils.RemoveIllegalCharacters(author);
            var post = ApplicablePosts.FirstOrDefault(
                p =>
                {
                    var legalTitle = Utils.RemoveIllegalCharacters(p.Author);
                    return legalAuthor.Equals(legalTitle, StringComparison.OrdinalIgnoreCase);
                });

            return post == null ? null : post.Blog;
        }

        /// <summary>
        /// Returns all posts in the specified category
        /// </summary>
        /// <param name="categoryId">
        /// The category Id.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByCategory(Guid categoryId)
        {
            var cat = Category.GetCategory(categoryId, Blog.CurrentInstance.IsSiteAggregation);
            return GetPostsByCategory(cat);
        }

        /// <summary>
        /// Returns all posts in the specified category
        /// </summary>
        /// <param name="cat">Category objuect</param>
        /// <returns>A list of posts</returns>
        public static List<Post> GetPostsByCategory(Category cat)
        {
            if (cat == null)
            {
                return null;
            }
            var col = new List<Post>();
            foreach (var p in Post.ApplicablePosts)
            {
                foreach (var c in p.Categories)
                {
                    if (Blog.CurrentInstance.IsSiteAggregation)
                    {
                        if (c.Title == cat.Title) col.Add(p);
                    }
                    else
                    {
                        if (c.Title == cat.Title && c.Id == cat.Id) col.Add(p);
                    }
                }
            }
            //var col = Post.ApplicablePosts.Where(p => p.Categories.Contains(cat)).ToList();
            col.Sort();
            return col;
        }

        /// <summary>
        /// Returns all posts published between the two dates.
        /// </summary>
        /// <param name="dateFrom">
        /// The date From.
        /// </param>
        /// <param name="dateTo">
        /// The date To.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByDate(DateTime dateFrom, DateTime dateTo)
        {
            var list = ApplicablePosts.FindAll(p => p.DateCreated.Date >= dateFrom && p.DateCreated.Date <= dateTo);
            return list;
        }

        /// <summary>
        /// Returns all posts tagged with the specified tag.
        /// </summary>
        /// <param name="tag">
        /// The tag of the post.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByTag(string tag)
        {
            tag = Utils.RemoveIllegalCharacters(tag);

            var list =
                ApplicablePosts.FindAll(
                    p =>
                    p.Tags.Any(t => Utils.RemoveIllegalCharacters(t).Equals(tag, StringComparison.OrdinalIgnoreCase)));

            return list;
        }

        /// <summary>
        /// Checks to see if the specified title has already been used
        ///     by another post.
        ///     <remarks>
        /// Titles must be unique because the title is part of the URL.
        ///     </remarks>
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <returns>
        /// The is title unique.
        /// </returns>
        public static bool IsTitleUnique(string title)
        {
            var legal = Utils.RemoveIllegalCharacters(title);
            return
                Posts.All(
                    post => !Utils.RemoveIllegalCharacters(post.Title).Equals(legal, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Called when [serving].
        /// </summary>
        /// <param name="post">
        /// The post being served.
        /// </param>
        /// <param name="arg">
        /// The <see cref="BlogEngine.Core.ServingEventArgs"/> instance containing the event data.
        /// </param>
        public static void OnServing(Post post, ServingEventArgs arg)
        {
            if (Serving != null)
            {
                Serving(post, arg);
            }
        }

        /// <summary>
        /// Force reload of all posts
        /// </summary>
        public static void Reload()
        {
            RefreshPostLists(Blog.CurrentInstance);
        }

        /// <summary>
        /// Imports Post (without all standard saving routines
        /// </summary>
        public void Import()
        {
            if (this.Deleted)
            {
                if (!this.New)
                {
                    BlogService.DeletePost(this);
                }
            }
            else
            {
                if (this.New)
                {
                    BlogService.InsertPost(this);
                }
                else
                {
                    BlogService.UpdatePost(this);
                }
            }
        }

        /// <summary>
        /// Marks the object as being an clean,
        ///     which means not dirty.
        /// </summary>
        public override void MarkOld()
        {
            this.Categories.MarkOld();
            this.Tags.MarkOld();
            this.NotificationEmails.MarkOld();
            base.MarkOld();
        }

        /// <summary>
        /// Adds a rating to the post.
        /// </summary>
        /// <param name="newRating">
        /// The rating.
        /// </param>
        public void Rate(int newRating)
        {
            if (this.Raters > 0)
            {
                var total = this.Raters * this.Rating;
                total += newRating;
                this.Raters++;
                this.Rating = total / this.Raters;
            }
            else
            {
                this.Raters = 1;
                this.Rating = newRating;
            }

            this.DataUpdate();
            this.OnRated(this);
        }

        /// <summary>
        /// Check if slug is unique and if not generate it
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <param name="postId">Post id</param>
        /// <returns>Slug that is unique across blogs</returns>
        public static string GetUniqueSlug(string slug, Guid postId)
        {
            string s = Utils.RemoveIllegalCharacters(slug.Trim());

            // will do for up to 100 unique post titles
            for (int i = 1; i < 101; i++)
            {
                if (IsUniqueSlug(s, postId))
                    break;

                s = $"{slug}{i}";
            }
            return s;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion

        #region Comment Public Methods

        /// <summary>
        /// Adds a comment to the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to add to the post.
        /// </param>
        public void AddComment(Comment comment)
        {
            var e = new CancelEventArgs();
            this.OnAddingComment(comment, e);
            if (e.Cancel)
            {
                return;
            }

            this.AllComments.Add(comment);
            this.DataUpdate();
            this.OnCommentAdded(comment);

            if (comment.IsApproved)
            {
                this.SendNotifications(comment);
            }
        }

        /// <summary>
        /// Approves all the comments in a post.  Included to save time on the approval process.
        /// </summary>
        public void ApproveAllComments()
        {
            foreach (var comment in this.Comments)
            {
                this.ApproveComment(comment);
            }
        }

        /// <summary>
        /// Approves a Comment for publication.
        /// </summary>
        /// <param name="comment">
        /// The Comment to approve
        /// </param>
        public void ApproveComment(Comment comment)
        {
            var e = new CancelEventArgs();
            Comment.OnApproving(comment, e);
            if (e.Cancel)
            {
                return;
            }

            var inx = this.Comments.IndexOf(comment);
            this.Comments[inx].IsApproved = true;
            this.Comments[inx].IsSpam = false;
            this.DateModified = comment.DateCreated;
            this.DataUpdate();
            Comment.OnApproved(comment);
            this.SendNotifications(comment);
        }

        /// <summary>
        /// Disapproves a Comment as Spam.
        /// </summary>
        /// <param name="comment">
        /// The Comment to approve
        /// </param>
        public void DisapproveComment(Comment comment)
        {
            var e = new CancelEventArgs();
            Comment.OnDisapproving(comment, e);
            if (e.Cancel)
            {
                return;
            }

            var inx = this.Comments.IndexOf(comment);
            this.Comments[inx].IsApproved = false;
            this.Comments[inx].IsSpam = true;
            this.DateModified = comment.DateCreated;
            this.DataUpdate();
            Comment.OnDisapproved(comment);
            this.SendNotifications(comment);
        }

        /// <summary>
        /// Imports a comment to comment collection.  Does not
        ///     notify user or run extension events.
        /// </summary>
        /// <param name="comment">
        /// The comment to add to the post.
        /// </param>
        public void ImportComment(Comment comment)
        {
            this.AllComments.Add(comment);
        }

        /// <summary>
        /// Updates a comment in the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to update in the post.
        /// </param>
        public void UpdateComment(Comment comment)
        {
            var e = new CancelEventArgs();
            this.OnUpdatingComment(comment, e);
            if (e.Cancel)
            {
                return;
            }

            var inx = this.Comments.IndexOf(comment);

            this.Comments[inx].IsApproved = comment.IsApproved;
            this.Comments[inx].Content = comment.Content;
            this.Comments[inx].Author = comment.Author;
            this.Comments[inx].Country = comment.Country;
            this.Comments[inx].Email = comment.Email;
            this.Comments[inx].IP = comment.IP;
            this.Comments[inx].Website = comment.Website;
            this.Comments[inx].ModeratedBy = comment.ModeratedBy;
            this.Comments[inx].IsSpam = comment.IsSpam;
            this.Comments[inx].IsDeleted = comment.IsDeleted;

            // need to mark post as "dirty"
            this.DateModified = DateTime.Now;
            this.DataUpdate();
            this.OnCommentUpdated(comment);
        }

        /// <summary>
        /// Removes a comment from the collection.
        /// Will update the post if <paramref name="updatePost"/> is set to true.
        /// If <paramref name="updatePost"/> is set to false, you have to call <see cref="DataUpdate"/> manually!
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="updatePost"></param>
        public void RemoveComment(Comment comment, bool updatePost)
        {
            var e = new CancelEventArgs();
            this.OnRemovingComment(comment, e);
            if (e.Cancel)
            {
                return;
            }

            var comm = Comments.FirstOrDefault(c => c.Id.Equals(comment.Id));
            if (comm == null)
            {
                return;
            }

            comm.IsDeleted = true;

            if (updatePost)
            {
                this.DataUpdate();
            }

            this.OnCommentRemoved(comment);  
        }

        /// <summary>
        /// Removes a comment from the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to remove from the post.
        /// </param>
        public void RemoveComment(Comment comment)
        {
            RemoveComment(comment, true);
        }

        /// <summary>
        /// Removes comment from the post
        /// </summary>
        /// <param name="comment">Comment</param>
        public void PurgeComment(Comment comment)
        {
            var e = new CancelEventArgs();
            this.OnPurgingComment(comment, e);
            if (e.Cancel)
            {
                return;
            }

            this.allcomments.Remove(comment);
            this.DataUpdate();
            this.OnCommentPurged(comment);
        }

        /// <summary>
        /// Restores comment from recycle bin
        /// </summary>
        /// <param name="comment">Comment</param>
        public void RestoreComment(Comment comment)
        {
            var e = new CancelEventArgs();
            this.OnRestoringComment(comment, e);
            if (e.Cancel)
            {
                return;
            }

            var comm = allcomments.FirstOrDefault(c => c.Id.Equals(comment.Id));
            if (comm == null)
            {
                return;
            }

            comm.IsDeleted = false;
            this.DataUpdate();
            this.OnCommentRestored(comment);
        }

        #endregion

        #region Implemented Interfaces

        #region IComparable<Post>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the 
        ///     objects being compared. The return value has the following meanings: 
        ///     Value Meaning Less than zero This object is less than the other parameter.Zero 
        ///     This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Post other)
        {
            return other.DateCreated.CompareTo(this.DateCreated);
        }

        #endregion

        #region IPublishable

        /// <summary>
        /// Raises the Serving event
        /// </summary>
        /// <param name="eventArgs">
        /// The event Args.
        /// </param>
        public void OnServing(ServingEventArgs eventArgs)
        {
            if (Serving != null)
            {
                Serving(this, eventArgs);
            }
        }

        /// <summary>
        /// When post is publishing
        /// </summary>
        /// <param name="e">Event arguments</param>
        public void OnPublishing(CancelEventArgs e)
        {
            if (Publishing != null)
            {
                Publishing(this, e);
            }
        }

        /// <summary>
        /// On post published
        /// </summary>
        protected virtual void OnPublished()
        {
            if (Published != null)
            {
                Published(this, EventArgs.Empty);
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the Post from the current BlogProvider.
        /// </summary>
        protected override void DataDelete()
        {
            this.IsDeleted = true;
            DataUpdate();

            lock (SyncRoot)
            {
                Guid blogId = Blog.CurrentInstance.Id;

                if (posts.ContainsKey(blogId))
                    posts.Remove(blogId);

                if (deletedposts.ContainsKey(blogId))
                    deletedposts.Remove(blogId);
            }
        }

        /// <summary>
        /// Deletes the Post from the current BlogProvider.
        /// </summary>
        public void Purge()
        {
            BlogService.DeletePost(this);
            if (!Posts.Contains(this))
            {
                RefreshPostLists(Blog.CurrentInstance);
                return;
            }

            Posts.Remove(this);
            this.Dispose();
            AddRelations(Posts);

            RefreshPostLists(Blog.CurrentInstance);
        }

        private static void RefreshPostLists(Blog blog)
        {
            lock (SyncRoot)
            {
                if (posts.ContainsKey(blog.BlogId))
                    posts.Remove(blog.BlogId);

                if (deletedposts.ContainsKey(blog.BlogId))
                    deletedposts.Remove(blog.BlogId);
            }
        }

        /// <summary>
        /// Restores the deleted posts.
        /// </summary>
        public void Restore()
        {
            this.IsDeleted = false;
            DataUpdate();

            RefreshPostLists(Blog.CurrentInstance);
        }

        /// <summary>
        /// Inserts a new post to the current BlogProvider.
        /// </summary>
        protected override void DataInsert()
        {
            if (this.isPublished)
            {
                var e = new CancelEventArgs();
                this.OnPublishing(e);
                if (e.Cancel) 
                {
                    this.isPublished = false;
                }
            }

            BlogService.InsertPost(this);

            if (!this.New)
            {
                return;
            }

            Posts.Add(this);
            Posts.Sort();
            AddRelations(Posts);

            if (this.isPublished)
            {
                this.OnPublished();
            }
        }

        /// <summary>
        /// Returns a Post based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The post id.
        /// </param>
        /// <returns>
        /// The selected Post.
        /// </returns>
        protected override Post DataSelect(Guid id)
        {
            return BlogService.SelectPost(id);
        }

        /// <summary>
        /// Updates the Post.
        /// </summary>
        protected override void DataUpdate()
        {
            if (AboutToPublishPost())
            {
                var e = new CancelEventArgs();
                OnPublishing(e);
                if (e.Cancel)
                {
                    isPublished = false;
                }
            }

            // trigger even only when post goes from unpublished to published
            var updateAndPublish = false;
            try
            {
                var isOldPublished = BlogService.SelectPost(Id).IsPublished;
                if(isPublished && !isOldPublished && !isDeleted)
                {
                    updateAndPublish = true;
                }
            }
            catch (Exception) { }

            BlogService.UpdatePost(this);
            Posts.Sort();
            AddRelations(Posts);
            ResetNestedComments();

            if (updateAndPublish)
            {
                OnPublished();
            }
        }

        bool AboutToPublishPost()
        {
            if (isPublished && !IsDeleted)
            {
                var p = DataSelect(Id);
                if (p != null && !p.isPublished)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when [adding comment].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnAddingComment(Comment comment, CancelEventArgs e)
        {
            if (AddingComment != null)
            {
                AddingComment(comment, e);
            }
        }

        /// <summary>
        /// Called when [comment added].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        protected virtual void OnCommentAdded(Comment comment)
        {
            if (CommentAdded != null)
            {
                CommentAdded(comment, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [comment removed].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        protected virtual void OnCommentRemoved(Comment comment)
        {
            if (CommentRemoved != null)
            {
                CommentRemoved(comment, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [comment purged].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        protected virtual void OnCommentPurged(Comment comment)
        {
            if (CommentPurged != null)
            {
                CommentPurged(comment, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [comment restored].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        protected virtual void OnCommentRestored(Comment comment)
        {
            if (CommentRestored != null)
            {
                CommentRestored(comment, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [comment updated].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        protected virtual void OnCommentUpdated(Comment comment)
        {
            if (CommentUpdated != null)
            {
                CommentUpdated(comment, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [rated].
        /// </summary>
        /// <param name="post">
        /// The rated post.
        /// </param>
        protected virtual void OnRated(Post post)
        {
            if (Rated != null)
            {
                Rated(post, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [removing comment].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnRemovingComment(Comment comment, CancelEventArgs e)
        {
            if (RemovingComment != null)
            {
                RemovingComment(comment, e);
            }
        }

        /// <summary>
        /// Called when [purging comment].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnPurgingComment(Comment comment, CancelEventArgs e)
        {
            if (PurgingComment != null)
            {
                PurgingComment(comment, e);
            }
        }

        /// <summary>
        /// Called when [restoring comment].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnRestoringComment(Comment comment, CancelEventArgs e)
        {
            if (RestoringComment != null)
            {
                RestoringComment(comment, e);
            }
        }

        /// <summary>
        /// Called when [updating comment].
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnUpdatingComment(Comment comment, CancelEventArgs e)
        {
            if (UpdatingComment != null)
            {
                UpdatingComment(comment, e);
            }
        }

        /// <summary>
        /// Validates the Post instance.
        /// </summary>
        protected override void ValidationRules()
        {
            this.AddRule("Title", "Title must be set", String.IsNullOrEmpty(this.Title));
            this.AddRule("Content", "Content must be set", String.IsNullOrEmpty(this.Content));
        }

        /// <summary>
        /// Sets the Previous and Next properties to all posts.
        /// </summary>
        private static void AddRelations(List<Post> posts)
        {
            for (var i = 0; i < posts.Count; i++)
            {
                posts[i].Next = null;
                posts[i].Previous = null;
                if (i > 0)
                {
                    posts[i].Next = posts[i - 1];
                }

                if (i < posts.Count - 1)
                {
                    posts[i].Previous = posts[i + 1];
                }
            }
        }

        /// <summary>
        /// Nests comments based on Id and ParentId
        /// </summary>
        private void CreateNestedComments()
        {
            // instantiate object
            this.nestedComments = new List<Comment>();

            // temporary ID/Comment table
            var commentTable = new Hashtable();

            foreach (var comment in this.Comments)
            {
                // add to hashtable for lookup
                commentTable.Add(comment.Id, comment);

                // check if this is a child comment
                if (comment.ParentId == Guid.Empty)
                {
                    // root comment, so add it to the list
                    this.nestedComments.Add(comment);
                }
                else
                {
                    // child comment, so find parent
                    var parentComment = commentTable[comment.ParentId] as Comment;
                    if (parentComment != null)
                    {
                        // double check that this sub comment has not already been added
                        if (parentComment.Comments.IndexOf(comment) == -1)
                        {
                            parentComment.Comments.Add(comment);
                        }
                    }
                    else
                    {
                        // just add to the base to prevent an error
                        this.nestedComments.Add(comment);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all nesting of comments
        /// </summary>
        private void ResetNestedComments()
        {
            // void the List<>
            this.nestedComments = null;

            // go through all comments and remove sub comments
            foreach (var c in this.Comments)
            {
                c.Comments.Clear();
            }
        }

        /// <summary>
        /// Sends a notification to all visitors  that has registered
        ///     to retrieve notifications for the specific post.
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        private void SendNotifications(Comment comment)
        {
            if (this.NotificationEmails.Count == 0 || comment.IsApproved == false)
            {
                return;
            }

            foreach (var email in this.NotificationEmails)
            {
                if (email == comment.Email)
                {
                    continue;
                }

                // Intentionally using AbsoluteLink instead of PermaLink so the "unsubscribe-email" QS parameter
                // isn't dropped when post.aspx.cs does a 301 redirect to the RelativeLink, before the unsubscription
                // process takes place.
                var unsubscribeLink = this.AbsoluteLink.ToString();
                unsubscribeLink += string.Format(
                    "{0}unsubscribe-email={1}",
                    unsubscribeLink.Contains("?") ? "&" : "?",
                    HttpUtility.UrlEncode(email));

                var defaultCulture = Utils.GetDefaultCulture();

                var sb = new StringBuilder();
                sb.AppendFormat(
                    "<div style=\"font: 11px verdana, arial\">New Comment added by {0}<br /><br />", comment.Author);
                sb.AppendFormat("{0}<br /><br />", comment.Content.Replace(Environment.NewLine, "<br />"));
                sb.AppendFormat(
                    "<strong>{0}</strong>: <a href=\"{1}#id_{2}\">{3}</a><br/>",
                    Utils.Translate("post", null, defaultCulture),
                    this.PermaLink,
                    comment.Id,
                    this.Title);
                sb.Append("<br />_______________________________________________________________________________<br />");
                sb.AppendFormat(
                    "<a href=\"{0}\">{1}</a></div>", unsubscribeLink, Utils.Translate("commentNotificationUnsubscribe"));

                var mail = new MailMessage
                {
                    From = new MailAddress(BlogSettings.Instance.Email, BlogSettings.Instance.Name),
                    Subject = $"New comment on {Title}",
                    Body = sb.ToString()
                };

                mail.To.Add(email);
                Utils.SendMailMessageAsync(mail);
            }
        }

        /// <summary>
        /// Check if post slug is unique accross all blogs
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <param name="postId">Post id</param>
        /// <returns>True if unique</returns>
        private static bool IsUniqueSlug(string slug, Guid postId)
        {
            return Post.ApplicablePosts.Where(p => p.slug != null && p.slug.ToLower() == slug.ToLower() 
                && p.Id != postId).FirstOrDefault() == null ? true : false;
        }

        #endregion

        #region Custom Fields

        /// <summary>
        /// Custom fields
        /// </summary>
        public Dictionary<String, CustomField> CustomFields
        {
            get
            {
                var postFields = BlogService.Provider.FillCustomFields().Where(f =>
                    f.CustomType == "POST" &&
                    f.ObjectId == this.Id.ToString()).ToList();

                if (postFields == null || postFields.Count < 1)
                    return null;

                var fields = new Dictionary<String, CustomField>();

                foreach (var item in postFields)
                {
                    fields.Add(item.Key, item);
                }
                return fields;
            }
        }

        #endregion
    }
}