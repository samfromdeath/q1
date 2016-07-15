using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace q2
{
    public class QCommand
    {
        public QConext Context;
        public string Command;

        public QCommand(QConext context)
        {
            Context = context;
        }

        public QCommand(string command, QConext context)
        {
            Context = context;
            Command = command;
        }

        public async Task ExecuteNonQuery()
        {
            HttpWebRequest request = Context.CreateExecuteNonQuery(Command);
            await Context.UpdateRequest(request);            
        }

        public T ExecuteScalar<T>()
        {
            return (T)new object();
        }

        public async Task<string> ExecuteReader()
        {
            HttpWebRequest request = Context.CreateExecuteReader(Command);
            return await Context.SelectRequest(request);            
        }
    }
}
