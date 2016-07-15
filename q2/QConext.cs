using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace q2
{
    public class QConext
    {
        public string Address { get; set; }
        public string Module { get; set; } = "/Module.ashx";
        public CookieCollection Session { get; set; } = null;

        public static void Configure(bool Expect100Continue = false, int DefaultConnectionLimit = 200, int MaxServicePointIdleTime = 2000,
            bool SetTcpKeepAlive = false, int KeepAliveTime = 0, int KeepAliveInterval = 0, bool ServerCertificateValidationCallback = true)
        {
            ServicePointManager.Expect100Continue = Expect100Continue;
            ServicePointManager.DefaultConnectionLimit = DefaultConnectionLimit;
            ServicePointManager.MaxServicePointIdleTime = MaxServicePointIdleTime;
            ServicePointManager.SetTcpKeepAlive(SetTcpKeepAlive, KeepAliveTime, KeepAliveInterval);

            ServicePointManager.ServerCertificateValidationCallback = delegate {
                return ServerCertificateValidationCallback;
            };
        }

        public async Task<bool> Login(string Username, string Password)
        {
            var Request = CreatePing();

            Request.Headers.Add("UserId", Username);
            Request.Headers.Add("PassId", Password);

            return await UpdateRequest(Request);            
        }

        public async Task<bool> UpdateRequest(HttpWebRequest Request)
        {
            using (HttpWebResponse responce = (HttpWebResponse)await Request.GetResponseAsync())
            {
                if (responce.StatusCode == HttpStatusCode.OK)
                {
                    if(responce.Cookies.Count > 0)
                        Session = responce.Cookies;
                    return true;
                }

                return false;
            }
        }

        public async Task<string> SelectRequest(HttpWebRequest Request)
        {
            using (HttpWebResponse responce = (HttpWebResponse)await Request.GetResponseAsync())
            {
                if (responce.StatusCode == HttpStatusCode.OK)
                {
                    if (responce.Cookies.Count > 0)
                        Session = responce.Cookies;

                    using (StreamReader readStream = new StreamReader(responce.GetResponseStream(), Encoding.UTF8))
                    {
                        return await readStream.ReadToEndAsync();
                    }
                }

                return string.Empty;
            }    
        }

        public HttpWebRequest CreateExecuteNonQuery(string Query)
        {
            return Create(Query, RequestType.ExecuteNonQuery);
        }

        public HttpWebRequest CreateExecuteReader(string Query)
        {
            return Create(Query, RequestType.ExecuteReader);
        }

        public HttpWebRequest CreatePing()
        {
            return Create(string.Empty, RequestType.Ping);
        }

        public HttpWebRequest Create(string Query, RequestType requestType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Address + Module);

            request.Headers.Add("RequestType", ((int)requestType).ToString());
            request.Headers.Add("Request", Query);

            request.Credentials = CredentialCache.DefaultCredentials;

            request.CookieContainer = new CookieContainer();
            if(Session != null && Session.Count > 0)
                request.CookieContainer.Add(Session);

            return request;
        }

        public enum RequestType
        {
            Ping,
            ExecuteNonQuery,
            ExecuteReader
        }
    }
}
