using UnityEngine;

public interface IFireable
{
    /// <summary>
    /// Initialises all the necessary details for the ammo
    /// </summary>
    /// <param name="ammoDetails"></param>
    /// <param name="aimAngle"></param>
    /// <param name="weaponAimAngle"></param>
    /// <param name="ammoSpeed"></param>
    /// <param name="weaponAimDirectionVector">The aim direction of the weapon, in vector3</param>
    /// <param name="overrideAmmoMovement">True if this ammo is part of an ammo pattern</param>
    void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, 
        float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false);

    GameObject GetGameObject();
}
