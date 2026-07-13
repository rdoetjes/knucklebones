using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System;

namespace KnuckleBones
{
    public class AI
    {
        public static int GetMove(GameState state)
        {
            int depth = (int)state.CurrentDifficulty;
            // Use a random seed for tie-breaking so it's not always picking the first column
            var (bestScore, bestCol) = Minimax(state.Player1Board, state.Player2Board, state.CurrentDie, depth, false);
            return bestCol;
        }

        // Knucklebones is a game of perfect information regarding current state but random future (dice).
        // For a stronger "Hard" AI, we use Expectiminimax or a weighted heuristic.
        private static (int score, int col) Minimax(int[][] p1Board, int[][] p2Board, int currentDie, int depth, bool p1Turn)
        {
            List<int> availableCols = GetAvailableCols(p1Turn ? p1Board : p2Board);

            if (availableCols.Count == 0)
            {
                return (Rules.CalculateScore(p2Board) - Rules.CalculateScore(p1Board), -1);
            }

            int bestScore = p1Turn ? int.MaxValue : int.MinValue;
            List<int> tiedCols = new List<int>();

            foreach (int col in availableCols)
            {
                int[][] nextP1 = CloneBoard(p1Board);
                int[][] nextP2 = CloneBoard(p2Board);
                
                // Simulate placing die in the FIRST available row of the column
                // (Since we don't choose the row, just the column)
                int row = GetFirstEmptyRow(p1Turn ? nextP1 : nextP2, col);
                
                if (p1Turn) 
                {
                    nextP1[col][row] = currentDie;
                    Rules.HandleDestruction(row, currentDie, nextP2);
                }
                else 
                {
                    nextP2[col][row] = currentDie;
                    Rules.HandleDestruction(row, currentDie, nextP1);
                }

                int scoreAfterMove;
                
                // If we have depth remaining, we should ideally look at all possible next dice (1-6)
                // and take the average (Expectiminimax). 
                if (depth > 1 && !Rules.IsBoardFull(nextP1) && !Rules.IsBoardFull(nextP2))
                {
                    long averageScore = 0;
                    for (int nextDie = 1; nextDie <= 6; nextDie++)
                    {
                        var (resScore, _) = Minimax(nextP1, nextP2, nextDie, depth - 1, !p1Turn);
                        averageScore += resScore;
                    }
                    scoreAfterMove = (int)(averageScore / 6);
                }
                else
                {
                    // Leaf node or end of depth: evaluate board state
                    scoreAfterMove = Rules.CalculateScore(nextP2) - Rules.CalculateScore(nextP1);
                }
                
                if (!p1Turn) // AI maximizing (p2 - p1)
                {
                    if (scoreAfterMove > bestScore)
                    {
                        bestScore = scoreAfterMove;
                        tiedCols.Clear();
                        tiedCols.Add(col);
                    }
                    else if (scoreAfterMove == bestScore)
                    {
                        tiedCols.Add(col);
                    }
                }
                else // Player minimizing (p2 - p1)
                {
                    if (scoreAfterMove < bestScore)
                    {
                        bestScore = scoreAfterMove;
                        tiedCols.Clear();
                        tiedCols.Add(col);
                    }
                    else if (scoreAfterMove == bestScore)
                    {
                        tiedCols.Add(col);
                    }
                }
            }

            // Tie-break randomly to make the AI less predictable
            Random rng = new Random();
            int chosenCol = tiedCols[rng.Next(tiedCols.Count)];
            return (bestScore, chosenCol);
        }

        private static List<int> GetAvailableCols(int[][] board)
        {
            List<int> cols = new List<int>();
            for (int i = 0; i < 3; i++)
                if (board[i].Any(x => x == 0)) cols.Add(i);
            return cols;
        }

        private static int GetFirstEmptyRow(int[][] board, int col)
        {
            for (int i = 0; i < 3; i++)
                if (board[col][i] == 0) return i;
            return -1;
        }

        private static int[][] CloneBoard(int[][] board)
        {
            return new int[][] { (int[])board[0].Clone(), (int[])board[1].Clone(), (int[])board[2].Clone() };
        }
    }
}
