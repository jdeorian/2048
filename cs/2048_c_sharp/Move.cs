using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Move: Node<Move>
    {
        const int BOARD_SIZE = 4;
        public int[,] StartState => Parent?.EndState ?? Board.StartState;
        public int[,] IntermediateState => StartState.Slide(Direction);

        private int[,] endState;
        public int[,] EndState
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
        public Direction RewardDirection => RootEldestChild.Direction; //throws exception if root

        public Move(Direction direction, Move parent = null, Board board = null, Dictionary<Direction, float> weights = null): base(parent)
        {
            Initialize(direction, board);
            Weights = weights;
        }

        public Move(Direction direction, Move parent, int[,] endState, float chance): base(parent)
        {
            Initialize(direction, parent.Board);
            EndState = endState;
            Chance = chance * parent.Chance;
        }

        public bool ChangedBoard() => !StartState.IsEqualTo(EndState);

        public void Initialize(Direction direction, Board board = null)
        {
            Board = board ?? Parent?.Board;
            Direction = direction;
            if (board == null) throw new Exception("Moves need to have boards.");
        }

        public override float GetReward() => EndState.Reward() - StartState.Reward();
        public float Score => EndState.Score();

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
    }
}
