using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    public class RLOne : AutoBase
    {
        public RLOne(DBTraining trainingDB, int i) : base(trainingDB, i) { }

        public override Dictionary<Direction, float> GetMoveWeights()
        {
            return XT.EnumVals<Direction>().ToDictionary(k => k, v => 0f);
        }


    }
}
