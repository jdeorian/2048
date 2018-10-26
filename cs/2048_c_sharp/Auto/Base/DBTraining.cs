using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _2048_c_sharp.Utilities;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Common;

namespace _2048_c_sharp.Auto
{
    public class DBTraining : DataConnection
    {
        public DBTraining(): base("TrainingData")
        {
            //create table(s) if not present. TODO: if we have more, this can be abstracted to automatically create any applicable tables
            var sp = DataProvider.GetSchemaProvider();
            var schema = sp.GetSchema(this);
            if (!schema.Tables.Any(t => t.TableName == typeof(Training).GetTableName()))
            {
                this.CreateTable<Training>();
            }
        }

        public ITable<Training> TrainingRecords => GetTable<Training>();

        public IEnumerable<Training> GetExisting(IEnumerable<long> ids)
            => from t in TrainingRecords
               where ids.Contains(t.Id)
               select t;

        public void Update(IEnumerable<(Move, float)> data) => Update(data.Select(d => (d.Item1.CanonicalFieldId, d.Item1.Direction, d.Item2)));


        public void Update(IEnumerable<(long, Direction, float)> data)
        {
            var records = GetExisting(data.Select(d => d.Item1)).ToDictionary(k => k.Id, v => v);

            BeginTransaction();
            foreach (var (id, dir, reward) in data)
            {
                if (records.TryGetValue(id, out Training training))
                    training.Update(dir, reward);
                else
                    training = new Training(id, dir, reward));

                this.InsertOrReplace(training);
            }
            CommitTransaction();
        }
    }
}
