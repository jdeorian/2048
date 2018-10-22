using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class BranchComparison
    {
        static readonly string EX_MSG = $"Press ESC to stop.";
        static Board bestBoard = new Board();
        //static int highestIt = 0;
        const int MAX_BOARDS = 8;
        const int SLEEP_LENGTH = 100; //in ms

        static void UpdateBestBoard(Board board)
        {
            //if (board.Iteration > highestIt)
            //{
            //    highestIt = board.Iteration;
            //    Console.Write($"It: {highestIt}");
            //}

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

        const string db_file = "branch.sqlite";

        public static void RunBranchComparisons(bool async = false)
        {
            Console.WriteLine();
            int it_cnt = 1;
            var tasks = new List<Task>();
            while (!(Console.KeyAvailable &&
                   Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                if (async)
                {
                    tasks = tasks.Where(t => !t.IsCompleted).ToList();
                    var newTaskCount = MAX_BOARDS - tasks.Count();
                    tasks.AddRange(Enumerable.Range(it_cnt, newTaskCount).Select(i => Task.Run(() => UpdateBestBoard(Run(i)))));
                    it_cnt += newTaskCount;
                }
                else
                {
                    UpdateBestBoard(Run(it_cnt++));
                }

                System.Threading.Thread.Sleep(SLEEP_LENGTH);

            }

            Console.WriteLine("Finishing remaining tasks...");
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("Press any key to exit.");
        }

        static readonly Dictionary<int, int> layerDict = new Dictionary<int, int> {
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

        private static Board Run(int iteration)
        {
            Console.WriteLine($"Starting iteration #{iteration} @ {DateTime.Now}");
            var board = new Board()
            {
                Iteration = iteration
            };
            var rnd = new Random();

            while (!board.Lost)
            {
                Console.WriteLine("----------------------------");
                Console.WriteLine(board.Field.AsString());
                int layerCount = layerDict[board.Field.MaxValue()];
                //Console.WriteLine($"Layer count: {layerCount}");

                var root = new Move(Direction.Up, null, board) { EndState = board.Field };
                var outcomes = root.BuildBranches(layerCount);

                //next layer is the final set of children. Get the reward weights for all directions
                var weights = outcomes.GroupBy(m => m.RewardDirection)
                                      .ToDictionary(g => g.Key,
                                                    g => g.Average(m => m.SumOfRewards));

                var topScore = weights.Max(d => d.Value);
                var topScorers = weights.Where(d => d.Value == topScore)
                                        .Select(d => d.Key)
                                        .ToArray();
                var recDir = XT.GetRandom(topScorers, rnd);

                Console.WriteLine($"Rec. Dir.: {recDir}");
                Console.WriteLine(string.Join(" ", weights.Select(w => $"{w.Key}:{w.Value}")));
                board.Move(recDir, weights);
                root = root.Children.Where(m => m.Direction == recDir)
                                    .Single(m => m.EndState.IsEqualTo(board.Field));
                root.Root = root;
                root.Parent = board.LastMove.Parent;
            }
            //Console.WriteLine("Board done.");
            //Console.WriteLine(board.Field.AsString());
            //Console.WriteLine($"Score: {board.Field.Score()}");
            return board;
        }
    }
}
