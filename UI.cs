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
        private static Texture2D BgImg;
        private static Shader StarShader;
        private static int TimeLoc;
        private static int ResLoc;

        public static void LoadResources()
        {
            string baseDir = AppContext.BaseDirectory;
            string resourcesPath = "";
            string currentDir = baseDir;
            while (!string.IsNullOrEmpty(currentDir))
            {
                string potentialPath = Path.Combine(currentDir, "resources");
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

            BgImg = Raylib.LoadTexture(Path.Combine(resourcesPath, "img", "bg.png"));

            // Load Star Shader
            string shaderPath = Path.Combine(resourcesPath, "shaders", "starfield.fs");
            StarShader = Raylib.LoadShader(null, shaderPath);
            TimeLoc = Raylib.GetShaderLocation(StarShader, "time");
            ResLoc = Raylib.GetShaderLocation(StarShader, "resolution");
            
            float[] res = { (float)ScreenWidth, (float)ScreenHeight };
            Raylib.SetShaderValue(StarShader, ResLoc, res, ShaderUniformDataType.Vec2);
        }

        public static void UnloadResources()
        {
            Raylib.UnloadFont(GameFont);
            for (int i = 1; i <= 6; i++)
            {
                Raylib.UnloadTexture(WhiteDice[i]);
                Raylib.UnloadTexture(BlackDice[i]);
            }
            Raylib.UnloadTexture(BgImg);
            Raylib.UnloadShader(StarShader);
        }

        public static void DrawBoard(GameState state)
        {
            Raylib.ClearBackground(Color.Black);

            // Update shader time
            float time = (float)Raylib.GetTime();
            Raylib.SetShaderValue(StarShader, TimeLoc, time, ShaderUniformDataType.Float);

            // Draw shader starfield integrated with BgImg
            Raylib.BeginShaderMode(StarShader);
            // Draw BgImg inside ShaderMode so shader can sample it and add stars to it
            Raylib.DrawTextureEx(BgImg, new Vector2(-400, -200), 0.0f, 2.0f, Color.White);
            Raylib.EndShaderMode();

            // Subtle dark overlay
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color(0, 0, 0, 60));

            DrawLayout(state);
            DrawGameState(state);
            DrawDifficultySelector(state);
        }

        private static void DrawLayout(GameState state)
        {
            DrawGradientLine(new Vector2(ScreenWidth / 2, 0), new Vector2(ScreenWidth / 2, ScreenHeight), 9);

            int totalGridWidth = 3 * 100 - 20;
            int p1StartX = (ScreenWidth / 2 - totalGridWidth) / 2;
            int p2StartX = ScreenWidth / 2 + (ScreenWidth / 2 - totalGridWidth) / 2;

            DrawPlayerGrid(state, state.Player1Board, p1StartX, true, Color.White, true);
            DrawPlayerGrid(state, state.Player2Board, p2StartX, false, Color.White, false);

            DrawScores(state, p1StartX, p2StartX);
        }

        private static void DrawScores(GameState state, int p1StartX, int p2StartX)
        {
            int p1Rows = Rules.CalculateRowsScore(state.Player1Board);
            int p1Cols = Rules.CalculateColsScore(state.Player1Board);
            int p2Rows = Rules.CalculateRowsScore(state.Player2Board);
            int p2Cols = Rules.CalculateColsScore(state.Player2Board);

            Raylib.DrawTextEx(GameFont, $"Player: {state.Player1Score}", new Vector2(p1StartX, 560), 24, 2, Color.White);
            string p1Breakdown = p1Rows >= p1Cols ? $"(Row Score: {p1Rows})" : $"(Col Score: {p1Cols})";
            Raylib.DrawTextEx(GameFont, p1Breakdown, new Vector2(p1StartX, 590), 16, 2, Color.Gray);

            Raylib.DrawTextEx(GameFont, $"AI: {state.Player2Score}", new Vector2(p2StartX, 560), 24, 2, Color.White);
            string p2Breakdown = p2Rows >= p2Cols ? $"(Row Score: {p2Rows})" : $"(Col Score: {p2Cols})";
            Raylib.DrawTextEx(GameFont, p2Breakdown, new Vector2(p2StartX, 590), 16, 2, Color.Gray);
        }

        private static void DrawGameState(GameState state)
        {
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
                DrawCurrentRoll(state);
            }
        }

        private static void DrawPlayerGrid(GameState state, int[][] grid, int startX, bool isWhite, Color baseColor, bool isPlayer1)
        {
            int spacing = 100;
            int hoveredCol = GetHoveredColumn(startX, spacing);

            for (int col = 0; col < 3; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    DrawGridCell(state, grid, startX, col, row, spacing, isWhite, isPlayer1, hoveredCol);
                }
                DrawColumnScore(grid, startX, col, spacing);
            }

            for (int row = 0; row < 3; row++)
            {
                DrawRowScore(grid, startX, row, spacing, isPlayer1);
            }
        }

        private static int GetHoveredColumn(int gridStartX, int spacing)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            int gridWidth = 3 * spacing - 20;
            int gridHeight = 3 * spacing - 20;
            if (mousePos.X >= gridStartX && mousePos.X <= gridStartX + gridWidth &&
                mousePos.Y >= 150 && mousePos.Y <= 150 + gridHeight)
            {
                return (int)((mousePos.X - gridStartX) / spacing);
            }
            return -1;
        }

        private static void DrawGridCell(GameState state, int[][] grid, int startX, int col, int row, int spacing, bool isWhite, bool isPlayer1, int hoveredCol)
        {
            int x = startX + col * spacing;
            int y = 150 + row * spacing;
            Rectangle rect = new Rectangle(x, y, 80, 80);

            if (!isPlayer1 && hoveredCol != -1 && state.Player1Turn && !state.GameOver)
            {
                Vector2 mousePos = Raylib.GetMousePosition();
                int targetRow = (int)((mousePos.Y - 150) / spacing);
                if (targetRow >= 0 && targetRow < 3)
                {
                    if ((col == hoveredCol || row == targetRow) && grid[col][row] == state.CurrentDie && grid[col][row] > 0)
                    {
                        Raylib.DrawRectangleRec(rect, new Color(255, 0, 0, 100));
                    }
                }
            }

            DrawGradientRoundedRect(rect, 0.2f, 9);

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

        private static void DrawColumnScore(int[][] grid, int startX, int col, int spacing)
        {
            int[] colValues = Rules.GetColValues(grid, col);
            int colScore = Rules.GetLineScore(colValues);
            string text = colScore.ToString();
            Vector2 size = Raylib.MeasureTextEx(GameFont, text, 18, 2);
            Color color = GetComboColor(colValues);
            Raylib.DrawTextEx(GameFont, text, new Vector2(startX + col * spacing + (80 - size.X) / 2, 150 + 3 * spacing + 5), 18, 2, color);
        }

        private static void DrawRowScore(int[][] grid, int startX, int row, int spacing, bool isPlayer1)
        {
            int[] rowValues = Rules.GetRowValues(grid, row);
            int rowScore = Rules.GetLineScore(rowValues);
            string text = rowScore.ToString();
            Vector2 size = Raylib.MeasureTextEx(GameFont, text, 18, 2);
            Color color = GetComboColor(rowValues);
            float y = 150 + row * spacing + (80 - size.Y) / 2;
            if (isPlayer1)
                Raylib.DrawTextEx(GameFont, text, new Vector2(startX - size.X - 15, y), 18, 2, color);
            else
                Raylib.DrawTextEx(GameFont, text, new Vector2(startX + 3 * spacing - 20 + 15, y), 18, 2, color);
        }

        private static Color GetComboColor(int[] values)
        {
            var counts = new Dictionary<int, int>();
            int maxCount = 0;
            foreach (int v in values)
            {
                if (v > 0)
                {
                    if (!counts.ContainsKey(v)) counts[v] = 0;
                    counts[v]++;
                    maxCount = Math.Max(maxCount, counts[v]);
                }
            }
            if (maxCount == 2) return Color.Yellow;
            if (maxCount == 3) return Color.Orange;
            return Color.SkyBlue;
        }

        private static void DrawCurrentRoll(GameState state)
        {
            Raylib.DrawTextEx(GameFont, "Current Roll:", new Vector2(ScreenWidth / 2 - 55, 10), 20, 2, Color.SkyBlue);
            Texture2D rollTex = state.Player1Turn ? WhiteDice[state.CurrentDie] : BlackDice[state.CurrentDie];
            float elapsed = (float)Raylib.GetTime() - state.CurrentDieRoll.StartTime;
            float duration = 0.25f;
            float scale = 0.5f;
            if (elapsed < duration)
            {
                float t = elapsed / duration;
                float s = 1.70158f;
                float backFactor = (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s) + 1;
                scale = backFactor * 0.5f;
            }
            Vector2 pos = new Vector2(ScreenWidth / 2 - (rollTex.Width * scale) / 2, 35 + (rollTex.Height * 0.5f - rollTex.Height * scale) / 2);
            Raylib.DrawTextureEx(rollTex, pos, 0, scale, Color.White);
            string turnText = state.Player1Turn ? "<< Your Turn" : "AI Thinking >>";
            Raylib.DrawTextEx(GameFont, turnText, new Vector2(ScreenWidth / 2 - 60, 125), 20, 2, state.Player1Turn ? Color.White : Color.Gray);
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

        private static void DrawGradientLine(Vector2 start, Vector2 end, float totalWidth)
        {
            Color[] gradientColors = new Color[] {
                new Color(0, 40, 120, 255),
                new Color(0, 80, 200, 255),
                new Color(100, 200, 255, 255),
                new Color(200, 240, 255, 255),
                new Color(255, 255, 255, 255),
                new Color(200, 240, 255, 255),
                new Color(100, 200, 255, 255),
                new Color(0, 80, 200, 255),
                new Color(0, 40, 120, 255)
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
                new Color(0, 40, 120, 255),
                new Color(0, 80, 200, 255),
                new Color(100, 200, 255, 255),
                new Color(200, 240, 255, 255),
                new Color(255, 255, 255, 255),
                new Color(200, 240, 255, 255),
                new Color(100, 200, 255, 255),
                new Color(0, 80, 200, 255),
                new Color(0, 40, 120, 255)
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
