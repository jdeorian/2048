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
        public int Iteration { get; set; } = 0;
        public Board Board { get; set; } = new Board();
        public Random rnd { get; set; } = new Random();

        public AutoBase(int iteration) { Iteration = iteration; }

        public void Run()
        {
            
            while (!Board.Lost)
            {
                PrintBoardState();
                var moveWeights = GetMoveWeights();
                var highestWeight = moveWeights.Values.Max();
                var highestDirs = moveWeights.Where(kvp => kvp.Value == highestWeight)
                                             .Select(kvp => kvp.Key);
                var recDir = highestDirs.Count() == 1 ? highestDirs.First()
                                                      : XT.GetRandom(XT.EnumVals<Direction>().ToArray(), rnd);                
                Board.Move(recDir);
                PrintMoveResults(moveWeights, recDir);
            }
            return;
        }

        public abstract Dictionary<Direction, float> GetMoveWeights();


        public void PrintPreMoveState()
        {
            log($"---------- IT: {Iteration} -------------");
            PrintBoardState();
        }

        public void PrintMoveResults(Dictionary<Direction, float> weights, Direction direction)
        {
            log($"Weights {string.Join(" ", weights.Select(w => $"{w.Key}:{w.Value}"))}");
            log($"Selected direction: {direction}");
            PrintBoardState();            
        }

        public void PrintBoardState()
        {
            log(Board.Field.AsString());
            log($"Score: {Board.Field.Score()}");
        }


    }
}
