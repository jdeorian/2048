using System;
using System.Text;
using System.Collections.Generic;
using _2048_c_sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Summary description for BranchComparisonTest
    /// </summary>
    [TestClass]
    public class BranchComparisonTest
    {
        [TestMethod]
        public void BranchCountSingleLayer()
        {
            //var seedState = new byte[4, 4] {
            //    {0, 1, 1, 4},
            //    {1, 1, 0, 0},
            //    {0, 1, 2, 1},
            //    {3, 1, 1, 4}
            //};

            var seedState = new byte[4, 4] {
                {1, 1, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}
            };

            var board = new Board(seedState);
            var root = new Move(Direction.Up, null, board) { EndState = board.State };
            var outcomes = root.BuildBranches(1);

            ///move outcomes in layer 1:
            ///left: change with 15 open, 30 possibilities
            ///up: no change(0)
            ///right: change with 15 open, 30 possibilities
            ///down: change with 14 open, 28 possibilities
            ///Total: 88 possibilities

            Assert.AreEqual(outcomes.Count, 88);

        }

        [TestMethod]
        public void BranchCountDoubleLayer()
        {
            var seedState = new byte[4, 4] {
                {1, 2, 4, 3},
                {4, 1, 1, 2},
                {1, 2, 3, 2},
                {7, 5, 2, 1}
            };

            var board = new Board(seedState);
            var root = new Move(Direction.Up, null, board) { EndState = board.State };
            var outcomes = root.BuildBranches(2);

            ///move outcomes in layer 1:
            ///left: change with 1 open, 2 possibilities
            ///up: change with 1 open, 2 possibilities
            ///right: change with 1 open, 2 possibilities
            ///down: change with 1 open, 2 possibilities
            ///Total: 8 possibilities
            ///
            ///move outcomes in layer 2:
            /// (PITA calculation on paper...)

            Assert.AreEqual(outcomes.Count, 88);
        }

        [TestMethod]
        public void BranchCountZeroOutcomes()
        {
            var seedState = new byte[4, 4] {
                {1, 2, 4, 3},
                {4, 5, 1, 2},
                {1, 2, 3, 9},
                {7, 5, 2, 1}
            };

            var board = new Board(seedState);
            for (int x = 1; x <= 3; x++)
            {
                var outcomes = new Move(Direction.Up, null, board) { EndState = board.State }.BuildBranches(x);
                Assert.AreEqual(outcomes.Count, 0);
            }
        }

        [TestMethod]
        public void BranchCountLosingBoard()
        {
            var seedState = new byte[4, 4] {
                {1, 2, 4, 3},
                {4, 5, 1, 2},
                {6, 2, 7, 9},
                {7, 5, 2, 2}
            };

            var board = new Board(seedState);
            for (int x = 1; x <= 3; x++)
            {
                var outcomes = new Move(Direction.Up, null, board) { EndState = board.State }.BuildBranches(x);
                Assert.AreEqual(outcomes.Count, 4);
            }
        }

        [TestMethod]
        public void BranchCountTripleLayer()
        {
            var seedState = new byte[4, 4] {
                {1, 6, 4, 3},
                {1, 5, 1, 2},
                {1, 8, 3, 9},
                {2, 5, 2, 1}
            };

            var board = new Board(seedState);
            var root = new Move(Direction.Up, null, board) { EndState = board.State };
            var outcomes = root.BuildBranches(3);

            ///move outcomes in layer 1:
            ///left: 0 changes
            ///up: 1, 2
            ///right: 0 changes
            ///down: 1, 2
            ///Total: 4 possibilities
            ///
            ///move outcomes in layer 2 & 3 by hand

            Assert.AreEqual(outcomes.Count, 58);
        }
    }
}
