using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoney
{
    /// <summary>
    /// Initialises the parameters necessary for the money to work
    /// </summary>
    /// <param name="moneyDetailsSO">The money details SO, usually is in the enemy script</param>
    /// <param name="moneyValue">The value for this specific money</param>
    void InitializeMoney(MoneyDetailsSO moneyDetailsSO, double moneyValue);
}
