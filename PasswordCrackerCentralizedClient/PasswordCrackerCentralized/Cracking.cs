using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using PasswordCrackerCentralized.model;
using PasswordCrackerCentralized.util;
using System;
using System.Collections.Generic;
using System.IO;

namespace PasswordCrackerCentralized
{
    public class Cracking
    {
        public readonly Queue<string> Lib = new Queue<string>();
        public Cracking()
        {
            //Smider orderbogen i en kø så den er nem at tilgå.
            using (var fs = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read))
            using (var dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {

                    String dictionaryEntry = dictionary.ReadLine();
                    Lib.Enqueue(dictionaryEntry);
                }
            }
        }

        //Opretter tasks til hver connection.
        public void RunCracking(List<TcpClient> tcpClients)
        {
            var tasks = new List<Task>();

            foreach (var tcpClient in tcpClients)
            {
                TcpClient client = tcpClient;
                tasks.Add(Task.Run(() => Send(client)));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void Send(TcpClient tcpClient)
        {
            var logging = new EventLog { Source = "Crack Client" };
            TcpClient clientSocket = tcpClient;
            Stream ns = clientSocket.GetStream();
            var sw = new StreamWriter(ns);
            var sr = new StreamReader(ns);
            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            var myResult = new List<string>();
            //Sender bruger listen til serveren.
            foreach (var ui in userInfos)
            {
                sw.WriteLine(ui);
            }
            sw.WriteLine("-1");
            sw.Flush();

            bool rdy = true;
            int numb = 1000;
            //Så længe at ordbogen ikke er tom skal den sende info afsted.
            while (Lib.Count > 0)
            {
                var stop = new Stopwatch();
                if (rdy)
                {
                    //Angiver hvor mange ord den skal sende afsted afgangen.
                    for (int j = 0; j < numb; j++)
                    {
                        if (Lib.Count > 0)
                        {
                            sw.WriteLine(Lib.Dequeue());
                        }
                    }
                    rdy = false;
                    sw.Flush();
                    stop.Start();
                    sw.WriteLine("-2");
                    sw.Flush();
                }
                string msg = sr.ReadLine();
                //Hvis den får beskeden Done tilbage skal den se hvor lang tid det tog og der efter tilføje 100 ord eller fjerne 100 ord fra den næste sending.
                //Dette gør intet for hastigheden men fordeler hvor mange gange der bliver sendt til de forskellige server.
                if (msg == "Done")
                {
                    rdy = true;
                    stop.Stop();
                    if (stop.Elapsed.Seconds >= 1 && numb != 100)
                    {
                        numb = numb - 100;
                    }
                    else
                    {
                        numb = numb + 100;
                    }
                }
                //Hvis det ikke er Done den får tilbage skal den udskrive det da det i dette tilfælde vil være et password der er blevet cracket.
                if (msg != "Done")
                {
                    if (msg != null)
                    {
                        string[] splited = msg.Split(':');
                        logging.WriteEntry("Password found for " + splited[0] + ". \nPassword is: "+splited[1], EventLogEntryType.SuccessAudit, 5);
                        myResult.Add(splited[0]);
                    }
                    Console.WriteLine(msg);
                }

            }
            foreach (var userInfo in userInfos)
            {
                //Udskriver liste over password der ikke blev fundet.
                if (!myResult.Contains(userInfo.Username))
                {
                    Console.WriteLine("Password for {0} was not found!", userInfo.Username);
                    logging.WriteEntry("Password for" + userInfo.Username + "was not found!", EventLogEntryType.FailureAudit, 7);
                }
            }
            sw.Close();
            logging.WriteEntry("Connection closed", EventLogEntryType.Information, 6);
            ns.Close();
        }
    }
}
