using System.Collections.Generic;

namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Lookups
    /// </summary>
    /// <remarks>
    /// This class provides lookup data for various dropdowns and selection lists used in the application.
    /// </remarks>
    public class Lookups
    {
        /// <summary>
        /// Cultures supported by BE
        /// </summary>
        /// <remarks>
        /// This property contains the list of cultures supported by the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> Cultures { get; set; }

        /// <summary>
        /// Roles for self registration
        /// </summary>
        /// <remarks>
        /// This property contains the list of roles available for self-registration in the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> SelfRegisterRoles { get; set; }

        /// <summary>
        /// Page list
        /// </summary>
        /// <remarks>
        /// This property contains the list of pages available in the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> PageList { get; set; }

        /// <summary>
        /// List of blog authors
        /// </summary>
        /// <remarks>
        /// This property contains the list of authors available in the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> AuthorList { get; set; }

        /// <summary>
        /// Category list
        /// </summary>
        /// <remarks>
        /// This property contains the list of categories available in the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> CategoryList { get; set; }

        /// <summary>
        /// List of installed themes
        /// </summary>
        /// <remarks>
        /// This property contains the list of themes installed in the BlogEngine application.
        /// </remarks>
        public IEnumerable<SelectOption> InstalledThemes { get; set; }

        /// <summary>
        /// Post editor options
        /// </summary>
        /// <remarks>
        /// This property contains the editor options for posts in the BlogEngine application.
        /// </remarks>
        public EditorOptions PostOptions { get; set; }

        /// <summary>
        /// Page editor options
        /// </summary>
        /// <remarks>
        /// This property contains the editor options for pages in the BlogEngine application.
        /// </remarks>
        public EditorOptions PageOptions { get; set; }
    }
}
