using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetPostViewStatsRPC:GetStatsRPC<PostViewDataPoint>
    {
        #region member variables

        private const string POST_VALUE = "post";
        private const string ID_VALUE = "id";
        private const string TITLE_VALUE = "title";
        private const string URL_VALUE = "url";

        #endregion

        #region constructors

        public GetPostViewStatsRPC()
            : base()
        {
            StatisticType = eStatisticType.PostViews;
        }

        public GetPostViewStatsRPC(Blog blog)
            : base(blog)
        {
            StatisticType = eStatisticType.PostViews;
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

        protected override List<PostViewDataPoint> ParseResponseContent(XDocument xDoc)
        {
            List<PostViewDataPoint> result = new List<PostViewDataPoint>();

            foreach (XElement element in xDoc.Descendants(POST_VALUE))
            {
                PostViewDataPoint dataPoint = ParseDataPoint(element);
                result.Add(dataPoint);
            }

            return result;
        }

        private PostViewDataPoint ParseDataPoint(XElement element)
        {
            int id, viewCount;
            string title, url;

            int.TryParse(element.Attribute(ID_VALUE).Value, out id);
            title = element.Attribute(TITLE_VALUE).Value;
            url = element.Attribute(URL_VALUE).Value;
            int.TryParse(element.Value, out viewCount);

            PostViewDataPoint dataPoint = new PostViewDataPoint
            {
                PostId = id,
                Title = title,
                Url = url,
                ViewCount = viewCount
            };

            return dataPoint;
        }

        #endregion
    }
}
