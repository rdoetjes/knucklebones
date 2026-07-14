using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

namespace DiceyStarCluster
{
    public class AI
    {
        public static int GetMove(GameState state, bool useRandom = true)
        {
            int timeLimitSeconds = (int)state.CurrentDifficulty;
            // Ensure a healthy minimum for CI runners (3 seconds minimum)
            int timeoutMs = Math.Max(timeLimitSeconds * 1000, 3000);
            var cts = new CancellationTokenSource(timeoutMs);
            
            int bestCol = -1;
            int currentDepth = 1;
            // Difficulty.Easy now = 2, so we limit search depth to keep it "Easy"
            // but enough for our tests to pass.
            int maxDepth = (state.CurrentDifficulty == Difficulty.Easy) ? 2 : 10;
            
            try {
                // Iterative Deepening
                while (!cts.IsCancellationRequested && currentDepth <= maxDepth)
                {
                    // Use a sub-timer check inside Minimax as well via the token
                    var (score, col) = Minimax(state.Player1Board, state.Player2Board, state.CurrentDie, currentDepth, false, useRandom, cts.Token);
                    
                    if (!cts.IsCancellationRequested && col != -1)
                    {
                        bestCol = col;
                        currentDepth++;
                    }
                    else break;
                }
            } catch (OperationCanceledException) {}

            if (bestCol == -1)
            {
                var available = GetAvailableCols(state.Player2Board);
                bestCol = available[new Random().Next(available.Count)];
            }

            return bestCol;
        }

        private static (int score, int col) Minimax(int[][] p1Board, int[][] p2Board, int currentDie, int depth, bool p1Turn, bool useRandom, System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            List<int> availableCols = GetAvailableCols(p1Turn ? p1Board : p2Board);

            if (availableCols.Count == 0)
            {
                return (EvaluateBoard(p1Board, p2Board), -1);
            }

            int bestScore = p1Turn ? int.MaxValue : int.MinValue;
            List<int> tiedCols = [];
            object lockObj = new ();

            // Check if we can finish this turn immediately (depth 1)
            // If depth 1, we just evaluate the immediate moves.

            if (depth >= 3 && availableCols.Count > 1)
            {
                System.Threading.Tasks.Parallel.ForEach(availableCols, new System.Threading.Tasks.ParallelOptions { CancellationToken = token }, col =>
                {
                    var result = SimulateMove(p1Board, p2Board, currentDie, depth, p1Turn, useRandom, col, token);
                    lock (lockObj) { UpdateBestMove(ref bestScore, ref tiedCols, result.score, col, p1Turn); }
                });
            }
            else
            {
                foreach (int col in availableCols)
                {
                    token.ThrowIfCancellationRequested();
                    var result = SimulateMove(p1Board, p2Board, currentDie, depth, p1Turn, useRandom, col, token);
                    UpdateBestMove(ref bestScore, ref tiedCols, result.score, col, p1Turn);
                }
            }

            if (tiedCols.Count == 0) return (EvaluateBoard(p1Board, p2Board), -1);

            // Stability: Always pick first tied column if not using random (for tests)
            int chosenCol = (useRandom) ? tiedCols[new Random().Next(tiedCols.Count)] : tiedCols[0];
            return (bestScore, chosenCol);
        }

        private static (int score, int col) SimulateMove(int[][] p1Board, int[][] p2Board, int currentDie, int depth, bool p1Turn, bool useRandom, int col, System.Threading.CancellationToken token)
        {
            int[][] nextP1 = CloneBoard(p1Board);
            int[][] nextP2 = CloneBoard(p2Board);

            // For Player 1 (user), they click a specific row, but AI currently finds first empty row.
            // We should evaluate all empty rows in that column for the AI too,
            // especially now that rows matter for scoring and destruction.

            int bestMoveScore = p1Turn ? int.MaxValue : int.MinValue;

            for (int row = 0; row < 3; row++)
            {
                int[][] tempP1 = CloneBoard(p1Board);
                int[][] tempP2 = CloneBoard(p2Board);
                int[][] myBoard = p1Turn ? tempP1 : tempP2;
                int[][] oppBoard = p1Turn ? tempP2 : tempP1;

                if (myBoard[col][row] != 0) continue;

                myBoard[col][row] = currentDie;
                Rules.HandleDestruction(col, row, currentDie, oppBoard);

                int currentScore;
                if (depth > 1 && !Rules.IsBoardFull(tempP1) && !Rules.IsBoardFull(tempP2))
                {
                    long averageScore = 0;
                    for (int nextDie = 1; nextDie <= 6; nextDie++)
                    {
                        token.ThrowIfCancellationRequested();
                        var (resScore, _) = Minimax(tempP1, tempP2, nextDie, depth - 1, !p1Turn, useRandom, token);
                        averageScore += resScore;
                    }
                    currentScore = (int)(averageScore / 6);
                }
                else
                {
                    currentScore = EvaluateBoard(tempP1, tempP2);
                }

                if (!p1Turn) { if (currentScore > bestMoveScore || bestMoveScore == int.MinValue) bestMoveScore = currentScore; }
                else { if (currentScore < bestMoveScore || bestMoveScore == int.MaxValue) bestMoveScore = currentScore; }
            }

            return (bestMoveScore, col);
        }

        private static void UpdateBestMove(ref int bestScore, ref List<int> tiedCols, int scoreAfterMove, int col, bool p1Turn)
        {
            if (!p1Turn) // AI maximizing
            {
                if (scoreAfterMove > bestScore || tiedCols.Count == 0) { bestScore = scoreAfterMove; tiedCols.Clear(); tiedCols.Add(col); }
                else if (scoreAfterMove == bestScore) { tiedCols.Add(col); }
            }
            else // Player minimizing
            {
                if (scoreAfterMove < bestScore || tiedCols.Count == 0) { bestScore = scoreAfterMove; tiedCols.Clear(); tiedCols.Add(col); }
                else if (scoreAfterMove == bestScore) { tiedCols.Add(col); }
            }
        }

        private static int EvaluateBoard(int[][] p1Board, int[][] p2Board)
        {
            // Simple score difference - this is the ground truth
            return Rules.CalculateScore(p2Board) - Rules.CalculateScore(p1Board);
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
