using Raylib_cs;

namespace KnuckleBones
{
    class Program
    {
        static void Main(string[] args)
        {
            Raylib.InitWindow(UI.ScreenWidth, UI.ScreenHeight, "Knuckle Bones");
            Raylib.SetTargetFPS(30);

            UI.LoadResources();
            GameState state = new GameState();

            while (!Raylib.WindowShouldClose())
            {
                UIHandling.Update(state);

                Raylib.BeginDrawing();
                UI.DrawBoard(state);
                Raylib.EndDrawing();
            }

            UI.UnloadResources();
            Raylib.CloseWindow();
        }
    }
}
