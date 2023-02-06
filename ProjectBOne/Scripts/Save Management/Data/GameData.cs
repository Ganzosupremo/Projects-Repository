using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the Game data that will be saved to the disk.
/// The <see cref="PersistenceDataManager"/> will do this automatically.
/// </summary>
[System.Serializable]
public class GameData
{
    public long lastUpdated;

    // Save the current bitcoin sats the player has to the disk
    public double satsOnHold;

    // Save the score of the player to the disk
    public HighScores highScores;

    // Save the current index of the dungeon, which is the index the dungeon builder uses to build the dungeon
    public int currentDungeonIndex;

    /// <summary>
    /// The data defined here are the default values
    /// when the game starts and there's no data to load
    /// </summary>
    public GameData()
    {
        satsOnHold = 0;
        currentDungeonIndex = 0;
        //highScores = new();
    }

    /// <summary>
    /// Gets the amount of sats that have been collected
    /// </summary>
    /// <returns>Returns the amount of sats collected and saved</returns>
    public double GetSatsOnHold()
    {
        return satsOnHold;
    }
}

