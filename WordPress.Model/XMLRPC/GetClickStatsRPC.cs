using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetClickStatsRPC:GetStatsRPC<ClickDataPoint>
    {
        #region member variables

        private const string CLICK_VALUE = "click";
        private const string VALUE_VALUE = "value";

        #endregion

        #region constructors

        public GetClickStatsRPC()
            : base()
        {
            StatisticType = eStatisticType.Clicks;
        }

        public GetClickStatsRPC(Blog blog)
            : base(blog)
        {
            StatisticType = eStatisticType.Clicks;
        }

        #endregion

        #region properties
        
        protected override Uri Uri
        {
            get
            {
                Uri uri = base.Uri;
                string urlString = uri.OriginalString + "&summarize";
                return new Uri(urlString, UriKind.Absolute);
            }
        }

        #endregion

        #region methods

        protected override List<ClickDataPoint> ParseResponseContent(XDocument xDoc)
        {
            List<ClickDataPoint> result = new List<ClickDataPoint>();

            foreach (XElement element in xDoc.Descendants(CLICK_VALUE))
            {
                ClickDataPoint dataPoint = ParseDataPoint(element);
                result.Add(dataPoint);
            }

            return result;
        }

        private ClickDataPoint ParseDataPoint(XElement element)
        {
            int count = 0;
            int.TryParse(element.Value, out count);

            ClickDataPoint dataPoint = new ClickDataPoint
            {
                Url = element.Attribute(VALUE_VALUE).Value,
                Count = count
            };

            return dataPoint;
        }

        #endregion
    }
}
