
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
}