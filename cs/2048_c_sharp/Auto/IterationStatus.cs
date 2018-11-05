using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    public class IterationStatus : INotifyPropertyChanged
    {
        public int Iteration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int PolicyMoves { get; set; } = 0;
        public int MethodMoves { get; set; } = 0;

        private int _moveCount = 0;
        public int MoveCount
        {
            get => _moveCount;
            set
            {
                if (value == _moveCount) return;
                _moveCount = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MethodMoves));
                NotifyPropertyChanged(nameof(PolicyMoves));
                NotifyPropertyChanged(nameof(Score));
            }
        }
        public DateTime TimeStarted { get; set; }

        private DateTime _timeStarted = DateTime.MinValue;
        public DateTime TimeEnded
        {
            get => _timeStarted;
            set
            {
                if (value == _timeStarted) return;
                _timeStarted = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Closed));
            }
        }

        public float Score { get; set; }

        public void Update(IterationStatus update)
        {
            MoveCount = update.MoveCount;
            TimeEnded = update.TimeEnded;
            Score = update.Score;
            MethodMoves = update.MethodMoves;
            PolicyMoves = update.PolicyMoves;
        }

        //public void UpdateToClose(IterationStatus update)
        //{
        //    Update(update);
        //    if (TimeEnded == default) TimeEnded = DateTime.Now;
        //}

        public void Close()
        {
            if (TimeEnded == default) TimeEnded = DateTime.Now;
        }

        public bool Closed => TimeEnded != default;
    }
}
