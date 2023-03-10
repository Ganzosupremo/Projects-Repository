using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SnakeDetails_", menuName = "Scriptable Objects/Player/Snake Details")]
public class SnakeDetailsSO : ScriptableObject
{
    #region Header Player Base Details
    [Space(10)]
    [Header("The Basic Details Of The Player")]
    #endregion

    #region Tooltip
    [Tooltip("The name for this snake")]
    #endregion
    public string snakeName;

    #region Tooltip
    [Tooltip("The prefab for this snake")]
    #endregion
    public GameObject snakePrefab;

    #region Header Health System
    [Space(10)]
    [Header("Health System")]
    #endregion
    #region Tooltip
    [Tooltip("The starting health of the player")]
    #endregion
    public int snakeInitialHealth;

    #region Tooltip
    [Tooltip("Select if the player will be invencible after hit - select the time in the next field")]
    #endregion
    public bool isImmuneAfterHit = false;

    #region Tooltip
    [Tooltip("Choose how many seconds the player will be invencible for")]
    #endregion
    public float immunityTime;

    #region Header Weapon
    [Space(10)]
    [Header("The Player Weapon")]
    #endregion
    #region Tooltip
    [Tooltip("The initial weapon the player will have")]
    #endregion
    public WeaponDetailsSO initialWeapon;

    #region Tooltip
    [Tooltip("The initial weapon list for different characters, this sets the initialWeapon variable above")]
    #endregion
    public List<WeaponDetailsSO> initialWeaponList;

    #region Header Other
    [Space(10)]
    [Header("Other Things")]
    #endregion

    #region Tooltip
    [Tooltip("Player icon sprite to be used in the minimap")]
    #endregion
    public Sprite snakeMinimapIcon;



    #region Header Enemy Materialize Settings
    [Space(10)]
    [Header("Materialize Player Settings")]
    #endregion

    #region Tooltip
    [Tooltip("This is the default lit material, populate it, because after the materialize effect" +
        "is finished the player material will be reset to this one.")]
    #endregion
    public Material defaultLitMaterial;

    #region Tooltip
    [Tooltip("The it will take for the player to materialize")]
    #endregion
    public float materializeTime;

    #region Tooltip
    [Tooltip("The shader that'll be used when the player materializes")]
    #endregion
    public Shader materializeShader;

    #region Tooltip
    [Tooltip("The color of the materialize effect - it's and HDR, so the intensity can make the color glow")]
    #endregion
    [ColorUsage(true, true)]
    public Color materializeColor;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(snakeName), snakeName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(snakePrefab), snakePrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(snakeInitialHealth), snakeInitialHealth, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(snakeMinimapIcon), snakeMinimapIcon);

        HelperUtilities.ValidateCheckNullValue(this, nameof(initialWeapon), initialWeapon);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(initialWeaponList), initialWeaponList);

        HelperUtilities.ValidateCheckNullValue(this, nameof(materializeShader), materializeShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(defaultLitMaterial), defaultLitMaterial);

        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(immunityTime), immunityTime, false);
        }
    }
#endif
    #endregion
}
