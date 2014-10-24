using System;
using System.Threading.Tasks;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            Console.Title = "6789";
            Cracking cracker = new Cracking();
            Parallel.Invoke(cracker.RunCracking);
        }
    }
}
