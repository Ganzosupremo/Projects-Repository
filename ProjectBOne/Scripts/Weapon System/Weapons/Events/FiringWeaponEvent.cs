using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FiringWeaponEvent : MonoBehaviour
{
    public Action<FiringWeaponEvent, FiringWeaponEventArgs> OnFireWeapon;

    public void CallOnFireWeaponEvent(bool hasFired, bool firedPreviousFrame, AimDirection aimDirection, 
        float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, WeaponShootEffectSO weaponShootEffectSO)
    {
        OnFireWeapon?.Invoke(this, new FiringWeaponEventArgs()
        {
            hasFired = hasFired,
            firedPreviousFrame = firedPreviousFrame,
            aimDirection = aimDirection,
            aimAngle = aimAngle,
            weaponAimAngle = weaponAimAngle,
            weaponAimDirectionVector = weaponAimDirectionVector,
            weaponShootEffectSO = weaponShootEffectSO
        });
    }
}

public class FiringWeaponEventArgs : EventArgs
{
    public bool hasFired;
    public bool firedPreviousFrame;
    public AimDirection aimDirection;
    public float aimAngle;
    public float weaponAimAngle;
    public Vector3 weaponAimDirectionVector;
    public WeaponShootEffectSO weaponShootEffectSO;
}