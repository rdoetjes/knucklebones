using Xunit;
using KnuckleBones;

namespace KnuckleBonesTests
{
    public class RulesTests
    {
        [Fact]
        public void CalculateScore_SingleDie_ReturnsValue()
        {
            int[][] board = new int[3][] { new int[3], new int[3], new int[3] };
            board[0][0] = 5;
            
            Assert.Equal(5, Rules.CalculateScore(board));
        }

        [Fact]
        public void CalculateScore_DoubleInRow_ReturnsMultipliedScore()
        {
            // Row 0 has two 5s
            int[][] board = new int[3][] { 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 0, 0, 0 } 
            };
            
            // Per Rules.cs: (5 * 2) * 2 = 20
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
            
            // Per Rules.cs: (6 * 3) * 3 = 54
            Assert.Equal(54, Rules.CalculateScore(board));
        }

        [Fact]
        public void CalculateScore_UserExample_Returns58()
        {
            // Row 0: 5 1 5 => (5*2)*2 + 1 = 21
            // Row 1: 3 1 6 => 3+1+6 = 10
            // Row 2: 3 6 6 => 3 + (6*2)*2 = 27
            // Total: 21 + 10 + 27 = 58
            int[][] board = new int[3][] { 
                new int[3] { 5, 3, 3 }, 
                new int[3] { 1, 1, 6 }, 
                new int[3] { 5, 6, 6 } 
            };
            
            Assert.Equal(58, Rules.CalculateScore(board));
        }

        [Fact]
        public void HandleDestruction_RemovesMatchingOpponentDiceInSameRow()
        {
            int[][] opponentBoard = new int[3][] { 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 5, 0, 0 }, 
                new int[3] { 1, 0, 0 } 
            };
            
            // Player places a 5 in row 0
            Rules.HandleDestruction(0, 5, opponentBoard);
            
            Assert.Equal(0, opponentBoard[0][0]);
            Assert.Equal(0, opponentBoard[1][0]);
            Assert.Equal(1, opponentBoard[2][0]); // Row 0, Col 2 (unchanged)
        }
    }
}
