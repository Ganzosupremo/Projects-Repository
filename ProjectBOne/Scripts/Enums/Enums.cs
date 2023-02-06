/// <summary>
/// Used to know the orientation of a room
/// </summary>
public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}

/// <summary>
/// Used to determine the direction the player is aiming
/// </summary>
public enum AimDirection
{
    Up,
    UpLeft,
    UpRight,
    Right,
    Left,
    Down
}

/// <summary>
/// To know when to spawn a chest
/// </summary>
public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

/// <summary>
/// The position the chest will spawn
/// </summary>
public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

/// <summary>
/// Define the state of a chest
/// </summary>
public enum ChestState
{
    closed,
    healthItem,
    ammoItem,
    weaponItem,
    empty
}

/// <summary>
/// Used to know the current state of a game, and do things dependant on that state
/// </summary>
public enum GameState
{
    gameStarted,
    playingLevel,
    killingEnemies,
    BossStage,
    killingBoss,
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused, 
    dungeonOverviewMap,
    gameRestarted
}

/// <summary>
/// Defines the type of the shop item
/// </summary>
public enum ItemShopType
{
    isWeapon,
    isAmmo,
    isHealth,
    isFiatExchange
}

namespace ComputeShader
{
    public enum Channel
    {
        Red,
        Green,
        Blue,
        Alpha,
        Zero
    }
}

