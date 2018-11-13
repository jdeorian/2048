using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using _2048_c_sharp.Auto;

namespace _2048_c_sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //PerfTest.Run();

            PolicyWork();

            Console.ReadKey();
        }
        
        static void PolicyWork()
        {
            var c = new Conductor<BranchComparison>();
            //var c = new Conductor<RLOne>(db);
            Console.WriteLine($"Current records: {PolicyData.CountIterations()}");
            c.Run(8);
        }
    }
}
