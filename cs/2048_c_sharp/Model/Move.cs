using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Move: Node<Move>
    {
        public byte[,] StartState => Parent?.EndState
                                  ?? Board.StartState;
        public byte[,] IntermediateState => StartState.Slide(Direction);

        private byte[,] endState;
        public byte[,] EndState
        {
            get
            {
                if (endState == null)
                {
                    endState = StartState.Slide(Direction);
                    endState.SetRandomSquare(Board.FOUR_CHANCE);
                }
                return endState;
            }
            set => endState = value;
        }
        public Direction Direction { get; set; }
        public Board Board { get; set; }
        public Dictionary<Direction, float> Weights { get; set; }
        public Direction RewardDirection => IsRoot ? Direction.Up : RootEldestChild.Direction;

        private long _canonicalFieldId = 0;
        public long CanonicalFieldId {
            get => _canonicalFieldId == 0 ? _canonicalFieldId = StartState.CanonicalFieldID() : _canonicalFieldId;
        }

        public Move(Direction direction, Move parent = null, Board board = null, Dictionary<Direction, float> weights = null): base(parent)
        {
            Initialize(direction, board);
            Weights = weights;
        }

        public Move(Direction direction, Move parent, byte[,] endState, float chance): base(parent)
        {
            Initialize(direction, parent.Board);
            EndState = endState;
            Chance = chance;
        }

        public bool ChangedBoard() => !StartState.IsEqualTo(EndState);

        public void Initialize(Direction direction, Board board = null)
        {
            Board = board ?? Parent?.Board;
            Direction = direction;
            if (board == null) throw new Exception("Moves need to have boards.");
        }

        private float _reward = 0f;
        public override float Reward
        {
            get
            {
                if (_reward == 0f)
                    _reward = EndState.Reward();
                return _reward;
            }
        }

        public override List<Move> GetChildren()
        {
            var moves = XT.EnumVals<Direction>().Select(d => new { dir = d, field = EndState.Slide(d) })
                                                .Where(f => !f.field.IsEqualTo(EndState));
            var outcomes = new List<Move>();
            var TWO_CHANCE = 1 - Board.FOUR_CHANCE;
            foreach (var move in moves)
            {
                foreach (var (i, j) in move.field.GetEmptySquares())
                {
                    var fld = move.field.AsCopyWithUpdate(i, j, 1);     // add outcomes that add a 2
                    outcomes.Add(new Move(move.dir, this, fld, TWO_CHANCE));
                    fld = move.field.AsCopyWithUpdate(i, j, 2);         // add outcomes that add a 4
                    outcomes.Add(new Move(move.dir, this, fld, Board.FOUR_CHANCE));
                }
            }
            return Children = outcomes;
        }

        //Index ParentIndex Field Chance
        public string AsTreeString => string.Join("\t", new [] {
            Index.ToString(),
            Parent?.Index.ToString() ?? "-1",
            Direction.ToString(),
            RewardDirection.ToString(),
            StartState.AsFlatString(),
            EndState.AsFlatString(),
            Reward.ToString(),
            SumOfRewards.ToString(),
            Chance.ToString()
        });

        public void SaveToTreeFile(string filename = "move.tree")
        {
            using (var sw = new StreamWriter(filename))
            {
                sw.WriteLine("Index\tParent\tDirection\tRewardDirection\tStart\tEnd\tReward\tSumOfRewards\tChance");
                var next_set = new List<Move> { this };
                var current_set = new List<Move>();
                while(next_set.Any())
                {
                    current_set = next_set;
                    next_set = new List<Move>();
                    foreach (var move in current_set)
                    {
                        sw.WriteLine(move.AsTreeString);
                        next_set.AddRange(move.Children);
                    }
                }
            }
        }

        public string[] GetOrderedWeights()
        {
            string getWeight(Direction direction) => Weights.TryGetValue(direction, out var val) ? val.ToString() : "";
            return new[] {
                getWeight(Direction.Up),
                getWeight(Direction.Down),
                getWeight(Direction.Left),
                getWeight(Direction.Right)
            };
        }
    }


}
