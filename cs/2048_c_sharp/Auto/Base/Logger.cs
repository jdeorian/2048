using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    public static class Logger
    {
        public static Priority DebugLevel { get; set; } = Priority.Med_High_3; //print anything with this priority or higher
        public static bool PrintOutput(Priority priority) => (int)DebugLevel >= (int)priority;

        public static void Output(string text, Priority debugLevel = Priority.Medium_5)
        {
            if (PrintOutput(debugLevel))
                OutputDirect(text);

            //TODO: put in some real logging functionality
        }

        private static void OutputDirect(string text) => Console.WriteLine(text);

        public static void log(string text) => Output(text);
        public static void log(string text, Priority priority) => Output(text, priority);
        //private static void log_direct(string text) => OutputDirect(text);
        //private static void log_direct(string text, Priority priority) => OutputDirect(text, priority);
    }

    public enum Priority
    {
        Critical_0 = 0,
        Highest_1,
        P2,         //things that happen on exceptional board iterations
        Med_High_3, //things that happen every board iteration
        P4,         //thing that happen on exceptional moves
        Medium_5,   //things that happen about every (decided) move
        P6,         //things that happen on exceptional internal moves
        Med_Low_7,  //things that happen every internal board move
        P8,
        Low_9
    }
}
