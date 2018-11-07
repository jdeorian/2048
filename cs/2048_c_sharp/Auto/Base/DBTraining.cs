using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        //TODO: Either these shouldn't be static, or the whole class should be. Arguable these just shouldn't be static, but
        //      for now we actually want the policy IDs to be accessible from anywhere. Maybe it should be a different class?
        private static TimeSpan POLICY_CACHE_UPDATE_TIME = new TimeSpan(4, 0, 0);
        private DateTime _nextPolicyUpdateTime = DateTime.MinValue;

        public ImmutableSortedSet<ulong> PolicyIds { get; set; }

        public void UpdatePolicyCache()
        {
            if (PolicyIds == null || timeToUpdate())
                PolicyIds = TrainingRecords.Select(tr => tr.Id).ToImmutableSortedSet();
        }

        private bool timeToUpdate()
        {
            if (DateTime.Now > _nextPolicyUpdateTime)
            {
                _nextPolicyUpdateTime = DateTime.Now + POLICY_CACHE_UPDATE_TIME; //set next time to update
                return true;
            }
            else return false;
        }

        public DBTraining(): base("TrainingData")
        {
            //create table(s) if not present. TODO: if we have more, this can be abstracted to automatically create any applicable tables
            var sp = DataProvider.GetSchemaProvider();
            var schema = sp.GetSchema(this);
            if (schema.Tables.All(t => t.TableName != typeof(Training).GetTableName()))
            {
                this.CreateTable<Training>();
            }
            UpdatePolicyCache();
        }

        public ITable<Training> TrainingRecords => GetTable<Training>();

        public IEnumerable<Training> GetExisting(IEnumerable<ulong> ids)
            => from t in TrainingRecords
               where ids.Contains(t.Id)
               select t;

        public void Update(IEnumerable<(Move, float)> data) => Update(data.Select(d => (d.Item1.CanonicalFieldId, d.Item1.Direction, d.Item2)));

        public void Update(IEnumerable<(ulong, Direction, float)> data)
        {

            Dictionary<ulong, Training> records;
            lock (this)
            {
                records = GetExisting(data.Select(d => d.Item1)).ToDictionary(k => k.Id, v => v);
            }

            BeginTransaction();
            foreach (var (id, dir, reward) in data)
            {
                if (records.TryGetValue(id, out Training training))
                    training.Update(dir, reward);
                else
                    training = new Training(id, dir, reward);

                this.InsertOrReplace(training);
            }
            CommitTransaction();
        }

        public void Export(string filename)
        {
            var PAGE_SIZE = 1000;
            var pages = 0;
            var count = 0;
            IEnumerable<Training> records;
            while ((records = TrainingRecords.Skip(PAGE_SIZE * pages++).Take(PAGE_SIZE)).Any())
                count += records.Sum(r => r.TotalCount);

            //return count;
        }

        public void Import(string filename)
        {
            
        }
    }
}
