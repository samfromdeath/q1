using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace q3
{        
    /// <summary>
    /// Summary description for Module
    /// </summary>
    public class Module : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            if (context.Request.Headers.ContainsKeys("UserId") && context.Request.Headers.ContainsKeys("PassId"))
            {
                if(context.Request.Headers["UserId"] == "samuel123" && context.Request.Headers["PassId"] == "test")
                {
                    ASPSession.Current.UserId = 1;
                }
                else
                {
                    context.Response.StatusCode = 502;
                }    
                return;
            }else
            {
                if(ASPSession.Current.UserId == 0)
                {
                    context.Response.StatusCode = 502;
                    return;
                }
            }

            if(context.Request.Headers.ContainsKeys("RequestType"))
            {
                string type = context.Request.Headers.GetValues("RequestType").FirstOrDefault();
                if(type == "2")
                {                    

                    context.Response.Write("BODY");
                }else
                {
                    context.Response.Write("");
                }
            }            
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}