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
        //scoring parameters
        const int EMPTY_SQUARE_REWARD = 8;

        const int SZ = 4; //this is an abomination, but getting the length of the array so often has a material performance impact, so.... *sigh*
        const int SZ_2D = 16;

        public static Square[] GetEmptySquares_Slow(this int[,] squares, int defVal = 0) => (
            from x in Enumerable.Range(0, SZ)
            from y in Enumerable.Range(0, SZ)
            where squares[x, y] == 0
            select new Square(x, y, defVal)
            ).ToArray();

        public static List<(int i, int j)> GetEmptySquares(this int[,] squares)
        {
            var cells = new List<(int i, int j)>();
            for (int i = 0; i < SZ; i++)
                for (int j = 0; j < SZ; j++)
                    if (squares[i, j] == 0) cells.Add((i, j));
            return cells;
        }

        public static int CountEmptySquares(this int[,] squares)
        {
            int count = 0;
            for (int i = 0; i < SZ; i++)
                for (int j = 0; j < SZ; j++)
                    if (squares[i, j] == 0) count++;

            return count;
        }

        //=> squares.Flatten().CountEmptySquares();
        public static int CountEmptySquares(this int[] squares)
        {
            int sz_x = SZ_2D;
            int count = 0;
            for (int x = 0; x < sz_x; x++)
                if (squares[x] == 0) count++;
            return count;
        }

        public static int[] Flatten(this int[,] squares)
        {
            int[] tmp = new int[SZ_2D];   //squares.GetLength(0) * squares.GetLength(1)];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D * sizeof(int));
            return tmp;
        }

        public static int[,] Unflatten(this int[] squares, int size)
        {
            int[,] tmp = new int[SZ, SZ];
            for (int x = 0; x < SZ; x++)
                for (int y = 0; y < SZ; y++)
                    tmp[x, y] = squares[y * SZ + x];
            return tmp;
        }

        //reverses x and y
        public static int[,] Transpose(this int[,] squares)
        {
            int[,] tmp = new int[SZ, SZ];
            for (int x = 0; x < SZ; x++)
                for (int y = 0; y < SZ; y++)
                    tmp[y, x] = squares[x, y];
            return tmp;
        }

        //flip vertically
        public static int[,] Flip(this int[,] squares)
        {
            //int sz_x = squares.GetLength(0);
            //int sz_y = squares.GetLength(1);
            int[,] tmp = new int[SZ, SZ];
            for (int x = 0; x < SZ; x++)
                for (int y = 0; y < SZ; y++)
                    tmp[x, y] = squares[SZ - x - 1, y];
            return tmp;
        }

        //flip horizontally
        public static int[,] Invert(this int[,] squares)
        {
            //int sz_x = squares.GetLength(0);
            //int sz_y = squares.GetLength(1);
            int[,] tmp = new int[SZ, SZ];
            for (int x = 0; x < SZ; x++)
                for (int y = 0; y < SZ; y++)
                    tmp[x, y] = squares[x, SZ - y - 1];
            return tmp;
        }

        public static int[,] AsCopy(this int[,] squares)
        {
            int[,] tmp = new int[SZ, SZ];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D * sizeof(int));
            return tmp;
        }

        //always returns a copy, not the original
        public static int[,] Transform(this int[,] squares, Direction direction)
        {
            switch (direction)
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
            //int sz_y = squares.GetLength(1);
            int sz_int = sizeof(int);
            int[] tmp = new int[SZ];
            Buffer.BlockCopy(squares, sz_int * SZ * row, tmp, 0, sz_int * SZ);
            return tmp;
        }

        public static void SetRow(this int[,] squares, int[] row, int row_idx)
        {
            int sz_int = sizeof(int);
            //int sz_y = squares.GetLength(1);
            Buffer.BlockCopy(row, 0, squares, sz_int * row_idx * SZ, sz_int * SZ);
        }

        //public static int[] GetRowWithoutZeros(this int[,] squares, int row_idx) => squares.GetRow(row_idx).GetRowWithoutZeros();
        //public static int[] GetRowWithoutZeros(this IEnumerable<int> row) => row.Where(i => i != 0).ToArray();

        public static int[] GetRowWithoutZeros(this int[,] squares, int row_idx)
        {
            int[] tmp = new int[SZ];
            int idx = 0;
            for (int x = 0; x < SZ; x++)
                if (squares[x, row_idx] != 0)
                    tmp[idx++] = squares[row_idx, x];
            return tmp;
        }
        public static int[] GetRowWithoutZeros(this List<int> row)
        {
            int[] tmp = new int[SZ];
            int idx = 0;
            for (int x = 0; x < SZ; x++)
                if (row[x] != 0)
                    tmp[idx++] = row[x];
            return tmp;
        }
            
            //=> row.Where(i => i != 0).ToArray();

        public static bool IsEqualTo(this int[,] squares1, int[,] squares2)
        {
            //int sz_x = squares1.GetLength(0);
            //int sz_y = squares1.GetLength(1);
            for (int x = 0; x < SZ; x++)
                for (int y = 0; y < SZ; y++)
                    if (squares1[x, y] != squares2[x, y]) return false;
            return true;
        }

        public static int[,] Slide(this int[,] squares, Direction d) => squares.Transform(d).SlideLeft().Untransform(d);

        public static int[,] SlideLeft(this int[,] squares)
        {
            //for each row
            int sz_int = sizeof(int);
            //int sz_x = squares.GetLength(0);
            //int sz_y = squares.GetLength(1);
            int[,] tmp = new int[SZ, SZ];
            for (int x = 0; x < SZ; x++)
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
                Buffer.BlockCopy(newRowCompressed, 0, tmp, sz_int * x * SZ, sz_int * newRowCompressed.GetLength(0));
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
            var moves = XT.EnumVals<Direction>().Select(d => new { dir = d, field = squares.Slide(d) })
                                                .Where(f => !f.field.IsEqualTo(squares));
            var outcomes = new List<Move>();
            var TWO_CHANCE = 1 - FOUR_CHANCE;
            foreach (var move in moves)
            {                
                foreach (var (i, j) in move.field.GetEmptySquares())
                {
                    var fld = move.field.AsCopyWithUpdate(i, j, 1);     // add outcomes that add a 2
                    outcomes.Add(new Move(move.dir, parent, fld, TWO_CHANCE));
                    fld = move.field.AsCopyWithUpdate(i, j, 2);         // add outcomes that add a 4
                    outcomes.Add(new Move(move.dir, parent, fld, FOUR_CHANCE));
                }
            }
            return outcomes;
        }

        public static IEnumerable<Direction> PossibleMoves(this int[,] squares)
        {
            return XT.EnumVals<Direction>().Where(d => !squares.Slide(d).IsEqualTo(squares));
        }

        public static decimal Score(this int[,] squares)
        {
            var t = 0;
            for (int i = 0; i < SZ; i++)
                for (int j = 0; j < SZ; j++)
                    t += 1 << squares[i, j];
            return t;
        }
        public static int MaxValue(this int[,] squares)
        {
            var max = 0;
            for (int i = 0; i < SZ; i++)
                for (int j = 0; j < SZ; j++)
                    if (squares[i, j] > max)
                        max = squares[i, j];
            return 1 << max;
        }
            //=> squares.Flatten().Max(s => 1 << s);
        public static decimal Reward(this int[,] squares)
        {
            return squares.Score() + squares.CountEmptySquares() * EMPTY_SQUARE_REWARD;
        }

        public static (int i, int j) GetRandomSquare(this int[,] squares, Random rnd = null)
        {
            if (rnd == null) rnd = new Random();
            var emptySquares = squares.GetEmptySquares();
            var len = emptySquares.Count();
            if (len == 0) return (-1, -1); //this isn't allowed
            if (len == 1) return emptySquares[0];

            return emptySquares[rnd.Next(len)];
        }

        public static bool SetRandomSquare(this int[,] squares, float FOUR_CHANCE)
        {
            var rnd = new Random();
            var rs = squares.GetRandomSquare(rnd);
            if (rs == (-1,-1)) return false;
            var val = rnd.NextDouble() > FOUR_CHANCE ? 1 : 2;
            squares[rs.i, rs.j] = val;
            return true;
        }

        public static string AsString(this int[,] squares)
        {
            var result = string.Empty;
            //int sz_x = squares.GetLength(0);
            //int sz_y = squares.GetLength(1);
            for (int x = 0; x < SZ; x++)
            {
                for (int y = 0; y < SZ; y++)
                    result += $"\t{1 << squares[x, y]}";
                result += Environment.NewLine;
            }
            return result;
        }
    }
}
