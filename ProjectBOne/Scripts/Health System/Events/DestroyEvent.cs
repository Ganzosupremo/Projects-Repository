using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyEvent : MonoBehaviour
{
    public Action<DestroyEvent, DestroyEventArgs> OnDestroy;

    /// <summary>
    /// The Gameobject gets destroyed when this event is fired
    /// </summary>
    /// <param name="playerDied">True when the player dies</param>
    /// <param name="points">The total amount of points scored</param>
    /// <param name="totalEnemiesKilled">The total amount of enemies killed, this parameter should only be filled in the EnemySpawner Class</param>
    public void CallOnDestroyEvent(bool playerDied, int points, int totalEnemiesKilled = 0)
    {
        OnDestroy?.Invoke(this, new DestroyEventArgs()
        {
            playerDeath = playerDied,
            points = points,
            totalEnemiesKilled = totalEnemiesKilled
        });
    }
}

public class DestroyEventArgs : EventArgs
{
    public bool playerDeath;
    public int points;
    public int totalEnemiesKilled;
}
