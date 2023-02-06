using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    #region Header REFERENCES
    [Header("REFERENCES")]
    #endregion

    #region Tooltip
    [Tooltip("Populate with the healthBar gameobject in the enemies")]
    #endregion
    [SerializeField] private HealthBar healthBar;

    public int StartingHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    private HealthEvent healthEvent;
    private Player player;
    private Coroutine immunityCoroutine;
    private bool isImmuneAfterHit = false;
    private float immunityTime;
    private SpriteRenderer spriteRendererAfterHit = null;
    private const float spriteFlashInterval = 0.3f;
    private WaitForSeconds waitForSecondsSpriteFlashinterval = new WaitForSeconds(spriteFlashInterval);
    private float dazedTime;

    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
    }

    private void Start()
    {
        //Trigger the health event for the UI update
        CallHealthEvent(0);

        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        //Get the player/enemy immunity details
        if (player != null)
        {
            if (player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.immunityTime;
                spriteRendererAfterHit = player.spriteRenderer;
            }
        }
        else if (enemy != null)
        {
            if (enemy.enemyDetails.immuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.immunityTime;
                spriteRendererAfterHit = enemy.spriteRendererArray[0];
            }
        }

        //Enable the health bar if required
        if (enemy != null && enemy.enemyDetails.isHealthBarDisplayed && healthBar != null)
        {
            healthBar.EnableHealthBar();
        }
        else if (healthBar != null)
        {
            healthBar.DisableHealthBar();
        }
    }

    private void Update()
    {
        //This is for the enemy only - it makes the enemy stop moving and firing when hit
        if (enemy != null)
        {
            if (dazedTime <= 0)
            {
                enemy.enemyMovementAI.enemySpeed = enemy.enemyMovementAI.movementDetails.GetMoveSpeed();
                enemy.fireWeapon.enabled = true;
            }
            else
            {
                enemy.enemyMovementAI.enemySpeed = 0;
                enemy.idleEvent.CallIdleEvent();
                enemy.fireWeapon.enabled = false;
                dazedTime -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// This method is called when the object takes damage
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        bool isPlayerRolling = false;

        if (player != null)
            isPlayerRolling = player.playerControl.isPlayerRolling;

        if (isDamageable && !isPlayerRolling)
        {
            CurrentHealth -= damageAmount;
            CallHealthEvent(damageAmount);

            EnemySoundEffectsAndDazedTime();

            PostHitImmunity();

            //Set the enemy health remaining as a percentage and display it in the health bar
            if (healthBar != null)
            {
                healthBar.SetHealthBarValue((float)CurrentHealth / (float)StartingHealth);
            }
        }
    }

    public void ReceiveExplosionDamage(int explosiveDamage)
    {
        bool isPlayerRolling = false;

        if (player != null)
            isPlayerRolling = player.playerControl.isPlayerRolling;

        if (isDamageable && !isPlayerRolling)
        {
            CurrentHealth -= explosiveDamage;
            CallHealthEvent(explosiveDamage);

            PostHitImmunity();

            if (healthBar != null)
            {
                healthBar.SetHealthBarValue((float)CurrentHealth / (float)StartingHealth);
            }
        }
    }

    /// <summary>
    /// Gives a post hit immunity for a certain amount of time
    /// </summary>
    private void PostHitImmunity()
    {
        //If the gameobject is not active then just return
        if (gameObject.activeSelf == false)
            return;

        //If theres immunity then proceed
        if (isImmuneAfterHit)
        {
            if (immunityCoroutine != null)
                StopCoroutine(immunityCoroutine);

            //Flash the gameobject red and give immunity time
            immunityCoroutine = StartCoroutine(PostHitImmunityRoutine(immunityTime, spriteRendererAfterHit));
        }
    }

    /// <summary>
    /// Coroutine to make the gameobject flash red and give the immunity time
    /// </summary>
    private IEnumerator PostHitImmunityRoutine(float immunityTime, SpriteRenderer spriteRendererAfterHit)
    {
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 2f);

        isDamageable = false;

        while (iterations > 0)
        {
            spriteRendererAfterHit.color = Color.red;
            
            if (player)
            {
                SoundManager.Instance.PlaySoundEffect(GameResources.Instance.afterHitInmunitySound);
            }

            yield return waitForSecondsSpriteFlashinterval;

            spriteRendererAfterHit.color = Color.white;
            yield return waitForSecondsSpriteFlashinterval;

            iterations--;
            yield return null;
        }

        isDamageable = true;
    }

    /// <summary>
    /// Updates the GUI with the changes on the health
    /// </summary>
    public void CallHealthEvent(int damageAmount)
    {
        healthEvent.CallHealthChangedEvent((float)CurrentHealth / (float)StartingHealth, CurrentHealth, damageAmount);
    }

    /// <summary>
    /// Set The Starting Health
    /// </summary>
    public void SetStartingHealth(int startingHealth)
    {
        this.StartingHealth = startingHealth;
        CurrentHealth = startingHealth;
    }

    /// <summary>
    /// Get The Starting Health
    /// </summary>
    public int GetStartingHealth()
    {
        return StartingHealth;
    }

    /// <summary>
    /// Increase the health by the specified percentage
    /// </summary>
    /// <param name="healthPercent">The percentage that the health will increase</param>
    public void AddHealth(int healthPercent)
    {
        int healthIncrease = Mathf.RoundToInt((StartingHealth * healthPercent) / 100f);

        int totalHealth = CurrentHealth + healthIncrease;

        if (totalHealth > StartingHealth)
        {
            CurrentHealth = StartingHealth;
        }
        else
        {
            CurrentHealth = totalHealth;
        }

        CallHealthEvent(0);
    }

    /// <summary>
    /// Just sets the enemy dazed time
    /// </summary>
    private void EnemySoundEffectsAndDazedTime()
    {
        //This is for the enemy only - it makes the enemy stop moving and firing when hit
        if (enemy != null)
        {
            dazedTime = enemy.enemyDetails.startDazeTime;
        }
    }
}
