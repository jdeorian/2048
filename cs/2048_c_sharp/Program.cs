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

            var c = new Conductor<RLOne>(new DBTraining());
            c.Run();

            Console.ReadKey();
        }        
    }
}
