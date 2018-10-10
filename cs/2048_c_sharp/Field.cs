using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Drawing;

namespace _2048_c_sharp
{
    public static class FieldExtensions
    {
        public static Square[] GetEmptySquares(this int[,] squares, int defVal = 0) => (
            from x in Enumerable.Range(0, squares.GetLength(0))
            from y in Enumerable.Range(0, squares.GetLength(1))
            where squares[x,y] == 0
            select new Square(x, y, defVal)
            ).ToArray();

        public static int[] Flatten(this int [,] squares)
        {
            int[] tmp = new int[squares.GetLength(0) * squares.GetLength(1)];
            Buffer.BlockCopy(squares, 0, tmp, 0, tmp.Length * sizeof(int));
            return tmp;
        }

        public static int[,] Unflatten(this int[] squares, int size)
        {
            int[,] tmp = new int[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = squares[y * size + x];
            return tmp;
        }

        //reverses x and y
        public static int[,] Transpose(this int[,] squares)
        {
            int[,] tmp = new int[squares.GetLength(1), squares.GetLength(0)];
            for (int x = 0; x < squares.GetLength(0); x++)
                for (int y = 0; y < squares.GetLength(1); y++)
                    tmp[y, x] = squares[x, y];
            return tmp;
        }

        //flip vertically
        public static int[,] Flip(this int[,] squares)
        {
            int sz_x = squares.GetLength(0);
            int sz_y = squares.GetLength(1);
            int[,] tmp = new int[sz_x, sz_y];
            for (int x = 0; x < sz_x; x++)
                for (int y = 0; y < sz_y; y++)
                    tmp[x, y] = squares[sz_x-x-1, y];
            return tmp;
        }

        //flip horizontally
        public static int[,] Invert(this int[,] squares)
        {
            int sz_x = squares.GetLength(0);
            int sz_y = squares.GetLength(1);
            int[,] tmp = new int[sz_x, sz_y];
            for (int x = 0; x < sz_x; x++)
                for (int y = 0; y < sz_y; y++)
                    tmp[x, y] = squares[x, sz_y-y-1];
            return tmp;
        }

        public static int[,] AsCopy(this int[,] squares)
        {
            int[,] tmp = new int[squares.GetLength(0), squares.GetLength(1)];
            Buffer.BlockCopy(squares, 0, tmp, 0, tmp.Length * sizeof(int));
            return tmp;
        }

        //always returns a copy, not the original
        public static int[,] Transform(this int[,] squares, Direction direction)
        {
            switch(direction)
            {
                case Direction.Down: return squares.Transpose().Invert();
                case Direction.Up: return squares.Transpose().Flip();
                case Direction.Left: return squares.AsCopy();
                case Direction.Right: return squares.Invert();
            }
            throw new Exception("Oops");
        }

        public static int[,] Untransform(this int[,] squares, Direction direction)
        {
            switch (direction)
            {                
                case Direction.Down: return squares.Invert().Transpose();
                case Direction.Up: return squares.Flip().Transpose();
                case Direction.Left: return squares.AsCopy();
                case Direction.Right: return squares.Invert();
            }
            throw new Exception("Oops");
        }

        public static int[] GetRow(this int[,] squares, int row)
        {
            int sz_y = squares.GetLength(1);
            int sz_int = sizeof(int);
            int[] tmp = new int[sz_y];
            Buffer.BlockCopy(squares, sz_int * sz_y * row, tmp, 0, sz_int * sz_y);
            return tmp;
        }

        public static void SetRow(this int[,] squares, int[] row, int row_idx)
        {
            int sz_int = sizeof(int);
            int sz_y = squares.GetLength(1);
            Buffer.BlockCopy(row, 0, squares, sz_int * row_idx * sz_y, sz_int * sz_y);
        }

        public static int[] GetRowWithoutZeros(this int[,] squares, int row_idx) => squares.GetRow(row_idx).GetRowWithoutZeros();
        public static int[] GetRowWithoutZeros(this IEnumerable<int> row) => row.Where(i => i != 0).ToArray();

        public static bool IsEqualTo(this int[,] squares1, int[,] squares2)
        {
            int sz_x = squares1.GetLength(0);
            int sz_y = squares1.GetLength(1);
            for (int x = 0; x < sz_x; x++)
                for (int y = 0; y < sz_y; y++)
                    if (squares1[x, y] != squares2[x, y]) return false;
            return true;
        }

        public static int[,] Slide(this int[,] squares, Direction d) => squares.Transform(d).SlideLeft().Untransform(d);

        public static int[,] SlideLeft(this int[,] squares)
        {
            //for each row
            int sz_int = sizeof(int);
            int sz_x = squares.GetLength(0);
            int sz_y = squares.GetLength(1);
            int[,] tmp = new int[sz_x, sz_y];
            for (int x = 0; x < sz_x; x++)
            {
                var newRow = new List<int>();
                var oldRow = squares.GetRowWithoutZeros(x);
                for (int y = 0; y < oldRow.GetLength(0); y++)
                {
                    //handle the case for the last item in the list
                    int num = oldRow[y];
                    if (y == oldRow.GetUpperBound(0))
                    {
                        newRow.Add(num);
                        break;
                    }

                    //handle pairs
                    if (num == oldRow[y + 1]) //if there's a pair
                    {
                        newRow.Add(num + 1);
                        oldRow[y + 1] = 0;
                    }
                    else { newRow.Add(num); } //handle singles
                }

                //remove the zeroes and copy them into the result matrix
                var newRowCompressed = newRow.GetRowWithoutZeros();
                Buffer.BlockCopy(newRowCompressed, 0, tmp, sz_int * x * sz_y, sz_int * newRowCompressed.GetLength(0));
            }

            return tmp;
        }

        public static int[,] AsCopyWithUpdate(this int[,] squares, int x, int y, int val)
        {
            var tmp = squares.AsCopy();
            tmp[x, y] = val;
            return tmp;
        }

        public static int[,] AsCopyWithUpdate(this int[,] squares, Square s) => squares.AsCopyWithUpdate(s.X, s.Y, s.Value);

        public static IEnumerable<Move> EnumerateOutcomes(this int[,] squares, Move parent, float FOUR_CHANCE)
        {
            (int value, float chance)[] values = {
                (2, FOUR_CHANCE),
                (1, 1 - FOUR_CHANCE)
            };
            var moves = XT.EnumVals<Direction>().Select(d => new { dir = d, field = squares.Slide(d) })
                                                .Where(f => !f.field.IsEqualTo(squares));
            var outcomes = new List<Move>();
            foreach(var move in moves)
            {
                //create the outcomes that place a 1
                var emptySquares = move.field.GetEmptySquares(1);
                outcomes.AddRange(emptySquares.Select(es => new Move(move.dir,
                                                                        parent,
                                                                        move.field.AsCopyWithUpdate(es),
                                                                        1 - FOUR_CHANCE)));
                //create the outcomes place a 2
                foreach (var s in emptySquares) s.Value = 2;
                outcomes.AddRange(emptySquares.Select(es => new Move(move.dir,
                                                                     parent,
                                                                     move.field.AsCopyWithUpdate(es),
                                                                     FOUR_CHANCE)));
            }

            return outcomes;
        }

        public static IEnumerable<Direction> PossibleMoves(this int[,] squares)
        {
            

            return XT.EnumVals<Direction>().Where(d => !squares.Slide(d).IsEqualTo(squares));
        }

        public static decimal Score(this int[,] squares) => squares.Flatten().Sum(s => 1 << s);
        public static int MaxValue(this int[,] squares) => squares.Flatten().Max(s => 1 << s);

        public static Square GetRandomSquare(this int[,] squares, Random rnd = null)
        {
            if (rnd == null) rnd = new Random();
            var emptySquares = squares.GetEmptySquares();
            var len = emptySquares.Length;
            if (len == 0) return null;
            if (len == 1) return emptySquares[0];

            return emptySquares[rnd.Next(len)];
        }

        public static bool SetRandomSquare(this int[,] squares, float FOUR_CHANCE)
        {
            var rnd = new Random();
            var rs = squares.GetRandomSquare(rnd);
            if (rs == null) return false;
            var val = rnd.NextDouble() > FOUR_CHANCE ? 1 : 2;
            squares[rs.X, rs.Y] = val;
            return true;
        }

        public static string AsString(this int[,] squares)
        {
            var result = string.Empty;
            int sz_x = squares.GetLength(0);
            int sz_y = squares.GetLength(1);
            for (int x = 0; x < sz_x; x++)
            {
                for (int y = 0; y < sz_y; y++)
                    result += $"\t{1 << squares[x, y]}";
                result += Environment.NewLine;
            }
            return result;
        }
    }
}
