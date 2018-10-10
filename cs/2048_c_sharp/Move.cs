using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Move
    {
        const int BOARD_SIZE = 4;

        private List<Move> children = new List<Move>();
        public List<Move> Children
        {
            get
            {
                GetChildren();
                return children;
            }
        }
        public Move Parent { get; set; }
        public int[,] StartState => Parent?.EndState ?? new int[BOARD_SIZE, BOARD_SIZE];
        public int[,] EndState { get; set; } = new int[BOARD_SIZE, BOARD_SIZE];
        public float Chance { get; set; } = 1f;
        public Direction Direction { get; set; }
        public Board Board { get; set; }

        private Dictionary<Direction, decimal> weights;
        public Dictionary<Direction, decimal> Weights
        {
            get
            {
                if (weights == null)
                    weights = XT.EnumVals<Direction>().ToDictionary(d => d, d => 0m);
                return weights;
            }
            set => weights = value;
        }

        public Direction RewardDirection
        {
            get
            {
                if (Parent == null) throw new Exception("Can't do that shit");
                return RootEldestChild.Direction;
            }
        }

        public Move RootEldestChild
        {
            get
            {
                if (Parent == null) return null; //this is the parent
                var m = this;
                while (m.Parent.Parent != null)
                    m = m.Parent;
                return m;
            }
        }

        public Move Root
        {
            get
            {
                if (Parent == null) return this;
                return RootEldestChild.Parent;
            }
        }

        public Move(Direction direction, Move parent = null)
        {
            Initialize(direction, parent);
        }

        public Move(Direction direction, Move parent, int[,] endState, float chance)
        {
            Initialize(direction, parent);
            EndState = endState;
            Chance = chance * parent.Chance;
        }

        public bool ChangedBoard() => !StartState.IsEqualTo(EndState);

        public void Initialize(Direction direction, Move parent = null)
        {
            Parent = parent;
            Direction = direction;
        }

        public decimal GetReward() => EndState.Score() - Root.EndState.Score();

        public void Apply(Board board)
        {
            Board = board;
            board.Field = EndState = board.Field.Slide(Direction);
        }

        public void GetChildren()
        {
            if (!children.Any())
                children.AddRange(EndState.EnumerateOutcomes(this, Board.FOUR_CHANCE));
        }
    }
}
