# KnuckleBones (C# + RayLib)

A faithful recreation of the KnuckleBones dice game using C# and RayLib-cs.

## Project Structure

- **Program.cs**: Application lifecycle and main loop.
- **UI.cs**: Rendering logic, resource management, and score calculation.
- **UIHandling.cs**: Input processing for human and AI turns.
- **AI.cs**: Decision-making logic for the computer opponent.

## Game Rules

Derived from the original implementation, the game follows these core mechanics:

### 1. Placement
- Players take turns placing a rolled die (1-6) into one of their three columns.
- Each column has a maximum capacity of 3 dice.

### 2. Scoring (Horizontal Combos)
The score for each board is calculated by summing the dice values in each **row**. Duplicate numbers in the same row are multiplied by their count:
- **One Die in Row**: `value * 1`
- **Two Matching Dice in Row**: `(value + value) * 2` (e.g., two 3s = 12 points)
- **Three Matching Dice in Row**: `(value + value + value) * 3` (e.g., three 6s = 54 points)

Total score is the sum of all three rows.

### 3. Countering (Row-Based Destruction)
- When you place a die, any dice of the **same value** in your opponent's **corresponding row** are **destroyed** (removed).
- For example, if you place a `6` in the middle row, all `6`s in the opponent's middle row are removed across all columns.

### 4. Game End
- The game ends immediately when any player fills all 9 slots in their grid.
- The player with the highest total score wins.

## Controls
- **Mouse Left Click**: Select a column to place your die.
- **R Key**: Restart the game after a Game Over.

## Technical Details
- **Resolution**: 800x800 pixels.
- **Assets**: 
    - Fonts: `resources/fonts/revvy.ttf`
    - Images: `resources/img/` (White dice for Player, Black dice for AI).
- **Dependencies**: Raylib-cs.
