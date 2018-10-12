using System;
using _2048_c_sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class FieldTests
    {
        [TestMethod]
        public void EmptySquaresCount()
        {
            byte[,] sq = new byte[4, 4];
            sq[3, 1] = 2;
            sq[2, 1] = 6;
            sq[0, 0] = 9;

            Assert.AreEqual(13, sq.GetEmptySquares().Count);
        }

        [TestMethod]
        public void EqualityCheck()
        {
            byte[,] sq = new byte[4, 4] {
                {0, 1, 1, 0},
                {1, 1, 0, 0},
                {0, 0, 2, 1},
                {3, 1, 1, 4}
            };

            byte[,] sq2 = new byte[4, 4] {
                {0, 1, 1, 0},
                {1, 1, 0, 0},
                {0, 0, 2, 1},
                {3, 1, 1, 4}
            };

            Assert.IsTrue(sq.IsEqualTo(sq2));
        }

        [TestMethod]
        public void Sliding()
        {
            byte[,] sq = new byte[4, 4] {
                {0, 1, 1, 0},
                {2, 2, 1, 1},
                {0, 0, 2, 1},
                {3, 1, 1, 4}
            };

            byte[,] sq2 = new byte[4, 4] {
                {2, 0, 0, 0},
                {3, 2, 0, 0},
                {2, 1, 0, 0},
                {3, 2, 4, 0}
            };

            Assert.IsTrue(sq.SlideLeft().IsEqualTo(sq2));
        }

        [TestMethod]
        public void Transform()
        {
            byte[,] sq = new byte[4, 4] {
                {  0,  1,  2,  3 },
                {  4,  5,  6,  7 },
                {  8,  9, 10, 11 },
                { 12, 13, 14, 15 }
            };

            byte[,] sq_left = new byte[4, 4] {
                {  0,  1,  2,  3 },
                {  4,  5,  6,  7 },
                {  8,  9, 10, 11 },
                { 12, 13, 14, 15 }
            };
            var result = sq.Transform(Direction.Left);
            Assert.IsTrue(result.IsEqualTo(sq_left));

            byte[,] sq_right = new byte[4, 4] {
                { 15, 14, 13, 12 },
                { 11, 10,  9,  8 },
                {  7,  6,  5,  4 },
                {  3,  2,  1,  0 }
            };
            result = sq.Transform(Direction.Right);
            Assert.IsTrue(result.IsEqualTo(sq_right) || result.IsEqualTo(sq_right.Flip()));

            byte[,] sq_up = new byte[4, 4] {
                {  3,  7, 11, 15 },
                {  2,  6, 10, 14 },
                {  1,  5,  9, 13 },
                {  0,  4,  8, 12 }
            };
            result = sq.Transform(Direction.Up);
            Assert.IsTrue(result.IsEqualTo(sq_up));

            byte[,] sq_down = new byte[4, 4] {
                { 12,  8,  4,  0 },
                { 13,  9,  5,  1 },
                { 14, 10,  6,  2 },
                { 15, 11,  7,  3 }
            };
            result = sq.Transform(Direction.Down);
            Assert.IsTrue(result.IsEqualTo(sq_down));
        }

        [TestMethod]
        public void Untransform()
        {
            byte[,] sq = new byte[4, 4] {
                {  0,  1,  2,  3 },
                {  4,  5,  6,  7 },
                {  8,  9, 10, 11 },
                { 12, 13, 14, 15 }
            };

            byte[,] sq_left = new byte[4, 4] {
                {  0,  1,  2,  3 },
                {  4,  5,  6,  7 },
                {  8,  9, 10, 11 },
                { 12, 13, 14, 15 }
            };
            var result = sq.Transform(Direction.Left).Untransform(Direction.Left);
            Assert.IsTrue(result.IsEqualTo(sq));

            byte[,] sq_right = new byte[4, 4] {
                { 15, 14, 13, 12 },
                { 11, 10,  9,  8 },
                {  7,  6,  5,  4 },
                {  3,  2,  1,  0 }
            };
            result = sq.Transform(Direction.Right).Untransform(Direction.Right);
            Assert.IsTrue(result.IsEqualTo(sq) || result.IsEqualTo(sq_right.Flip()));

            byte[,] sq_up = new byte[4, 4] {
                {  3,  7, 11, 15 },
                {  2,  6, 10, 14 },
                {  1,  5,  9, 13 },
                {  0,  4,  8, 12 }
            };
            result = sq.Transform(Direction.Up).Untransform(Direction.Up);
            Assert.IsTrue(result.IsEqualTo(sq));

            byte[,] sq_down = new byte[4, 4] {
                { 12,  8,  4,  0 },
                { 13,  9,  5,  1 },
                { 14, 10,  6,  2 },
                { 15, 11,  7,  3 }
            };
            result = sq.Transform(Direction.Down).Untransform(Direction.Down);
            Assert.IsTrue(result.IsEqualTo(sq));
        }
    }
}
