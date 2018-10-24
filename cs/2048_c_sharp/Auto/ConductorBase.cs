using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static _2048_c_sharp.Auto.Logger;

namespace _2048_c_sharp.Auto
{
    public abstract class ConductorBase<T> where T: AutoBase
    {
        static readonly string EX_MSG = $"Press ESC to stop.";
        public Board BestBoard { get; set; }
        private List<Task> tasks { get; set; }
        const int MAX_CONCURRENT_BOARDS = 8;
        const int SLEEP_LENGTH = 100; //in ms

        public void Run(bool async = false)
        {
            int it_cnt = 1;
            tasks = Enumerable.Range(0, MAX_CONCURRENT_BOARDS)
                              .Select(i => Task.Run(() => { }))
                              .ToList(); //initialized as empty, completed tasks
            while (!(Console.KeyAvailable &&
                   Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                if (async)
                {
                    var removed = tasks.RemoveAll(t => t.IsCompleted);
                    if (removed > 0)
                        tasks.AddRange(Enumerable.Range(it_cnt, removed)
                                                 .Select(i => Task.Run(() => {
                                                     var iter = (T)Activator.CreateInstance(typeof(T), i);
                                                     iter.Run();
                                                     UpdateBestBoard(iter);
                                                     UpdateTrainingDB(iter); //this may not be moved to a sychronous thread
                                                 })));
                    it_cnt += removed;
                }
                else
                {
                    UpdateBestBoard((T)Activator.CreateInstance(typeof(T), it_cnt++));
                }

                System.Threading.Thread.Sleep(SLEEP_LENGTH);

            }

            log("Finishing remaining tasks...", Priority.Highest_1);
            Task.WaitAll(tasks.ToArray());

            log("Finishing remaining tasks...", Priority.Highest_1);
            log("Done.", Priority.Highest_1);
        }

        public abstract void UpdateTrainingDB(T finishedIteration);

        private void UpdateBestBoard(T board)
        {
            if (board.Board.Score >= BestBoard.Score) return;
            Console.WriteLine(Environment.NewLine);
            var lines = new[] {
                        $"Iteration: {board.Iteration}: {board.ToString()} @ {DateTime.Now}",
                        board.Board.Field.AsString()
                    };
            
            Console.WriteLine(string.Join(Environment.NewLine, lines));
            BestBoard = board.Board;

        }
    }
}
