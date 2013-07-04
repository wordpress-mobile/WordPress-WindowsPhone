using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class RegisterPushNotificationToken : XmlRemoteProcedureCall<BooleanResponseObject>
    {

        private const string METHODNAME_VALUE = "wpcom.mobile_push_register_token";
        private string _channelURI;
        private string _sandbox = "0";
        private string _device_uuid;
        private string deviceName;
        String reqTemplate = "<?xml version=\"1.0\" ?>"+
           "<methodCall>" +
           "<methodName>{0}</methodName>" +
           "<params>" +
           "<param><value><string>{1}</string></value></param>" +
           "<param><value><string>{2}</string></value></param>" +
           "<param><value><string>{3}</string></value></param>" +
           "<param><value><string>{4}</string></value></param>" +
           "<param><value><string>{5}</string></value></param>" +
           "<param><value><string>{6}</string></value></param>" +
           "<param><value><string>{7}</string></value></param>" +
           "<param><value><string>{8}</string></value></param>" +
           "</params>" +
           "</methodCall>";

        public RegisterPushNotificationToken(String url, string username, string password, string sandbox, string device_uuid, string channelURI, string deviceName)
            : base(url, METHODNAME_VALUE, username, password)
        {
            this._sandbox = sandbox;
            this._device_uuid = device_uuid;
            this._channelURI = channelURI;
            this.deviceName = deviceName;
        }

        protected override string BuildPostContentString()
        {
          
            string result = string.Format(reqTemplate,
               MethodName,
               Credentials.UserName.HtmlEncode(),
               Credentials.Password.HtmlEncode(),
               this._device_uuid.HtmlEncode(),
               this._device_uuid.HtmlEncode(),
               "win_phone",
               this._sandbox,
               this._channelURI.HtmlEncode(),
               deviceName
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
