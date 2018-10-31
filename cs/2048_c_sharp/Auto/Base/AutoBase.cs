using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static _2048_c_sharp.Auto.Logger;

namespace _2048_c_sharp.Auto
{
    public abstract class AutoBase
    {
        const int MIN_TRAINING_COUNT = 10;
        const int MAX_BOARDS = 8;

        public float DiscountRate { get; set; } = .9f;

        public int Iteration { get; set; } = 0;
        public Board Board { get; set; } = new Board();
        public Random rnd { get; set; } = new Random();

        private DBTraining db { get; set; }

        public AutoBase(DBTraining trainingDB, int iteration) { Iteration = iteration; db = trainingDB; }

        public void Run()
        {            
            while (!Board.Lost)
            {
                PrintBoardState();

                //get the recommended direction algorithmically
                var moveWeights = GetMoveWeights();               

                //get the database data
                var pastResults = db.TrainingRecords.FirstOrDefault(t => t.Id == Board.Field.CanonicalFieldID());
                var recDir = PickMoveDirection(pastResults, moveWeights);

                Board.Move(recDir);
                PrintMoveResults(moveWeights, recDir);
            }
            return;
        }

        public abstract Dictionary<Direction, float> GetMoveWeights();

        private Direction PickMoveDirection(Training td, Dictionary<Direction, float> moveWeights)
        {
            //get the recommended direction for whatever algorithm is in use
            var highestWeight = moveWeights.Values.Max();
            var highestDirs = moveWeights.Where(kvp => kvp.Value == highestWeight)
                                         .Select(kvp => kvp.Key);
            var recDir = highestDirs.Count() == 1 ? highestDirs.First()
                                                  : XT.GetRandom(XT.EnumVals<Direction>().ToArray(), rnd);

            //if no training data is available, we need to get some.
            if (td == null) return recDir;
            var weightData = td.GetWeightData;
            if (weightData.Any(wd => wd.Value.Count < MIN_TRAINING_COUNT))
            {
                return weightData.FirstOrDefault(wd => wd.Value.Count < MIN_TRAINING_COUNT).Key;
            }

            //if there is training data, return the best value
            return weightData.GetRecDirection();
        }        

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

        public void PrintTrainingData(Training td)
        {
            log("Training data:");
            PrintWeights(td.GetWeights);
        }

        public void PrintPreMoveState()
        {
            log($"---------- IT: {Iteration} -------------");
            PrintBoardState();
        }

        public void PrintMoveResults(Dictionary<Direction, float> weights, Direction direction)
        {
            PrintWeights(weights);
            log($"Selected direction: {direction}");
            PrintBoardState();            
        }

        public void PrintWeights(Dictionary<Direction, float> weights)
        {
            log($"Weights {string.Join(" ", weights.Select(w => $"{w.Key}:{w.Value}"))}");
        }

        public void PrintBoardState()
        {
            log(Board.Field.AsString());
            log($"Score: {Board.Field.Score()}");
        }


    }
}
