using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemDetails_", menuName = "Scriptable Objects/Shop System/Good's Details")]
public class ShopItemSO : ScriptableObject
{
    #region Tooltip
    [Tooltip("The universal prefab for the item shop")]
    #endregion
    public GameObject itemShopPrefab;
    #region Tooltip
    [Tooltip("the name of the good")]
    #endregion
    public string itemName;
    #region Tooltip
    [Tooltip("Write some unique description here")]
    #endregion
    public string itemDescription;
    #region Tooltip
    [Tooltip("the sprite of the good")]
    #endregion
    public Sprite itemSprite;
    #region Tooltip
    [Tooltip("the good's price in bitcoin denomination")]
    #endregion
    public double itemPriceBits;
    #region Tooltip
    [Tooltip("The item's price in bits for display in the Shop, it should match the price in Bitcoin. " +
        "The Bit is a unit within Bitcoin Ecosystem, the denomination in sats can be confusing for no-coiners " +
        "so instead of displaying the prices like for example so, 0.0003 sats, we multyply that by 100 and display " +
        "the prices like so ₿300. 100 sats = ₿1.00 or 1 bit. NOTE: The conversion is done by the ShopManager automatically, " +
        "so it's not necessary that this field is populated in the inspector.")]
    #endregion
    public double displayPriceBits;
    #region Tooltip
    [Tooltip("the min good's price in fiat denomination")]
    #endregion
    public float minFiatPrice;
    #region Tooltip
    [Tooltip("the max good's price in fiat denomination")]
    #endregion
    public float maxFiatPrice;
    #region Tooltip
    [Tooltip("If the item is a weapon, pupulate with the matching weaponDetailsSO")]
    #endregion
    public WeaponDetailsSO weaponDetails;
    #region Tooltip
    [Tooltip("Define the type of item this is")]
    #endregion
    public ItemShopType itemShopType;

    [HideInInspector] public bool isSelected;

    private double fiatFixedPrice;

    /// <summary>
    /// Updates the randomly selected fiat price for this item
    /// </summary>
    /// <param name="newPrice">The new updated value</param>
    public void SetFixedFiatPrice(double newPrice)
    {
        fiatFixedPrice = newPrice;
    }

    /// <summary>
    /// Retrieves the randomly selected price for the item
    /// </summary>
    /// <returns>Returns the now fixed fiat price</returns>
    public double GetFixedFiatPrice()
    {
        return fiatFixedPrice;
    }

    /// <summary>
    /// Selects a random value for the item's price in fiat
    /// </summary>
    /// <returns>Returns the random value</returns>
    public double GetRandomFiatPrice()
    {
        fiatFixedPrice = Random.Range(minFiatPrice, maxFiatPrice);
        return fiatFixedPrice;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(itemName), itemName);

        HelperUtilities.ValidateCheckNullValue(this, nameof(itemSprite), itemSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemShopPrefab), itemShopPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(itemPriceBits), itemPriceBits, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(minFiatPrice), minFiatPrice, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minFiatPrice), minFiatPrice, nameof(maxFiatPrice), maxFiatPrice, false);

        if (itemShopType == ItemShopType.isWeapon)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(weaponDetails), weaponDetails);
        }
    }
#endif
    #endregion
}
