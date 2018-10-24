using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace _2048_c_sharp.Auto.RL
{
    /// <summary>
    /// This is base class 
    /// </summary>
    public class RLBase: AutoBase
    {
        static readonly string EX_MSG = $"Press ESC to stop.";
        static Board bestBoard = new Board();
        //static int highestIt = 0;
        const int MAX_BOARDS = 8;
        const int SLEEP_LENGTH = 100; //in ms
        static SQLiteConnection _conn = null;

        public RLBase(int iteration) : base(iteration) { }

        const string db_file = "branch.sqlite";

        public override Dictionary<Direction, float> GetMoveWeights() => XT.EnumVals<Direction>().ToDictionary(k => k, k => 0f);

        const string DB_FILE = "training.sqlite";

        private static void InitializeDB()
        {
            SQLiteConnection.CreateFile(DB_FILE);
            var connection = GetDBConnection();

            //create table
            string sql = "CREATE TABLE IF NOT EXISTS training_raw (Id INTEGER, Decision VARCHAR(4), Score REAL)";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        private static SQLiteConnection GetDBConnection()
        {
            var connection = new SQLiteConnection($"Data Source={DB_FILE};Version=3;");
            connection.Open();
            return connection;
        }

        private static void InsertRawTrainingRecord(SQLiteConnection conn, long id, Direction decision, float score)
        {
            var sql = $"INSERT INTO training_raw (Id, Decision, Score) values ({id}, '{decision}', {score})";
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        private static void InsertBoardMoveHistoryRawTrainingRecords(SQLiteConnection conn, Board board)
        {
            var tr = conn.BeginTransaction();
            foreach (var m in board.MoveHistory)
            {
                var command = conn.CreateCommand();
                command.CommandText = $"INSERT INTO training_raw (Id, Decision, Score) values ({m.StartState.CanonicalFieldID()}, '{m.Direction}', {board.Score})";
                command.ExecuteNonQuery();
            }
            try
            {
                tr.Commit();
            }
            catch (Exception)
            {
                Console.WriteLine("Error writing training data, rolled back");
                tr.Rollback();
            }
            finally
            {
                tr.Dispose();
            }
            
        }
    }
}
