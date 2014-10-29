using System;
using System.Threading.Tasks;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Server";
            var cracker = new Cracking();
            Parallel.Invoke(cracker.RunCracking);
        }
    }
}
