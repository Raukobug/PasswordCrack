using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            List<TcpClient> tcpClients = new List<TcpClient>();
            Console.Title = "Jeg er fucking Client";
            bool startup = true;
            while (startup) { 
            string call = Console.ReadLine();
                if (call == "start")
                {
                    startup = false;
                }
                else 
                {
                    try
                    {
                        string[] split = call.Split(':');
                        tcpClients.Add(new TcpClient(split[0], Convert.ToInt32(split[1])));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not find server.");
                    }

                }
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Task> tasks = new List<Task>();
            Cracking cracker = new Cracking();
            tasks.Add(Task.Run(() => cracker.RunCracking(tcpClients)));
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }
    }
}
