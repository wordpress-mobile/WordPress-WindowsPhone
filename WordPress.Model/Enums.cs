
namespace WordPress.Model
{
    public enum eCommentStatus
    {
        approve,
        hold,
        spam
    }

    public enum eXmlRPCStatus
    {
        /// <summary>
        /// Indicates the RPC is ready to execute
        /// </summary>
        Ready,

        /// <summary>
        /// Indicates the RPC is executing
        /// </summary>
        Working,

        /// <summary>
        /// Indicates the RPC has returned the expected result
        /// </summary>
        Succeeded,

        /// <summary>
        /// Indicates that the Async callback was unsuccessful
        /// </summary>
        CallbackFailed,

        /// <summary>
        /// Indicates that a fault code was received from the web server
        /// </summary>
        RPCFailed,

        /// <summary>
        /// Catastrophe inc
        /// </summary>
        Exception
    }

    public enum ePostType
    {
        post,
        page
    }

    /// <summary>
    /// Specifies the type of statistic to query the WordPress statistic service for
    /// </summary>
    public enum eStatisticType
    {
        Views,
        PostViews,
        Referrers,
        SearchTerms,
        Clicks
    }

    /// <summary>
    /// Specifies the time period to query to WordPress statistic service for the given
    /// eStatisticType
    /// </summary>
    public enum eStatisticPeriod
    {
        LastWeek,
        LastMonth,
        LastQuarter,
        LastYear,
        AllTime
    }

     public enum eGalleryLinkTo
     {
         AttachmentPage,
         MediaFile
     }
 
     public enum eGalleryType
     {
         Default,
         Tiles,
         SquareTiles,
         Circles,
         Slideshow
     }
}