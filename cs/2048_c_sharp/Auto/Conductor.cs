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

        public RLTrainingSettings RLTSettings { get; set; }

        private SQLiteConnection _conn = null;
        private SQLiteConnection Conn
        {
            get
            {
                if (_conn == null)
                {
                    _conn = new SQLiteConnection(RLTSettings.ConnectionString);
                    _conn.Open(); //automatically creates file if it doesn't exist
                    InitializeDB(_conn); //creates tables if they don't exist
                }                
                return _conn;
            }
        }
        
        public Conductor(RLTrainingSettings trainingSettings)
        {
            RLTSettings = trainingSettings;
        }

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

        #region Update Training DB

        private void InitializeDB(SQLiteConnection connection)
        {
            //create table
            string sql = RLTSettings.DBInitializationSQL; //"CREATE TABLE IF NOT EXISTS training_raw (Id INTEGER, Decision VARCHAR(4), Score REAL)";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private void InsertRawTrainingRecord(Move move, float finalScore)
        {
            var sql = RLTSettings.GetInsertRecordSQL(move, finalScore); // $"INSERT INTO training_raw (Id, Decision, Score) values ({id}, '{decision}', {score})";
            var command = new SQLiteCommand(sql, Conn);
            command.ExecuteNonQuery();
        }

        private void UpdateTrainingDB(T iteration)
        {
            var tr = Conn.BeginTransaction();
            foreach (var m in iteration.Board.MoveHistory)
            {
                var command = Conn.CreateCommand();
                command.CommandText = RLTSettings.GetInsertRecordSQL(m, iteration.Board.Score); // $"INSERT INTO training_raw (Id, Decision, Score) values ({m.StartState.CanonicalFieldID()}, '{m.Direction}', {board.Score})";
                command.ExecuteNonQuery();
            }

            //commit
            try { tr.Commit(); }
            catch (Exception)
            {
                Console.WriteLine("Error writing training data, rolled back");
                tr.Rollback();
            }
            finally { tr.Dispose(); }
        }

        #endregion
    }
}
