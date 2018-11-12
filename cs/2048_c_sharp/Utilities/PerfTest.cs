using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _2048_c_sharp.Utilities;

namespace _2048_c_sharp
{
    public static class PerfTest
    {
        static readonly int tries = 100_000_000;
        public static void Run()
        {
            ///////////////////////////
            ///     Setup 1
            //////////////////////////
            Random rnd = new Random();
            var fieldArrays = Enumerable.Range(1, tries).Select(i => ArrayMagic.GetRandom(rnd)).ToArray();
            var fieldIDs = fieldArrays.Select(f => f.FieldID()).ToArray();
            //////////////////////////

            Console.WriteLine("Starting test 1...");
            var start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                ///////////////////////////
                ///     Test 1
                //////////////////////////
                //var fld = fieldArrays[x].Transpose();
                /////////////////////////
            }
            var end = DateTime.Now;
            var duration = end - start;
            Console.WriteLine(duration.ToString());

            ///////////////////////////
            ///     Setup 2
            //////////////////////////

            //////////////////////////
            Console.WriteLine("Starting test 2...");
            start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                ///////////////////////////
                ///     Test 2
                //////////////////////////
                var fld = fieldIDs[x].Transpose();
                /////////////////////////
            }
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(duration.ToString());

            ///////////////////////////
            ///     Setup 3
            //////////////////////////
            
            //////////////////////////

            Console.WriteLine("Starting test 3...");
            start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                ///////////////////////////
                ///     Test 3
                //////////////////////////
                var fld = fieldIDs[x].Transpose();
                /////////////////////////
            }
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(duration.ToString());
        }
    }
}
