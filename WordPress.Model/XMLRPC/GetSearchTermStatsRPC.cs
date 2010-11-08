using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetSearchTermStatsRPC:GetStatsRPC<SearchTermDataPoint>
    {
        #region member variables

        private const string SEARCHTERM_VALUE = "searchterm";
        private const string VALUE_VALUE = "value";

        #endregion

        #region constructors

        public GetSearchTermStatsRPC()
            : base()
        {
            StatisticType = eStatisticType.SearchTerms;
        }

        public GetSearchTermStatsRPC(Blog blog)
            : base(blog)
        {
            StatisticType = eStatisticType.SearchTerms;
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

        protected override List<SearchTermDataPoint> ParseResponseContent(XDocument xDoc)
        {
            List<SearchTermDataPoint> result = new List<SearchTermDataPoint>();

            foreach (XElement element in xDoc.Descendants(SEARCHTERM_VALUE))
            {
                SearchTermDataPoint dataPoint = ParseDataPoint(element);
                result.Add(dataPoint);
            }

            return result;
        }

        private SearchTermDataPoint ParseDataPoint(XElement element)
        {
            int count = 0;
            int.TryParse(element.Value, out count);

            SearchTermDataPoint dataPoint = new SearchTermDataPoint
            {
                SearchTerm = element.Attribute(VALUE_VALUE).Value,
                Count = count
            };
            return dataPoint;
        }

        #endregion
    }
}
