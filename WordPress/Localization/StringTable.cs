
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
            public string AddNewCategory { get { return LocalizedResources.Title_AddNewCategory; } }
            public string NewPost { get { return LocalizedResources.Title_NewPost; } }
            public string NewPage { get { return LocalizedResources.Title_NewPage; } }
            public string CheckTheUrl { get { return LocalizedResources.Title_CheckTheUrl; } }
            public string ConfirmDelete { get { return LocalizedResources.Title_ConfirmDelete; } }
            public string Error { get { return LocalizedResources.Title_Error; } }
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
            public string PreserveBandwidth { get { return LocalizedResources.ControlText_PreserveBandwidth; } }
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
            public string Tags { get { return LocalizedResources.ControlText_Tags; } }            
            public string TagsHelpText { get { return LocalizedResources.ControlText_TagsHelpText; } }
            public string Categories { get { return LocalizedResources.ControlText_Categories; } }
            public string SelectCategories { get { return LocalizedResources.ControlText_SelectCategories; } }
            public string Status { get { return LocalizedResources.ControlText_Status; } }
            public string PostFormat { get { return LocalizedResources.ControlText_PostFormat; } }
            public string PostContent { get { return LocalizedResources.ControlText_PostContent; } }
            public string TapToEdit { get { return LocalizedResources.ControlText_TapToEdit; } }
            public string StartNewBlog { get { return LocalizedResources.ControlText_StartNewBlog; } }
            public string AddExistingWPBlog { get { return LocalizedResources.ControlText_AddExistingWPBlog; } }
            public string AddExistingWPSite { get { return LocalizedResources.ControlText_AddExistingSite; } }
            public string Delete { get { return LocalizedResources.ControlText_Delete; } }
            public string DeleteSelected { get { return LocalizedResources.ControlText_DeleteSelected; } }
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
            public string Reader { get { return LocalizedResources.ControlText_Reader; } }
            public string Back { get { return LocalizedResources.ControlText_Back; } }
            public string SelectedCategories { get { return LocalizedResources.ControlText_SelectedCategories; } }
            public string CategoryName { get { return LocalizedResources.ControlText_CategoryName; } }
            public string CategorySlug { get { return LocalizedResources.ControlText_CategorySlug; } }
            public string CategoryDescription { get { return LocalizedResources.ControlText_CategoryDescription; } }
            public string CategoryParent { get { return LocalizedResources.ControlText_CategoryParent; } }
            public string None { get { return LocalizedResources.ControlText_None; } }
            public string Apikey { get { return LocalizedResources.ControlText_Apikey; } }
            public string Date { get { return LocalizedResources.ControlText_Date; } }
            public string Views { get { return LocalizedResources.ControlText_Views; } }
            public string PostTitle { get { return LocalizedResources.ControlText_PostTitle; } }
            public string Referrers { get { return LocalizedResources.ControlText_Referrers; } }
            public string SearchTerms { get { return LocalizedResources.ControlText_SearchTerms; } }
            public string Clicks { get { return LocalizedResources.ControlText_Clicks; } }
            public string Title { get { return LocalizedResources.ControlText_Title; } }
            public string NoTitle { get { return LocalizedResources.ControlText_NoTitle; } }
            public string Content { get { return LocalizedResources.ControlText_Content; } }
            public string SelectBlogs { get { return LocalizedResources.ControlText_SelectBlogs; } }
            public string AddAll { get { return LocalizedResources.ControlText_AddAll; } }
            public string AddSelected { get { return LocalizedResources.ControlText_AddSelected; } }
            public string EULA { get { return LocalizedResources.ControlText_EULA; } }
            public string EULA_Content { get { return LocalizedResources.Eula; } }
            public string Accept { get { return LocalizedResources.ControlText_Accept; } }
            public string Decline { get { return LocalizedResources.ControlText_Decline; } }
            public string DeleteBlog { get { return LocalizedResources.ControlText_DeleteBlog; } }
            public string Clear { get { return LocalizedResources.ControlText_Clear; } }
            public string OriginalSize { get { return LocalizedResources.ControlText_OriginalSize; } }
            public string OriginalSizeAbbr { get { return LocalizedResources.ControlText_OriginalSizeAbbr; } }
            public string EnterUrl { get { return LocalizedResources.ControlText_EnterURL; } }
            public string EnterLinkText { get { return LocalizedResources.ControlText_EnterLinkText; } }
            public string InsertLink { get { return LocalizedResources.ControlText_InsertLink; } }
            public string visitForums { get { return LocalizedResources.ControlText_Forums; } }
            public string visitFAQ { get { return LocalizedResources.ControlText_FAQ; } }
            public string copyErrorMessage { get { return LocalizedResources.ControlText_CopyErrorMessage; } }
            public string LoadingContent { get { return LocalizedResources.ControlText_LoadingContent; } }
            public string SaveDraft { get { return LocalizedResources.ControlText_SaveDraft; } }
            public string PullDownToRefresh { get { return LocalizedResources.ControlText_PullDownToRefresh; } }
            public string ReleaseToRefresh { get { return LocalizedResources.ControlText_ReleaseToRefresh; } }
            public string Pin { get { return LocalizedResources.ControlText_Pin; } }
            public string Unpin { get { return LocalizedResources.ControlText_Unpin; } }
            public string ViewStats { get { return LocalizedResources.ControlText_ViewStats; } }
            public string PostStatus { get { return LocalizedResources.ControlText_PostStatus; } }
            public string PublishDate { get { return LocalizedResources.ControlText_PublishDate; } }
            public string Scheduled { get { return LocalizedResources.ControlText_Scheduled; } }
            public string Draft { get { return LocalizedResources.ControlText_Draft; } }
            public string PendingReview { get { return LocalizedResources.ControlText_PendingReview; } }
            public string Remove { get { return LocalizedResources.ControlText_Remove; } }
            public string LocalDraft { get { return LocalizedResources.ControlText_LocalDraft; } }
            public string Private { get { return LocalizedResources.ControlText_Private; } }
            public string ReplyToComment { get { return LocalizedResources.ControlText_ReplyToComment; } }
            public string EditComment { get { return LocalizedResources.ControlText_EditComment; } }
            public string Send { get { return LocalizedResources.ControlText_Send; } }
            public string Moderate { get { return LocalizedResources.ControlText_Moderate; } }
            
            public string FilterAll { get { return LocalizedResources.ControlText_FilterAll; } }
            public string FilterApproved { get { return LocalizedResources.ControlText_FilterApproved; } }
            public string FilterUnapproved { get { return LocalizedResources.ControlText_FilterUnapproved; } }
            public string FilterSpam { get { return LocalizedResources.ControlText_FilterSpam; } }
            

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
            public string EditingComment { get { return LocalizedResources.Messages_EditingComment; } }
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
            public string RetrievingCategories { get { return LocalizedResources.Messages_RetrievingCategories; } }
            public string RetrievingPage { get { return LocalizedResources.Messages_RetrievingPage; } }
            public string CreatingNewCategory { get { return LocalizedResources.Messages_CreatingNewCategory; } }
            public string AcquiringPermalink { get { return LocalizedResources.Messages_AcquiringPermalink; } }
            public string Loading { get { return LocalizedResources.Messages_Loading; } }
            public string NoStatsAvailable { get { return LocalizedResources.Messages_NoStatsAvailable; } }
            public string DownloadingStatistics { get { return LocalizedResources.Message_DownloadingStatistics; } }
            public string CheckTheUrl { get { return LocalizedResources.Message_CheckTheUrl; } }
            public string UploadingMedia { get { return LocalizedResources.Messages_UploadingMedia; } }
            public string Hello { get { return LocalizedResources.Messages_Hello; } }
            public string MarketDescription { get { return LocalizedResources.Messages_MarketDescription; } }
            public string Info { get { return LocalizedResources.Messages_Info; } }
            public string ContentCopied { get { return LocalizedResources.Messages_ContentCopied; } }
            public string InvalidCredentials { get { return LocalizedResources.Messages_InvalidCredentials; } }
            public string MissingFields { get { return LocalizedResources.Messages_MissingFields; } }
            public string NoBlogsFoundAtThisURL { get { return LocalizedResources.Messages_NoBlogFoundAtThisURL; } }
            public string TitleAndContentEmpty { get { return LocalizedResources.Messages_TitleAndContentEmpty; } }
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

            public string StatisticType_Views { get { return LocalizedResources.Options_StatisticType_Views; } }
            public string StatisticType_PostViews { get { return LocalizedResources.Options_StatisticType_PostViews; } }
            public string StatisticType_Referrers { get { return LocalizedResources.Options_StatisticType_Referrers; } }
            public string StatisticType_SearchTerms { get { return LocalizedResources.Options_StatisticType_SearchTerms; } }
            public string StatisticType_Clicks { get { return LocalizedResources.Options_StatisticType_Clicks; } }

            public string StatisticPeriod_LastWeek { get { return LocalizedResources.Options_StatisticPeriod_LastWeek; } }
            public string StatisticPeriod_LastMonth { get { return LocalizedResources.Options_StatisticPeriod_LastMonth; } }
            public string StatisticPeriod_LastQuarter { get { return LocalizedResources.Options_StatisticPeriod_LastQuarter; } }
            public string StatisticPeriod_LastYear { get { return LocalizedResources.Options_StatisticPeriod_LastYear; } }
            public string StatisticPeriod_AllTime { get { return LocalizedResources.Options_StatisticPeriod_AllTime; } }

            public string MediaOptions_PlaceBefore { get { return LocalizedResources.Options_MediaOptions_PlaceBefore; } }
            public string MediaOptions_PlaceAfter { get { return LocalizedResources.Options_MediaOptions_PlaceAfter; } }
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
            public string SelectCategory { get { return LocalizedResources.Prompts_SelectCategory; } }
            public string MissingCategoryName { get { return LocalizedResources.Prompts_MissingCategoryName; } }
            public string SelectStatisticType { get { return LocalizedResources.Prompts_SelectStatisticType; } }
            public string SelectStatisticPeriod { get { return LocalizedResources.Prompts_SelectStatisticPeriod; } }
            public string MissingApikey { get { return LocalizedResources.Prompts_MissingApikey; } }
            public string ConfirmDeletePageFormat { get { return LocalizedResources.Prompt_ConfirmDeletePageFormat; } }
            public string ConfirmDeletePostFormat { get { return LocalizedResources.Prompt_ConfirmDeletePostFormat; } }
            public string ConfirmDeleteComment { get { return LocalizedResources.Prompt_ConfirmDeleteComment; } }
            public string ConfirmMarkSpamComment { get { return LocalizedResources.Prompt_ConfirmMarkSpamComment; } }
            public string ConfirmDeleteCommentsFormat { get { return LocalizedResources.Prompt_ConfirmDeleteCommentsFormat; } }
            public string ConfirmMarkSpamCommentsFormat { get { return LocalizedResources.Prompt_ConfirmMarkSpamCommentsFormat; } }
            public string SelectBlogToDelete { get { return LocalizedResources.Prompts_SelectBlogToDelete; } }
            public string SureCancel { get { return LocalizedResources.Prompts_SureCancel; } }
            public string CancelEditing { get { return LocalizedResources.Prompts_CancelEditing; } }
            public string Confirm { get { return LocalizedResources.Prompts_Confirm; } }
            public string Page { get { return LocalizedResources.Prompts_Page; } }
            public string Post { get { return LocalizedResources.Prompts_Post; } }
            public string Comment { get { return LocalizedResources.Prompts_Comment; } }
            public string Comments { get { return LocalizedResources.Prompts_Comments; } }
            public string MediaPlacement { get { return LocalizedResources.Prompts_MediaPlacement; } }
        
            /*Error String*/
            public string ServerReturnedInvalidXmlRpcMessage { get { return LocalizedResources.Prompts_ServerReturnedInvalidXmlRpcMessage; } }
            public string XmlRpcOperationFailed { get { return LocalizedResources.Prompts_XmlRpcOperationFailed; } }
            public string XeElementMissing { get { return LocalizedResources.Prompts_XeElementMissing; } }
            public string MediaError { get { return LocalizedResources.Prompts_MediaError; } }
            public string MediaErrorContent { get { return LocalizedResources.Prompts_MediaErrorContent; } }
            public string NoConnectionErrorContent { get { return LocalizedResources.Prompts_NoConnectionError; } }
        }

        #endregion
    }
}
