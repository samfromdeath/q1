using Nito.AsyncEx;
using q2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qTest
{
    class Program
    {
        static async void MainAsync(string[] args)
        {
            QConext.Configure();
            var context = new QConext() { Address = "http://localhost:61388" };
            if (await context.Login("Test", "Test"))
            {
                QCommand cmd = new QCommand("SELECT * FROM main.Sales;", context);

                await cmd.ExecuteNonQuery();

                Console.WriteLine(await cmd.ExecuteReader());
            }
        }

        static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
            
        }
    }
}
