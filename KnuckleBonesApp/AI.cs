using Raylib_cs;
using System.Collections.Generic;
using System.Linq;

namespace KnuckleBones
{
    public class AI
    {
        public static int GetMove(GameState state)
        {
            // Simple AI: Prefer columns where it can destroy opponent dice,
            // then columns where it has a match, else random available column.

            List<int> availableCols = [];
            for (int c = 0; c < 3; c++)
            {
                if (state.Player2Board[c].Any(x => x == 0))
                    availableCols.Add(c);
            }

            if (availableCols.Count == 0) return -1;

            // 1. Can we destroy opponent dice?
            foreach (int c in availableCols)
            {
                if (state.Player1Board[c].Any(x => x == state.CurrentDie))
                    return c;
            }

            // 2. Can we match our own?
            foreach (int c in availableCols)
            {
                if (state.Player2Board[c].Any(x => x == state.CurrentDie))
                    return c;
            }

            // 3. Pick random
            return availableCols[Raylib.GetRandomValue(0, availableCols.Count - 1)];
        }
    }
}
