using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048_c_sharp.Auto
{
    public class RandomPlay: AutoBase
    {
        public RandomPlay(DBTraining training, int i) : base(training, i) { }
        public override Dictionary<Direction, float> GetMoveWeights() => XT.EnumVals<Direction>().ToDictionary(k => k, k => 0f);
    }
}
