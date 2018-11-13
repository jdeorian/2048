using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Move
    {
        public Direction Direction { get; set; }
        public ulong StartState { get; private set; }
        public ulong EndState { get; private set; }

        public override int GetHashCode() => EndState.GetHashCode();

        public Move(ulong startstate, Direction direction)
        {
            StartState = startstate;
            EndState = startstate.Slide(direction);
        }

        public bool ChangedBoard() => StartState != EndState;
    }


}
