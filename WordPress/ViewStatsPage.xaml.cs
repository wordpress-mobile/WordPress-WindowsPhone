using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using WordPress.Localization;
using WordPress.Model;

namespace WordPress
{
    public partial class ViewStatsPage : PhoneApplicationPage
    {
        #region member variables

        private List<string> _statisticTypeOptions;
        private List<string> _statisticPeriodOptions;
        private StringTable _localizedStrings;
        private SelectionChangedEventHandler _popupServiceSelectionChangedHandler;

        #endregion
        public ViewStatsPage()
        {
            
            InitializeComponent();

            DataContext = App.MasterViewModel;

            _localizedStrings = App.Current.Resources["StringTable"] as StringTable;

            _statisticTypeOptions = new List<string>(5);
            _statisticTypeOptions.Add(_localizedStrings.Options.StatisticType_Views);
            _statisticTypeOptions.Add(_localizedStrings.Options.StatisticType_PostViews);
            _statisticTypeOptions.Add(_localizedStrings.Options.StatisticType_Referrers);
            _statisticTypeOptions.Add(_localizedStrings.Options.StatisticType_SearchTerms);
            _statisticTypeOptions.Add(_localizedStrings.Options.StatisticType_Clicks);

            _statisticPeriodOptions = new List<string>(5);
            _statisticPeriodOptions.Add(_localizedStrings.Options.StatisticPeriod_LastWeek);
            _statisticPeriodOptions.Add(_localizedStrings.Options.StatisticPeriod_LastMonth);
            _statisticPeriodOptions.Add(_localizedStrings.Options.StatisticPeriod_LastQuarter);
            _statisticPeriodOptions.Add(_localizedStrings.Options.StatisticPeriod_LastYear);
            _statisticPeriodOptions.Add(_localizedStrings.Options.StatisticPeriod_AllTime);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
	    {
	        base.OnNavigatedTo(e);
	        Blog currentBlog = App.MasterViewModel.CurrentBlog;
	    }


        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if ( App.PopupSelectionService.IsPopupOpen )
            {
                App.PopupSelectionService.SelectionChanged -= _popupServiceSelectionChangedHandler;
                App.PopupSelectionService.HidePopup();
                e.Cancel = true;
            }
            else
            {
                 base.OnBackKeyPress(e);
            }
        }


        #region properties

        public eStatisticPeriod StatisticPeriod { get; set; }

        public eStatisticType StatisticType { get; set; }

        #endregion

        #region methods

        private void OnStatsButtonClick(object sender, RoutedEventArgs e)
        {
            RetrieveStats(false);
        }



        private void RetrieveStats(bool invalidCredentials)
        {
            //make sure the current blog has an api key associated to it.
            if (string.IsNullOrEmpty(App.MasterViewModel.CurrentBlog.ApiKey))
            {
                if (App.MasterViewModel.CurrentBlog.DotcomUsername == null || invalidCredentials)
                {
                    dotcomLoginGrid.Visibility = System.Windows.Visibility.Visible;
                    Storyboard sB = new Storyboard();
                    DoubleAnimation doubleAnimation = new DoubleAnimation();
                    PropertyPath pPath = new PropertyPath("dotcomLoginGrid.Opacity");    // your button name.Opacity

                    doubleAnimation.Duration = TimeSpan.FromMilliseconds(500);
                    doubleAnimation.From = 0;
                    doubleAnimation.To = 1;
                    sB.Children.Add(doubleAnimation);
                    Storyboard.SetTargetProperty(doubleAnimation, pPath);
                    Storyboard.SetTarget(doubleAnimation, (dotcomLoginGrid));    // your button name
                    sB.Begin();

                    if (invalidCredentials)
                    {
                        MessageBox.Show(_localizedStrings.Messages.InvalidCredentials);
                    }
                    
                    return;
                }
                else
                {
                    loadingStatsProgressBar.Opacity = 1.0;
                    GetApiKeyRPC rpc = new GetApiKeyRPC(App.MasterViewModel.CurrentBlog, true);
                    rpc.Completed += OnGetApiKeyRPCCompleted;
                    rpc.ExecuteAsync();
                    return;
                }
            }

            switch (StatisticType)
            {
                case eStatisticType.Views:
                    RetrieveViews();
                    break;
                case eStatisticType.PostViews:
                    RetrievePostViews();
                    break;
                case eStatisticType.Referrers:
                    RetrieveReferrers();
                    break;
                case eStatisticType.SearchTerms:
                    RetrieveSearchTerms();
                    break;
                case eStatisticType.Clicks:
                    RetrieveClicks();
                    break;
            }
        }

        private void OnGetApiKeyRPCCompleted(object sender, XMLRPCCompletedEventArgs<Blog> args)
        {
            loadingStatsProgressBar.Opacity = 0.0;
            if (App.MasterViewModel.CurrentBlog.ApiKey != null)
            {
                RetrieveStats(false);
            }
            else
            {
                RetrieveStats(true);
            }
            
        }

        private void OnDotcomCancelButtonClick(object sender, RoutedEventArgs e)
        {
            fadeOutDoctomGrid();
        }

        private void OnDotcomOKButtonClick(object sender, RoutedEventArgs e)
        {
            if (dotcomUsernameBox.Text != "" && dotcomPasswordBox.Password != "")
            {
                App.MasterViewModel.CurrentBlog.DotcomUsername = dotcomUsernameBox.Text;
                App.MasterViewModel.CurrentBlog.DotcomPassword = dotcomPasswordBox.Password;
                fadeOutDoctomGrid();
                RetrieveStats(false);
            }
            else
            {
                MessageBox.Show(_localizedStrings.Messages.MissingFields);
            }
        }

        private void fadeOutDoctomGrid()
        {
            Storyboard sB = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            PropertyPath pPath = new PropertyPath("dotcomLoginGrid.Opacity");    // your button name.Opacity

            doubleAnimation.Duration = TimeSpan.FromMilliseconds(500);
            doubleAnimation.From = 1;
            doubleAnimation.To = 0;
            sB.Children.Add(doubleAnimation);
            Storyboard.SetTargetProperty(doubleAnimation, pPath);
            Storyboard.SetTarget(doubleAnimation, (dotcomLoginGrid));    // your button name
            sB.Begin();
            dotcomLoginGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void RetrieveViews()
        {
            GetViewStatsRPC rpc = new GetViewStatsRPC(App.MasterViewModel.CurrentBlog);
            rpc.StatisicPeriod = StatisticPeriod;
            rpc.Completed += OnGetViewStatsRPCCompleted;
            rpc.ExecuteAsync();

            loadingStatsProgressBar.Opacity = 1.0;
        }

        private void OnGetViewStatsRPCCompleted(object sender, XMLRPCCompletedEventArgs<ViewDataPoint> args)
        {
            //DEV NOTE: this link was really helpful getting things going:
            //http://silverlighthack.com/post/2010/10/08/Windows-Phone-7-RTM-Charting-using-the-Silverlight-Control-Toolkit.aspx

            GetViewStatsRPC rpc = sender as GetViewStatsRPC;
            rpc.Completed -= OnGetViewStatsRPCCompleted;

            if (null == args.Error)
            {
                if (null == args.Items) return;

                if (0 == args.Items.Count)
                {
                    MessageBox.Show(_localizedStrings.Messages.NoStatsAvailable);
                }
                else
                {
                    if (0 != viewsStatsChart.Series.Count)
                    {
                        HideStatControls();

                        viewsStatsScrollViewer.Visibility = Visibility.Visible;

                        ColumnSeries series = viewsStatsChart.Series[0] as ColumnSeries;

                        DateTimeAxis axis = series.IndependentAxis as DateTimeAxis;
                        axis.Interval = ConvertStatisticPeriodToInterval();
                        axis.IntervalType = ConvertStatisticPeriodToIntervalType();    
                    }

                    ObservableObjectCollection viewStatsDataSource = Resources["viewStatsDataSource"] as ObservableObjectCollection;
                    viewStatsDataSource.Clear();
                    args.Items.ForEach(item => viewStatsDataSource.Add(item));
                }
            }
            else
            {
                this.HandleException(args.Error);
            }

            loadingStatsProgressBar.Opacity = 0;
        }

        private void HideStatControls()
        {
            viewsStatsScrollViewer.Visibility = Visibility.Collapsed;
            postViewsGrid.Visibility = Visibility.Collapsed;
            searchTermsGrid.Visibility = Visibility.Collapsed;
            referrersGrid.Visibility = Visibility.Collapsed;
            clicksGrid.Visibility = Visibility.Collapsed;
        }

        private DateTimeIntervalType ConvertStatisticPeriodToIntervalType()
        {
            switch (StatisticPeriod)
            {
                case eStatisticPeriod.LastWeek:
                case eStatisticPeriod.LastMonth:
                    return DateTimeIntervalType.Days;
                case eStatisticPeriod.LastQuarter:
                    return DateTimeIntervalType.Weeks;
                case eStatisticPeriod.LastYear:
                case eStatisticPeriod.AllTime:
                    return DateTimeIntervalType.Months;
                default:
                    return DateTimeIntervalType.Auto;
            }
        }

        private int ConvertStatisticPeriodToInterval()
        {            
            if (eStatisticPeriod.LastMonth == StatisticPeriod)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        private void RetrievePostViews()
        {
            GetPostViewStatsRPC rpc = new GetPostViewStatsRPC(App.MasterViewModel.CurrentBlog);
            rpc.StatisicPeriod = StatisticPeriod;
            rpc.Completed += OnGetPostViewStatsRPCCompleted;
            rpc.ExecuteAsync();

            loadingStatsProgressBar.Opacity = 1.0;
        }

        private void OnGetPostViewStatsRPCCompleted(object sender, XMLRPCCompletedEventArgs<PostViewDataPoint> args)
        {
            GetPostViewStatsRPC rpc = sender as GetPostViewStatsRPC;
            rpc.Completed -= OnGetPostViewStatsRPCCompleted;

            if (null == args.Error)
            {
                HideStatControls();

                postViewsGrid.Visibility = Visibility.Visible;

                ObservableObjectCollection dataSource = Resources["postViewStatsDataSource"] as ObservableObjectCollection;
                dataSource.Clear();
                args.Items.ForEach(item => dataSource.Add(item));
            }
            else
            {
                this.HandleException(args.Error);
            }

            loadingStatsProgressBar.Opacity = 0.0;
        }

        private void RetrieveReferrers()
        {
            GetReferrerStatsRPC rpc = new GetReferrerStatsRPC(App.MasterViewModel.CurrentBlog);
            rpc.StatisicPeriod = StatisticPeriod;
            rpc.Completed += OnGetReferrerStatsRPCCompleted;
            rpc.ExecuteAsync();

            loadingStatsProgressBar.Opacity = 1.0;
        }

        private void OnGetReferrerStatsRPCCompleted(object sender, XMLRPCCompletedEventArgs<ReferrerDataPoint> args)
        {
            GetReferrerStatsRPC rpc = sender as GetReferrerStatsRPC;
            rpc.Completed -= OnGetReferrerStatsRPCCompleted;

            if (null == args.Error)
            {
                HideStatControls();

                referrersGrid.Visibility = Visibility.Visible;

                ObservableObjectCollection dataSource = Resources["referrerStatsDataSource"] as ObservableObjectCollection;
                dataSource.Clear();
                args.Items.ForEach(item => dataSource.Add(item));
            }
            else
            {
                this.HandleException(args.Error);
            }

            loadingStatsProgressBar.Opacity = 0.0;
        }

        private void RetrieveSearchTerms()
        {
            GetSearchTermStatsRPC rpc = new GetSearchTermStatsRPC(App.MasterViewModel.CurrentBlog);
            rpc.StatisicPeriod = StatisticPeriod;
            rpc.Completed += OnGetSearchTermStatsRPCCompleted;
            rpc.ExecuteAsync();

            loadingStatsProgressBar.Opacity = 1.0;
        }

        private void OnGetSearchTermStatsRPCCompleted(object sender, XMLRPCCompletedEventArgs<SearchTermDataPoint> args)
        {
            GetSearchTermStatsRPC rpc = sender as GetSearchTermStatsRPC;
            rpc.Completed -= OnGetSearchTermStatsRPCCompleted;

            if (null == args.Error)
            {
                HideStatControls();

                searchTermsGrid.Visibility = Visibility.Visible;

                ObservableObjectCollection dataSource = Resources["searchTermStatsDataSource"] as ObservableObjectCollection;
                dataSource.Clear();
                args.Items.ForEach(item => dataSource.Add(item));
            }
            else
            {
                this.HandleException(args.Error);
            }

            loadingStatsProgressBar.Opacity = 0.0;
        }

        private void RetrieveClicks()
        {
            GetClickStatsRPC rpc = new GetClickStatsRPC(App.MasterViewModel.CurrentBlog);
            rpc.StatisicPeriod = StatisticPeriod;
            rpc.Completed += OnGetClickStatsRPCCompleted;
            rpc.ExecuteAsync();

            loadingStatsProgressBar.Opacity = 1.0;
        }

        private void OnGetClickStatsRPCCompleted(object sender, XMLRPCCompletedEventArgs<ClickDataPoint> args)
        {
            GetClickStatsRPC rpc = sender as GetClickStatsRPC;
            rpc.Completed -= OnGetClickStatsRPCCompleted;

            if (null == args.Error)
            {
                HideStatControls();

                clicksGrid.Visibility = Visibility.Visible;

                ObservableObjectCollection dataSource = Resources["clickStatsDataSource"] as ObservableObjectCollection;
                dataSource.Clear();
                args.Items.ForEach(item => dataSource.Add(item));
            }
            else
            {
                this.HandleException(args.Error);
            }

            loadingStatsProgressBar.Opacity = 0.0;
        }

        private void OnStatisticPeriodButtonClick(object sender, RoutedEventArgs args)
        {
            PresentStatisticPeriodOptions();
        }

        private void PresentStatisticPeriodOptions()
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.SelectStatisticPeriod;
            App.PopupSelectionService.ItemsSource = _statisticPeriodOptions;
            App.PopupSelectionService.SelectionChanged += OnStatisticPeriodOptionsSelectionChanged;
            _popupServiceSelectionChangedHandler = OnStatisticPeriodOptionsSelectionChanged;
            App.PopupSelectionService.ShowPopup();
        }

        private void OnStatisticPeriodOptionsSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnStatisticPeriodOptionsSelectionChanged;
            _popupServiceSelectionChangedHandler = null;

            if (1 < args.AddedItems.Count) return;

            string selection = args.AddedItems[0] as string;
            statisticPeriodButton.Content = selection;
        }

        private void OnStatisticTypeButtonClick(object sender, RoutedEventArgs args)
        {
            PresentStatisticTypeOptions();
        }

        private void PresentStatisticTypeOptions()
        {
            App.PopupSelectionService.Title = _localizedStrings.Prompts.SelectStatisticType;
            App.PopupSelectionService.ItemsSource = _statisticTypeOptions;
            App.PopupSelectionService.SelectionChanged += OnStatisticTypeOptionsSelectionChanged;
            _popupServiceSelectionChangedHandler = OnStatisticTypeOptionsSelectionChanged;
            App.PopupSelectionService.ShowPopup();
        }

        private void OnStatisticTypeOptionsSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            App.PopupSelectionService.SelectionChanged -= OnStatisticTypeOptionsSelectionChanged;
            _popupServiceSelectionChangedHandler = null;

            if (1 < args.AddedItems.Count) return;

            string selection = args.AddedItems[0] as string;
            statisticTypeButton.Content = selection;
        }

        private void OnHyperLinkButtonClick(object sender, RoutedEventArgs args)
        {
            HyperlinkButton button = sender as HyperlinkButton;
            if (null == button) return;

            string url = button.Content as string;
            string urlFormatString = "/BrowserShellPage.xaml?uri={0}";
            string pageUrl = string.Format(urlFormatString, url);
            NavigationService.Navigate(new Uri(pageUrl, UriKind.Relative));
        }

        #endregion

    }
}