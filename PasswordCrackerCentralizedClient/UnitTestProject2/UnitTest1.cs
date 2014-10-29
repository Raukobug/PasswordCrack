using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        static readonly List<TcpClient> TcpClients = new List<TcpClient>();
        [TestMethod]
        public void TestMethod1()
        {
            string line = Etellerandet("123.123.123.123:1234");
            Assert.AreEqual("Could not find server.",line);
        }
        private static string Etellerandet(string call)
        {
            string code;
            try
            {
                if (call != null)
                {
                    string[] split = call.Split(':');
                    TcpClients.Add(new TcpClient(split[0], Convert.ToInt32(split[1])));
                }
                code = "Connection done";
            }
            catch (Exception)
            {
                return "Could not find server.";
            }
            return code;
        }
    }


}
