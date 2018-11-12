using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class ArrayMagic
    {
        const byte SZ = 4; //this is an abomination, but getting the length of the array so often has a material performance impact, so.... *sigh*
        const byte SZ_2D = 16;

        public static byte[] Flatten(this byte[,] squares)
        {
            var tmp = new byte[SZ_2D];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D);
            return tmp;
        }

        public static byte[,] Unflatten(this byte[] squares)//, byte size)
        {
            var tmp = new byte[SZ, SZ];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D);
            return tmp;
        }

        //reverses x and y
        public static byte[,] Transpose(this byte[,] squares)
        {
            var tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[y, x] = squares[x, y];
            return tmp;
        }

        //flip vertically
        public static byte[,] Flip(this byte[,] squares)
        {
            var tmp = new byte[SZ, SZ];
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
            var tmp = new byte[SZ, SZ];
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    tmp[x, y] = squares[x, SZ - y - 1];
            return tmp;
        }


        public static byte[,] Invert_InPlace(this byte[,] squares)
        {
            ulong state = squares.FieldID();
            ulong ulResult = state.Invert();
            return ulResult.FromFieldID();
        }



        public static byte[,] AsCopy(this byte[,] squares)
        {
            var tmp = new byte[SZ, SZ];
            Buffer.BlockCopy(squares, 0, tmp, 0, SZ_2D * sizeof(byte));
            return tmp;
        }

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

        public static byte CountEmptySquares(this byte[] squares)
        {
            var sz_x = SZ_2D;
            byte count = 0;
            for (byte x = 0; x < sz_x; x++)
                if (squares[x] == 0) count++;
            return count;
        }

        public static byte[] GetRow(this byte[,] squares, byte row)
        {
            //byte sz_y = squares.GetLength(1);
            var tmp = new byte[SZ];
            Buffer.BlockCopy(squares, SZ * row, tmp, 0, SZ);
            return tmp;
        }

        public static void SetRow(this byte[,] squares, byte[] row, byte row_idx)
        {
            Buffer.BlockCopy(row, 0, squares, row_idx * SZ, SZ);
        }

        public static byte[] GetRowWithoutZeros(this byte[,] squares, int row_idx)
        {
            byte[] row = new byte[SZ];
            Buffer.BlockCopy(squares, row_idx * SZ, row, 0, SZ);
            return row.GetRowWithoutZeros();
        }

        public static byte[] GetRowWithoutZeros(this byte[] row)
        {
            var tmp = new byte[SZ];
            byte idx = 0;
            for (byte x = 0; x < row.Length; x++)
                if (row[x] != 0)
                    tmp[idx++] = row[x];

            //return array with a length equal to the count of non-zero values
            Array.Resize(ref tmp, idx);
            return tmp;
        }

        public static byte[] GetRowWithoutZeros(this List<byte> row)
        {
            var tmp = new byte[SZ];
            var sz_x = (byte)row.Count();
            byte idx = 0;
            for (byte x = 0; x < sz_x; x++)
                if (row[x] != 0)
                    tmp[idx++] = row[x];

            //return array with a length equal to the count of non-zero values
            Array.Resize(ref tmp, idx);
            return tmp;
        }

        public static bool IsEqualTo(this byte[,] squares1, byte[,] squares2)
        {
            //byte sz_x = squares1.GetLength(0);
            //byte sz_y = squares1.GetLength(1);
            for (byte x = 0; x < SZ; x++)
                for (byte y = 0; y < SZ; y++)
                    if (squares1[x, y] != squares2[x, y]) return false;
            return true;
        }

        public static byte[,] AsCopyWithUpdate(this byte[,] squares, int x, int y, byte val)
        {
            var tmp = squares.AsCopy();
            tmp[x, y] = val;
            return tmp;
        }

        public static byte MaxValue(this byte[,] squares)
        {
            byte max = 0;
            for (var i = 0; i < SZ; i++)
                for (var j = 0; j < SZ; j++)
                    if (squares[i, j] > max)
                        max = squares[i, j];
            return max;
        }

        public static byte[,] GetRandom(Random rnd)
        {
            var retVal = new byte[SZ, SZ];
            for (byte i = 0; i < SZ; i++)
                for (byte j = 0; j < SZ; j++)
                {
                    retVal[i, j] = (byte)rnd.Next(0, 10); //technically 15, but this is more useful
                }
            return retVal;
        }

    }
}
