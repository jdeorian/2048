using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace _2048_c_sharp
{
    public class Square
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }
        public float Chance { get; set; }

        public Square(int x, int y, int val, float chance = 1f)
        {
            X = x;
            Y = y;
            Value = val;
            Chance = chance;
        }
    }
}
