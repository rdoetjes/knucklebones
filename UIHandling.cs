using Raylib_cs;
using System.Numerics;

namespace KnuckleBones
{
    public class UIHandling
    {
        private static float aiTimer = 0;

        private static bool isThinking = false;

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
                    int startX = 210;
                    int btnWidth = 100;
                    int btnHeight = 40;
                    int spacing = 20;

                    Difficulty[] diffs = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
                    for (int i = 0; i < diffs.Length; i++)
                    {
                        Rectangle rect = new Rectangle(startX + i * 120, startY - 10, btnWidth, btnHeight);
                        if (Raylib.CheckCollisionPointRec(mousePos, rect))
                        {
                            state.CurrentDifficulty = diffs[i];
                            return;
                        }
                    }

                    int grid1StartX = 40;
                    int grid1StartY = 150; 
                    int cellSize = 80;
                    int stride = 100; // 80 size + 20 spacing
                    
                    if (mousePos.X >= grid1StartX && mousePos.X <= grid1StartX + (stride * 2) + cellSize &&
                        mousePos.Y >= grid1StartY && mousePos.Y <= grid1StartY + (stride * 2) + cellSize)
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
            else if (!isThinking) // AI Turn
            {
                aiTimer += Raylib.GetFrameTime();
                if (aiTimer > 0.5f) // Reduced delay since thinking itself takes time
                {
                    isThinking = true;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        int move = AI.GetMove(state);
                        // We need to sync back to main thread to modify state, 
                        // but PlaceDie is simple enough to call if we're careful.
                        // Raylib isn't thread safe, but GameState is just logic.
                        if (move != -1)
                        {
                            state.PlaceDie(move);
                        }
                        isThinking = false;
                        aiTimer = 0;
                    });
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
