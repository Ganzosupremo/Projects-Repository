using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the things that are gonna be saved to the disk
/// need to implement this interface.
/// The <see cref="PersistenceDataManager"/> will find all the objects implementing this interface.
/// </summary>
public interface IPersistenceData
{
    /// <summary>
    /// Loads the saved game data from the disk
    /// </summary>
    /// <param name="data"></param>
    void LoadData(GameData data);
    
    /// <summary>
    /// Saves the game data to the disk
    /// </summary>
    /// <param name="data"></param>
    void SaveData(GameData data);
}
