using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static _2048_c_sharp.Auto.Logger;

namespace _2048_c_sharp.Auto
{
    public class BranchComparison: AutoBase
    {
        public BranchComparison(int iteration) : base(iteration) { }

        private readonly Dictionary<int, int> layerDict = new Dictionary<int, int> {
                { 1, 1 }, // 2
                { 2, 1 }, // 4
                { 3, 2 }, // 8
                { 4, 2 }, // 16
                { 5, 2 }, // 32
                { 6, 2 }, // 64
                { 7, 3 }, // 128
                { 8, 3 }, // 256
                { 9, 3 }, // 512
                {10, 3 }, // 1024
                {11, 5 }, // 2048
                {12, 5 } // 4096
            };

        public override Dictionary<Direction, float> GetMoveWeights()
        {            
            int layerCount = layerDict[Board.State.MaxValue()];
            return Board.State.GetExpectedRewards(layerCount);
        }
    }
}
