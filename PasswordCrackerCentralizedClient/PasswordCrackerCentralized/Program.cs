using System.Threading.Tasks;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            Cracking cracker = new Cracking();
            Parallel.Invoke(cracker.RunCracking);
        }
    }
}
