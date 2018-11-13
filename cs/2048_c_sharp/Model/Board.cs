using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048_c_sharp
{
    public class Board
    {
        public static Random rnd = new Random();

        public readonly ulong StartState = 0;
        public ulong State;

        public List<Move> MoveHistory { get; private set; } = new List<Move>();
        public Move LastMove { get; set; }

        public Board()
        {
            ULongMagic.SetRandomSquare(ref StartState, rnd);
            ULongMagic.SetRandomSquare(ref StartState, rnd);
            State = StartState;
        }

        public void Move(Direction direction)
        {
            LastMove = new Move(State, direction);
            MoveHistory.Add(LastMove);
            State = LastMove.EndState;
            ULongMagic.SetRandomSquare(ref State, rnd);
        }

        public bool Lost => !State.PossibleDirections().Any();

        public override string ToString() => $"Score: {Score} Moves: {MoveHistory.Count()} Max: {State.MaxValue()}";

        public float Score => State.Score();
        public float Reward => State.Reward();
    }
}
