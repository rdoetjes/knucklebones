using System.Collections.Generic;

namespace KnuckleBones
{
    public static class Rules
    {
        public static int CalculateScore(int[][] board)
        {
            int total = 0;
            // Scoring is calculated per ROW (horizontal combos)
            for (int row = 0; row < 3; row++)
            {
                var counts = new Dictionary<int, int>();
                for (int col = 0; col < 3; col++)
                {
                    int val = board[col][row];
                    if (val > 0)
                    {
                        if (!counts.ContainsKey(val)) counts[val] = 0;
                        counts[val]++;
                    }
                }

                foreach (var pair in counts)
                {
                    int val = pair.Key;
                    int count = pair.Value;
                    
                    // User Rule: Matching dice in a row are summed and then multiplied by their count.
                    // Example: Three 6s = (6 + 6 + 6) * 3 = 18 * 3 = 54.
                    // Example: Two 3s = (3 + 3) * 2 = 12.
                    // Example: One 4 = 4.
                    total += (val * count) * count;
                }
            }
            return total;
        }

        public static void HandleDestruction(int row, int dieValue, int[][] opponentBoard)
        {
            // Rule: "Whenever the player places a die in a row and the opponent has 
            // that same number in that same row, the opponent's die with that same number need to be removed"
            for (int col = 0; col < 3; col++)
            {
                if (opponentBoard[col][row] == dieValue)
                {
                    opponentBoard[col][row] = 0;
                }
            }
        }

        public static bool IsBoardFull(int[][] board)
        {
            for (int c = 0; c < 3; c++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (board[c][r] == 0) return false;
                }
            }
            return true;
        }
    }
}
