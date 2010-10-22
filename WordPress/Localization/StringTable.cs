
namespace WordPress.Localization
{
    public class StringTable
    {
        #region member variables

        #endregion

        #region constructor 

        public StringTable() 
        {
            this.PageTitles = new Titles();
            this.ControlsText = new ControlText();
            this.Messages = new UserMessages();
        }

        #endregion

        #region properties

        public string ApplicationTitle { get { return LocalizedResources.ApplicationTitle; } }

        public Titles PageTitles { get; private set; }

        public ControlText ControlsText { get; private set; }

        public UserMessages Messages { get; private set; }

        #endregion

        #region Titles class definition

        public class Titles
        {
            public Titles() { }

            public string Actions { get { return LocalizedResources.Title_Actions; } }
            public string Comments { get { return LocalizedResources.Title_Comments; } }
            public string Posts { get { return LocalizedResources.Title_Posts; } }
            public string Pages { get { return LocalizedResources.Title_Pages; } }
            public string Stats { get { return LocalizedResources.Title_Stats; } }
            public string AddAccount { get { return LocalizedResources.Title_AddAccount; } }
            public string Settings { get { return LocalizedResources.Title_Settings; } }
            public string Preferences { get { return LocalizedResources.Title_Preferences; } }
            public string Blogs { get { return LocalizedResources.Title_Blogs; } }
            public string EditPage { get { return LocalizedResources.Title_EditPage; } }
            public string EditPost { get { return LocalizedResources.Title_EditPost; } }
            public string ModerateComment { get { return LocalizedResources.Title_ModerateComment; } }
        }

        #endregion

        #region ControlText class definition

        public class ControlText
        {
            public ControlText() { }

            public string Username { get { return LocalizedResources.ControlText_Username; } }
            public string Password { get { return LocalizedResources.ControlText_Password; } }
            public string Cancel { get { return LocalizedResources.ControlText_Cancel; } }
            public string Save { get { return LocalizedResources.ControlText_Save; } }
            public string NeedBlog { get { return LocalizedResources.ControlText_NeedBlog; } }
            public string GetBlog { get { return LocalizedResources.ControlText_GetBlog; } }
            public string BlogUrl { get { return LocalizedResources.ControlText_BlogUrl; } }
            public string Refresh { get { return LocalizedResources.ControlText_Refresh; } }
            public string Edit { get { return LocalizedResources.ControlText_Edit; } }
            public string Add { get { return LocalizedResources.ControlText_Add; } }
            public string AddMedia { get { return LocalizedResources.ControlText_AddMedia; } }
            public string AccountDetails { get { return LocalizedResources.ControlText_AccountDetails; } }
            public string Media { get { return LocalizedResources.ControlText_Media; } }
            public string PlaceImage { get { return LocalizedResources.ControlText_PlaceImage; } }
            public string AboveText { get { return LocalizedResources.ControlText_AboveText; } }
            public string BelowText { get { return LocalizedResources.ControlText_BelowText; } }
            public string MaximumThumbnailPixelWidth { get { return LocalizedResources.ControlText_MaximumThumbnailPixelWidth; } }
            public string AlignThumbnailToCenter { get { return LocalizedResources.ControlText_AlignThumbnailToCenter; } }
            public string UploadAndLinkToFullImage { get { return LocalizedResources.ControlText_UploadAndLinkToFullImage; } }
            public string Location { get { return LocalizedResources.ControlText_Location; } }
            public string GeoTagPosts { get { return LocalizedResources.ControlText_GeoTagPosts; } }
            public string PageContent { get { return LocalizedResources.ControlText_PageContent; } }
            public string Bold { get { return LocalizedResources.ControlText_Bold; } }
            public string Italic { get { return LocalizedResources.ControlText_Italic; } }
            public string Underline { get { return LocalizedResources.ControlText_Underline; } }
            public string StrikeThrough { get { return LocalizedResources.ControlText_StrikeThrough; } }
            public string Link { get { return LocalizedResources.ControlText_Link; } }
            public string BlockQuote { get { return LocalizedResources.ControlText_BlockQuote; } }
            public string Publish { get { return LocalizedResources.ControlText_Publish; } }
            public string UploadChanges { get { return LocalizedResources.ControlText_UploadChanges; } }
            public string TagsAndCategories { get { return LocalizedResources.ControlText_TagsAndCategories; } }
            public string SelectCategories { get { return LocalizedResources.ControlText_SelectCategories; } }
            public string Status { get { return LocalizedResources.ControlText_Status; } }
            public string PostContent { get { return LocalizedResources.ControlText_PostContent; } }
            public string StartNewBlog { get { return LocalizedResources.ControlText_StartNewBlog; } }
            public string AddExistingWPBlog { get { return LocalizedResources.ControlText_AddExistingWPBlog; } }
            public string AddExistingWPSite { get { return LocalizedResources.ControlText_AddExistingSite; } }
            public string Delete { get { return LocalizedResources.ControlText_Delete; } }
            public string Reply { get { return LocalizedResources.ControlText_Reply; } }
            public string Approve { get { return LocalizedResources.ControlText_Approve; } }
            public string Unapprove { get { return LocalizedResources.ControlText_Unapprove; } }
            public string Spam { get { return LocalizedResources.ControlText_Spam; } }
            public string CommentNotifications { get { return LocalizedResources.ControlText_CommentNotifications; } }
            public string PlayNotificationSound { get { return LocalizedResources.ControlText_PlayNotificationSound; } }
            public string Vibrate { get { return LocalizedResources.ControlText_Vibrate; } }
            public string BlinkNotificationLight { get { return LocalizedResources.ControlText_BlinkNotificationLight; } }
            public string PostSignature { get { return LocalizedResources.ControlText_PostSignature; } }
            public string AddTaglineToNewPosts { get { return LocalizedResources.ControlText_AddTaglineToNewPosts; } }
            public string DefaultTagline { get { return LocalizedResources.ControlText_DefaultTagLine; } }
            public string StartBloggingInSeconds { get { return LocalizedResources.ControlText_StartBloggingInSeconds; } }
        }

        #endregion

        #region Messages class definition

        public class UserMessages
        {
            public UserMessages() { }

            public string MarkingAsSpam { get { return LocalizedResources.Messages_MarkingAsSpam; } }
            public string ApprovingComment { get { return LocalizedResources.Messages_ApprovingComment; } }
            public string UnapprovingComment { get { return LocalizedResources.Messages_UnapprovingComment; } }
            public string DeletingComment { get { return LocalizedResources.Messages_DeletingComment; } }
            public string MissingReply { get { return LocalizedResources.Messages_MissingReply; } }
            public string ReplyingToComment { get { return LocalizedResources.Messages_ReplyingToComment; } }

        }

        #endregion
    }
}
