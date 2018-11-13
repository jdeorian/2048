using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class ULongMagic
    {
        #region Constants

        const byte SZ_BITS = 4; //the number of bits used for each value
        const byte SZ_ROW = 4; //this is an abomination, but getting the length of the array so often has a material performance impact, so.... *sigh*
        const byte SZ_FLD = SZ_ROW * SZ_ROW;
        const byte SZ_FLD_BITS = SZ_FLD * SZ_BITS;
        const byte SZ_ROW_BITS = SZ_BITS * SZ_ROW;

        #endregion

        #region Transforming

        public static ulong Invert(this ulong state)
        {
            ulong c1 = state & 0xF000F000F000F000L;
            ulong c2 = state & 0x0F000F000F000F00L;
            ulong c3 = state & 0x00F000F000F000F0L;
            ulong c4 = state & 0x000F000F000F000FL;

            return (c1 >> 12) | (c2 >> 4) | (c3 << 4) | (c4 << 12);
        }

        //flip vertically
        public static ulong Flip(this ulong state)
        {
            ulong r1 = state & 0xFFFF000000000000L;
            ulong r2 = state & 0x0000FFFF00000000L;
            ulong r3 = state & 0x00000000FFFF0000L;
            ulong r4 = state & 0x000000000000FFFFL;

            return (r1 >> (SZ_FLD_BITS - SZ_ROW_BITS)) | (r2 >> SZ_ROW_BITS) | (r3 << SZ_ROW_BITS) | (r4 << (SZ_FLD_BITS - SZ_ROW_BITS));
        }

        public static ulong Transpose(this ulong state)
        {
            return state & 0xF0000F0000F0000FL //unchanged diagonals
                | (state & 0x0F0000F0000F0000L) >> 12
                | (state & 0x0000F0000F0000F0L) << 12
                | (state & 0x00F0000F00000000L) >> 24
                | (state & 0x00000000F0000F00L) << 24
                | (state & 0x000F000000000000L) >> 36
                | (state & 0x000000000000F000L) << 36;
        }

        public static ulong Transform(this ulong fld, Direction direction)
        {
            switch (direction)
            {
                case Direction.Down: return fld.Transpose().Invert();
                case Direction.Up: return fld.Transpose().Flip();                
                case Direction.Right: return fld.Invert();
                default:
                case Direction.Left: return fld;
            }
        }

        public static ulong Untransform(this ulong fld, Direction direction)
        {
            switch (direction)
            {
                case Direction.Down: return fld.Invert().Transpose();
                case Direction.Up: return fld.Flip().Transpose();                
                case Direction.Right: return fld.Invert();
                default:
                case Direction.Left: return fld;
            }
        }

        public static IEnumerable<Direction> PossibleMoves(this ulong fld)
            => XT.EnumVals<Direction>().Where(d => fld.Slide(d) != fld);

        #endregion

        #region Board Management

        /// <summary>
        /// Gets the ushort state from a ulong state given a row number.
        /// </summary>
        /// <param name="fld">The field state representation.</param>
        /// <param name="row_num">This is not a 0-based index, but the actual 1-based row number</param>
        /// <returns></returns>
        public static ushort GetRow(this ulong fld, byte row_num)
        {
            ushort offset = (ushort)(SZ_FLD_BITS - (SZ_ROW_BITS * row_num));
            return (ushort)((fld >> offset) & 0x000000000000FFFFul); //offset and clear everything to the left
        }

        public static ulong SetRow(this ulong fld, byte row_num, ushort value)
        {            
            fld &= GetRowMask(row_num, out int offset); //set row to 0            
            fld |= (ulong)value << offset; //set row to correct value
            return fld;
        }

        //columns and rows are 1-indexed, positions are 0-based
        public static ulong SetTile(this ulong fld, byte pos, byte value)
        {
            fld &= ~GetTileMask(pos, out int offset); //set tile to 0        
            fld |= (ulong)(value & 0x0F) << offset; //set tile to correct value
            return fld;
        }

        //columns and rows are 1-indexed, positions are 0-based
        public static byte GetTile(this ulong fld, byte pos)
        {
            fld >>= GetTileOffset(pos);
            return (byte)(fld & 0x0F);
        }

        public static byte GetTile(this ulong fld, byte row_num, byte col_num) => fld.GetTile(getPos(row_num, col_num));
        public static ulong SetTile(this ulong fld, byte row_num, byte col_num, byte value) => fld.SetTile(getPos(row_num, col_num), value);

        private static byte getPos(byte row_num, byte col_num) => (byte)(((row_num - 1) * SZ_ROW) + (col_num - 1));

        /// <param name="pos">0-based position of the tile</param>
        private static ulong GetTileMask(byte pos, out int offset) => GetTileMask(pos, (1 << SZ_BITS) - 1, out offset);
        private static ulong GetTileMask(byte pos, byte val, out int offset) //only last 4 bits of val are used
        {
            ulong mask = val & 0x0FUL; //position 15 mask (final position)
            offset = GetTileOffset(pos);
            mask <<= offset;
            return mask;
        }

        private static int GetTileOffset(byte pos) => (SZ_FLD - pos - 1) * SZ_BITS;
        private static int GetRowOffset(byte row_num) => SZ_ROW_BITS * (SZ_ROW - row_num); //rows are 1-based

        /// <summary>
        /// Gets a mask with 1s set to the specified row. The offset is returned
        /// so it can be used to set the value as well without recalculating.
        /// </summary>
        /// <param name="row_num"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private static ulong GetRowMask(byte row_num, out int offset)
        {
            offset = GetRowOffset(row_num);
            return ~((ulong)ushort.MaxValue << offset);
        }

        public static byte MaxValue(this ulong fld)
        {           
            var mask = GetTileMask(SZ_FLD - 1, out int _);
            byte retVal = (byte)(fld & mask);
            for (int x = 1; x < SZ_FLD; x++)
            {
                fld >>= SZ_BITS;
                byte val = (byte)(fld & mask);
                if (val > retVal) retVal = val;
            }
            return retVal;
        }

        public static byte[,] ToByteArray(this ulong fieldID)
        {
            var flatRetVal = new byte[SZ_FLD];
            for (byte x = 1; x <= SZ_FLD; x++)
            {
                flatRetVal[x - 1] = (byte)(
                    (fieldID >> (SZ_BITS * (SZ_FLD - x)))
                    & 0x0F); //bit shift and take only the last 4 bits
            }
            return flatRetVal.Unflatten();
        }

        public static uint[,] ToIntArrayDisplayValues(this ulong fieldID)
        {
            var flatRetVal = new uint[SZ_FLD];
            for (byte x = 1; x <= SZ_FLD; x++)
            {
                int val = (int)((fieldID >> (SZ_BITS * (SZ_FLD - x)))
                                 & 0x0F); //bit shift and take only the last 4 bits
                if (val > 0)
                    flatRetVal[x - 1] = 0x1u << val;
            }
            var retVal = new uint[SZ_ROW, SZ_ROW];
            Buffer.BlockCopy(flatRetVal, 0, retVal, 0, SZ_FLD * sizeof(uint));
            return retVal;
        }

        #endregion

        #region Random Gets & Sets

        /// <summary>
        /// Returns 16 bit integer where each position represents a flag. 1 means empty.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ushort GetEmptySquareFlags(this ulong state)
        {
            ushort retVal = default;
            for (int x = 0; x < SZ_FLD; x++)
            {
                //if the last 4 bits are empty, set a bit in the return value
                if ((state & 0xFL) == 0)
                    retVal |= (ushort)(1 << (SZ_FLD - x));
                state >>= SZ_BITS;
            }
            return retVal;
        }

        //returns an array of empty positions
        public static byte[] GetEmptySquares(this ulong state)
        {
            var retval = new byte[SZ_FLD];
            var es_idx = 0;
            for (int x = 0; x < SZ_FLD; x++)
            {
                //if the last 4 bits are empty, set a bit in the return value
                if ((state & 0xFL) == 0)
                    retval[es_idx++] = (byte)(SZ_FLD - 1 - x);
                state >>= SZ_BITS;
            }
            Array.Resize(ref retval, es_idx);
            return retval;
        }

        public static byte CountEmptySquares(this ulong state)
        {
            byte retVal = default;
            for (int x = 0; x < SZ_FLD; x++)
            {
                //if the last 4 bits are empty, set a bit in the return value
                if ((state & 0xFL) == 0) retVal++;
                state >>= SZ_BITS;
            }
            return retVal;
        }

        //returns the 0-based position of a random empty square
        public static byte GetRandomSquare(this ulong fld, Random rnd = null)
        {
            if (rnd == null) rnd = new Random();
            var emptySquares = fld.GetEmptySquares();
            var len = emptySquares.Count();
            switch (len)
            {
                case 0:
                    return byte.MaxValue; //this isn't allowed
                case 1:
                    return emptySquares[0];
                default:
                    return emptySquares[rnd.Next(len)];
            }
        }

        public static bool SetRandomSquare(ref ulong fld, float FOUR_CHANCE, Random rnd = null)
        {
            rnd = rnd ?? new Random();
            var rs = fld.GetRandomSquare(rnd);
            if (rs == byte.MaxValue) return false;
            var val = rnd.NextDouble() > FOUR_CHANCE ? (byte)1 : (byte)2;
            fld = fld.SetTile(rs, val);
            return true;
        }

        public static string AsBoardString(this ulong fld) => fld.ToByteArray().AsString();
        public static string AsFlatBoardString(this ulong fld) => fld.ToByteArray().AsString("|", ",");

        #endregion

        #region Scoring

        const byte EMPTY_SQUARE_REWARD = 2;

        public static float Reward(this ulong fld) => fld.Score() + fld.CountEmptySquares() * EMPTY_SQUARE_REWARD;

        public static float Score(this ulong fld)
        {
            var mask = 0xFUL; //last 4 bits only
            float retVal = 0;
            for (int x = 0; x < SZ_FLD; x++)
            {                
                byte val = (byte)(fld & mask);
                if (val > 0)
                    retVal += 0x1UL << val;
                fld >>= SZ_BITS;
            }
            return retVal;
        }

        #endregion

        #region Sliding

        public static ulong Slide(this ulong fld, Direction d) => fld.Transform(d).SlideLeft().Untransform(d);

        ///Encode a full row state as two bytes; this is the index.
        ///And the resulting row as two bytes, stored in the array (upper bound of short max value).
        //the row state (as a ushort) is the index which can be used to get the resulting state
        private static ushort[] buildSlideTable()
            => Enumerable.Range(0, ushort.MaxValue + 1)
                         .Select(i => ((ushort)i).GetRowFromShort()
                                                 .SlideRowLeft()
                                                 .GetRowAsShort())
                         .ToArray();

        public static ushort[] SlideTable = buildSlideTable();

        public static byte[,] SlideLeft_Alt(this byte[,] squares)
        {
            var tmp = new byte[4, 4];
            for (byte x = 0; x < 4; x++)
            {
                var newRow = SlideTable[squares.GetRow(x).GetRowAsShort()].GetRowFromShort();
                Buffer.BlockCopy(newRow, 0, tmp, x * 4, newRow.Length);
            }

            return tmp;
        }

        public static ulong SlideLeft(this ulong fld)
        {
            ulong retVal = default;
            for (byte x = 1; x <= SZ_ROW; x++)
            {
                var newRow = SlideTable[fld.GetRow(x)];
                retVal <<= SZ_ROW_BITS;
                retVal |= newRow;
            }

            return retVal;
        }

        #endregion
    }
}
