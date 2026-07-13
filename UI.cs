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
        public const int ScreenHeight = 700;

        public static Font GameFont;
        public static Texture2D[] WhiteDice = new Texture2D[7];
        public static Texture2D[] BlackDice = new Texture2D[7];
        public static Texture2D Background;
        private static RenderTexture2D plasmaTarget;
        private static float plasmaTime = 0;

        public static void LoadResources()
        {
            // Try to find the resources directory starting from the executable's location
            string baseDir = AppContext.BaseDirectory;
            string resourcesPath = "";

            // Search up the directory tree to find the "resources" folder
            string currentDir = baseDir;
            while (!string.IsNullOrEmpty(currentDir))
            {
                string potentialPath = Path.Combine(currentDir, "resources");
                if (Directory.Exists(potentialPath))
                {
                    resourcesPath = potentialPath;
                    break;
                }

                // Also check knucklebones/resources if we're in the parent
                potentialPath = Path.Combine(currentDir, "knucklebones", "resources");
                if (Directory.Exists(potentialPath))
                {
                    resourcesPath = potentialPath;
                    break;
                }

                currentDir = Path.GetDirectoryName(currentDir) ?? "";
            }

            if (string.IsNullOrEmpty(resourcesPath))
            {
                throw new DirectoryNotFoundException("Could not find 'resources' directory in any parent path.");
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

            // Initialize plasma effect texture
            plasmaTarget = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
        }

        public static void UnloadResources()
        {
            Raylib.UnloadFont(GameFont);
            for (int i = 1; i <= 6; i++)
            {
                Raylib.UnloadTexture(WhiteDice[i]);
                Raylib.UnloadTexture(BlackDice[i]);
            }
            Raylib.UnloadRenderTexture(plasmaTarget);
        }

        public static void DrawBoard(GameState state)
        {
            DrawPlasmaBackground();

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

                // Centering logic
                int overlayHeight = 120;
                int overlayY = (ScreenHeight - 100 - overlayHeight) / 2 + 50; // Center vertically in the play area (above difficulty)

                Raylib.DrawRectangle(0, overlayY, ScreenWidth, overlayHeight, new Color(0, 0, 0, 220));

                Vector2 winnerSize = Raylib.MeasureTextEx(GameFont, winner, 40, 2);
                Raylib.DrawTextEx(GameFont, winner, new Vector2((ScreenWidth - winnerSize.X) / 2, overlayY + 20), 40, 2, Color.Yellow);

                string restartText = "Press R to Restart";
                Vector2 restartSize = Raylib.MeasureTextEx(GameFont, restartText, 20, 2);
                Raylib.DrawTextEx(GameFont, restartText, new Vector2((ScreenWidth - restartSize.X) / 2, overlayY + 75), 20, 2, Color.White);
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
                Raylib.DrawTextEx(GameFont, turnText, new System.Numerics.Vector2(ScreenWidth/2 - 60, 125), 20, 2, turnColor);
            }

            // Difficulty UI
            DrawDifficultySelector(state);
        }

        private static void DrawPlasmaBackground()
        {
            plasmaTime += Raylib.GetFrameTime() * 1.5f; // Faster for more intensity

            Raylib.BeginTextureMode(plasmaTarget);
            // Don't clear to pure black, allow some trails/accumulation
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 20));

            // Old school additive-style blending using many overlapping circles
            // We use many more circles with varying properties for a complex look
            for (int i = 0; i < 12; i++)
            {
                float t = plasmaTime + i * 0.8f;

                // Complex paths for the "blobs"
                float x = (float)(Math.Sin(t * 0.7f) * Math.Cos(t * 0.3f) * ScreenWidth * 0.4f + ScreenWidth * 0.5f);
                float y = (float)(Math.Cos(t * 0.5f) * Math.Sin(t * 0.4f) * ScreenHeight * 0.4f + ScreenHeight * 0.5f);

                // Pulsating radius
                float radius = (float)(Math.Sin(t * 0.9f) * 80 + 150);

                // Intense, cycling colors
                float hue = (i * 30 + plasmaTime * 100) % 360;
                Color c = ColorFromHSV(hue, 1.0f, 1.0f);
                c.A = 120; // Transparency for layering/blending

                Raylib.DrawCircleV(new Vector2(x, y), radius, c);
            }
            Raylib.EndTextureMode();

            // Draw the generated texture with additive blending style (simulated)
            Raylib.DrawTextureRec(plasmaTarget.Texture,
                new Rectangle(0, 0, plasmaTarget.Texture.Width, -plasmaTarget.Texture.Height),
                Vector2.Zero, Color.White);

            // Add a vibrant color grading overlay instead of just dark gray
            // This creates the "psychedelic" blend look
            Color grading = ColorFromHSV((plasmaTime * 20) % 360, 0.4f, 0.2f);
            grading.A = 150;
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, grading);
        }

        private static Color ColorFromHSV(float hue, float saturation, float value)
        {
            float c = value * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = value - c;
            float r = 0, g = 0, b = 0;

            if (hue < 60) { r = c; g = x; }
            else if (hue < 120) { r = x; g = c; }
            else if (hue < 180) { g = c; b = x; }
            else if (hue < 240) { g = x; b = c; }
            else if (hue < 300) { r = x; b = c; }
            else { r = c; b = x; }

            return new Color((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255), (byte)255);
        }

        private static void DrawDifficultySelector(GameState state)
        {
            int startY = 620;
            Raylib.DrawLineEx(new Vector2(0, startY - 10), new Vector2(ScreenWidth, startY - 10), 1, Color.DarkGray);
            Raylib.DrawTextEx(GameFont, "Difficulty:", new Vector2(20, startY), 20, 2, Color.LightGray);

            Difficulty[] diffs = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            int btnWidth = 100;
            int btnHeight = 40;
            int spacing = 20;
            int startX = 150;

            for (int i = 0; i < diffs.Length; i++)
            {
                Difficulty d = diffs[i];
                int x = startX + i * (btnWidth + spacing);
                Rectangle rect = new Rectangle(x, startY - 10, btnWidth, btnHeight);
                bool isSelected = state.CurrentDifficulty == d;

                Raylib.DrawRectangleRec(rect, isSelected ? Color.SkyBlue : Color.DarkGray);
                Raylib.DrawRectangleLinesEx(rect, 2, Color.White);

                string text = d.ToString();
                Vector2 textSize = Raylib.MeasureTextEx(GameFont, text, 18, 2);
                Raylib.DrawTextEx(GameFont, text, new Vector2(x + (btnWidth - textSize.X)/2, startY - 10 + (btnHeight - textSize.Y)/2), 18, 2, isSelected ? Color.Black : Color.White);
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
}
