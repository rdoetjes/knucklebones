using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System;

namespace DiceyStarCluster
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
            star.StarColor = Color.White; // Changed from randomized HSV colors to pure white
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

            int totalGridWidth = 3 * 100 - 20; // 3 cells (80) + 2 spacings (20)
            int p1StartX = (ScreenWidth / 2 - totalGridWidth) / 2;
            int p2StartX = ScreenWidth / 2 + (ScreenWidth / 2 - totalGridWidth) / 2;

            DrawPlayerGrid(state, state.Player1Board, p1StartX, true, Color.White, true);
            DrawPlayerGrid(state, state.Player2Board, p2StartX, false, Color.White, false);

            int player1Rows = Rules.CalculateRowsScore(state.Player1Board);
            int player1Cols = Rules.CalculateColsScore(state.Player1Board);
            int player2Rows = Rules.CalculateRowsScore(state.Player2Board);
            int player2Cols = Rules.CalculateColsScore(state.Player2Board);

            Raylib.DrawTextEx(GameFont, $"Player: {state.Player1Score}", new Vector2(p1StartX, 560), 24, 2, Color.White);
            string p1Breakdown = player1Rows >= player1Cols ? $"(Row Score: {player1Rows})" : $"(Col Score: {player1Cols})";
            Raylib.DrawTextEx(GameFont, p1Breakdown, new Vector2(p1StartX, 590), 16, 2, Color.Gray);

            Raylib.DrawTextEx(GameFont, $"AI: {state.Player2Score}", new Vector2(p2StartX, 560), 24, 2, Color.White);
            string p2Breakdown = player2Rows >= player2Cols ? $"(Row Score: {player2Rows})" : $"(Col Score: {player2Cols})";
            Raylib.DrawTextEx(GameFont, p2Breakdown, new Vector2(p2StartX, 590), 16, 2, Color.Gray);

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
                
                // Animation for current die roll: zoom in and overshoot
                float elapsed = (float)Raylib.GetTime() - state.CurrentDieRoll.StartTime;
                float duration = 0.25f;
                float scale = 0.5f;

                if (elapsed < duration)
                {
                    // Ease Out Back: t: current time, b: beginning value, c: change in value, d: duration, s: overshoot amount
                    float t = elapsed / duration;
                    float s = 1.70158f; 
                    float backFactor = (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s) + 1;
                    scale = backFactor * 0.5f;
                }

                Vector2 pos = new Vector2(ScreenWidth/2 - (rollTex.Width * scale)/2, 35 + (rollTex.Height * 0.5f - rollTex.Height * scale)/2);
                Raylib.DrawTextureEx(rollTex, pos, 0, scale, Color.White);
                
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
                byte alpha = (byte)Math.Clamp(255 - (z / 2000 * 150), 100, 255); // Reduced falloff, higher minimum alpha
                starColor.A = alpha;
                if (maxDist > 1.0f - edgeThreshold) {
                    float intensity = Math.Clamp((maxDist - (1.0f - edgeThreshold)) / edgeThreshold, 0, 1);
                    Color redShift = Color.Red; redShift.A = (byte)(alpha * intensity * 0.8f);
                    Color blueShift = Color.Blue; blueShift.A = (byte)(alpha * intensity * 0.8f);
                    float shiftAmount = intensity * 4.0f;
                    Raylib.DrawLineEx(new Vector2(px - shiftAmount, py), new Vector2(x - shiftAmount, y), Math.Clamp(2.5f / (z / 500), 1.0f, 4.0f), redShift);
                    Raylib.DrawLineEx(new Vector2(px + shiftAmount, py), new Vector2(x + shiftAmount, y), Math.Clamp(2.5f / (z / 500), 1.0f, 4.0f), blueShift);
                }
                Raylib.DrawLineEx(new Vector2(px, py), new Vector2(x, y), Math.Clamp(3.0f / (z / 500), 1.5f, 6.0f), starColor); // Increased thickness
            }
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 130)); // Slightly lighter overlay (was 160)
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

        private static void DrawPlayerGrid(GameState state, int[][] grid, int startX, bool isWhite, Color baseColor, bool isPlayer1)
        {
            int spacing = 100;
            Vector2 mousePos = Raylib.GetMousePosition();
            int hoveredCol = -1;

            // Calculate Grid Bounds for Hover
            int gridWidth = 3 * spacing - 20;
            int gridHeight = 3 * spacing - 20;

            // Check if mouse is hovering over Player 1's grid (the only interactive one)
            if (state.Player1Turn && !state.GameOver)
            {
                int totalGridWidth = 3 * 100 - 20;
                int p1StartX = (ScreenWidth / 2 - totalGridWidth) / 2;

                if (mousePos.X >= p1StartX && mousePos.X <= p1StartX + gridWidth &&
                    mousePos.Y >= 150 && mousePos.Y <= 150 + gridHeight)
                {
                    hoveredCol = (int)((mousePos.X - p1StartX) / spacing);
                }
            }

            for (int col = 0; col < 3; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    int x = startX + col * spacing;
                    int y = 150 + row * spacing;

                    Rectangle rect = new Rectangle(x, y, 80, 80);

                    // Option 4: Destruction Preview
                    // If we are drawing the AI's board (not isPlayer1) and P1 is hovering a column/row,
                    // highlight matching dice in that same row or column on the AI's side.
                    bool isTargeted = false;
                    if (!isPlayer1 && hoveredCol != -1)
                    {
                        // Find what row the die WOULD land in if the user clicks this cell
                        // Since we now allow clicking specific cells, we check the specific row.
                        int targetRow = (int)((mousePos.Y - 150) / spacing);

                        if (targetRow >= 0 && targetRow < 3)
                        {
                            // Check Column Destruction (Vertical)
                            if (col == hoveredCol && grid[col][row] == state.CurrentDie)
                            {
                                isTargeted = true;
                            }
                            // Check Row Destruction (Horizontal)
                            if (row == targetRow && grid[col][row] == state.CurrentDie)
                            {
                                isTargeted = true;
                            }
                        }
                    }

                    if (isTargeted)
                    {
                        // Draw a red "target" highlight
                        Raylib.DrawRectangleRec(rect, new Color(255, 0, 0, 100));
                    }

                    DrawGradientRoundedRect(rect, 0.2f, 9);

                    // Draw AI Move Highlight
                    if (!isPlayer1 && state.AILastMove.HasValue)
                    {
                        var lastMove = state.AILastMove.Value;
                        if (lastMove.Col == col && lastMove.Row == row)
                        {
                            float elapsed = (float)Raylib.GetTime() - lastMove.Time;
                            if (elapsed < 1.5f)
                            {
                                float alpha = 1.0f - (elapsed / 1.5f);
                                Color highlight = Color.SkyBlue;
                                highlight.A = (byte)(alpha * 200);
                                Raylib.DrawRectangleRounded(rect, 0.2f, 16, highlight);
                            }
                        }
                    }

                    if (grid[col][row] > 0)
                    {
                        Texture2D tex = isWhite ? WhiteDice[grid[col][row]] : BlackDice[grid[col][row]];
                        float scale = 80f / tex.Width * 0.8f;
                        Raylib.DrawTextureEx(tex, new Vector2(x + (80 - tex.Width * scale) / 2, y + (80 - tex.Height * scale) / 2), 0, scale, Color.White);
                    }
                }

                // Draw Column Score Below
                int[] colValues = Rules.GetColValues(grid, col);
                int colScore = Rules.GetLineScore(colValues);
                string colScoreText = colScore.ToString();
                Vector2 colScoreSize = Raylib.MeasureTextEx(GameFont, colScoreText, 18, 2);

                // Color logic for combos
                Color colColor = Color.SkyBlue;
                var counts = new Dictionary<int, int>();
                int maxCount = 0;
                foreach(int v in colValues) if(v > 0) {
                    if(!counts.ContainsKey(v)) counts[v] = 0;
                    counts[v]++;
                    maxCount = Math.Max(maxCount, counts[v]);
                }
                if (maxCount == 2) colColor = Color.Yellow;
                else if (maxCount == 3) colColor = Color.Orange;

                Raylib.DrawTextEx(GameFont, colScoreText, new Vector2(startX + col * spacing + (80 - colScoreSize.X) / 2, 150 + 3 * spacing + 5), 18, 2, colColor);
            }

            for (int row = 0; row < 3; row++)
            {
                // Draw Row Score
                int[] rowValues = Rules.GetRowValues(grid, row);
                int rowScore = Rules.GetLineScore(rowValues);
                string rowScoreText = rowScore.ToString();
                Vector2 rowScoreSize = Raylib.MeasureTextEx(GameFont, rowScoreText, 18, 2);

                // Color logic for combos
                Color rowColor = Color.SkyBlue;
                var counts = new Dictionary<int, int>();
                int maxCount = 0;
                foreach(int v in rowValues) if(v > 0) {
                    if(!counts.ContainsKey(v)) counts[v] = 0;
                    counts[v]++;
                    maxCount = Math.Max(maxCount, counts[v]);
                }
                if (maxCount == 2) rowColor = Color.Yellow;
                else if (maxCount == 3) rowColor = Color.Orange;

                if (isPlayer1)
                {
                    // Draw on the LEFT
                    Raylib.DrawTextEx(GameFont, rowScoreText, new Vector2(startX - rowScoreSize.X - 15, 150 + row * spacing + (80 - rowScoreSize.Y) / 2), 18, 2, rowColor);
                }
                else
                {
                    // Draw on the RIGHT
                    Raylib.DrawTextEx(GameFont, rowScoreText, new Vector2(startX + 3 * spacing - 20 + 15, 150 + row * spacing + (80 - rowScoreSize.Y) / 2), 18, 2, rowColor);
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
