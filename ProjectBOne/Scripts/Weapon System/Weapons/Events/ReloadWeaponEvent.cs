using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ReloadWeaponEvent : MonoBehaviour
{
    public Action<ReloadWeaponEvent, ReloadWeaponEventArgs> OnReloadWeapon;

    /// <summary>
    /// Call this method when a weapon needs to be realoaded, or top-up the ammo
    /// </summary>
    /// <param name="weapon">The current weapon, you can get the current weapon by calling the GetCurrentWeapon() in the Active Weapon class</param>
    /// <param name="topUpAmmoPorcent">The percent that the ammo of the current weapon will top-up</param>
    public void CallReloadWeaponEvent(Weapon weapon, int topUpAmmoPorcent)
    {
        OnReloadWeapon?.Invoke(this, new ReloadWeaponEventArgs()
        {
            weapon = weapon,
            topUpAmmoPorcent = topUpAmmoPorcent
        });

    }
}

public class ReloadWeaponEventArgs : EventArgs
{
    public Weapon weapon;
    public int topUpAmmoPorcent;
}