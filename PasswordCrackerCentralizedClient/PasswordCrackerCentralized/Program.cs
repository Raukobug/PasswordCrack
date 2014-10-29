using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PasswordCrackerCentralized
{
    public class Program
    {
        static void Main()
        {
            var logging = new EventLog { Source = "Crack Client" };
            var tcpClients = new List<TcpClient>();
            Console.Title = "Client";
            bool startup = true;
            Console.WriteLine("Type in ip and port: xxx.xxx.xxx.xxx:xxxx");
            Console.WriteLine("Type start when ready.");
            while (startup)
            {
                string call = Console.ReadLine();
                if (call == "start")
                {
                    startup = false;
                    logging.WriteEntry("Cracking begun", EventLogEntryType.Information, 3);
                }
                else
                {
                    if (call == "1")
                    {
                        call = "10.154.1.207:65080";
                    }
                    if (call == "2")
                    {
                        call = "10.154.1.162:65080";
                    }
                    if (call == "3")
                    {
                        call = "10.154.2.61:65080";
                    }
                    if (call == "4")
                    {
                        call = "localhost:65080";
                    }
                    try
                    {
                        if (call != null)
                        {
                            string[] split = call.Split(':');
                            tcpClients.Add(new TcpClient(split[0], Convert.ToInt32(split[1])));
                            logging.WriteEntry("Connecet to " + call, EventLogEntryType.Information, 1);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not find server.");
                        logging.WriteEntry("Could not connecet to " + call, EventLogEntryType.FailureAudit, 2);
                    }

                }
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            var cracker = new Cracking();
            tasks.Add(Task.Run(() => cracker.RunCracking(tcpClients)));
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
            logging.WriteEntry("Cracking done in " + stopwatch.Elapsed, EventLogEntryType.Information, 4);
        }
    }
}
