using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// This is where all the input of the player gets managed and processed.
/// </summary>
[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("The Scriptable Object containing the information about the movement")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;

    /// <summary>
    /// The player script
    /// </summary>
    private Player player;
    private bool fireButtonPressedPreviousFrame = false;
    private int currentWeaponIndex = 1;
    private SpriteRenderer weaponSpriteRenderer;

    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private float playerRollCooldownTimer = 0f;
    private bool isPlayerMovementDisabled = false;

    private CharacterInput playerInputActions;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public bool isPlayerRolling = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
        weaponSpriteRenderer = player.weaponGameObject.GetComponent<SpriteRenderer>();

        playerInputActions = new();
        playerInputActions.Player.Enable();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetPlayerInitialWeapon();

        SetPlayerAnimationSpeed();
    }

    private void Update()
    {
        if (isPlayerMovementDisabled) return;

        //If the player is rolling, then return
        if (isPlayerRolling) return;

        //Process the player input
        MovementInput();

        //Process all the input needed for the weapon
        WeaponInput();

        //Allows the player to use items 
        UseItemInput();

        //Reset the cooldown timer for the player rolling
        ReducePlayerRollCooldownTimer();
    }

    /// <summary>
    /// Process the player movement input
    /// </summary>
    public void MovementInput()
    {
        // Get the movement input
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        bool rightMouseButtonDown = playerInputActions.Player.Roll.WasPressedThisFrame();
        Vector2 moveDirectionVector = new(inputVector.x, inputVector.y);

        //Adjust the diagonal movement (pythagoras approximation)
        if (inputVector.x != 0f && inputVector.y != 0f)
        {
            moveDirectionVector *= 0.8f;
        }

        //If there's movement, either move or roll
        if (moveDirectionVector != Vector2.zero)
        {
            if (!rightMouseButtonDown)
            {
                moveSpeed = movementDetails.GetMoveSpeed();

                player.movementByVelEvent.CallMovementByVelocityEvent(moveDirectionVector, moveSpeed);
            }
            else if (playerRollCooldownTimer <= 0) //Roll if there's no cooldown
            {
                PlayerRoll((Vector3)moveDirectionVector);
            }
        }
        else// If there's no movement
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Sets The Initial Weapon For The Player At The Start Of The Game
    /// </summary>
    private void SetPlayerInitialWeapon()
    {
        int index = 1;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.playerDetails.initialWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }

    /// <summary>
    /// Sets The Animation Speed Based On The Player Movement Speed
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.playerAnimationSpeed;
    }

    /// <summary>
    /// The Player Roll Mechanic
    /// </summary>
    private void PlayerRoll(Vector3 moveDirectionVector)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollCoroutine(moveDirectionVector));
    }

    /// <summary>
    /// Makes the player roll
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayerRollCoroutine(Vector3 moveDirectionVector)
    {
        //Minimun distance to exit the coroutine loop
        float minDistance = 0.2f;

        isPlayerRolling = true;

        Vector3 targetPosition = player.transform.position + (Vector3)moveDirectionVector * movementDetails.rollDistance;

        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementByPosEvent.CallMovementByPositionEvent(targetPosition, player.transform.position,
                movementDetails.rollSpeed, moveDirectionVector, isPlayerRolling);
            
            //Yield and wait for fixed update
            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    private void ReducePlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// This Process The Weapon Input
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        //Aim Weapon Input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        //Fire Weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        //Switch weapon input
        SwitchWeaponInput();

        //Reload weapon input
        ReloadWeaponInput();
    }

    /// <summary>
    /// Aims The Weapon At The Mouse Position
    /// </summary>
    public void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        //Get the position of the mouse in the world
        // TODO - make it compatible with a controller
        Vector2 mousePosition = HelperUtilities.GetMouseWorldPosition();

        //Calculate Direction vector of the mouse cursor from the weapon shoot position
        weaponDirection = (Vector3)mousePosition - player.currentWeapon.GetFirePosition();

        //Calculate direction vector of mouse cursor from the player transform position
        Vector3 playerDirection = (Vector3)mousePosition - transform.position;

        //Get angle from the weapon and the cursor
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        //Get the angle from the player and the cursor
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        //Set the player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        //Trigger the weapon aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);

        // Aim with the controller
        if (Gamepad.current != null)
        {
            Vector2 rightJoystickPosition = HelperUtilities.GetAimPositionGamepad();

            //Calculate Direction vector of the joystick from the weapon shoot position
            weaponDirection = (Vector3)rightJoystickPosition - player.currentWeapon.GetFirePosition();

            //Calculate direction vector of the right joystick from the player transform position
            playerDirection = (Vector3)rightJoystickPosition - transform.position;

            //Get angle from the weapon and the cursor
            weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

            //Get the angle from the player and the cursor
            playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

            //Set the player aim direction
            playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

            //Trigger the weapon aim event
            player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
        }
    }

    /// <summary>
    /// This Method Makes The Weapon Fire
    /// </summary>
    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (playerInputActions.Player.Fire.IsPressed())
        {
            //Trigger The fire weapon event
            player.firingWeaponEvent.CallOnFireWeaponEvent(true,
                fireButtonPressedPreviousFrame,
                playerAimDirection,
                playerAngleDegrees,
                weaponAngleDegrees,
                weaponDirection,
                player.activeWeapon.GetCurrentWeapon().weaponDetails.shootEffect);

            fireButtonPressedPreviousFrame = true;
        }
        else
        {
            fireButtonPressedPreviousFrame = false;
        }
    }

    /// <summary>
    /// Switch The Active Weapon With The Mouse Wheel
    /// </summary>
    public void SwitchWeaponInput()
    {
        Vector2 mouseScroll = playerInputActions.Player.SelectWeapon.ReadValue<Vector2>();

        if (mouseScroll.y < 0f || mouseScroll.x < -0.5f)
            SelectPreviousWeapon();
        if (mouseScroll.y > 0f || mouseScroll.x > 0.5f)
            SelectNextWeapon();

        if (playerInputActions.Player.SetWeaponAtFirst.WasPressedThisFrame())
            SetCurrentWeaponToFirstInTheList();
    }

    public void SetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponList.Count)
        {
            currentWeaponIndex = weaponIndex;
            player.activeWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex - 1]);
            
            ScreenCursor.Instance.ChangeCrosshair(player.activeWeapon.GetCurrentWeapon().weaponDetails.weaponCrosshair);
        }
    }

    public void SelectNextWeapon()
    {
        currentWeaponIndex++;

        if (currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    public void SelectPreviousWeapon() 
    {
        currentWeaponIndex--;

        if (currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    /// <summary>
    /// Set The Current Weapon To Be The First Weapon In The Weapon List
    /// </summary>
    public void SetCurrentWeaponToFirstInTheList()
    {
        List<Weapon> tempWeaponList = new List<Weapon>();

        //Add current weapon to be the first in the temporary list
        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        //Loop through the existing weapon list - skipping the current weapon
        int index = 2;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon) continue;

            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        //Assign the temporary list to the player weapon list
        player.weaponList = tempWeaponList;

        currentWeaponIndex = 1;

        SetWeaponByIndex(currentWeaponIndex);
    }

    /// <summary>
    /// Reloads The Weapon 
    /// </summary>
    public void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        //If the current weapon is already reloading then just return
        if (currentWeapon.isWeaponReloading)
            return;

        //If the remaining ammo is less than the weapon mag max capacity and we dont have infinity ammo, then just return
        if (currentWeapon.weaponTotalAmmoCapacity < currentWeapon.weaponDetails.weaponMagMaxCapacity && !currentWeapon.weaponDetails.hasInfiniteAmmo)
            return;

        //If the remaining ammo in the mag is equal to the mag max capacity, then return
        if (currentWeapon.weaponClipRemaining == currentWeapon.weaponDetails.weaponMagMaxCapacity)
            return;

        // Change the sprite when the clip is depletted
        if ((currentWeapon.weaponClipRemaining == 0 || currentWeapon.weaponTotalAmmoCapacity == 0) && currentWeapon.weaponDetails.weaponReloadSprite != null)
            weaponSpriteRenderer.sprite = currentWeapon.weaponDetails.weaponReloadSprite;

        if (playerInputActions.Player.Reload.WasPressedThisFrame())
            // Call the reload weapon event
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
    }

    /// <summary>
    /// Use the nearest item within 2 unity units from the player
    /// </summary>
    public void UseItemInput()
    {
        if (playerInputActions.Player.Interactions.IsPressed())
        {
            float useItemRadius = 2f;

            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

            foreach (Collider2D collider2D in collider2Ds)
            {
                if (collider2D.TryGetComponent<IUseable>(out var iUseable))
                    iUseable.UseItem();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If collided with something this stops the player coroutine
        StopPlayerRollCoroutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //If collided with something this stops the player coroutine
        StopPlayerRollCoroutine();
    }

    /// <summary>
    /// Stops The Player Coroutine, If Collided With Something
    /// </summary>
    private void StopPlayerRollCoroutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);

            isPlayerRolling = false;
        }
    }

    /// <summary>
    /// Enables the player to move
    /// </summary>
   public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
        playerInputActions.Player.Enable();
    }

    /// <summary>
    /// Disables the player to move
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        playerInputActions.Player.Disable();
        player.idleEvent.CallIdleEvent();
    }

    /// <summary>
    /// Enable the UI Controls
    /// </summary>
    public void EnableUIControls()
    {
        playerInputActions.UI.Enable();
    }

    /// <summary>
    /// Disable the UI Controls
    /// </summary>
    public void DisableUIControls()
    {
        playerInputActions.UI.Disable();
    }

    public CharacterInput GetPlayerInputActions()
    {
        return playerInputActions;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
