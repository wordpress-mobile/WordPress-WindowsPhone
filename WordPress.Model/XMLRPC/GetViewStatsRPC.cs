using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    /// <summary>
    /// Queries the WordPress statistic service for the number of views for a blog
    /// over the given time period.
    /// </summary>
    public class GetViewStatsRPC: GetStatsRPC<ViewDataPoint>
    {
        #region constructors

        public GetViewStatsRPC()
            : base()
        {
            StatisticType = eStatisticType.Views;
        }

        public GetViewStatsRPC(Blog blog)
            : base(blog)
        {
            StatisticType = eStatisticType.Views;
        }

        #endregion

        #region methods

        protected override List<ViewDataPoint> ParseResponseContent(XDocument xDoc)
        {
            /*----------------------------------------------------------------------
             * DEV NOTE: the xml structure can vary depending on the requested period.  The structures
             * will look similar to the following:
             * 
             * LastWeek, LastMonth:
             * <views>
             *   <day date="2010-10-29">800</day>
             *   <day date=2010-10-30>900</day>
             * </views>
             * 
             * LastQuarter
             * <views>
             *   <week date="2010-33">900</week>
             *   <week date="2010-34">1000</week>
             * </views>
             * 
             * LastYear, AllTime
             * <views>
             *   <month date="2009-11">3000</month>
             *   <month date="2009-12>4000</month>
             * </views>
             * --------------------------------------------------------------------*/

            List<ViewDataPoint> result = new List<ViewDataPoint>();
            string elementName = GetDataElementName();
            foreach (XElement element in xDoc.Descendants(elementName))
            {
                ViewDataPoint dataPoint = ParseDataPoint(element.Attribute("date").Value, element.Value);
                result.Add(dataPoint);
            }

            result = result.OrderBy(dataPoint => dataPoint.ViewDate).ToList();

            return result;
        }

        private string GetDataElementName()
        {
            string elementName = string.Empty;

            switch (StatisicPeriod)
            {
                case eStatisticPeriod.LastWeek:
                    elementName = "day";
                    break;
                case eStatisticPeriod.LastMonth:
                    elementName = "day";
                    break;
                case eStatisticPeriod.LastQuarter:
                    elementName = "week";
                    break;
                case eStatisticPeriod.LastYear:
                    elementName = "month";
                    break;
                case eStatisticPeriod.AllTime:
                    elementName = "month";
                    break;
            }

            return elementName;
        }

        private ViewDataPoint ParseDataPoint(string date, string count)
        {
            int viewCount, year, week, month;
            DateTime viewDate = DateTime.MinValue;

            int.TryParse(count, out viewCount);

            switch (StatisicPeriod)
            {
                case eStatisticPeriod.LastWeek:
                case eStatisticPeriod.LastMonth:
                    DateTime.TryParse(date, out viewDate);
                    break;
                case eStatisticPeriod.LastQuarter:
                    year = int.Parse(date.Substring(0, 4));
                    week = int.Parse(date.Substring(5));
                    viewDate = new DateTime(year, 1, 1);
                    viewDate = viewDate.AddDays(week * 7);
                    break;
                case eStatisticPeriod.LastYear:
                case eStatisticPeriod.AllTime:
                    year = int.Parse(date.Substring(0, 4));
                    month = int.Parse(date.Substring(5));
                    viewDate = new DateTime(year, month, 1);
                    break;
            }
            return new ViewDataPoint { ViewCount = viewCount, ViewDate = viewDate };
        }

        #endregion
    }
}
