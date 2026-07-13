using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System;

namespace KnuckleBones
{
    public class UI
    {
        public const int ScreenWidth = 600;
        public const int ScreenHeight = 600;
        
        public static Font GameFont;
        public static Texture2D[] WhiteDice = new Texture2D[7];
        public static Texture2D[] BlackDice = new Texture2D[7];
        public static Texture2D Background;

        public static void LoadResources()
        {
            // Use Path.Combine for cross-platform compatibility
            string baseDir = AppContext.BaseDirectory;
            string resourcesPath = Path.Combine(baseDir, "..", "..", "..", "..", "resources");
            
            // Check if directory exists, if not try a simpler relative path (fallback for production)
            if (!Directory.Exists(resourcesPath))
            {
                resourcesPath = "resources";
            }

            string fontPath = Path.Combine(resourcesPath, "fonts", "revvy.ttf");
            GameFont = Raylib.LoadFont(fontPath);
            Raylib.SetTextureFilter(GameFont.Texture, TextureFilter.Bilinear);
            
            for (int i = 1; i <= 6; i++)
            {
                string whitePath = Path.Combine(resourcesPath, "img", $"{i}_white.png");
                string blackPath = Path.Combine(resourcesPath, "img", $"{i}_black.png");
                
                WhiteDice[i] = Raylib.LoadTexture(whitePath);
                BlackDice[i] = Raylib.LoadTexture(blackPath);
            }
        }

        public static void UnloadResources()
        {
            Raylib.UnloadFont(GameFont);
            for (int i = 1; i <= 6; i++)
            {
                Raylib.UnloadTexture(WhiteDice[i]);
                Raylib.UnloadTexture(BlackDice[i]);
            }
        }

        public static void DrawBoard(GameState state)
        {
            Raylib.ClearBackground(new Color(40, 44, 52, 255)); // Dark theme

            // Vertical Divider Line
            Raylib.DrawLineEx(new System.Numerics.Vector2(ScreenWidth / 2, 0), 
                             new System.Numerics.Vector2(ScreenWidth / 2, ScreenHeight), 2, Color.Gray);

            // Draw Grids (Left: Player 1, Right: Player 2)
            DrawPlayerGrid(state.Player1Board, 20, true);   // Left
            DrawPlayerGrid(state.Player2Board, 320, false); // Right

            // UI Text
            Raylib.DrawTextEx(GameFont, $"Player: {state.Player1Score}", new System.Numerics.Vector2(20, 560), 24, 2, Color.White);
            Raylib.DrawTextEx(GameFont, $"AI: {state.Player2Score}", new System.Numerics.Vector2(320, 560), 24, 2, Color.White);
            
            if (state.GameOver)
            {
                string winner = state.Player1Score > state.Player2Score ? "Player Wins!" : "AI Wins!";
                if (state.Player1Score == state.Player2Score) winner = "Draw!";
                Raylib.DrawRectangle(0, 250, 600, 100, new Color(0, 0, 0, 200));
                Raylib.DrawTextEx(GameFont, winner, new System.Numerics.Vector2(ScreenWidth/2 - 100, ScreenHeight/2 - 25), 40, 2, Color.Yellow);
                Raylib.DrawTextEx(GameFont, "Press R to Restart", new System.Numerics.Vector2(ScreenWidth/2 - 80, ScreenHeight/2 + 20), 20, 2, Color.White);
            }
            else
            {
                Color turnColor = state.Player1Turn ? Color.White : Color.Gray;
                Raylib.DrawTextEx(GameFont, "Current Roll:", new System.Numerics.Vector2(ScreenWidth/2 - 55, 10), 20, 2, Color.SkyBlue);
                
                // Draw the actual die image for the current roll
                Texture2D rollTex = state.Player1Turn ? WhiteDice[state.CurrentDie] : BlackDice[state.CurrentDie];
                float rollScale = 0.5f;
                Vector2 rollPos = new Vector2(ScreenWidth/2 - (rollTex.Width * rollScale)/2, 35);
                Raylib.DrawTextureEx(rollTex, rollPos, 0, rollScale, Color.White);

                string turnText = state.Player1Turn ? "<< Your Turn" : "AI Thinking >>";
                Raylib.DrawTextEx(GameFont, turnText, new System.Numerics.Vector2(ScreenWidth/2 - 60, 95), 20, 2, turnColor);
            }
        }

        private static void DrawPlayerGrid(int[][] grid, int startX, bool isWhite)
        {
            int cellWidth = 80;
            int cellHeight = 80;
            int padding = 10;
            int gridStartY = 150;

            for (int col = 0; col < 3; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    int x = startX + col * (cellWidth + padding);
                    int y = gridStartY + row * (cellHeight + padding);
                    
                    Raylib.DrawRectangleLines(x, y, cellWidth, cellHeight, Color.DarkGray);
                    
                    int val = grid[col][row];
                    if (val > 0)
                    {
                        Texture2D tex = isWhite ? WhiteDice[val] : BlackDice[val];
                        // Scale dice to fit cell (assuming dice images are larger)
                        float scale = (float)cellWidth / tex.Width * 0.8f;
                        Vector2 pos = new Vector2(x + (cellWidth - tex.Width * scale) / 2, y + (cellHeight - tex.Height * scale) / 2);
                        Raylib.DrawTextureEx(tex, pos, 0, scale, Color.White);
                    }
                }
            }
        }
    }

    public enum Difficulty { Easy = 1, Medium = 3, Hard = 5 }

    public class GameState
    {
        public int[][] Player1Board = new int[3][] { new int[3], new int[3], new int[3] };
        public int[][] Player2Board = new int[3][] { new int[3], new int[3], new int[3] };
        public int Player1Score => Rules.CalculateScore(Player1Board);
        public int Player2Score => Rules.CalculateScore(Player2Board);
        public int CurrentDie;
        public bool Player1Turn = true;
        public bool GameOver = false;
        public Difficulty CurrentDifficulty = Difficulty.Medium;

        public GameState()
        {
            CurrentDie = Raylib.GetRandomValue(1, 6);
        }

        public bool PlaceDie(int col, int preferredRow = -1)
        {
            int[][] myBoard = Player1Turn ? Player1Board : Player2Board;
            int[][] opponentBoard = Player1Turn ? Player2Board : Player1Board;

            int actualRow = -1;

            if (preferredRow >= 0 && preferredRow < 3 && myBoard[col][preferredRow] == 0)
            {
                actualRow = preferredRow;
            }
            else
            {
                for (int row = 0; row < 3; row++)
                {
                    if (myBoard[col][row] == 0)
                    {
                        actualRow = row;
                        break;
                    }
                }
            }

            if (actualRow != -1)
            {
                myBoard[col][actualRow] = CurrentDie;
                
                // Rule: "Whenever the player places a die in a row and the opponent has 
                // that same number in that same row, the opponent's die with that same number need to be removed"
                Rules.HandleDestruction(actualRow, CurrentDie, opponentBoard);

                AdvanceTurn();
                return true;
            }
            return false;
        }

        private void AdvanceTurn()
        {
            if (Rules.IsBoardFull(Player1Board) || Rules.IsBoardFull(Player2Board))
            {
                GameOver = true;
            }

            if (!GameOver)
            {
                Player1Turn = !Player1Turn;
                CurrentDie = Raylib.GetRandomValue(1, 6);
            }
        }
    }
}
