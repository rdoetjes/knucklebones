using Raylib_cs;
using System.Numerics;

namespace KnuckleBones
{
    public class UIHandling
    {
        private static float aiTimer = 0;

        public static void Update(GameState state)
        {
            if (state.GameOver)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    ResetGame(state);
                }
                return;
            }

            if (state.Player1Turn)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Vector2 mousePos = Raylib.GetMousePosition();
                    // Check Difficulty Buttons
                    int startY = 620;
                    int startX = 150;
                    int btnWidth = 100;
                    int btnHeight = 40;
                    int spacing = 20;

                    Difficulty[] diffs = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
                    for (int i = 0; i < diffs.Length; i++)
                    {
                        Rectangle rect = new Rectangle(startX + i * (btnWidth + spacing), startY - 10, btnWidth, btnHeight);
                        if (Raylib.CheckCollisionPointRec(mousePos, rect))
                        {
                            state.CurrentDifficulty = diffs[i];
                            return;
                        }
                    }

                    int grid1StartX = 20;
                    int grid1StartY = 150; // Matching UI.cs DrawPlayerGrid
                    int cellSize = 80;
                    int padding = 10;
                    int stride = cellSize + padding; // 90px
                    
                    if (mousePos.X >= grid1StartX && mousePos.X <= grid1StartX + (stride * 3) &&
                        mousePos.Y >= grid1StartY && mousePos.Y <= grid1StartY + (stride * 3))
                    {
                        int col = (int)((mousePos.X - grid1StartX) / stride);
                        int row = (int)((mousePos.Y - grid1StartY) / stride);
                        
                        if (col >= 0 && col < 3 && row >= 0 && row < 3)
                        {
                            state.PlaceDie(col, row);
                        }
                    }
                }
            }
            else // AI Turn
            {
                aiTimer += Raylib.GetFrameTime();
                if (aiTimer > 1.0f) // Delay AI for better UX
                {
                    int move = AI.GetMove(state);
                    if (move != -1)
                    {
                        state.PlaceDie(move);
                    }
                    aiTimer = 0;
                }
            }
        }

        private static void ResetGame(GameState state)
        {
            state.Player1Board = new int[3][] { new int[3], new int[3], new int[3] };
            state.Player2Board = new int[3][] { new int[3], new int[3], new int[3] };
            state.GameOver = false;
            state.Player1Turn = true;
            state.CurrentDie = Raylib.GetRandomValue(1, 6);
        }
    }
}
