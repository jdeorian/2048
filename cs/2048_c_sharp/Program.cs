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

            //PolicyWork();

            var rnd = new Random(); var fld = ULongMagic.LongRandom(rnd); fld &= 0x0F0FF00F0FF0FF00FUL;
            var r = fld.GetExpectedRewards(3);

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
