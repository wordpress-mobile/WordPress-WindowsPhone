using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{

    public class PushNotificationsSendBlogsList : XmlRemoteProcedureCall<BooleanResponseObject>
    {
        private const string blogIDFormatTemplate = "<value><i4>{0}</i4></value>";
        private const string METHODNAME_VALUE = "wpcom.mobile_push_set_blogs_list";
        private string _device_uuid;
        private Queue<int> _blogIDs;
        private string _appVersion;
        String reqTemplate = "<?xml version=\"1.0\" ?>"+
           "<methodCall>" +
           "<methodName>{0}</methodName>" +
           "<params>" +
           "<param><value><string>{1}</string></value></param>" +
           "<param><value><string>{2}</string></value></param>" +
           "<param><value><string>{3}</string></value></param>" +
           "<param><value><array><data>{4}</data></array></value></param>" +
           "<param><value><string>{5}</string></value></param>" +
           "<param><value><string>{6}</string></value></param>" +
           "</params>" +
           "</methodCall>";

        public PushNotificationsSendBlogsList(String url, string username, string password, string device_uuid, Queue<int> blogIDs, string appVersion)
            : base(url, METHODNAME_VALUE, username, password)
        {
            this._device_uuid = device_uuid;
            _blogIDs = blogIDs;
            _appVersion = appVersion;
        }

        protected override string BuildPostContentString()
        {
            string blogIDsString = "";
            while (_blogIDs.Count > 0)
            {
                int cID = _blogIDs.Dequeue();
                blogIDsString += string.Format(blogIDFormatTemplate, cID);
            }

            string result = string.Format(reqTemplate,
               MethodName,
               Credentials.UserName.HtmlEncode(),
               Credentials.Password.HtmlEncode(),
               this._device_uuid.HtmlEncode(),
               blogIDsString,
               "win_phone",
               _appVersion
               );
            return result;
        }

        protected override List<BooleanResponseObject> ParseResponseContent(XDocument xDoc)
        {
            //Server response is ok. Do not check the bool response.
            List<BooleanResponseObject> result = new List<BooleanResponseObject>();
            result.Add(new BooleanResponseObject(true));
            return result;
        }
    }
}
