using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto.RL
{
    public class RLOne : AutoBase
    {
        public RLOne(int i) : base(i) { }

        public override Dictionary<Direction, float> GetMoveWeights()
        {
            throw new NotImplementedException();
        }
    }
}
