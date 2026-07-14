using Raylib_cs;
using System.Numerics;

namespace DiceyStarCluster
{
    public class UIHandling
    {
        private static float aiTimer = 0;
        private static bool isThinking = false;

        public static void Update(GameState state)
        {
            if (state.GameOver)
            {
                HandleGameOverInput(state);
                return;
            }

            if (state.Player1Turn)
            {
                HandlePlayerInput(state);
            }
            else if (!isThinking)
            {
                HandleAITurn(state);
            }
        }

        private static void HandleGameOverInput(GameState state)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                ResetGame(state);
            }
        }

        private static void HandlePlayerInput(GameState state)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

            Vector2 mousePos = Raylib.GetMousePosition();

            if (HandleDifficultyClick(state, mousePos)) return;

            HandleGridClick(state, mousePos);
        }

        private static bool HandleDifficultyClick(GameState state, Vector2 mousePos)
        {
            int startY = 620;
            int startX = 210;
            int btnWidth = 100;
            int btnHeight = 40;

            Difficulty[] diffs = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            for (int i = 0; i < diffs.Length; i++)
            {
                Rectangle rect = new Rectangle(startX + i * 120, startY - 10, btnWidth, btnHeight);
                if (Raylib.CheckCollisionPointRec(mousePos, rect))
                {
                    state.CurrentDifficulty = diffs[i];
                    return true;
                }
            }
            return false;
        }

        private static void HandleGridClick(GameState state, Vector2 mousePos)
        {
            int totalGridWidth = 3 * 100 - 20;
            int grid1StartX = (UI.ScreenWidth / 2 - totalGridWidth) / 2;
            int grid1StartY = 150;
            int cellSize = 80;
            int stride = 100;

            if (mousePos.X >= grid1StartX && mousePos.X < grid1StartX + totalGridWidth &&
                mousePos.Y >= grid1StartY && mousePos.Y < grid1StartY + totalGridWidth)
            {
                float relativeX = mousePos.X - grid1StartX;
                int col = (int)(relativeX / stride);
                float cellRelX = relativeX % stride;
                
                float relativeY = mousePos.Y - grid1StartY;
                int row = (int)(relativeY / stride);
                float cellRelY = relativeY % stride;

                if (col >= 0 && col < 3 && cellRelX <= cellSize &&
                    row >= 0 && row < 3 && cellRelY <= cellSize)
                {
                    state.PlaceDie(col, row);
                }
            }
        }

        private static void HandleAITurn(GameState state)
        {
            aiTimer += Raylib.GetFrameTime();
            // Thinking delay to make moves readable
            if (aiTimer > 0.5f)
            {
                isThinking = true;
                System.Threading.Tasks.Task.Run(() =>
                {
                    int move = AI.GetMove(state);
                    if (move != -1)
                    {
                        state.PlaceDie(move);
                    }
                    isThinking = false;
                    aiTimer = 0;
                });
            }
        }

        private static void ResetGame(GameState state)
        {
            state.Player1Board = new int[3][] { new int[3], new int[3], new int[3] };
            state.Player2Board = new int[3][] { new int[3], new int[3], new int[3] };
            state.GameOver = false;
            state.Player1Turn = true;
            state.CurrentDie = Raylib.GetRandomValue(1, 6);
            state.AILastMove = null;
            state.CurrentDieRoll = new GameState.DieRollState { 
                Value = state.CurrentDie, 
                StartTime = (float)Raylib.GetTime() 
            };
        }
    }
}
