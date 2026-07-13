using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System;

namespace KnuckleBones
{
    public class UI
    {
        public const int ScreenWidth = 720;
        public const int ScreenHeight = 700;

        private static Font GameFont;
        private readonly static Texture2D[] WhiteDice = new Texture2D[7];
        private readonly static Texture2D[] BlackDice = new Texture2D[7];

        private struct Star
        {
            public Vector3 Position;
            public float Speed;
            public Color StarColor;
        }

        private const int StarCount = 2000;
        private readonly static Star[] starfield = new Star[StarCount];

        public static void LoadResources()
        {
            string baseDir = AppContext.BaseDirectory;
            string resourcesPath = "";
            string currentDir = baseDir;
            while (!string.IsNullOrEmpty(currentDir))
            {
                string potentialPath = Path.Combine(currentDir, "resources");
                if (Directory.Exists(potentialPath)) { resourcesPath = potentialPath; break; }
                potentialPath = Path.Combine(currentDir, "knucklebones", "resources");
                if (Directory.Exists(potentialPath)) { resourcesPath = potentialPath; break; }
                currentDir = Path.GetDirectoryName(currentDir) ?? "";
            }

            if (string.IsNullOrEmpty(resourcesPath))
                throw new DirectoryNotFoundException("Could not find 'resources' directory.");

            string fontPath = Path.Combine(resourcesPath, "fonts", "revvy.ttf");
            GameFont = Raylib.LoadFont(fontPath);
            Raylib.SetTextureFilter(GameFont.Texture, TextureFilter.Bilinear);

            for (int i = 1; i <= 6; i++)
            {
                WhiteDice[i] = Raylib.LoadTexture(Path.Combine(resourcesPath, "img", $"{i}_white.png"));
                BlackDice[i] = Raylib.LoadTexture(Path.Combine(resourcesPath, "img", $"{i}_black.png"));
            }

            for (int i = 0; i < StarCount; i++)
                ResetStar(ref starfield[i], true);
        }

        private static void ResetStar(ref Star star, bool randomZ)
        {
            Random rng = new Random();
            star.Position = new Vector3(
                (float)(rng.NextDouble() * 1000 - 500),
                (float)(rng.NextDouble() * 1000 - 500),
                randomZ ? (float)(rng.NextDouble() * 2000 + 10) : 2000
            );
            star.Speed = (float)(rng.NextDouble() * 2.0f + 1.0f);
            star.StarColor = ColorFromHSV((float)(rng.NextDouble() * 360), 0.6f, 1.0f);
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
            DrawWarpStarfield();

            // Draw Gradient Vertical Divider (9 pixels wide for symmetry)
            DrawGradientLine(new Vector2(ScreenWidth / 2, 0), new Vector2(ScreenWidth / 2, ScreenHeight), 9);

            DrawPlayerGrid(state.Player1Board, 40, true, Color.White);
            DrawPlayerGrid(state.Player2Board, 400, false, Color.White);

            Raylib.DrawTextEx(GameFont, $"Player: {state.Player1Score}", new Vector2(40, 560), 24, 2, Color.White);
            Raylib.DrawTextEx(GameFont, $"AI: {state.Player2Score}", new Vector2(400, 560), 24, 2, Color.White);

            if (state.GameOver)
            {
                string winner = state.Player1Score > state.Player2Score ? "Player Wins!" : (state.Player1Score == state.Player2Score ? "Draw!" : "AI Wins!");
                int overlayY = (ScreenHeight - 220) / 2 + 50;
                Raylib.DrawRectangle(0, overlayY, ScreenWidth, 120, new Color(0, 0, 0, 220));
                Vector2 winnerSize = Raylib.MeasureTextEx(GameFont, winner, 40, 2);
                Raylib.DrawTextEx(GameFont, winner, new Vector2((ScreenWidth - winnerSize.X) / 2, overlayY + 20), 40, 2, Color.Yellow);
                Vector2 restartSize = Raylib.MeasureTextEx(GameFont, "Press R to Restart", 20, 2);
                Raylib.DrawTextEx(GameFont, "Press R to Restart", new Vector2((ScreenWidth - restartSize.X) / 2, overlayY + 75), 20, 2, Color.White);
            }
            else
            {
                Raylib.DrawTextEx(GameFont, "Current Roll:", new Vector2(ScreenWidth/2 - 55, 10), 20, 2, Color.SkyBlue);
                Texture2D rollTex = state.Player1Turn ? WhiteDice[state.CurrentDie] : BlackDice[state.CurrentDie];
                Raylib.DrawTextureEx(rollTex, new Vector2(ScreenWidth/2 - (rollTex.Width * 0.5f)/2, 35), 0, 0.5f, Color.White);
                string turnText = state.Player1Turn ? "<< Your Turn" : "AI Thinking >>";
                Raylib.DrawTextEx(GameFont, turnText, new Vector2(ScreenWidth/2 - 60, 125), 20, 2, state.Player1Turn ? Color.White : Color.Gray);
            }
            DrawDifficultySelector(state);
        }

        private static void DrawWarpStarfield()
        {
            Raylib.ClearBackground(Color.Black);
            float dt = Raylib.GetFrameTime();
            for (int i = 0; i < StarCount; i++)
            {
                starfield[i].Position.Z -= starfield[i].Speed * dt * 50;
                if (starfield[i].Position.Z <= 10) ResetStar(ref starfield[i], false);
                float z = starfield[i].Position.Z;
                float x = (starfield[i].Position.X / z) * 400 + ScreenWidth / 2;
                float y = (starfield[i].Position.Y / z) * 400 + ScreenHeight / 2;
                if (x < -100 || x > ScreenWidth + 100 || y < -100 || y > ScreenHeight + 100) { if (z < 500) { ResetStar(ref starfield[i], false); continue; } }
                float prevZ = z + starfield[i].Speed * 0.8f;
                float px = (starfield[i].Position.X / prevZ) * 400 + ScreenWidth / 2;
                float py = (starfield[i].Position.Y / prevZ) * 400 + ScreenHeight / 2;
                float edgeThreshold = 0.2f;
                float distFromCenterNormX = Math.Abs(x - ScreenWidth / 2) / (ScreenWidth / 2);
                float distFromCenterNormY = Math.Abs(y - ScreenHeight / 2) / (ScreenHeight / 2);
                float maxDist = Math.Max(distFromCenterNormX, distFromCenterNormY);
                Color starColor = Color.White;
                byte alpha = (byte)Math.Clamp(255 - (z / 2000 * 255), 0, 255);
                starColor.A = alpha;
                if (maxDist > 1.0f - edgeThreshold) {
                    float intensity = Math.Clamp((maxDist - (1.0f - edgeThreshold)) / edgeThreshold, 0, 1);
                    Color redShift = Color.Red; redShift.A = (byte)(alpha * intensity * 0.6f);
                    Color blueShift = Color.Blue; blueShift.A = (byte)(alpha * intensity * 0.6f);
                    float shiftAmount = intensity * 4.0f;
                    Raylib.DrawLineEx(new Vector2(px - shiftAmount, py), new Vector2(x - shiftAmount, y), Math.Clamp(2.0f / (z / 500), 0.5f, 3.0f), redShift);
                    Raylib.DrawLineEx(new Vector2(px + shiftAmount, py), new Vector2(x + shiftAmount, y), Math.Clamp(2.0f / (z / 500), 0.5f, 3.0f), blueShift);
                }
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(x, y), Math.Clamp(2.0f / (z / 500), 0.5f, 3.0f), starColor);
            }
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 160));
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
            int startX = 210;
            for (int i = 0; i < diffs.Length; i++)
            {
                int x = startX + i * 120;
                Rectangle rect = new Rectangle(x, startY - 10, 100, 40);
                Raylib.DrawRectangleRec(rect, state.CurrentDifficulty == diffs[i] ? Color.SkyBlue : Color.DarkGray);
                Raylib.DrawRectangleLinesEx(rect, 2, Color.White);
                Vector2 textSize = Raylib.MeasureTextEx(GameFont, diffs[i].ToString(), 18, 2);
                Raylib.DrawTextEx(GameFont, diffs[i].ToString(), new Vector2(x + (100 - textSize.X)/2, startY - 10 + (40 - textSize.Y)/2), 18, 2, state.CurrentDifficulty == diffs[i] ? Color.Black : Color.White);
            }
        }

        private static void DrawPlayerGrid(int[][] grid, int startX, bool isWhite, Color baseColor)
        {
            int spacing = 100; // Increased from 90 (80 size + 20 spacing)
            for (int col = 0; col < 3; col++)
                for (int row = 0; row < 3; row++)
                {
                    int x = startX + col * spacing;
                    int y = 150 + row * spacing;

                    Rectangle rect = new Rectangle(x, y, 80, 80);
                    DrawGradientRoundedRect(rect, 0.2f, 9);

                    if (grid[col][row] > 0) {
                        Texture2D tex = isWhite ? WhiteDice[grid[col][row]] : BlackDice[grid[col][row]];
                        float scale = 80f / tex.Width * 0.8f;
                        Raylib.DrawTextureEx(tex, new Vector2(x + (80 - tex.Width * scale) / 2, y + (80 - tex.Height * scale) / 2), 0, scale, Color.White);
                    }
                }
        }

        private static void DrawGradientLine(Vector2 start, Vector2 end, float totalWidth)
        {
            Color[] gradientColors = new Color[] {
                new Color(0, 40, 120, 255),    // 1: Dark Blue (Outer)
                new Color(0, 80, 200, 255),    // 2: Mid Blue
                new Color(100, 200, 255, 255), // 3: Light Blue
                new Color(200, 240, 255, 255), // 4: Very Light Blue
                new Color(255, 255, 255, 255), // 5: Pure White (Center)
                new Color(200, 240, 255, 255), // 6: Very Light Blue
                new Color(100, 200, 255, 255), // 7: Light Blue
                new Color(0, 80, 200, 255),    // 8: Mid Blue
                new Color(0, 40, 120, 255)     // 9: Dark Blue (Inner)
            };

            float stepWidth = totalWidth / 9.0f;
            for (int i = 0; i < 9; i++)
            {
                float currentThickness = totalWidth - (i * stepWidth);
                Raylib.DrawLineEx(start, end, currentThickness, gradientColors[i]);
            }
        }

        private static void DrawGradientRoundedRect(Rectangle rect, float roundness, float totalWidth)
        {
            Color[] gradientColors = new Color[] {
                new Color(0, 40, 120, 255),    // 1: Dark Blue (Outer)
                new Color(0, 80, 200, 255),    // 2: Mid Blue
                new Color(100, 200, 255, 255), // 3: Light Blue
                new Color(200, 240, 255, 255), // 4: Very Light Blue
                new Color(255, 255, 255, 255), // 5: Pure White (Center)
                new Color(200, 240, 255, 255), // 6: Very Light Blue
                new Color(100, 200, 255, 255), // 7: Light Blue
                new Color(0, 80, 200, 255),    // 8: Mid Blue
                new Color(0, 40, 120, 255)     // 9: Dark Blue (Inner)
            };

            float stepWidth = totalWidth / 9.0f;
            for (int i = 0; i < 9; i++)
            {
                float currentThickness = totalWidth - (i * stepWidth);
                Raylib.DrawRectangleRoundedLinesEx(rect, roundness, 16, currentThickness, gradientColors[i]);
            }
        }
    }
}
