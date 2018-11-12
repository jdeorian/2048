using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public class Board
    {
        public const int BOARD_SIZE = 4;
        public const float FOUR_CHANCE = .11f;
        public readonly ulong StartState;
        public ulong Field => LastMove?.EndState ?? StartState;
        public List<Move> MoveHistory { get; set; } = new List<Move>();
        public int MoveCount => _lastMoveIndex + 1;
        private int _lastMoveIndex = -1; //here to reduce traversing through the history list every time we access the end state
        public Move LastMove => _lastMoveIndex == -1 ? null : MoveHistory[_lastMoveIndex]; //error on board with no moves
        public Board(ulong startState = 0)
        {
            if (startState == 0)
            {
                Random rnd = new Random();
                ULongMagic.SetRandomSquare(ref StartState, FOUR_CHANCE, rnd);
                ULongMagic.SetRandomSquare(ref StartState, FOUR_CHANCE, rnd);
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

        public IEnumerable<string> ToTrainingData() => MoveHistory.Select(m => $"{m.StartState}\t{m.Direction}\t{Score}");
    }
}
