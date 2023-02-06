using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class LaserAmmo : MonoBehaviour, IFireable
{
    [SerializeField] private LineRenderer lineRenderer;

    // This is under construction, I'm trying to do a laser weapon
    private bool overrideAmmoMovement = false;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private float ammoRange = 0f;
    private float ammoSpeed;
    private float fireDirectionAngle;
    private Vector3 fireDirectionVector;
    private float ammoChargeTimer;
    private bool hasAmmoMaterialSet;
    private Player player;
    private PolygonCollider2D polygonCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        player = GameManager.Instance.GetPlayer();
    }

    private void Start()
    {
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (ammoChargeTimer > 0)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!hasAmmoMaterialSet)
        {
            SetAmmoLaserMaterial(ammoDetails.ammoMaterial);
            hasAmmoMaterialSet = true;
        }

        //Don't move the ammo if the movement has been overriden, meaning this ammo is part of an ammo pattern
        if (!overrideAmmoMovement)
        {
            UpdateLaser();

            //Calculate distance vector to move the bullet
            Vector3 distanceVector = ammoSpeed * Time.deltaTime * fireDirectionVector;

            transform.position += distanceVector;

            //Disable after the max range has been reached
            ammoRange -= distanceVector.magnitude;

            if (ammoRange < 0)
            {
                if (ammoDetails.isPlayerAmmo)
                    //Call the multiply score event
                    StaticEventHandler.CallMultiplierEvent(false);

                DisableAmmo();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DealLaserDamage(other);

        LaserHitEffect();

        DisableAmmo();
    }

    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    DealLaserDamage(other);

    //    LaserHitEffect();
    //}

    private void DealLaserDamage(Collider2D otherCollider)
    {
        otherCollider.TryGetComponent(out Health health);

        //bool enemyHit = false;

        if (health != null)
        {
            health.TakeDamage(ammoDetails.ammoDamage);

            //if (health.enemy != null)
            //    enemyHit = true;
        }

        //if (ammoDetails.isPlayerAmmo)
        //{
        //    if (enemyHit)
        //        //Update the multiplier by 1
        //        StaticEventHandler.CallMultiplierEvent(true);
        //    else
        //        //Reduce the multiplier by 1
        //        StaticEventHandler.CallMultiplierEvent(false);
        //}
    }

    /// <summary>
    /// Spawns the particles for the hit effect of the laser
    /// </summary>
    private void LaserHitEffect()
    {
        // Process if there is a hit effect & prefab
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.bulletHitEffectPrefab != null)
        {
            // Get ammo hit effect gameobject from the pool with particle system component
            AmmoHitEffect hitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent
                (ammoDetails.ammoHitEffect.bulletHitEffectPrefab, transform.position, Quaternion.identity);

            // Set hit effect
            hitEffect.SetAmmoHitEffect(ammoDetails.ammoHitEffect);

            // Set gameobject active (the particle system is set to automatically disable the
            // gameobject once finished)
            hitEffect.gameObject.SetActive(true);
        }
    }

    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        #region Bullet
        this.ammoDetails = ammoDetails;

        //Sets the fire direction
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);
        
        UpdateLaser();

        SetPolygonColliderShapePoints();

        if (ammoDetails.ammoChargeTime > 0)
        {
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            hasAmmoMaterialSet = false;
        }
        else
        {
            ammoChargeTimer = 0f;
            hasAmmoMaterialSet = true;
        }

        //Set ammo range
        ammoRange = ammoDetails.ammoRange;

        //Set ammo speed
        this.ammoSpeed = ammoSpeed;

        //Activate the ammo gameobject
        gameObject.SetActive(true);
        lineRenderer.enabled = true;
        #endregion
    }

    private void UpdateLaser()
    {
        Vector2 direction = fireDirectionVector - transform.position;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, direction.normalized, direction.magnitude);

        if (hit)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, fireDirectionVector);
        }
    }

    private void SetPolygonColliderShapePoints()
    {
        if (polygonCollider != null && spriteRenderer.sprite != null)
        {
            List<Vector2> spritePhysicsPointsList = new();
            spriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsPointsList);

            polygonCollider.points = spritePhysicsPointsList.ToArray();
        }
    }

    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        float spreadRandomAngle = Random.Range(ammoDetails.ammoSpreadMin, ammoDetails.ammoSpreadMax);

        Vector3 weaponFirePosition = player.activeWeapon.GetFirePosition();
        lineRenderer.SetPosition(0, weaponFirePosition);

        //Get a random toggle between 1 or -1
        int spreadRandomToggle = Random.Range(0, 2) * 2 - 1;

        if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
        {
            fireDirectionAngle = aimAngle;
        }
        else
        {
            fireDirectionAngle = weaponAimAngle;
        }

        //Adjust the bullet fire angle with the random spread
        fireDirectionAngle += spreadRandomToggle * spreadRandomAngle;

        //Set the bullet rotation if needed
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        //Set the bullet fire direction
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    /// <summary>
    /// Sets the material for the ammo and laser beam
    /// </summary>
    /// <param name="ammoMaterial"></param>
    private void SetAmmoLaserMaterial(Material ammoMaterial)
    {
        spriteRenderer.material = ammoMaterial;
        lineRenderer.material = ammoMaterial;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void DisableAmmo()
    {
        lineRenderer.enabled = false;
        gameObject.SetActive(false);
    }
}
