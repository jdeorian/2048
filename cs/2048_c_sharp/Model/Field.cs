using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class FieldExtensions
    {
        //scoring parameters
        const byte EMPTY_SQUARE_REWARD = 2;
        static readonly byte sz_byte = sizeof(byte);

        const byte SZ = 4; //this is an abomination, but getting the length of the array so often has a material performance impact, so.... *sigh*
        const byte SZ_2D = 16;

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

        public static byte[,] Slide(this byte[,] squares, Direction d) => squares.Transform(d).SlideLeft().Untransform(d);

        public static byte[,] SlideLeft(this byte[,] squares)
        {
            var tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
            {
                var newRow = squares.GetRow(x).SlideRowLeft();
                Buffer.BlockCopy(newRow, 0, tmp, x * SZ, newRow.Length);
            }

            return tmp;
        }

        public static byte[] SlideRowLeft(this byte[] row)
        {
            var newRow = new List<byte>();
            var oldRow = row.GetRowWithoutZeros();
            for (var y = 0; y < oldRow.GetLength(0); y++)
            {
                //skip 0s
                var num = oldRow[y];
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
            return newRow.ToArray().GetRowWithoutZeros();
        }

        public static IEnumerable<Direction> PossibleMoves(this byte[,] squares)
            => XT.EnumVals<Direction>().Where(d => !squares.Slide(d).IsEqualTo(squares));

        public static float Score(this byte[,] squares) => squares.AsDisplayValues().Cast<int>().Sum();

        public static int[,] AsDisplayValues(this byte[,] squares)
        {
            var retVal = new int[SZ, SZ];
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                {
                    var val = 0;
                    if ((val = 1 << squares[i, j]) > 1) //prevents 1s from being added
                        retVal[i, j] = val;
                }
            return retVal;
        }

        public static float Reward(this byte[,] squares)
            => squares.Score() + squares.CountEmptySquares() * EMPTY_SQUARE_REWARD;

        public static (byte i, byte j) GetRandomSquare(this byte[,] squares, Random rnd = null)
        {
            if (rnd == null) rnd = new Random();
            var emptySquares = squares.GetEmptySquares();
            var len = emptySquares.Count();
            switch (len)
            {
                case 0:
                    return (byte.MaxValue, byte.MaxValue); //this isn't allowed
                case 1:
                    return emptySquares[0];
                default:
                    return emptySquares[rnd.Next(len)];
            }
        }

        public static bool SetRandomSquare(this byte[,] squares, float FOUR_CHANCE)
        {
            var rnd = new Random();
            var rs = squares.GetRandomSquare(rnd);
            if (rs == (byte.MaxValue, byte.MaxValue)) return false;
            var val = rnd.NextDouble() > FOUR_CHANCE ? (byte)1 : (byte)2;
            squares[rs.i, rs.j] = val;
            return true;
        }

        public static string AsString(this byte[,] squares, string row_sep = "\n", string col_sep = "\t")
        {
            var sb = new StringBuilder();
            var disp = squares.AsDisplayValues();
            for (var x = 0; x < SZ; x++)
            {
                sb.Append(string.Join(col_sep,
                                      Enumerable.Range(0, SZ)
                                                .Select(y => $"{disp[x, y]}")));
                sb.Append(row_sep);
            }
            return sb.ToString().TrimEnd(); //we know by default we'll have a trailing newline
        }

        /// <summary>
        /// Version without line breaks
        /// </summary>
        /// <param name="squares"></param>
        /// <returns></returns>
        public static string AsFlatString(this byte[,] squares) => squares.AsString("|", ",");

        public static ulong CanonicalFieldID(this byte[,] squares)
        {
            var ID = squares.FieldID();
            for (var x = 1; x < 4; x++) //for each value of the direction enumerable except the first
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
        public static ulong FieldID(this byte[,] squares)
        {
            ulong fieldID = default;
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                {
                    fieldID <<= 4; //bitshift left 4 bits
                    fieldID += squares[i, j];
                }
            return fieldID;
        }

        public static byte[,] FromFieldID(this ulong fieldID)
        {
            var flatRetVal = new byte[SZ_2D];
            for (byte x = 1; x <= SZ_2D; x++)
            {
                flatRetVal[x - 1] = (byte)(
                    (fieldID >> (SZ * (SZ_2D - x)))
                    & 0x0F); //bit shift and take only the last 4 bits
            }
            return flatRetVal.Unflatten();
        }

        public static ushort GetRowAsShort(this byte[,] squares, byte row) => squares.GetRow(row).GetRowAsShort();

        public static ushort GetRowAsShort(this byte[] row)
        {
            Array.Resize(ref row, SZ); //make sure this is the correct length so it is offset correctly
            ushort retVal = row[0];
            for (byte x = 1; x < row.Length; x++)
            {
                retVal <<= SZ;
                retVal |= row[x];
            }
            return retVal;
        }

        public static byte[] GetRowFromShort(this ushort row)
        {
            var retVal = new byte[SZ];
            for (byte x = 1; x <= SZ; x++)
            {
                retVal[x-1] = (byte)(
                    (row >> (SZ * (SZ - x)))
                    & 0x0F); //bit shift and take only the last 4 bits
            }
            return retVal;
        }
    }
}
