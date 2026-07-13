using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System;

namespace KnuckleBones
{
    public class AI
    {
        public static int GetMove(GameState state, bool useRandom = true)
        {
            int depth = (int)state.CurrentDifficulty;
            // Use a fixed seed for testing to avoid randomness in tests
            var (bestScore, bestCol) = Minimax(state.Player1Board, state.Player2Board, state.CurrentDie, depth, false, useRandom: useRandom);
            return bestCol;
        }

        private static (int score, int col) Minimax(int[][] p1Board, int[][] p2Board, int currentDie, int depth, bool p1Turn, bool useRandom)
        {
            List<int> availableCols = GetAvailableCols(p1Turn ? p1Board : p2Board);

            if (availableCols.Count == 0)
            {
                return (Rules.CalculateScore(p2Board) - Rules.CalculateScore(p1Board), -1);
            }

            int bestScore = p1Turn ? int.MaxValue : int.MinValue;
            List<int> tiedCols = [];

            foreach (int col in availableCols)
            {
                int[][] nextP1 = CloneBoard(p1Board);
                int[][] nextP2 = CloneBoard(p2Board);

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

                if (depth > 1 && !Rules.IsBoardFull(nextP1) && !Rules.IsBoardFull(nextP2))
                {
                    long averageScore = 0;
                    for (int nextDie = 1; nextDie <= 6; nextDie++)
                    {
                        var (resScore, _) = Minimax(nextP1, nextP2, nextDie, depth - 1, !p1Turn, useRandom);
                        averageScore += resScore;
                    }
                    scoreAfterMove = (int)(averageScore / 6);
                }
                // Leaf node or end of depth: evaluate board state
                else
                {
                    scoreAfterMove = EvaluateBoard(nextP1, nextP2);
                }

                if (depth <= 1)
                {
                    int p1DestructionCount = CountDifferences(p1Board, nextP1);
                    int p2MatchCount = CountMatches(p2Board, nextP2, currentDie);
                    
                    if (!p1Turn)
                    {
                        scoreAfterMove += p1DestructionCount * 150; // Increased weight
                        scoreAfterMove += p2MatchCount * 80;       // Increased weight
                    }
                    else
                    {
                        scoreAfterMove -= p1DestructionCount * 150;
                        scoreAfterMove -= p2MatchCount * 80;
                    }
                }

                if (!p1Turn) // AI maximizing
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
                else // Player minimizing
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

            int chosenCol = (useRandom) ? tiedCols[new Random().Next(tiedCols.Count)] : tiedCols[0];
            return (bestScore, chosenCol);
        }

        private static int EvaluateBoard(int[][] p1Board, int[][] p2Board)
        {
            int scoreDiff = Rules.CalculateScore(p2Board) - Rules.CalculateScore(p1Board);
            
            // Heuristic: Prefer keeping columns open for future high rolls
            // and penalize boards that are close to filling up with low scores
            int p1Empty = p1Board.Sum(col => col.Count(x => x == 0));
            int p2Empty = p2Board.Sum(col => col.Count(x => x == 0));
            
            return scoreDiff + (p2Empty * 10) - (p1Empty * 10);
        }

        private static List<int> GetAvailableCols(int[][] board)
        {
            List<int> cols = [];
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

        private static int CountDifferences(int[][] oldBoard, int[][] newBoard)
        {
            int diff = 0;
            for (int c = 0; c < 3; c++)
                for (int r = 0; r < 3; r++)
                    if (oldBoard[c][r] != 0 && newBoard[c][r] == 0) diff++;
            return diff;
        }

        private static int CountMatches(int[][] oldBoard, int[][] newBoard, int val)
        {
            int matches = 0;
            for (int c = 0; c < 3; c++)
            {
                int oldCount = oldBoard[c].Count(x => x == val);
                int newCount = newBoard[c].Count(x => x == val);
                if (newCount > oldCount && oldCount > 0) matches += (newCount - oldCount);
            }
            return matches;
        }
    }
}
