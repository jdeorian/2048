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
        const byte EMPTY_SQUARE_REWARD = 2;
        static readonly byte sz_byte = sizeof(byte);

        const byte SZ = 4; //this is an abomination, but getting the length of the array so often has a material performance impact, so.... *sigh*
        const byte SZ_2D = 16;

        public static List<(int i, int j)> GetEmptySquares_Slow(this byte[,] squares) => (
            from x in Enumerable.Range(0, SZ)
            from y in Enumerable.Range(0, SZ)
            where squares[x, y] == 0
            select (x, y)
            ).ToList();

        public static List<(byte i, byte j)> GetEmptySquares(this byte[,] squares)
        {
            var cells = new List<(byte i, byte j)>();
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                    if (squares[i, j] == 0) cells.Add((i, j));
            return cells;
        }

        public static byte CountEmptySquares(this byte[,] squares)
        {
            byte count = 0;
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                    if (squares[i, j] == 0) count++;

            return count;
        }

        //=> squares.Flatten().CountEmptySquares();
        public static byte CountEmptySquares(this byte[] squares)
        {
            byte sz_x = SZ_2D;
            byte count = 0;
            for (byte x = 0; x < sz_x; x++)
                if (squares[x] == 0) count++;
            return count;
        }

        public static byte[] Flatten(this byte[,] squares)
        {
            byte[] tmp = new byte[SZ_2D];   //squares.GetLength(0) * squares.GetLength(1)];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D * sz_byte);
            return tmp;
        }

        public static byte[,] Unflatten(this byte[] squares, byte size)
        {
            byte[,] tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[x, y] = squares[y * SZ + x];
            return tmp;
        }

        //reverses x and y
        public static byte[,] Transpose(this byte[,] squares)
        {
            byte[,] tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[y, x] = squares[x, y];
            return tmp;
        }

        //flip vertically
        public static byte[,] Flip(this byte[,] squares)
        {
            //byte sz_x = squares.GetLength(0);
            //byte sz_y = squares.GetLength(1);
            byte[,] tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[x, y] = squares[SZ - x - 1, y];
            return tmp;
        }

        //flip horizontally
        public static byte[,] Invert(this byte[,] squares)
        {
            //byte sz_x = squares.GetLength(0);
            //byte sz_y = squares.GetLength(1);
            byte[,] tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[x, y] = squares[x, SZ - y - 1];
            return tmp;
        }

        public static byte[,] AsCopy(this byte[,] squares)
        {
            byte[,] tmp = new byte[SZ, SZ];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D * sizeof(byte));
            return tmp;
        }

        //always returns a copy, not the original
        public static byte[,] Transform(this byte[,] squares, Direction direction)
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

        public static byte[,] Untransform(this byte[,] squares, Direction direction)
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

        public static byte[] GetRow(this byte[,] squares, byte row)
        {
            //byte sz_y = squares.GetLength(1);
            byte[] tmp = new byte[SZ];
            Buffer.BlockCopy(squares, sz_byte * SZ * row, tmp, 0, sz_byte * SZ);
            return tmp;
        }

        public static void SetRow(this byte[,] squares, byte[] row, byte row_idx)
        {
            Buffer.BlockCopy(row, 0, squares, sz_byte * row_idx * SZ, sz_byte * SZ);
        }

        //public static byte[] GetRowWithoutZeros(this byte[,] squares, byte row_idx) => squares.GetRow(row_idx).GetRowWithoutZeros();
        //public static byte[] GetRowWithoutZeros(this IEnumerable<byte> row) => row.Where(i => i != 0).ToArray();

        public static byte[] GetRowWithoutZeros(this byte[,] squares, int row_idx)
        {
            byte[] tmp = new byte[SZ];
            byte idx = 0;
            for (byte x = 0; x < SZ; x++)
                if (squares[row_idx, x] != 0)
                    tmp[idx++] = squares[row_idx, x];

            //return array with a length equal to the count of non-zero values
            Array.Resize(ref tmp, idx);
            return tmp;
        }
        public static byte[] GetRowWithoutZeros(this List<byte> row)
        {
            byte[] tmp = new byte[SZ];
            byte sz_x = (byte)row.Count();
            byte idx = 0;
            for (byte x = 0; x < sz_x; x++)
                if (row[x] != 0)
                    tmp[idx++] = row[x];

            //return array with a length equal to the count of non-zero values
            Array.Resize(ref tmp, idx);
            return tmp;
        }
            
            //=> row.Where(i => i != 0).ToArray();

        public static bool IsEqualTo(this byte[,] squares1, byte[,] squares2)
        {
            //byte sz_x = squares1.GetLength(0);
            //byte sz_y = squares1.GetLength(1);
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    if (squares1[x, y] != squares2[x, y]) return false;
            return true;
        }

        public static byte[,] Slide(this byte[,] squares, Direction d) => squares.Transform(d).SlideLeft().Untransform(d);

        public static byte[,] SlideLeft(this byte[,] squares)
        {
            //for each row
            //byte sz_x = squares.GetLength(0);
            //byte sz_y = squares.GetLength(1);
            byte[,] tmp = new byte[SZ, SZ];
            for (int x = 0; x < SZ; x++)
            {
                var newRow = new List<byte>();
                var oldRow = squares.GetRowWithoutZeros(x);
                for (int y = 0; y < oldRow.GetLength(0); y++)
                {
                    //skip 0s
                    byte num = oldRow[y];
                    if (num == 0) continue;
                    
                    //handle the case for the last item in the list                        
                    if (y == oldRow.GetUpperBound(0))
                    {
                        newRow.Add(num);
                        break;
                    }

                    //handle pairs
                    if (num == oldRow[y + 1]) //if there's a pair
                    {
                        newRow.Add(++num);
                        oldRow[y + 1] = 0;
                    }
                    else { newRow.Add(num); } //handle singles
                }

                //remove the zeroes and copy them byteo the result matrix
                var newRowCompressed = newRow.GetRowWithoutZeros();
                Buffer.BlockCopy(newRowCompressed, 0, tmp, sz_byte * x * SZ, sz_byte * newRowCompressed.GetLength(0));
            }

            return tmp;
        }

        public static byte[,] AsCopyWithUpdate(this byte[,] squares, int x, int y, byte val)
        {
            var tmp = squares.AsCopy();
            tmp[x, y] = val;
            return tmp;
        }

        public static byte[,] AsCopyWithUpdate(this byte[,] squares, Square s) => squares.AsCopyWithUpdate(s.X, s.Y, s.Value);

        public static IEnumerable<Direction> PossibleMoves(this byte[,] squares)
        {
            return XT.EnumVals<Direction>().Where(d => !squares.Slide(d).IsEqualTo(squares));
        }

        public static float Score(this byte[,] squares)
        {
            var t = 0;
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                    t += 1 << squares[i, j];
            return t;
        }

        public static int[,] AsDisplayValues(this byte[,] squares)
        {
            var retVal = new int[SZ, SZ];
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                    retVal[i, j] = 1 << squares[i, j];
            return retVal;
        }

        public static byte MaxValue(this byte[,] squares)
        {
            byte max = 0;
            for (int i = 0; i < SZ; i++)
                for (int j = 0; j < SZ; j++)
                    if (squares[i, j] > max)
                        max = squares[i, j];
            return max;
        }
            //=> squares.Flatten().Max(s => 1 << s);
        public static float Reward(this byte[,] squares)
        {
            return squares.Score() + squares.CountEmptySquares() * EMPTY_SQUARE_REWARD;
        }

        public static (byte i, byte j) GetRandomSquare(this byte[,] squares, Random rnd = null)
        {
            if (rnd == null) rnd = new Random();
            var emptySquares = squares.GetEmptySquares();
            var len = emptySquares.Count();
            if (len == 0) return (Byte.MaxValue, Byte.MaxValue); //this isn't allowed
            if (len == 1) return emptySquares[0];

            return emptySquares[rnd.Next(len)];
        }

        public static bool SetRandomSquare(this byte[,] squares, float FOUR_CHANCE)
        {
            var rnd = new Random();
            var rs = squares.GetRandomSquare(rnd);
            if (rs == (Byte.MaxValue, Byte.MaxValue)) return false;
            byte val = rnd.NextDouble() > FOUR_CHANCE ? (byte)1 : (byte)2;
            squares[rs.i, rs.j] = val;
            return true;
        }

        public static string AsString(this byte[,] squares, string row_sep = "\n", string col_sep = "\t")
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < SZ; x++)
            {
                sb.Append(string.Join(col_sep,
                                      Enumerable.Range(0, SZ)
                                                .Select(y => $"{(squares[x, y] == 0 ? 0 : 1 << squares[x, y])}")));
                sb.Append(row_sep);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Version without line breaks
        /// </summary>
        /// <param name="squares"></param>
        /// <returns></returns>
        public static string AsFlatString(this byte[,] squares) => squares.AsString("|", ",");

        public static long CanonicalFieldID(this byte[,] squares)
        {
            var ID = squares.FieldID();
            for (int x = 1; x < 4; x++) //for each value of the direction enumerable except the first
            {
                var txID = squares.Slide((Direction)x).FieldID();
                if (txID < ID) ID = txID;
            }
            return ID;
        }

        /// <summary>
        /// This bears some explanation.
        /// 
        /// A given value can be between 0 and 15 (15 being a 32,768 tile). This means it takes
        /// 4 bits. There are 16 tiles, which means we need 64 bits, or 8 bytes. That means we can
        /// represent any board as a long value, which is 8 bytes.
        /// </summary>
        /// <param name="squares"></param>
        /// <returns></returns>
        public static long FieldID(this byte[,] squares)
        {
            long fieldID = default;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    fieldID <<= 4; //bitshift left 4 bits
                    fieldID += squares[i, j];
                }
            return fieldID;
        }
    }
}
