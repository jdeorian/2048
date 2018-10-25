using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace _2048_c_sharp.Auto
{
    /// <summary>
    /// This is base class 
    /// </summary>
    public abstract class RLBase: AutoBase
    {
        private Board bestBoard = new Board();
        private SQLiteConnection _conn = null;
        private RLTrainingSettings RLTrainingSettings = null;

        public RLBase(int iteration) : base(iteration)
        {
            RLTrainingSettings = GetTrainingSettings();
        }

        /////////// abstract methods //////////////
        public abstract RLTrainingSettings GetTrainingSettings();
        public abstract string GetInsertRecordSQL(Move move, float finalScore);
        ///////////////////////////////////////////

        public override Dictionary<Direction, float> GetMoveWeights() => XT.EnumVals<Direction>().ToDictionary(k => k, k => 0f);

        private void InitializeDB()
        {
            SQLiteConnection.CreateFile(RLTrainingSettings.DBFilename);
            var connection = GetDBConnection();

            //create table
            string sql = RLTrainingSettings.DBInitializationSQL; //"CREATE TABLE IF NOT EXISTS training_raw (Id INTEGER, Decision VARCHAR(4), Score REAL)";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        private SQLiteConnection GetDBConnection()
        {
            var connection = new SQLiteConnection(RLTrainingSettings.ConnectionString);
            connection.Open();
            return connection;
        }        

        private void InsertRawTrainingRecord(SQLiteConnection conn, Move move, float finalScore)
        {
            var sql = GetInsertRecordSQL(move, finalScore); // $"INSERT INTO training_raw (Id, Decision, Score) values ({id}, '{decision}', {score})";
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        private void InsertBoardMoveHistoryRawTrainingRecords(SQLiteConnection conn, Board board)
        {
            var tr = conn.BeginTransaction();
            foreach (var m in board.MoveHistory)
            {
                var command = conn.CreateCommand();
                command.CommandText = GetInsertRecordSQL(m, board.Score); // $"INSERT INTO training_raw (Id, Decision, Score) values ({m.StartState.CanonicalFieldID()}, '{m.Direction}', {board.Score})";
                command.ExecuteNonQuery();
            }

            //commit
            try { tr.Commit(); }
            catch (Exception) {
                Console.WriteLine("Error writing training data, rolled back");
                tr.Rollback();
            }
            finally { tr.Dispose(); }            
        }
    }
}
