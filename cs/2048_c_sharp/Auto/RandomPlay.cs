using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048_c_sharp.Auto
{
    public class RandomPlay: AutoBase
    {
        public RandomPlay(int i) : base(i) { }
        public override Dictionary<Direction, float> GetMoveWeights() => Board.State.PossibleDirections().ToDictionary(k => k, v =>  0f);
    }
}
