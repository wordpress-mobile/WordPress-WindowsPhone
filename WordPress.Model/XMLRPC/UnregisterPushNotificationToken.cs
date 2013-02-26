using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class UnregisterPushNotificationToken : XmlRemoteProcedureCall<BooleanResponseObject>
    {

        private const string METHODNAME_VALUE = "wpcom.mobile_push_unregister_token";
        private string _device_uuid;
        String reqTemplate = "<?xml version=\"1.0\" ?>" +
           "<methodCall>" +
           "<methodName>{0}</methodName>" +
           "<params>" +
           "<param><value><string>{1}</string></value></param>" +
           "<param><value><string>{2}</string></value></param>" +
           "<param><value><string>{3}</string></value></param>" +
           "<param><value><string>{4}</string></value></param>" +
           "<param><value><string>{5}</string></value></param>" +
           "</params>" +
           "</methodCall>";

        public UnregisterPushNotificationToken(String url, string username, string password, string device_uuid)
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
               this._device_uuid.HtmlEncode(),
               "win_phone");
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
