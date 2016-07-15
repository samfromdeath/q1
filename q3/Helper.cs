using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace q3
{
    public static class Helper
    {
        public static bool ContainsKeys(this NameValueCollection collection, string Name)
        {
            for (int i = 0; i < collection.AllKeys.Length; i++)
            {
                if(string.CompareOrdinal(collection.AllKeys[i], Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}