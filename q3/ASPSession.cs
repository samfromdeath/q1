using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace q3
{
    public class ASPSession
    {
        // private constructor
        private const string sessionKey = "asp_session";

        public long UserId = 0L;

        private ASPSession()
        {
            DateStamp = DateTime.Now;
        }

        // Gets the current session.
        public static ASPSession Current
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return new ASPSession();

                return HttpContext.Current.Session[sessionKey] as ASPSession ?? (ASPSession)(HttpContext.Current.Session[sessionKey] = new ASPSession());
            }
        }

        public DateTime DateStamp { get; set; }        
    }
}