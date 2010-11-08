using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetReferrerStatsRPC: GetStatsRPC<ReferrerDataPoint>
    {
        #region member variables

        private const string REFERRER_VALUE = "referrer";
        private const string VALUE_VALUE = "value";

        #endregion

        #region constructors

        public GetReferrerStatsRPC():base()
        {
            StatisticType = eStatisticType.Referrers;
        }

        public GetReferrerStatsRPC(Blog blog)
            : base(blog)
        {
            StatisticType = eStatisticType.Referrers;
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

        protected override List<ReferrerDataPoint> ParseResponseContent(XDocument xDoc)
        {
            List<ReferrerDataPoint> result = new List<ReferrerDataPoint>();

            foreach (XElement element in xDoc.Descendants(REFERRER_VALUE))
            {
                ReferrerDataPoint dataPoint = ParseDataPoint(element);
                result.Add(dataPoint);
            }

            return result;
        }

        private ReferrerDataPoint ParseDataPoint(XElement element)
        {
            //DEV NOTE: there are count and limit fields in the xml as well, but we're not 
            //interested in them
            int referralCount = 0;
            int.TryParse(element.Value, out referralCount);

            ReferrerDataPoint dataPoint = new ReferrerDataPoint
            {
                Url = element.Attribute(VALUE_VALUE).Value,
                Referrals = referralCount
            };
            return dataPoint;
        }

        #endregion
    }
}
