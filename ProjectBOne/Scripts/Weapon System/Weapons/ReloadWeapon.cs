using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(ActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private Player player;
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponReloadedEvent weaponReloadedEvent;
    private ActiveWeaponEvent activeWeaponEvent;
    private Coroutine reloadWeaponCoroutine;
    private SpriteRenderer weaponSprite;

    private void Awake()
    {
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        activeWeaponEvent = GetComponent<ActiveWeaponEvent>();
        player = GetComponent<Player>();
        weaponSprite = player.weaponGameObject.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        //subscribe to the reload weapon event
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnWeaponReloading;
        
        //suscribe to the active weapon event
        activeWeaponEvent.OnSetActiveWeapon += ActiveWeaponEvent_OnSetActiveWeapon;
    }

    private void OnDisable()
    {
        //unsubscribe to the reload weapon event
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnWeaponReloading;

        //unsuscribe to the active weapon event
        activeWeaponEvent.OnSetActiveWeapon -= ActiveWeaponEvent_OnSetActiveWeapon;
    }

    /// <summary>
    /// Handles The Reload Weapon Event
    /// </summary>
    private void ReloadWeaponEvent_OnWeaponReloading(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        StartReloadingWeapon(reloadWeaponEventArgs);
    }

    /// <summary>
    /// Start Reloading The Weapon With A Coroutine
    /// </summary>
    private void StartReloadingWeapon(ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }

        reloadWeaponCoroutine = StartCoroutine(ReloadingWeaponCoroutine(reloadWeaponEventArgs.weapon, reloadWeaponEventArgs.topUpAmmoPorcent));
    }

    private IEnumerator ReloadingWeaponCoroutine(Weapon weapon, int topUpAmmoPorcent)
    {
        // Change the sprite when the clip is depletted
        if (weapon.weaponDetails.weaponReloadSprite != null)
            weaponSprite.sprite = weapon.weaponDetails.weaponReloadSprite;

        // Play the reload sound when the weapon is reloaded
        if (weapon.weaponDetails.weaponReloadSoundEffect != null)
            SoundManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadSoundEffect);

        weapon.isWeaponReloading = true;

        // Update the reload timer
        while (weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        // If the total ammo should be increased
        if (topUpAmmoPorcent != 0)
        {
            int ammoIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponTotalAmmoCapacity * topUpAmmoPorcent) / 100);

            int totalAmmo = weapon.weaponTotalAmmoCapacity + ammoIncrease;

            if (totalAmmo > weapon.weaponDetails.weaponTotalAmmoCapacity)
            {
                weapon.weaponTotalAmmoCapacity = weapon.weaponDetails.weaponTotalAmmoCapacity;
            }
            else
            {
                weapon.weaponTotalAmmoCapacity = totalAmmo;
            }
        }

        // If the weapon has infinity ammo, then just refill the mag
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponClipRemaining = weapon.weaponDetails.weaponMagMaxCapacity;
        }
        // else if not infinite ammo then if remaining ammo is greater than the amount required to
        // refill the clip, then fully refill the clip
        else if (weapon.weaponTotalAmmoCapacity >= weapon.weaponDetails.weaponMagMaxCapacity)
        {
            weapon.weaponClipRemaining = weapon.weaponDetails.weaponMagMaxCapacity;
        }
        // else set the clip to the remaining ammo
        else
        {
            weapon.weaponClipRemaining = weapon.weaponTotalAmmoCapacity;
        }

        // Reset weapon reload timer
        weapon.weaponReloadTimer = 0f;

        // Set weapon as not reloading
        weapon.isWeaponReloading = false;

        // Change the sprite to normal again
        if (weapon.weaponClipRemaining != 0 && weapon.weaponTotalAmmoCapacity != 0)
            weaponSprite.sprite = weapon.weaponDetails.weaponSprite;

        // Call weapon reloaded event
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Handles The Active Weapon Event
    /// </summary>
    private void ActiveWeaponEvent_OnSetActiveWeapon(ActiveWeaponEvent activeWeaponEvent, ActiveWeaponEventArgs activeWeaponEventArgs)
    {
        if (activeWeaponEventArgs.playerWeapon.isWeaponReloading)
        {
            if (reloadWeaponCoroutine != null)
            {
                reloadWeaponCoroutine = StartCoroutine(ReloadingWeaponCoroutine(activeWeaponEventArgs.playerWeapon, 0));
            }
        }
    }
}
