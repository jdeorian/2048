using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class PerfTest
    {
        static readonly int tries = 1_000_000;
        public static void Run()
        {
            ///////////////////////////
            ///     Setup 1
            //////////////////////////
            var board = new Board();
            var root = new Move(Direction.Up, null, board) { EndState = board.Field };
            byte[,] sq = new byte[4, 4];
            sq[3, 1] = 2;
            sq[2, 1] = 6;
            sq[0, 0] = 9;
            //////////////////////////

            Console.WriteLine("Starting test 1...");
            var start = DateTime.Now;
            for (int x = 0; x < tries; x++)
            {
                ///////////////////////////
                ///     Test 1
                //////////////////////////
                var o = sq.CanonicalFieldID();
                o++;
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
                var o = sq.GetEmptySquares_Slow();
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
                var o = sq.GetEmptySquares();
                /////////////////////////
            }
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(duration.ToString());
        }
    }
}
