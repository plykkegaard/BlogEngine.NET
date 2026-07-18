namespace BlogEngine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BlogEngine.Core.Providers;
    using Data.Models;
    /// <summary>
    /// A page is much like a post, but is not part of the
    ///     blog chronology and is more static in nature.
    ///     <remarks>
    /// Pages can be used for "About" pages or other static
    ///         information.
    ///     </remarks>
    /// </summary>
    public sealed class Page : BusinessBase<Page, Guid>, IPublishable
    {
        #region Constants and Fields

        /// <summary>
        /// The _ sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The pages that not deleted.
        /// </summary>
        private static Dictionary<Guid, List<Page>> pages;

        /// <summary>
        /// The deleted pages.
        /// </summary>
        private static Dictionary<Guid, List<Page>> deletedpages;

        /// <summary>
        /// The _ content.
        /// </summary>
        private string content;

        /// <summary>
        /// The description - UNIFIED METADATA: This serves as the primary meta description.
        /// Part of the unified SEO/GEO metadata model. Used for meta description tags,
        /// Open Graph og:description, Twitter Card description, and Schema.org description.
        /// </summary>
        /// <remarks>
        /// BACKWARD COMPATIBILITY: Legacy property maintained for existing code.
        /// In the unified model, this is the authoritative source for all description metadata.
        /// SemanticSummary provides an alternative AI-optimized description when set.
        /// </remarks>
        private string description;

        /// <summary>
        /// The keywords - UNIFIED METADATA: This serves as the primary keywords storage.
        /// Part of the unified SEO/GEO metadata model. Used for meta keywords tags and
        /// Schema.org keywords property.
        /// </summary>
        /// <remarks>
        /// BACKWARD COMPATIBILITY: Legacy property maintained for existing code.
        /// In the unified model, this is the authoritative source for all keyword metadata.
        /// </remarks>
        private string keywords;

        /// <summary>
        /// The _ parent.
        /// </summary>
        private Guid parent;

        /// <summary>
        /// The _ show in list.
        /// </summary>
        private bool showInList;

        /// <summary>
        /// The _ slug.
        /// </summary>
        private string slug;

        /// <summary>
        /// The _ title.
        /// </summary>
        private string title;

        /// <summary>
        /// The front page.
        /// </summary>
        private bool frontPage;

        /// <summary>
        /// The published.
        /// </summary>
        private bool isPublished;

        /// <summary>
        /// The deleted.
        /// </summary>
        private bool isDeleted;

        /// <summary>
        /// The sort order
        /// </summary>
        private int sortOrder;

        #region Unified SEO/GEO Metadata Fields

        /// <summary>
        ///     UNIFIED METADATA: Canonical URL for the page.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Prevents duplicate content penalties, establishes primary URL identity
        ///     - GEO: Helps AI systems identify the authoritative source for this content
        ///     Used in: canonical link tag, og:url, Schema.org url property
        /// </remarks>
        private string canonicalUrl;

        /// <summary>
        ///     UNIFIED METADATA: Schema.org type (e.g., "WebPage", "AboutPage", "ContactPage").
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Helps search engines understand content type for rich snippets
        ///     - GEO: Critical for AI systems to properly classify and understand content
        ///     Defaults to "WebPage" if not specified.
        /// </remarks>
        private string schemaType;

        /// <summary>
        ///     UNIFIED METADATA: Comma-separated key entities mentioned in the page.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model (primarily GEO-focused).
        ///     - GEO: Named entities (people, organizations, locations, concepts) for AI extraction
        ///     - SEO: Can enhance semantic understanding in advanced search systems
        ///     Example: "Company History, Leadership Team, Global Offices"
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
        ///     Should be a single, clear phrase (e.g., "Company Information", "Contact Details")
        ///     Used in: Custom classification tags, Schema.org about property
        /// </remarks>
        private string contentMSL;

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
        ///     UNIFIED METADATA: Breadcrumb label for navigation structure.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Used in Schema.org BreadcrumbList for rich snippets
        ///     - GEO: Helps AI understand page hierarchy and site structure
        ///     Falls back to page title if not set.
        ///     Used in: Schema.org BreadcrumbList structured data
        /// </remarks>
        private string breadcrumbLabel;

        /// <summary>
        ///     UNIFIED METADATA: Schema.org organization name for this page.
        /// </summary>
        /// <remarks>
        ///     Part of the unified SEO/GEO metadata model.
        ///     - SEO: Organization attribution in structured data for rich snippets
        ///     - GEO: Helps AI systems understand organizational context and authority
        ///     Can override blog-level organization for pages representing specific departments.
        ///     Used in: Schema.org publisher/organization properties
        /// </remarks>
        private string schemaOrganization;

        #endregion

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class. 
        ///     The contructor sets default values.
        /// </summary>
        public Page()
        {
            this.Id = Guid.NewGuid();
            this.DateCreated = BlogSettings.Instance.FromUtc();
        }

        static Page()
        {
            Blog.Saved += (s, e) =>
            {
                if (e.Action == SaveAction.Delete)
                {
                    Blog blog = s as Blog;
                    if (blog != null)
                    {
                        // remove deleted blog from static 'pages' and 'deletedpages'

                        if (pages != null && pages.ContainsKey(blog.Id))
                            pages.Remove(blog.Id);

                        if (deletedpages != null && deletedpages.ContainsKey(blog.Id))
                            deletedpages.Remove(blog.Id);
                    }
                }
            };
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the page is being served to the output stream.
        /// </summary>
        public static event EventHandler<ServingEventArgs> Serving;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets an unsorted list of all pages excluding deleted.
        /// </summary>
        public static List<Page> Pages
        {
            get
            {
                Blog blog = Blog.CurrentInstance;

                if (pages == null || !pages.ContainsKey(blog.Id))
                {
                    lock (SyncRoot)
                    {
                        if (pages == null || !pages.ContainsKey(blog.Id))
                        {
                            if (pages == null)
                                pages = new Dictionary<Guid, List<Page>>();

                            pages[blog.Id] = BlogService.FillPages().Where(p => p.IsDeleted == false).ToList();
                            pages[blog.Id].Sort((p1, p2) => String.Compare(p1.Title, p2.Title));
                        }
                    }
                }

                return pages[blog.Id];
            }
        }

        /// <summary>
        ///     Gets an unsorted list of deleted pages.
        /// </summary>
        public static List<Page> DeletedPages
        {
            get
            {
                Blog blog = Blog.CurrentInstance;

                if (deletedpages == null || !deletedpages.ContainsKey(blog.Id))
                {
                    lock (SyncRoot)
                    {
                        if (deletedpages == null || !deletedpages.ContainsKey(blog.Id))
                        {
                            if (deletedpages == null)
                                deletedpages = new Dictionary<Guid, List<Page>>();

                            deletedpages[blog.Id] = BlogService.FillPages().Where(p => p.IsDeleted == true).ToList();
                            deletedpages[blog.Id].Sort((p1, p2) => String.Compare(p1.Title, p2.Title));
                        }
                    }
                }

                return deletedpages[blog.Id];
            }
        }

        /// <summary>
        ///     Gets the absolute link to the page.
        /// </summary>
        public Uri AbsoluteLink
        {
            get
            {
                return Utils.ConvertToAbsolute(this.RelativeLink);
            }
        }

        /// <summary>
        ///     Gets or sets the Description or the object.
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
            }
        }

        /// <summary>
        ///     Gets or sets the Description of the page.
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
        ///     Gets or sets a value indicating whether or not this page should be displayed on the front page.
        /// </summary>
        public bool IsFrontPage
        {
            get
            {
                return this.frontPage;
            }

            set
            {
                base.SetValue("IsFrontPage", value, ref this.frontPage);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the has child pages.
        /// </summary>
        /// Does this page have child pages
        public bool HasChildPages
        {
            get
            {
                return Pages.Any(p => p.Parent == this.Id);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the has parent page.
        /// </summary>
        /// Does this page have a parent page?
        public bool HasParentPage
        {
            get
            {
                return this.Parent != Guid.Empty;
            }
        }

        /// <summary>
        ///     Gets or sets the Keywords for the page.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: This is the unified keywords property in the SEO/GEO model.
        ///     - SEO: Used in meta keywords tags for search engine classification
        ///     - GEO: Valuable for AI systems and specialized search applications
        ///     BACKWARD COMPATIBILITY: Legacy property maintained. This is the authoritative source
        ///     for all keyword metadata. In the unified model, this replaces separate keyword storage.
        ///     Used by MetadataBuilder and SeoMetadataManager for meta keywords and Schema.org keywords.
        /// </remarks>
        public string Keywords
        {
            get
            {
                return this.keywords;
            }

            set
            {
                base.SetValue("Keywords", value, ref this.keywords);
            }
        }

        /// <summary>
        ///     Gets or sets the parent of the Page. It is used to construct the 
        ///     hierachy of the pages.
        /// </summary>
        public Guid Parent
        {
            get
            {
                return this.parent;
            }

            set
            {
                base.SetValue("Parent", value, ref this.parent);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not this page should be published.
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
        ///     Gets or sets a value indicating whether or not this page is deleted.
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
        ///     Gets a relative-to-the-site-root path to the page.
        ///     Only for in-site use.
        /// </summary>
        public string RelativeLink
        {
            get
            {
                var theslug = Utils.RemoveIllegalCharacters(this.Slug) + BlogConfig.FileExtension;
                return $"{Utils.RelativeWebRoot}page/{theslug}";
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
                if (this.Blog.DoesHostnameDifferFromSiteAggregationBlog)
                    return this.AbsoluteLink.ToString();
                else
                    return this.RelativeLink;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not this page should be in the sitemap list.
        /// </summary>
        public bool ShowInList
        {
            get
            {
                return this.showInList;
            }

            set
            {
                base.SetValue("ShowInList", value, ref this.showInList);
            }
        }

        /// <summary>
        ///     Gets or sets the Slug of the Page.
        ///     A Slug is the relative URL used by the pages.
        /// </summary>
        public string Slug
        {
            get
            {
                if (string.IsNullOrEmpty(this.slug))
                {
                    return Utils.RemoveIllegalCharacters(this.Title);
                }

                return this.slug;
            }

            set
            {
                base.SetValue("Slug", value, ref this.slug);
            }
        }

        /// <summary>
        ///     Gets or sets the Title or the object.
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
        ///     Gets a value indicating whether or not this page should be shown
        /// </summary>
        /// <value></value>
        public bool IsVisible
        {
            get
            {
                if (this.isDeleted)
                    return false;
                else if (this.IsPublished)
                    return true;
                else if (Security.IsAuthorizedTo(Rights.ViewUnpublishedPages))
                    return true;

                return false;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not this page is visible to visitors not logged into the blog.
        /// </summary>
        /// <value></value>
        public bool IsVisibleToPublic
        {
            get
            {
                return this.IsPublished && !this.IsDeleted;
            }
        }

        /// <summary>
        /// Gets Author.
        /// </summary>
        string IPublishable.Author
        {
            get
            {
                return BlogSettings.Instance.AuthorName;
            }
        }

        /// <summary>
        /// Gets whether or not the current user owns this page.
        /// </summary>
        /// <returns></returns>
        public override bool CurrentUserOwns
        {
            get
            {
                // Because we are not storing an author name for each page,
                // any user that have edit page access is an owner
                return Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnPages);
            }
        }

        /// <summary>
        /// Gets whether the current user can delete this page.
        /// </summary>
        /// <returns></returns>
        public override bool CanUserDelete
        {
            get
            {
                return Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnPages);
            }
        }

        /// <summary>
        /// Gets whether the current user can edit this page.
        /// </summary>
        /// <returns></returns>
        public override bool CanUserEdit
        {
            get
            {
                return Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnPages);
            }
        }

        /// <summary>
        /// Gets Categories.
        /// </summary>
        /// <remarks>
        /// 
        /// 10/21/10
        /// This was returning null. I'm not sure what the purpose of that is. An IEnumerable should return
        /// an empty collection rather than null to indicate that there's nothing there.
        /// 
        /// </remarks>
        StateList<Category> IPublishable.Categories
        {
            get
            {
                return this.categories;
            }
        }
        private StateList<Category> categories = new StateList<Category>();

        StateList<string> IPublishable.Tags
        {
            get
            {
                return tags;
            }
        }

        /// <summary>
        /// The sort order of the page
        /// </summary>
        public int SortOrder
        {
            get { return sortOrder; }
            set { SetValue("SortOrder", value, ref sortOrder); }
        }

        private StateList<string> tags = new StateList<string>();

        #region SEO & GEO (Generative Engine Optimization) Properties

        /// <summary>
        ///     Gets or sets the canonical URL for this page.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     The canonical URL is used to prevent duplicate content penalties in search engines
        ///     and to establish the primary URL identity for this specific page when duplicated elsewhere.
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
        ///     Specifies the schema type for this page (e.g., "WebPage", "AboutPage", "ContactPage").
        ///     Used in structured data to help search engines and AI systems understand the content type.
        ///     Defaults to "WebPage" if not specified.
        /// </remarks>
        public string SchemaType
        {
            get { return this.schemaType; }
            set { base.SetValue("SchemaType", value, ref this.schemaType); }
        }

        /// <summary>
        ///     Gets or sets comma-separated key entities mentioned in the page.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model (GEO-focused).
        ///     A comma-separated list of named entities (people, organizations, locations, concepts)
        ///     mentioned in the page content. Used by AI systems for entity extraction and semantic understanding.
        ///     Example: "Company History, Leadership Team, Global Offices"
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
        ///     The primary subject or main topic of the page content, used for topic modeling
        ///     and content categorization by AI systems. Should be a single, clear phrase representing
        ///     the page's main subject (e.g., "Company Information", "Contact Details").
        /// </remarks>
        public string ContentMSL
        {
            get { return this.contentMSL; }
            set { base.SetValue("ContentMSL", value, ref this.contentMSL); }
        }

        /// <summary>
        ///     Gets or sets meta robots directives for search engine crawling behavior.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     Controls how search engine crawlers and AI indexers should treat this page.
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
        ///     Gets or sets the breadcrumb label for navigation structure.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     A label for this page in breadcrumb navigation and structural hierarchy.
        ///     Used in Schema.org BreadcrumbList markup to establish navigation paths that
        ///     help AI systems understand page hierarchy and relationships.
        /// </remarks>
        public string BreadcrumbLabel
        {
            get { return this.breadcrumbLabel; }
            set { base.SetValue("BreadcrumbLabel", value, ref this.breadcrumbLabel); }
        }

        /// <summary>
        ///     Gets or sets the Schema.org organization name for this page.
        /// </summary>
        /// <remarks>
        ///     UNIFIED METADATA: Part of the unified SEO/GEO model.
        ///     The organization responsible for or associated with this page content in Schema.org markup.
        ///     Can override the blog-level organization if the page is associated with a specific organization.
        ///     Used in structured data for proper attribution and organizational identification by AI systems.
        /// </remarks>
        public string SchemaOrganization
        {
            get { return this.schemaOrganization; }
            set { base.SetValue("SchemaOrganization", value, ref this.schemaOrganization); }
        }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether the current user can publish this page.
        /// </summary>
        /// <returns></returns>
        public bool CanPublish()
        {
            return Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnPages);
        }

        /// <summary>
        /// Returns the front page if any is available.
        /// </summary>
        /// <returns>The front Page.</returns>
        public static Page GetFrontPage()
        {
            return Pages.Find(page => page.IsFrontPage);
        }

        /// <summary>
        /// Returns a page based on the specified id.
        /// </summary>
        /// <param name="id">The page id.</param>
        /// <returns>The Page requested.</returns>
        public static Page GetPage(Guid id)
        {
            return Pages.FirstOrDefault(page => page.Id == id);
        }

        /// <summary>
        /// Called when [serving].
        /// </summary>
        /// <param name="page">The page being served.</param>
        /// <param name="arg">The <see cref="BlogEngine.Core.ServingEventArgs"/> instance containing the event data.</param>
        public static void OnServing(Page page, ServingEventArgs arg)
        {
            if (Serving != null)
            {
                Serving(page, arg);
            }
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

        #region Implemented Interfaces

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

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the page from the current BlogProvider.
        /// </summary>
        protected override void DataDelete()
        {
            this.IsDeleted = true;
            this.DateModified = DateTime.Now;
            DataUpdate();
           
            Pages.Remove(this);

            if(!DeletedPages.Contains(this))
                DeletedPages.Add(this);
        }

        /// <summary>
        /// Deletes the Page from the current BlogProvider.
        /// </summary>
        public void Purge()
        {
            BlogService.DeletePage(this);
            DeletedPages.Remove(this);
        }

        /// <summary>
        /// Restores the deleted page.
        /// </summary>
        public void Restore()
        {
            this.IsDeleted = false;
            this.DateModified = DateTime.Now;
            DataUpdate();
            
            DeletedPages.Remove(this);
            Pages.Add(this);
        }

        /// <summary>
        /// Inserts a new page to current BlogProvider.
        /// </summary>
        protected override void DataInsert()
        {
            BlogService.InsertPage(this);

            if (this.New)
            {
                Pages.Add(this);
            }
        }

        /// <summary>
        /// Retrieves a page form the BlogProvider
        /// based on the specified id.
        /// </summary>
        /// <param name="id">The page id.</param>
        /// <returns>The Page requested.</returns>
        protected override Page DataSelect(Guid id)
        {
            return BlogService.SelectPage(id);
        }

        /// <summary>
        /// Updates the object in its data store.
        /// </summary>
        protected override void DataUpdate()
        {
            BlogService.UpdatePage(this);
        }

        /// <summary>
        /// Validates the properties on the Page.
        /// </summary>
        protected override void ValidationRules()
        {
            this.AddRule("Title", "Title must be set", string.IsNullOrEmpty(this.Title));
            this.AddRule("Content", "Content must be set", string.IsNullOrEmpty(this.Content));
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
                var pageFields = BlogService.Provider.FillCustomFields().Where(f =>
                    f.CustomType == "PAGE" &&
                    f.ObjectId == this.Id.ToString()).ToList();

                if (pageFields == null || pageFields.Count < 1)
                    return null;

                var fields = new Dictionary<String, CustomField>();

                foreach (var item in pageFields)
                {
                    fields.Add(item.Key, item);
                }
                return fields;
            }
        }

        #endregion
    }
}