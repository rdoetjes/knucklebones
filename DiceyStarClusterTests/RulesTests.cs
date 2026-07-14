using Xunit;
using DiceyStarCluster;

namespace DiceyStarClusterTests
{
    public class RulesTests
    {
        [Fact]
        public void CalculateScore_SingleDie_ReturnsValue()
        {
            int[][] board = new int[3][] { new int[3], new int[3], new int[3] };
            board[0][0] = 5;
            
            // Row 0 has a 5 (score 5) 
            // Col 0 has a 5 (score 5)
            // Max is 5
            Assert.Equal(5, Rules.CalculateScore(board));
        }

        [Fact]
        public void CalculateScore_DoubleInRow_ReturnsMultipliedScore()
        {
            // Row 0 has two 5s in Col 0 and Col 1
            int[][] board = new int[3][] { 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 0, 0, 0 } 
            };
            
            // Row 0: two 5s = (5*2)*2 = 20
            // Col 0: one 5 = 5
            // Col 1: one 5 = 5
            // Rows total = 20
            // Cols total = 5 + 5 = 10
            // Max = 20
            Assert.Equal(20, Rules.CalculateScore(board));
        }

        [Fact]
        public void CalculateScore_TripleInRow_ReturnsMultipliedScore()
        {
            // Row 0 has three 6s
            int[][] board = new int[3][] { 
                new int[3] { 6, 0, 0 }, 
                new int[3] { 6, 0, 0 }, 
                new int[3] { 6, 0, 0 } 
            };
            
            // Row 0: three 6s = (6*3)*3 = 54
            // Col 0, 1, 2: one 6 each = 6*3 = 18
            // Rows total = 54
            // Cols total = 18
            // Max = 54
            Assert.Equal(54, Rules.CalculateScore(board));
        }

        [Fact]
        public void CalculateScore_UserExample_ReturnsHigherAxis()
        {
            // Rows breakdown:
            // Row 0: 5, 1, 5 -> (5*2)*2 + 1 = 21
            // Row 1: 3, 1, 6 -> 3 + 1 + 6 = 10 (+10 Bonus = 20)
            // Row 2: 3, 6, 6 -> 3 + (6*2)*2 = 27
            // Total Rows = 21 + 20 + 27 = 68
            
            // Cols breakdown:
            // Col 0: 5, 3, 3 -> 5 + (3*2)*2 = 17
            // Col 1: 1, 1, 6 -> (1*2)*2 + 6 = 10
            // Col 2: 5, 6, 6 -> 5 + (6*2)*2 = 29
            // Total Cols = 17 + 10 + 29 = 56
            
            // Max(56, 68) = 68
            int[][] board = new int[3][] { 
                new int[3] { 5, 3, 3 }, 
                new int[3] { 1, 1, 6 }, 
                new int[3] { 5, 6, 6 } 
            };
            
            Assert.Equal(68, Rules.CalculateScore(board));
        }

        [Fact]
        public void HandleDestruction_RemovesMatchingOpponentDiceInSameRowAndCol()
        {
            // opponentBoard is [col][row]
            int[][] opponentBoard = new int[3][] { 
                new int[3] { 5, 5, 0 }, // Col 0: Row0=5, Row1=5
                new int[3] { 5, 0, 0 }, // Col 1: Row0=5
                new int[3] { 1, 0, 0 }  // Col 2: Row0=1
            };
            
            // Player places a 5 in Col 0, Row 0
            Rules.HandleDestruction(0, 0, 5, opponentBoard);
            
            Assert.Equal(0, opponentBoard[0][0]); // Same cell
            Assert.Equal(0, opponentBoard[1][0]); // Same row (Row 0, Col 1)
            Assert.Equal(0, opponentBoard[0][1]); // Same col (Col 0, Row 1)
            Assert.Equal(1, opponentBoard[2][0]); // Same row, different value (unchanged)
        }
    }
}
