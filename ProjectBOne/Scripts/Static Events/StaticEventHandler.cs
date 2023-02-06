using System;

public static class StaticEventHandler
{
    #region ON ROOM CHANGED EVENT
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }
    #endregion

    #region ON ROOM ENEMIES DEFEATED EVENT
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    public static void CallRoomEnemiesDefeatedEvent(Room room)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs() { room = room });
    }
    #endregion

    #region ON POINTS SCORED EVENT
    public static event Action<PointsScoredArgs> OnPointsScored;

    public static void CallPointsScoredEvent(int points)
    {
        OnPointsScored?.Invoke(new PointsScoredArgs()
        {
            score = points
        });
    }
    #endregion

    #region ON SCORE CHANGED EVENT
    public static event Action<ScoreChangedArgs> OnScoreChanged;

    public static void CallScoreChangedEvent(long score, int multiplier)
    {
        OnScoreChanged?.Invoke(new ScoreChangedArgs()
        {
            score = score,
            multiplier = multiplier
        });
    }
    #endregion

    #region ON MULTIPLIER EVENT
    public static event Action<MultiplierArgs> OnMultiplier;

    public static void CallMultiplierEvent(bool multiplier)
    {
        OnMultiplier?.Invoke(new MultiplierArgs()
        {
            multiplier = multiplier
        });
    }
    #endregion

    #region MONEY EVENT
    /// <summary>
    /// Used to create the money, and then the OnMoneyChanged updates the money showed in the UI
    /// </summary>
    public static event Action<MoneyEventArgs> OnMoneyEvent;

    /// <summary>
    /// Spawns new money into existant
    /// </summary>
    /// <param name="value">The value of the new money</param>
    /// <param name="isBitcoin">Wheter this money is Bitcoin or not.</param>
    public static void CallMoneyEvent(double value, bool isBitcoin)
    {
        OnMoneyEvent?.Invoke(new MoneyEventArgs()
        {
            value = value,
            isBitcoin = isBitcoin
        });
    }
    #endregion

    #region ON MONEY CHANGED EVENT
    /// <summary>
    /// This event is used to update the money in the UI
    /// </summary>
    public static event Action<MoneyChangedEventArgs> OnMoneyChanged;

    /// <summary>
    /// Updates the money showed in the UI
    /// </summary>
    /// <param name="value">The value of this money</param>
    /// <param name="isBitcoin">Whether this money is Bitcoin or not</param>
    public static void CallMoneyChangedEvent(double value, bool isBitcoin)
    {
        OnMoneyChanged?.Invoke(new MoneyChangedEventArgs()
        {
            value = value,
            isBitcoin = isBitcoin
        });
    }
    #endregion
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}

public class PointsScoredArgs : EventArgs
{
    public int score;
}

public class ScoreChangedArgs : EventArgs
{
    public long score;
    public int multiplier;
}

public class MultiplierArgs : EventArgs
{
    public bool multiplier;
}

public class MoneyEventArgs : EventArgs
{
    public double value;
    public bool isBitcoin;
}

public class MoneyChangedEventArgs : EventArgs
{
    public double value;
    public bool isBitcoin;
}