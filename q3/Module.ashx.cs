using System;
using System.Collections.Generic;
using System.IO;
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
            try
            {
                context.Response.ContentType = "application/octet-stream";

                if (context.Request.Headers.ContainsKeys("UserId") && context.Request.Headers.ContainsKeys("PassId"))
                {
                    if (context.Request.Headers["UserId"] == "samuel123" && context.Request.Headers["PassId"] == "test")
                    {
                        ASPSession.Current.ConnectionString = File.ReadAllText(@"C:\Users\samuel grahame\Dropbox\q1\q3\ConnectionString.txt");
                        ASPSession.Current.UserId = 1;
                    }
                    else
                    {
                        throw new Exception("Invalid username or password!");
                    }
                    return;
                }
                else
                {
                    if (ASPSession.Current.UserId == 0)
                    {
                        throw new Exception("Invalid session!");                                                
                    }
                }

                if (context.Request.Headers.ContainsKeys("RequestType"))
                {
                    string type = context.Request.Headers["RequestType"];
                    if (type == "2")
                    {
                        if (context.Request.Headers.ContainsKeys("Request"))
                        {
                            using (Database db = new Database("main", ASPSession.Current.ConnectionString))
                            {
                                var dt = db.GetDataTable(context.Request.Headers["Request"]);
                                if (dt != null && dt.Columns.Count > 0)
                                {
                                    using (BinaryWriter bw = new BinaryWriter(context.Response.OutputStream))
                                    {
                                        int Columns = dt.Columns.Count;
                                        int Rows = dt.Rows.Count;

                                        bw.Write(Columns);
                                        bw.Write(Rows);

                                        TypeCode[] Codes = new TypeCode[Columns];
                                        byte[] CodesSht = new byte[Columns];
                                        bool[] CheckForDBNull = new bool[Columns];

                                        byte dbNull = (byte)TypeCode.DBNull;

                                        for (int i = 0; i < Columns; i++)
                                        {
                                            bw.Write((CheckForDBNull[i] = dt.Columns[i].AllowDBNull));
                                            bw.Write(dt.Columns[i].ColumnName);
                                            bw.Write((CodesSht[i] = (byte)(Codes[i] = Type.GetTypeCode(dt.Columns[i].DataType))));
                                        }

                                        for (int y = 0; y < Columns; y++)
                                        {
                                            TypeCode code = Codes[y];
                                            byte codeSht = CodesSht[y];
                                            bool checkForDBNull = CheckForDBNull[y];                                            

                                            for (int x = 0; x < Rows; x++)
                                            {
                                                TypeCode typeCode = code;
                                                if (checkForDBNull)
                                                {
                                                    if (dt.Rows[x][y] == DBNull.Value)
                                                    {
                                                        bw.Write(dbNull);
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        TypeCode tc = Type.GetTypeCode(dt.Rows[x][y].GetType());
                                                        if (typeCode != tc)
                                                        {
                                                            bw.Write((short)tc);
                                                            typeCode = tc;
                                                        }
                                                        else
                                                        {
                                                            bw.Write(codeSht);
                                                        }                                                        
                                                    }
                                                }
                                                switch (typeCode)
                                                {
                                                    case TypeCode.Boolean:
                                                        bw.Write((bool)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Char:
                                                        bw.Write((char)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.SByte:
                                                        bw.Write((sbyte)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Byte:
                                                        bw.Write((byte)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Int16:
                                                        bw.Write((short)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.UInt16:
                                                        bw.Write((ushort)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Int32:
                                                        bw.Write((int)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.UInt32:
                                                        bw.Write((uint)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Int64:
                                                        bw.Write((long)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.UInt64:
                                                        bw.Write((ulong)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Single:
                                                        bw.Write((float)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Double:
                                                        bw.Write((double)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.Decimal:
                                                        bw.Write((decimal)dt.Rows[x][y]);
                                                        break;
                                                    case TypeCode.DateTime:
                                                        bw.Write(((DateTime)dt.Rows[x][y]).ToBinary());
                                                        break;
                                                    case TypeCode.String:
                                                        bw.Write((string)dt.Rows[x][y]);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 502;
                context.Response.StatusDescription = e.Message;                
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}