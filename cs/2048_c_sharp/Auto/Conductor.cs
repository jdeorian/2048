﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

using static _2048_c_sharp.Auto.Logger;

namespace _2048_c_sharp.Auto
{
    public class Conductor<T> where T : AutoBase
    {
        public Board BestBoard { get; set; }
        public List<Task> Tasks { get; set; }
        public List<T> ActiveBoards { get; private set; } = new List<T>();
        public const int MAX_CONCURRENT_BOARDS = 16;
        const int SLEEP_LENGTH = 500; //in ms

        //Triggers the run loop to stop (but does not cancel existing threads)
        public bool Stop { get; set; } = false;

        private int _boards = 0;
        public int BoardsCount
        {
            get => _boards;
            set
            {
                if (value < 0 || value > MAX_CONCURRENT_BOARDS) return;
                _boards = value;
            }
        }

        public void Run(int boards = 1)
        {
            Stop = false;
            var it_cnt = 1;
            BoardsCount = boards;
            Tasks = Enumerable.Range(0, BoardsCount)
                              .Select(i => Task.Run(() => { }))
                              .ToList(); //initialized as empty, completed tasks
            while (!Stop)
            {
                Tasks.RemoveAll(t => t.IsCompleted);
                int newBoards = BoardsCount - Tasks.Count();
                if (newBoards > 0)
                { 
                    Tasks.AddRange(Enumerable.Range(it_cnt, newBoards)
                                             .Select(i => Task.Run(() => {
                                                 var iter = (T)Activator.CreateInstance(typeof(T), i);
                                                 lock (ActiveBoards) {
                                                     ActiveBoards.Add(iter);
                                                 }
                                                 iter.Run();
                                                 UpdateBestBoard(iter);
                                                 UpdateTrainingDB(iter);
                                                 lock (ActiveBoards) {
                                                     ActiveBoards.Remove(iter);
                                                 }
                                             })));
                    it_cnt += newBoards;
                }                
                System.Threading.Thread.Sleep(SLEEP_LENGTH); //it's not important for this loop to run continuously
            }

            log("Finishing remaining tasks...", Priority.Highest_1);
            Task.WaitAll(Tasks.ToArray());
            log("Done.", Priority.Highest_1);
        }

        private void UpdateBestBoard(T board)
        {
            if (board.Board.Score <= (BestBoard?.Score ?? 0)) return;
            var lines = new[] {
                        string.Empty,
                        $"Iteration: {board.Iteration}: {board.ToString()} @ {DateTime.Now}",
                        board.Board.State.AsBoardString()
                    };
            
            log(string.Join(Environment.NewLine, lines), Priority.P2);
            BestBoard = board.Board;
        }

        private void UpdateTrainingDB(T iteration)
            => PolicyData.UpdatePolicy(iteration.GetSumOfRewards()
                                                .Select(kvp => (kvp.Key.StartState,
                                                                kvp.Key.Direction,
                                                                kvp.Value)));
    }
}
