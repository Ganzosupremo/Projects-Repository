using System.Collections.Generic;

/// <summary>
/// Defines the objects to spawn based on the <seealso cref="GameLevelSO"/>.
/// </summary>
/// <typeparam name="T">The type of object to spawn</typeparam>
[System.Serializable]
public class SpawnableObjectByLevel<T>
{
    public GameLevelSO gameLevel;
    public List<SpawnableObjectRatio<T>> spawnableObjectRatioList;
}
