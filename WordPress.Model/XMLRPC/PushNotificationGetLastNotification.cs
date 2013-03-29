using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class PushNotificationGetLastNotification : XmlRemoteProcedureCall<IntResponseObject>
    {

        private const string METHODNAME_VALUE = "wpcom.mobile_push_win_phone_get_last_notification";
        private string _device_uuid;
        String reqTemplate = "<?xml version=\"1.0\" ?>" +
           "<methodCall>" +
           "<methodName>{0}</methodName>" +
           "<params>" +
           "<param><value><string>{1}</string></value></param>" +
           "<param><value><string>{2}</string></value></param>" +
           "<param><value><string>{3}</string></value></param>" +
           "<param><value><string>{4}</string></value></param>" +
           "</params>" +
           "</methodCall>";

        public PushNotificationGetLastNotification(String url, string username, string password, string device_uuid)
            : base(url, METHODNAME_VALUE, username, password)
        {
            this._device_uuid = device_uuid;
        }

        protected override string BuildPostContentString()
        {

            string result = string.Format(reqTemplate,
               MethodName,
               Credentials.UserName.HtmlEncode(),
               Credentials.Password.HtmlEncode(),
               this._device_uuid.HtmlEncode(),
               "win_phone");
            return result;
        }

        protected override List<IntResponseObject> ParseResponseContent(XDocument xDoc)
        {
            XElement intElement = xDoc.Descendants(XmlRPCResponseConstants.PARAMS).First();
            List<IntResponseObject> result = new List<IntResponseObject>();

            string valueNotSplitted = intElement.GetValueAsString(false);
            string[] values = valueNotSplitted.Split('-');
            if (values.Count() > 1)
            {
                int blogID = 0;
                int.TryParse(values[0], out blogID);
                int commentID = 0;
                int.TryParse(values[1], out commentID);
                result.Add(new IntResponseObject(blogID));
                result.Add(new IntResponseObject(commentID));
            }
            else
            {
                result.Add(new IntResponseObject(0));
                result.Add(new IntResponseObject(0));
            }
            return result;
        }
    }
}
