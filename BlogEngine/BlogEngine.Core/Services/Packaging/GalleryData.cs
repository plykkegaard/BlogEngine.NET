using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents extended metadata for a package available in the package gallery.
/// </summary>
/// <remarks>
/// This class encapsulates additional information about a package including its type (extension, theme, or widget),
/// download statistics, user ratings, and reviews. It is used to display comprehensive package information in the
/// BlogEngine package management system.
/// </remarks>
public class PackageExtra
{
    /// <summary>
    /// Specifies the type of package in the BlogEngine gallery.
    /// </summary>
    /// <remarks>
    /// Packages can be one of three types: extensions that add functionality, themes that customize appearance,
    /// or widgets that provide specific UI components.
    /// </remarks>
    public enum PackageType
    {
        /// <summary>
        /// An extension package that adds new functionality to BlogEngine.
        /// </summary>
        Extension,

        /// <summary>
        /// A theme package that provides customized design and styling for the blog.
        /// </summary>
        Theme,

        /// <summary>
        /// A widget package that provides a specific UI component or feature.
        /// </summary>
        Widget
    }

    /// <summary>
    /// Gets or sets the type of this package.
    /// </summary>
    /// <value>
    /// A PackageType enumeration value indicating whether this is an Extension, Theme, or Widget.
    /// This property is serialized using a StringEnumConverter for JSON compatibility.
    /// </value>
    [JsonConverter(typeof(StringEnumConverter))]
    public PackageType PkgType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this package.
    /// </summary>
    /// <value>
    /// A string uniquely identifying the package in the gallery.
    /// </value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the total number of times this package has been downloaded.
    /// </summary>
    /// <value>
    /// An integer representing the total download count.
    /// </value>
    public int DownloadCount { get; set; }

    /// <summary>
    /// Gets the average rating of this package based on user reviews.
    /// </summary>
    /// <value>
    /// A float representing the average rating calculated from all reviews.
    /// Returns 0 if there are no reviews.
    /// </value>
    /// <remarks>
    /// The rating is calculated by summing all individual review ratings and dividing by the number of reviews.
    /// If the Reviews collection is null or empty, the rating is 0.
    /// </remarks>
    public float Rating
    {
        get
        {
            if (Reviews == null || Reviews.Count == 0) return 0;

            float totalVoters = 0;
            float totalPoints = 0;

            for (int i = 0; i < Reviews.Count; i++)
            {
                totalVoters++;
                totalPoints += Reviews[i].Rating;
            }
            return Convert.ToInt32(totalPoints / totalVoters);
        }
    }

    /// <summary>
    /// Gets or sets the collection of user reviews for this package.
    /// </summary>
    /// <value>
    /// A List of Review objects containing user feedback and ratings for this package.
    /// </value>
    public List<Review> Reviews { get; set; }
}

/// <summary>
/// Represents a user review for a package in the BlogEngine gallery.
/// </summary>
/// <remarks>
/// This class captures user feedback about a package including their rating, written review body,
/// contact information, and the date the review was posted. Reviews are used to build the overall
/// rating for a package.
/// </remarks>
public class Review
{
    /// <summary>
    /// Gets or sets the name of the reviewer.
    /// </summary>
    /// <value>
    /// A string containing the name of the user who submitted the review.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the reviewer.
    /// </summary>
    /// <value>
    /// A string representing the IP address from which the review was submitted.
    /// This may be used for spam prevention or tracking purposes.
    /// </value>
    public string Ip { get; set; }

    /// <summary>
    /// Gets or sets the rating given by the reviewer.
    /// </summary>
    /// <value>
    /// An integer representing the rating score (e.g., 1-5 or 1-10 depending on the rating scale).
    /// </value>
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets the review text content.
    /// </summary>
    /// <value>
    /// A string containing the reviewer's written feedback about the package.
    /// </value>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the review was posted.
    /// </summary>
    /// <value>
    /// A DateTime representing when the review was submitted.
    /// </value>
    public DateTime DatePosted { get; set; }
}
