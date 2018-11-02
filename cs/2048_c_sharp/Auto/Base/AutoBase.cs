﻿using System;
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
        public DateTime TimeStarted { get; private set; }
        public DateTime TimeEnded { get; private set; }

        private DBTraining db { get; set; }

        public AutoBase(DBTraining trainingDB, int iteration) { Iteration = iteration; db = trainingDB; }

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
            //get the database value
            Training td = null;
            lock (db) { td = db.TrainingRecords.FirstOrDefault(t => t.Id == Board.Field.CanonicalFieldID()); }
            
            if (td?.DecisionSufficient() ?? false)
            {
                moveWeights = td.GetWeights();
                log("Holy shit using a policy weight!", Priority.Medium_5);
                return GetRecommendation(moveWeights);
            }

            //It wasn't sufficient, so use the algorithm
            moveWeights = GetMoveWeights();
            var recDir = GetRecommendation(moveWeights);
            return recDir;
        }

        private Direction GetRecommendation(Dictionary<Direction, float> moveWeights)
        {

            var highestWeight = moveWeights.Values.Max();
            var highestDirs = moveWeights.Where(kvp => kvp.Value == highestWeight)
                                         .Select(kvp => kvp.Key);
            var recDir = highestDirs.Count() == 1 ? highestDirs.First()
                                                  : XT.GetRandom(XT.EnumVals<Direction>().ToArray(), rnd);
            return recDir;
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
            PrintWeights(td.GetWeights());
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

        public IterationStatus GetStatus() => new IterationStatus() {
            Iteration = Iteration,
            MoveCount = Board.MoveHistory.Count(),
            TimeStarted = TimeStarted,
            TimeEnded = TimeEnded,
            Score = Board.Score
        };
    }

    public struct IterationStatus
    {
        public int Iteration { get; set; }
        public int MoveCount { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public float Score { get; set; }
    }
}
