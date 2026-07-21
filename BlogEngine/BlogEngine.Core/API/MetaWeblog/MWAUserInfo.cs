namespace BlogEngine.Core.API.MetaWeblog
{
    /// <summary>
    /// MetaWeblog UserInfo struct
    /// returned from GetUserInfo call
    /// </summary>
    /// <remarks>
    /// Not used currently, but here for completeness.
    /// </remarks>
#pragma warning disable CS0649 // Field is never assigned, will always have default value this is a false positive, as the fields are populated via reflection from the XML-RPC call
#pragma warning disable S101 // Types should be named in PascalCase
    internal struct MWAUserInfo
    {
        /// <summary>
        /// User Name Proper
        /// </summary>
        public string nickname;
        
        /// <summary>
        /// Login ID
        /// </summary>
        public string userID;
        
        /// <summary>
        /// Url to User Blog?
        /// </summary>
        public string url;
        
        /// <summary>
        /// Email address of User
        /// </summary>
        public string email;
        
        /// <summary>
        /// User LastName
        /// </summary>
        public string lastName;
        
        /// <summary>
        /// User First Name
        /// </summary>
        public string firstName;
    }
#pragma warning restore S101
#pragma warning restore CS0649
}