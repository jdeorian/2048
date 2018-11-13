using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        const float RANDOMNESS = .005f; //some percent of the time, a random move will be chosen just to enrich the policy

        public float DiscountRate { get; set; } = .9f;

        public int Iteration { get; set; } = 0;
        public Board Board { get; set; } = new Board();
        public Random rnd { get; set; } = new Random();
        public DateTime TimeStarted { get; private set; }
        public DateTime TimeEnded { get; private set; }

        public int PolicyMoves { get; private set; } = 0;
        public int MethodMoves { get; private set; } = 0;
        public int RandomMoves { get; private set; } = 0;

        public AutoBase(int iteration) { Iteration = iteration; }

        public void Run()
        {
            TimeStarted = DateTime.Now;
            while (!Board.Lost)
            {
                PrintBoardState();
                var recDir = PickMoveDirection(out var moveWeights);
                Board.Move(recDir, moveWeights);
                PrintMoveResults(moveWeights, recDir);                          
            }
            TimeEnded = DateTime.Now;
            return;
        }

        public abstract Dictionary<Direction, float> GetMoveWeights();

        private Direction PickMoveDirection(out Dictionary<Direction, float> moveWeights)
        {
            //check for a forced random value
            var forceRandom = rnd.NextDouble() <= RANDOMNESS;
            if (forceRandom)
            {
                var options = Board.Field.PossibleMoves().ToArray();
                var d = XT.GetRandom(options, rnd);
                moveWeights = options.ToDictionary(k => k, v => 0f);
                RandomMoves++;
                return d;
            }

            //get the database/policy value
            var canID = Board.Field;
            var policy = PolicyData.GetPolicy(canID)?.Result;
            if (policy != null)
            {
                moveWeights = policy.GetWeights();
                PolicyMoves++;
                return GetRecommendation(moveWeights);
            }

            //It wasn't sufficient, so use the algorithm
            moveWeights = GetMoveWeights();
            var recDir = GetRecommendation(moveWeights);
            MethodMoves++;
            return recDir;
        }

        private Direction GetRecommendation(Dictionary<Direction, float> moveWeights)
        {

            var highestWeight = moveWeights.Values.Max();
            var highestDirs = moveWeights.Where(kvp => kvp.Value == highestWeight)
                                         .Select(kvp => kvp.Key).ToArray();
            var recDir = highestDirs.Count() == 1 ? highestDirs.First()
                                                  : highestDirs.GetRandom(rnd);
            return recDir;
        }

        public Dictionary<Move, float> GetSumOfRewards()
        {
            var count = Board.MoveHistory.Count();
            var rewards = new float[count];
            for (var reward_move_index = 0; reward_move_index < count; reward_move_index++)
            {
                var reward = Board.MoveHistory[reward_move_index].Reward;
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
            PrintWeights(td.GetWeights());
        }

        public void PrintPreMoveState()
        {
            log($"---------- IT: {Iteration} -------------");
            PrintBoardState();
        }

        public void PrintMoveResults(Dictionary<Direction, float> weights, Direction direction)
        {
            return; //TODO: make sure these don't execute if they're won't be printed
            PrintWeights(weights);
            log($"Selected direction: {direction}");
            PrintBoardState();            
        }

        public void PrintWeights(Dictionary<Direction, float> weights)
        {
            return; //TODO: make sure these don't execute if they're won't be printed
            log($"Weights {string.Join(" ", weights.Select(w => $"{w.Key}:{w.Value}"))}");
        }

        public void PrintBoardState()
        {
            return; //TODO: make sure these don't execute if they're won't be printed
            log(Board.Field.AsBoardString());
            log($"Score: {Board.Field.Score()}");
        }

        public IterationStatus GetStatus() => new IterationStatus() {
            Iteration = Iteration,
            MoveCount = Board.MoveHistory.Count(),
            TimeStarted = TimeStarted,
            TimeEnded = TimeEnded,
            Score = Board.Score,
            PolicyMoves = PolicyMoves,
            MethodMoves = MethodMoves,
            RandomMoves = RandomMoves
        };
    }
}
