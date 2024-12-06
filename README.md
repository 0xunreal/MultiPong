# MultiPong

![alt text](https://github.com/0xunreal/MultiPong/blob/main/imgs/game.PNG)
 
# WebSocket-Based Multiplayer and State Synchronization

This system uses a WebSocket connection to synchronize game state and player statistics between the server and connected clients.

## Overview

- **ServerWebSocketService**:  
  The server side is built on `WebSocketSharp` and runs inside `GameServer`. When a client connects, it creates a session. Incoming messages are deserialized from JSON into `BaseMessage` objects using `MessageFactory`. Handlers on the server then process these messages, updating the `GameState` or player statistics as needed.

- **Client (WebSocketConnection)**:  
  Each client runs a `WebSocketConnection` MonoBehaviour. It connects to the server and listens for messages. Incoming JSON messages are queued and processed on the main Unity thread (`Update()`), ensuring thread safety. Registered message handlers match specific message types (e.g., `GameStateMessage`, `PlayerStatisticsMessage`) and update the client’s local game objects or UI.

- **Message Flow**:
  - **From Client to Server**:  
    - `Client_PaddleInput`: Client input for paddle movement.  
    - `Client_PlayerJoin`: Informs the server when a player joins.  
    - `Client_PlayerLeave`: Informs the server when a player leaves.
  - **From Server to Client**:  
    - `Server_GameStateSync`: Authoritative game state (ball, paddles).  
    - `Server_PlayerStatisticsSync`: Player stats and awarded badges.

- **Extensibility**:  
  By registering additional message types and handlers, the system can easily be extended to synchronize more data (e.g., leaderboard updates, chat messages) without changing the underlying infrastructure.


# Player Statistics (KPI) and Badge System

The system tracks and persists player statistics and awards badges based on predefined conditions.

## Tracked Statistics

The following statistics are tracked globally and aggregated based on defined rules:

| Stat Name   | Aggregation | Description                                   |
|-------------|-------------|-----------------------------------------------|
| Joins       | SUM         | Counts how many times a player has joined     |
| Score       | SUM         | Tracks the cumulative score a player earns    |
| HighScore   | MAX         | Records the highest single-game score         |
| Wins        | SUM         | Counts the total number of wins a player has  |

**Note:** The `Score` stat is aggregated cumulatively across all sessions, while `HighScore` only updates if the player achieves a new personal best in a single session. `Joins` and `Wins` are simply summed over time.

## Badges

Badges are awarded when certain conditions are met. They can be triggered by reaching a threshold in a specific stat or by one-off events.

| Badge ID         | Badge Type      | Trigger Condition                            |
|------------------|-----------------|-----------------------------------------------|
| Join5Times       | StatThreshold   | `Joins >= 5`                                 |
| Win10Times       | StatThreshold   | `Wins >= 10`                                 |
| Score100         | StatThreshold   | `TotalScore >= 100` (uses cumulative Score)   |
| FirstGoalScored  | OneOff          | Triggered by the event `FirstGoalScored`      |

## Overview

- **Player Statistics**:  
  Each player has persistent stats stored on the server, such as `Wins` or `Score`.  
  Statistics are updated through a `PlayerStatisticsService` that applies the correct aggregation method (SUM, MAX, or fallback to LAST).

- **Badge System**:  
  - **StatThreshold badges**: Automatically awarded when a player’s stat reaches/exceeds a threshold.  
  - **OneOff badges**: Awarded for special events that occur once.

- **Global Definitions**:
  - **GlobalPlayerStatistics**: Defines which stats are tracked and their aggregation rules.
  - **GlobalBadgeDefinitions**: Defines which badges exist, their types, and their conditions.

- **PlayerStatisticsService**:
  - Handles updating player stats with the correct aggregation rules.
  - Checks and awards badges after each stat update.

- **BadgeService**:
  - Evaluates conditions whenever player stats or events are updated.
  - Awards badges and ensures they are persisted and synced.

- **Repository and Persistence**:
  - `PlayerStatisticsRepository`: Manages the storage and retrieval of player stats.
  - `GetAllStats()` provides data to be packaged into `PlayerStatisticsMessage` for client sync.

## Syncing With Clients

- The server periodically creates `PlayerStatisticsMessage` objects with updated stats and badges.
- Clients receive these messages and update their UI or local records accordingly.

# How to Build & Run

![alt text](https://github.com/0xunreal/MultiPong/blob/main/imgs/buildmenu.PNG)

- Server logs are saved to ./Logs folder

## Requirements

- Unity (6000.0.29f1)
- Windows Dedicated Server Build Support module

- Tested on Windows 10 and Windows 11
