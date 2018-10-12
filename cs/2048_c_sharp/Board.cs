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
        public readonly int[,] StartState;
        public int[,] Field => MoveHistory.LastOrDefault()?.EndState ?? StartState;
        public List<Move> MoveHistory { get; set; } = new List<Move>();
        public int MoveCount => _lastMoveIndex + 1;
        private int _lastMoveIndex = -1; //here to reduce traversing through the history list every time we access the end state
        public Move LastMove => _lastMoveIndex == -1 ? null : MoveHistory[_lastMoveIndex]; //error on board with no moves
        public Board(int[,] startState = null)
        {
            if (startState == null)
            {
                int[,] start = new int[BOARD_SIZE, BOARD_SIZE];
                start.SetRandomSquare(FOUR_CHANCE);
                start.SetRandomSquare(FOUR_CHANCE);
                StartState = start;
            }
            else
            {
                StartState = startState;
            }
        }

        public Board(Move rootMove)
        {
            StartState = rootMove.EndState;
        }

        public void Move(Direction direction, Dictionary<Direction, float> weights = null)
        {
            MoveHistory.Add(new Move(direction, LastMove, this, weights));
            _lastMoveIndex++;            
        }

        public bool Lost => !Field.PossibleMoves().Any();

        public override string ToString()
        {
            return $"Score: {Score} Moves: {MoveHistory.Count()} Max: {Field.MaxValue()}";
        }

        public float Score => Field.Score();

        public float Reward => Field.Reward();
    }
}
