using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class PerfTest
    {
        static void Run()
        {
            int tries = 100000;

            var squares = new int[4, 4];
            squares[2, 1] = 2;
            squares[3, 0] = 1;
            var start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                var es = squares.GetRowWithoutZeros(2);
                var t = es.Count();
            }
            var end = DateTime.Now;
            var duration = end - start;
            Console.WriteLine(duration.ToString());

            start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                //var es = squares.EnumerateOutcomes3(new Move(Direction.Up), .11f);
                //var t = es.Count();
            }
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(duration.ToString());

            start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                var es = squares.GetRowWithoutZeros(2);
                var t = es.Count();
            }
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(duration.ToString());
        }
    }
}
