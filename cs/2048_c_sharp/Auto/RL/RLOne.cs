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

        public float DiscountRate { get; set; } = .8f;

        public Dictionary<Move, float> GetSumOfRewards()
        {
            var count = Board.MoveHistory.Count();
            float[] rewards = new float[count];
            for (int reward_move_index = 0; reward_move_index < count; reward_move_index++)
            {
                float reward = Board.MoveHistory[reward_move_index].Reward;
                for (int apply_move_index = reward_move_index; apply_move_index >= 0; apply_move_index--) //going backward means I can multiply instead of using powers
                {
                    rewards[apply_move_index] += reward;
                    reward *= DiscountRate;
                }
            }
            return Enumerable.Range(0, count)
                             .ToDictionary(k => Board.MoveHistory[k], 
                                           v => rewards[v]);
        }


        ///Calculate a move's score something like this:
        ///moveCount = moves.Count
        ///rewards = moves.Select(m_i, m => Enumerable.Range(1, m_i+1).Select(r_i => 
        ///from last move to first move:
        ///
        ///
        ///Move.EndState.Reward - Move.StartState.Reward + (
    }
}
