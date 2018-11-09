using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    //this acts as a queue for interactions with the database to avoid thread conflicts
    public static class PolicyData
    {
        private static DBTraining db { get; set; } = new DBTraining();
        private static readonly Task Task = Task.Run(() => RunQueue());

        #region Policy Update Logic

        public static ImmutableSortedSet<ulong> PolicyIds { get; set; } = db.TrainingRecords.Select(tr => tr.Id).ToImmutableSortedSet();
        private static TimeSpan POLICY_CACHE_UPDATE_TIME = new TimeSpan(4, 0, 0);
        private static DateTime _nextPolicyUpdateTime = DateTime.MinValue;
        private static bool updatePolicyCache() //returns whether an update occurred
        {
            Action update = () => PolicyIds = db.TrainingRecords.Select(tr => tr.Id).ToImmutableSortedSet();
            if (timeToUpdate(_nextPolicyUpdateTime))
            {
                var task = new Task(() => { lock (PolicyIds) { update(); } });
                if (PolicyIds == null) task = new Task(() => update());
                _lpQueue.Push(task);
                _nextPolicyUpdateTime = DateTime.Now + POLICY_CACHE_UPDATE_TIME; //set next time to update
                return true;
            }
            return false;
        }

        private static bool timeToUpdate(DateTime updateTime) => DateTime.Now > updateTime;

        #endregion

        #region DB Queue

        private const int EMPTY_QUEUE_SLEEP_TIME = 100; //ms

        private static void RunQueue()
        {
            Task t;
            bool didSomething;
            while (true)
            {
                didSomething = updatePolicyCache();

                //run low priority tasks, if appropriate                
                if (_lpQueue.Any() && timeToUpdate(_nextLPUpdateTime))
                {
                    didSomething = true;
                    while (_lpQueue.Any())
                        _lpQueue.Pop().RunSynchronously();
                }

                //Run all the requests currently in queue
                var pqCount = _getPolicyQueue.Count();
                if (pqCount > 0)
                {
                    didSomething = true;
                    for (int x = 0; x < pqCount; x++)
                        _getPolicyQueue.Pop().RunSynchronously();
                }

                //sleep a bit if the queues are empty
                if (!didSomething)
                    System.Threading.Thread.Sleep(100);
            }
        }

        #region DB Queue::LP Queue

        /// <summary>
        /// Low priority queue; items that have no return values and can be deferred if
        /// there are time-sensitive items in queue. This also includes importing and
        /// exporting. TODO: to guarantee periodic execution, add update interval. Also,
        /// policy update will be added to the update queue based on the interval specified above.
        /// </summary>
        private static Stack<Task> _lpQueue = new Stack<Task>();
        private static TimeSpan LP_QUEUE_GUARANTEE = new TimeSpan(0, 15, 0); //every 15 minutes
        private static DateTime _nextLPUpdateTime = DateTime.MinValue;

        public static void UpdatePolicy(IEnumerable<(ulong, Direction, float)> data)
            => _lpQueue.Push(new Task(() => db.Update(data)));

        #endregion

        #region DB Queue: Get Policy Queue        

        private static Stack<Task<Training>> _getPolicyQueue = new Stack<Task<Training>>();

        public static async Task<Training> GetPolicy(ulong state)
        {
            //check the cache to see if a policy even exists
            if (!PolicyIds.Any(pid => pid == state)) return null;

            var task = new Task<Training>(() => db.GetPolicy(state));
            _getPolicyQueue.Push(task);
            return await task;
        }

        #endregion

        #endregion

        public static int CountIterations()
        {
            var PAGE_SIZE = 1000;
            var pages = 0;
            var count = 0;
            IEnumerable<Training> records;
            while ((records = db.TrainingRecords.Skip(PAGE_SIZE * pages++).Take(PAGE_SIZE)).Any())
                count += records.Sum(r => r.TotalCount);

            return count;
        }
    }
}
