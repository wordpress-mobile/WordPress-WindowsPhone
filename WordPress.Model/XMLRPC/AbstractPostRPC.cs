using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace WordPress.Model
{
    public abstract class AbstractPostRPC: XmlRemoteProcedureCall<Post>
    {
 
        #region member variables

        protected string _content;

        #endregion
  

        #region properties

        public Post Post { get; set; }

        public bool Publish { get; set; }

        public ePostType PostType { get; set; }

        #endregion


        #region constructors

        public AbstractPostRPC() : base(){ }

        public AbstractPostRPC (string url, string methodName, string username, string password) : base(url, methodName, username, password)
        { }

        #endregion


        #region methods

        protected string FormatCategories()
        {
            string dataFormatString = "<value><string>{0}</string></value>";

            StringBuilder categoryBuilder = new StringBuilder();
            string data = string.Empty;

            foreach (string category in Post.Categories)
            {
                data = string.Format(dataFormatString, category.XmlEscape().Replace("&#38;", "&#38;amp;"));
                categoryBuilder.Append(data);
            }
            return categoryBuilder.ToString();
        }
        #endregion

    }
}
