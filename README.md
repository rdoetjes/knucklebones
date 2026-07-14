# Dicey Star Cluster (C# + RayLib)

A faithful recreation of the KnuckleBones dice game with enhanced dual-axis strategic depth, set in the deep reaches of space.

## Game Rules

The game is played on a 3x3 grid. Players must balance offensive destruction and defensive scoring across two directions.

### 1. Placement
- Players take turns placing a rolled die (1-6) into one of their available grid slots.
- Each of the 9 slots can hold one die.

### 2. Dual-Axis Scoring
The score is calculated for both the **Rows** and the **Columns** independently, but **only the higher of the two scores is kept**. This requires players to check two directions simultaneously when placing a die.

For each line (Row or Column):
- **Single Die**: `value * 1`
- **Matching Dice**: Summed and then multiplied by their count (e.g., two 3s = `(3+3)*2 = 12`, three 6s = `(6+6+6)*3 = 54`).
- **Diversity Bonus**: If a line is full (3 dice) and contains **three different values** (e.g., 1, 4, 6), it receives a **+10 bonus**.

### 3. Destruction (Row & Column)
When you place a die, it acts as an attack in both directions:
- **Horizontal**: Any dice of the same value in your opponent's **corresponding row** are removed.
- **Vertical**: Any dice of the same value in your opponent's **corresponding column** are removed.

### 4. Game End
- The game ends immediately when any player fills all 9 slots in their grid.
- The player with the highest final score (using their best axis) wins.

## Controls
- **Mouse Left Click**: Select a slot to place your die.
- **Difficulty Buttons**: Select Easy (5s), Medium (6s), or Hard (8s) thinking time for the AI.
- **R Key**: Restart the game after a Game Over.,

## Strategic Tips
- Use the **Destruction Preview** (red highlight) to see which opponent dice will be removed before you click.
- Monitor the **Score Breakdown** labels to see which direction (Row or Column) is currently providing your highest score.
- Sometimes placing a die to complete a "Diversity Bonus" is better than chasing a multiplier if it secures a higher total for that axis.

## Technical Details
- **Resolution**: 720x700 pixels.
- **AI**: Uses Iterative Deepening Minimax with Parallel processing and a time-limited search.
- **Dependencies**: Raylib-cs.
