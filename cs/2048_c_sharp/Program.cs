using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncBranchComparisons();
            Console.ReadKey();
        }

        static string EX_MSG = $"Press ESC to stop.";
        static Board bestBoard = new Board();
        static int highestIt = 0;
        const int MAX_BOARDS = 8;
        const int SLEEP_LENGTH = 100; //in ms

        static void AsyncBranchComparisons()
        {
            var tasks = new List<Task>();
            int it_cnt = 0;
            while (!(Console.KeyAvailable &&
                   Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                tasks = tasks.Where(t => !t.IsCompleted).ToList();
                var newTaskCount = MAX_BOARDS - tasks.Count();
                tasks.AddRange(Enumerable.Range(1, newTaskCount).Select(i => Task.Run(() => UpdateBestBoard(BranchComparison(it_cnt + i)))));
                it_cnt += newTaskCount;

                //instead of sleeping, we just use this as a separate instance
                UpdateBestBoard(BranchComparison(++it_cnt));
            }
        }

        static void UpdateBestBoard(Board board)
        {
            if (board.Iteration > highestIt)
            {
                highestIt = board.Iteration;
                Console.Write($"It: {highestIt}");
            }

            if (board.Field.Score() > bestBoard.Field.Score())
            {
                Console.WriteLine(Environment.NewLine);
                var lines = new[] {
                        $"Iteration: {board.Iteration}: {board.ToString()} @ {DateTime.Now.ToString()}",
                        board.Field.AsString(),
                        EX_MSG
                    };
                bestBoard = board;
                Console.WriteLine(string.Join(Environment.NewLine, lines));
            }
        }

        static void RunBranchComparisons()
        {            
            int i = 0;            
            while (!(Console.KeyAvailable &&
                   Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                UpdateBestBoard(BranchComparison(++i));
            }

            Console.WriteLine("Press any key to exit.");
        }

        static Dictionary<int, int> layerDict = new Dictionary<int, int> {
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

        private static Board BranchComparison(int iteration)
        {
            var board = new Board() {
                Iteration = iteration
            };
            var rnd = new Random();

            while (!board.Lost)
            {
                //Console.WriteLine(board.ToString());
                //Console.WriteLine(board.Field.AsString());
                int layerCount = layerDict[board.Field.Flatten().Max()];
                //Console.WriteLine($"Layer count: {layerCount}");

                var currentLayer = new List<Move>();
                var root = new Move(Direction.Up)
                {
                    Board = board,
                    EndState = board.Field.AsCopy()
                };
                var nextLayer = new List<Move> { root };
                for (int layer = 0; layer < layerCount; layer++)
                {
                    currentLayer = nextLayer;
                    nextLayer = new List<Move>();

                    //this triggers the children to be generated concurrently, useful for large numbers of children
                    var tasks = new List<Task>();
                    foreach (var m in currentLayer)
                        tasks.Add(Task.Run(() => m.GetChildren()));
                    Task.WaitAll(tasks.ToArray());

                    nextLayer.AddRange(currentLayer.SelectMany(m => m.Children));
                    if (!nextLayer.Any())
                    {
                        nextLayer = currentLayer;
                        break;
                    }
                }

                //next layer is the final set of children. Get the reward weights for all directions
                var weights = XT.EnumVals<Direction>().ToDictionary(key => key,
                                                                    dir => nextLayer.Where(m => m.RewardDirection == dir)
                                                                                    .Select(m => m.GetReward())
                                                                                    .DefaultIfEmpty(0) //prevent errors if there aren't any
                                                                                    .Average());
                var topScore = weights.Max(d => d.Value);
                var topScorers = weights.Where(d => d.Value == topScore)
                                        .Select(d => d.Key)
                                        .ToArray();
                var recDir = XT.GetRandom(topScorers, rnd);

                board.Move(recDir, weights);

                root = root.Children.Where(m => m.Direction == recDir)
                                    .Single(m => m.EndState.IsEqualTo(board.Field));
                root.Parent = null;
            }
            //Console.WriteLine("Board done.");
            //Console.WriteLine(board.Field.AsString());
            //Console.WriteLine($"Score: {board.Field.Score()}");
            return board;
        }
    }
}
