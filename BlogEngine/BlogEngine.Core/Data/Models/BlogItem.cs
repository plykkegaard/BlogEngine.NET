namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Blog item
    /// </summary>
    public class BlogItem
    {
        /// <summary>
        /// Blog name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        /// <remarks>
        /// This property is used to store the user's password. It should be kept secure and not exposed in plain text.
        /// </remarks>
        public string Password { get; set; }

        /// <summary>
        /// Confirm password
        /// </summary>
        /// <remarks>
        /// This property is used to confirm the user's password during registration or password change.
        /// It should match the value of the <see cref="Password"/> property.
        /// </remarks>
        public string ConfirmPassword { get; set; }
    }
}
