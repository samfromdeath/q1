using Nito.AsyncEx;
using q2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (await context.Login("samuel123", "test"))
            {
                while(true)
                {
                    QCommand cmd = new QCommand(Console.ReadLine(), context);
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        byte[] data = await cmd.ExecuteReader();
                        sw.Stop();
                        string[] sizes = { "B", "KB", "MB", "GB" };
                        double len = data.LongLength;
                        int order = 0;
                        while (len >= 1024 && ++order < sizes.Length)
                        {
                            len = len / 1024;
                        }

                        // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                        // show a single decimal place, and no space.
                        string result = string.Format("{0:0.##} {1}", len, sizes[order]);

                        Console.WriteLine("Size: " + result);
                        Console.WriteLine("Elapsed ms: " + sw.ElapsedMilliseconds);
                        Console.WriteLine("Would you like to print data? (y, n)");
                        string input = Console.ReadLine().ToLower();
                        if (input == "y")
                            Console.WriteLine(Encoding.UTF8.GetString(data));
                    }
                    catch (Exception)
                    {
                        
                    }
                                        
                }
                
            }
        }

        static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
            
        }
    }
}
