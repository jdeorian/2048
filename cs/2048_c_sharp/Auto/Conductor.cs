using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

using static _2048_c_sharp.Auto.Logger;

namespace _2048_c_sharp.Auto
{
    public class Conductor<T> where T : AutoBase
    {
        static readonly string EX_MSG = $"Press ESC to stop.";
        public Board BestBoard { get; set; }
        private List<Task> tasks { get; set; }
        const int MAX_CONCURRENT_BOARDS = 8;
        const int SLEEP_LENGTH = 100; //in ms

        public DBTraining db { get; set; } = new DBTraining();
        
        public Conductor(DBTraining trainingDB) { }

        public void Run(bool async = false)
        {

            int it_cnt = 1;
            int boards = async ? MAX_CONCURRENT_BOARDS : 1;
            tasks = Enumerable.Range(0, boards)
                              .Select(i => Task.Run(() => { }))
                              .ToList(); //initialized as empty, completed tasks
            while (!(Console.KeyAvailable &&
                   Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                var removed = tasks.RemoveAll(t => t.IsCompleted);
                if (removed > 0)
                    tasks.AddRange(Enumerable.Range(it_cnt, removed)
                                                .Select(i => Task.Run(() => {
                                                    var iter = (T)Activator.CreateInstance(typeof(T), db, i);
                                                    iter.Run();
                                                    UpdateBestBoard(iter);
                                                    UpdateTrainingDB(iter); //this may not be moved to a sychronous thread
                                                })));
                it_cnt += removed;
                System.Threading.Thread.Sleep(SLEEP_LENGTH);
            }

            log("Finishing remaining tasks...", Priority.Highest_1);
            Task.WaitAll(tasks.ToArray());
            log("Done.", Priority.Highest_1);
        }

        private void UpdateBestBoard(T board)
        {
            if (board.Board.Score <= (BestBoard?.Score ?? 0)) return;
            var lines = new[] {
                        string.Empty,
                        $"Iteration: {board.Iteration}: {board.ToString()} @ {DateTime.Now}",
                        board.Board.Field.AsString()
                    };
            
            log(string.Join(Environment.NewLine, lines), Priority.P2);
            BestBoard = board.Board;
        }

        private void UpdateTrainingDB(T iteration)
        {
            lock (db)
            {                
                db.Update(iteration.GetSumOfRewards()
                                   .Select(kvp => (kvp.Key.StartState.CanonicalFieldID(),
                                                   kvp.Key.Direction,
                                                   kvp.Value)));
            }
        }

        public int CountIterations()
        {
            var PAGE_SIZE = 1000;
            var pages = 0;
            var count = 0;
            IEnumerable<Training> records;
            while ((records = db.TrainingRecords.Skip(PAGE_SIZE*pages++).Take(PAGE_SIZE)).Any())
                count += records.Sum(r => r.TotalCount);

            return count;
        }
    }
}
