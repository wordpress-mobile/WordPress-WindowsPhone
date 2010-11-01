
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
            this.Options = new UserOptions();
            this.Prompts = new UserPrompts();
        }

        #endregion

        #region properties

        public string ApplicationTitle { get { return LocalizedResources.ApplicationTitle; } }

        public Titles PageTitles { get; private set; }

        public ControlText ControlsText { get; private set; }

        public UserMessages Messages { get; private set; }

        public UserOptions Options { get; private set; }

        public UserPrompts Prompts { get; private set; }

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
            public string ModerateComments { get { return LocalizedResources.Title_ModerateComments; } }
            public string All { get { return LocalizedResources.Title_All; } }
            public string Approve { get { return LocalizedResources.Title_Approve; } }
            public string Unapprove { get { return LocalizedResources.Title_Unapprove; } }
            public string Spam { get { return LocalizedResources.Title_Spam; } }
            public string SelectCategories { get { return LocalizedResources.ControlText_SelectCategories; } }
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
            public string ModerateComments { get { return LocalizedResources.ControlText_ModerateComments; } }
            public string Write { get { return LocalizedResources.ControlText_Write; } }
            public string AddAPage { get { return LocalizedResources.ControlText_AddAPage; } }
            public string BlogSettings { get { return LocalizedResources.ControlText_BlogSettings; } }
            public string AddBlog { get { return LocalizedResources.ControlText_AddBlog; } }
            public string Preferences { get { return LocalizedResources.ControlText_Preferences; } }
        }

        #endregion

        #region Messages class definition

        public class UserMessages
        {
            public UserMessages() { }

            public string MarkingAsSpam { get { return LocalizedResources.Messages_MarkingAsSpam; } }
            public string MarkingCommentsAsSpam { get { return LocalizedResources.Messages_MarkingCommentsAsSpam; } }
            public string ApprovingComment { get { return LocalizedResources.Messages_ApprovingComment; } }
            public string ApprovingComments { get { return LocalizedResources.Messages_ApprovingComments; } }
            public string UnapprovingComment { get { return LocalizedResources.Messages_UnapprovingComment; } }
            public string UnapprovingComments { get { return LocalizedResources.Messages_UnapprovingComments; } }
            public string DeletingComment { get { return LocalizedResources.Messages_DeletingComment; } }
            public string DeletingComments { get { return LocalizedResources.Messages_DeletingComments; } }
            public string MissingReply { get { return LocalizedResources.Messages_MissingReply; } }
            public string ReplyingToComment { get { return LocalizedResources.Messages_ReplyingToComment; } }
            public string RetrievingComments { get { return LocalizedResources.Messages_RetrievingComments; } }
            public string RetrievingPosts { get { return LocalizedResources.Messages_RetrievingPosts; } }
            public string RetrievingPages { get { return LocalizedResources.Messages_RetrievingPages; } }
            public string RetrievingEverything { get { return LocalizedResources.Messages_RetrievingEverything; } }
            public string RetrievingPost { get { return LocalizedResources.Messages_RetrievingPost; } }
            public string UploadingChanges { get { return LocalizedResources.Messages_UploadingChanges; } }
            public string LoggingIn { get { return LocalizedResources.Messages_LoggingIn; } }
            public string DeletingPage { get { return LocalizedResources.Messages_DeletingPage; } }
            public string DeletingPost { get { return LocalizedResources.Messages_DeletingPost; } }
        }

        #endregion

        #region UserOptions class definition

        public class UserOptions
        {
            public UserOptions() { }

            public string RefreshEntity_Comments { get { return LocalizedResources.Options_RefreshEntity_Comments; } }
            public string RefreshEntity_Posts { get { return LocalizedResources.Options_RefreshEntity_Posts; } }
            public string RefreshEntity_Pages { get { return LocalizedResources.Options_RefreshEntity_Pages; } }
            public string RefreshEntity_Everything { get { return LocalizedResources.Options_RefreshEntity_Everything; } }

            public string PostOptions_ViewPost { get { return LocalizedResources.Options_PostActions_ViewPost; } }
            public string PostOptions_ViewComments { get { return LocalizedResources.Options_PostActions_ViewComments; } }
            public string PostOptions_EditPost { get { return LocalizedResources.Options_PostActions_EditPost; } }
            public string PostOptions_DeletePost { get { return LocalizedResources.Options_PostActions_DeletePost; } }

            public string PageOptions_ViewPage { get { return LocalizedResources.Options_PageActions_ViewPage; } }
            public string PageOptions_ViewComments { get { return LocalizedResources.Options_PageActions_ViewComments; } }
            public string PageOptions_EditPage { get { return LocalizedResources.Options_PageActions_EditPage; } }
            public string PageOptions_DeletePage { get { return LocalizedResources.Options_PageActions_DeletePage; } }            
        }

        #endregion

        #region UserPrompts class definition

        public class UserPrompts
        {
            public UserPrompts(){}

            public string RefreshEntity { get { return LocalizedResources.Prompts_RefreshEntity; } }
            public string MissingUserName { get { return LocalizedResources.Prompts_MissingUserName; } }
            public string MissingPassword { get { return LocalizedResources.Prompts_MissingPassword; } }
            public string MissingUrl { get { return LocalizedResources.Prompts_MissingUrl; } }
            public string SelectThumbnailSize { get { return LocalizedResources.Prompt_SelectThumbnailSize; } }
            public string PostActions { get { return LocalizedResources.Prompt_PostActions; } }
            public string PageActions { get { return LocalizedResources.Prompt_PageActions; } }
        }

        #endregion
    }
}
