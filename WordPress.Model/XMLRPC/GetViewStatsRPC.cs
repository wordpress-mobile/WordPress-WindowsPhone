using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    /// <summary>
    /// Queries the WordPress statistic service for the number of views for a blog
    /// over the given time period.
    /// </summary>
    public class GetViewStatsRPC: GetStatsRPC<StatisticLinearDataPoint>
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

        protected override List<StatisticLinearDataPoint> ParseResponseContent(XDocument xDoc)
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

            List<StatisticLinearDataPoint> result = new List<StatisticLinearDataPoint>();
            string elementName = GetDataElementName();
            foreach (XElement element in xDoc.Descendants(elementName))
            {
                StatisticLinearDataPoint dataPoint = new StatisticLinearDataPoint
                {
                    DependentAxisValue = element.Value,
                    IndependentAxisValue = element.Attribute("date").Value
                };
                result.Add(dataPoint);
            }

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
    }
}
