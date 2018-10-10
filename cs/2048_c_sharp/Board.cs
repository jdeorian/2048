using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Board
    {
        public int Iteration { get; set; } // here as an easy way to store the iteration count of external multi-processing methods
        public const int BOARD_SIZE = 4;
        public const float FOUR_CHANCE = .11f;
        public int[,] Field { get; set; } = new int[BOARD_SIZE, BOARD_SIZE];
        public List<Move> MoveHistory { get; set; } = new List<Move>();
        public Board()
        {
            Field.SetRandomSquare(FOUR_CHANCE);
            Field.SetRandomSquare(FOUR_CHANCE);
        }

        public Move Move(Direction direction, Dictionary<Direction, decimal> weights = null, bool addRandomSquares = true, bool applyResults = true)
        {
            var lastMove = MoveHistory.LastOrDefault();

            //make sure we're not repeating something that didn't work
            if (lastMove != null)
            {
                if (lastMove.Direction == direction && !lastMove.ChangedBoard())
                    return lastMove;
            }

            var m = new Move(direction, lastMove);
            var oldState = Field.AsCopy();
            m.Apply(this);
            if (Field.IsEqualTo(oldState))
                throw new Exception("You can't make a move that doesn't change the board.");

            if (m.ChangedBoard() && addRandomSquares)
            {
                Field.SetRandomSquare(FOUR_CHANCE);
                m.EndState = Field.AsCopy();
            }

            if (applyResults)
            {
                m.Weights = weights;
                MoveHistory.Add(m);
            }
            else
            {
                Field = m.StartState.AsCopy();
            }

            return m;
        }

        public bool Lost => !Field.PossibleMoves().Any();

        public override string ToString()
        {
            return $"Score: {Score} Moves: {MoveHistory.Count()} Max: {Field.MaxValue()}";
        }

        public decimal Score => Field.Score();

        public decimal Reward => Field.Reward();
    }
}
