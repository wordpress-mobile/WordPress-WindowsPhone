using System;

namespace WordPress
{
    /// <summary>
    /// This exception can be intentionally thrown in order to forcefully quit the application
    /// </summary>
    public class ApplicationShouldEndException:Exception
    {
        #region constructors

        public ApplicationShouldEndException() : base() { }

        public ApplicationShouldEndException(string message) : base(message) { }

        public ApplicationShouldEndException(string message, Exception innerException) : base(message, innerException) { }

        #endregion
    }
}
