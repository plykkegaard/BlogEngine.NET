using System;

namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Represents a custom filter
    /// </summary>
    /// <remarks>
    /// This class is used to define custom filters for comments, allowing for more advanced filtering options.
    /// </remarks>
    public class CustomFilter
    {
        /// <summary>
        /// Short name
        /// </summary>
        /// <remarks>
        /// This property represents the short name of the custom filter.
        /// </remarks>
        public string Name { get; set; }
        /// <summary>
        /// Long (class) name
        /// </summary>
        /// <remarks>
        /// This property represents the full class name of the custom filter.
        /// </remarks>
        public string FullName { get; set; }
        /// <summary>
        /// If filter enabled
        /// </summary>
        /// <remarks>
        /// This property indicates whether the custom filter is enabled.
        /// </remarks>
        public bool Enabled { get; set; }
        /// <summary>
        /// Number of comments checked by filter
        /// </summary>
        /// <remarks>
        /// This property represents the number of comments that have been checked by the custom filter.
        /// </remarks>
        public int Checked { get; set; }
        /// <summary>
        /// Spam comments identified
        /// </summary>
        /// <remarks>
        /// This property represents the number of spam comments identified by the custom filter.
        /// </remarks>
        public int Spam { get; set; }
        /// <summary>
        /// Number of mistakes made
        /// </summary>
        /// <remarks>
        /// This property represents the number of mistakes made by the custom filter.
        /// </remarks>
        public int Mistakes { get; set; }
        /// <summary>
        /// Accuracy
        /// </summary>
        /// <remarks>
        /// This property calculates the accuracy of the custom filter based on the number of mistakes and checked comments.
        /// </remarks>
        public string Accuracy
        {
            get
            {
                try
                {
                    if (this.Mistakes < 1 || this.Checked < 1)
                        return "100";

                    if (this.Mistakes >= this.Checked)
                        return "0";

                    var c = (double)this.Checked;
                    var m = (double)this.Mistakes;
                    double a = 100 - (100 / c * m);

                    return String.Format("{0:0.00}", a);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
    }
}
