# Functional Requirements

**1. User connects to server:** The player’s browser client can establish a connection with the Rust server and stay connected for real-time communication.

**2. Lobby creation and listing:** Players can create new game lobbies, which appear in the public lobby list for others to join.

**3. Join game room:** Players can join existing game rooms and immediately see a list of all connected players.

**4. Admin controls:** The host can remove (kick) players from a game lobby.

**5. Start game:** The host (when all players are marked ready) can start the game, triggering server-side initialization of the board and turn order.

**6. Authoritative game state and turn control:** The server holds the canonical version of the game state and controls which player’s turn it is, rejecting invalid or out-of-turn actions.

**7. Active Player's turn:** Active player shall be able to perform any number actions when their preconditions are met. Possible actions are building(road/settlement/town), trading(with player/bank) and buying/playing a development card.

**8. Inactive Player's turn:** When a player is not on turn he shall still be able to accept trades made by active player.

**9. Turn timer:** Each turn has a countdown timer, and if the player fails to act within the limit, the server automatically ends their turn.

**10. End turn:** The active player can signal the end of their turn, which advances gameplay to the next player in the sequence.

**11. Board state synchronization:** The server keeps all connected clients updated with the current game board and sends periodic or event-based state updates.

**12. Dice roll and resource generation:** The server simulates dice rolls at the start of turns and distributes resources to players based on their settlements and hex values.

**13. Resource tracking:** The server maintains accurate resource counts for each player and verifies whether players can afford to perform certain actions.

**14. Win condition detection:** The server automatically detects when a player reaches the required victory points and ends the game, announcing the winner.

**15. Game result:** After a player has reached given number of points, game ends and ending screen with the winner is displayed.

**16. Player disconnect and reconnection:** If a player disconnects, the game preserves their state, and they can reconnect within a short time window to resume playing.