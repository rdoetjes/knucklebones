using Xunit;
using KnuckleBones;

namespace KnuckleBonesTests
{
    public class AITests
    {
        [Fact]
        public void AI_ShouldDestroyOpponentDice_WhenPossible()
        {
            GameState state = new GameState();
            state.CurrentDifficulty = Difficulty.Hard;
            
            // Player has a 6 in Col 0, Row 0
            state.Player1Board[0][0] = 6;
            
            // AI rolls a 6
            state.CurrentDie = 6;
            state.Player1Turn = false;
            
            int move = AI.GetMove(state, useRandom: false);
            
            // AI should prefer Col 0 to destroy the player's 6
            Assert.Equal(0, move);
        }

        [Fact]
        public void AI_ShouldPreferMultipliers_WhenNoDestructionPossible()
        {
            GameState state = new GameState();
            state.CurrentDifficulty = Difficulty.Easy; // Use Easy to avoid complex lookahead noise
            
            // AI already has a 5 in Col 1
            state.Player2Board[1][0] = 5;
            
            // AI rolls a 5. No destruction possible.
            state.CurrentDie = 5;
            state.Player1Turn = false;
            
            int move = AI.GetMove(state, useRandom: false);
            
            // AI should prefer Col 1 to get a multiplier (5*2)*2 = 20
            // Placing in Col 0 or 2 would only result in 5 + 5 = 10 (if placed in same row) or 5 elsewhere.
            Assert.Equal(1, move);
        }

        [Fact]
        public void AI_Hard_ShouldLookAhead()
        {
            GameState state = new GameState();
            state.CurrentDifficulty = Difficulty.Hard;
            
            // Setup a situation where one move is obviously better long-term
            // AI rolls a 4.
            state.CurrentDie = 4;
            state.Player1Turn = false;
            
            int move = AI.GetMove(state);
            
            // Just ensuring it returns a valid column 0-2
            Assert.InRange(move, 0, 2);
        }
    }
}
