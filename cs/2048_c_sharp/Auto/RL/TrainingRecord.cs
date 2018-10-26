using System;
using System.Collections.Generic;

using LinqToDB.Mapping;

namespace _2048_c_sharp.Auto
{
    [Table(Name = "training")]
    public sealed class Training
    {
        public Training() { }
        public Training(long id) => Id = id;
        public Training(long id, Direction direction, float reward) : this(id) => Update(direction, reward);


        [PrimaryKey, Identity]
        public long Id { get; set; }

        [Column, NotNull] public float UpWeight {
            get => Weights[Direction.Up];
            set => Weights[Direction.Up] = value;
        }
        [Column, NotNull] public float DownWeight {
            get => Weights[Direction.Down];
            set => Weights[Direction.Down] = value;
        }
        [Column, NotNull] public float LeftWeight {
            get => Weights[Direction.Left];
            set => Weights[Direction.Left] = value;
        }
        [Column, NotNull] public float RightWeight {
            get => Weights[Direction.Right];
            set => Weights[Direction.Right] = value;
        }

        [Column, NotNull] public int UpCount {
            get => Counts[Direction.Up];
            set => Counts[Direction.Up] = value;
        }
        [Column, NotNull] public int DownCount {
            get => Counts[Direction.Down];
            set => Counts[Direction.Down] = value;
        }
        [Column, NotNull] public int LeftCount {
            get => Counts[Direction.Left];
            set => Counts[Direction.Left] = value;
        }
        [Column, NotNull] public int RightCount {
            get => Counts[Direction.Right];
            set => Counts[Direction.Right] = value;
        }

        public Dictionary<Direction, int> Counts = new Dictionary<Direction, int> {
            { Direction.Up, default },
            { Direction.Down, default },
            { Direction.Left, default },
            { Direction.Right, default }
        };

        public Dictionary<Direction, float> Weights = new Dictionary<Direction, float> {
            { Direction.Up, default },
            { Direction.Down, default },
            { Direction.Left, default },
            { Direction.Right, default }
        };

        public float GetWeight(Direction direction)
        {
            var cnt = Counts[direction];
            if (cnt == 0) return default;
            return Weights[direction] / cnt;
        }           

        public void Update(Direction direction, float reward)
        {
            Counts[direction]++;
            Weights[direction] += reward;
        }
    }
}
