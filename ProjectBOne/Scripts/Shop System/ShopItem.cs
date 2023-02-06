using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    // This will appear in the UI
    public Image itemImage;
    public TextMeshProUGUI itemNameUI;
    public TextMeshProUGUI itemDescriptionUI;

    public TextMeshProUGUI itemPriceFiatText;
    public TextMeshProUGUI itemPriceBitsText;
    
    // This are used to know the price of the item internally
    [HideInInspector] public double itemPriceBits;
    [HideInInspector] public double itemPriceFiat;
    [HideInInspector] public ItemShopType itemShopType;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemImage), itemImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemNameUI), itemNameUI);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemDescriptionUI), itemDescriptionUI);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemPriceFiatText), itemPriceFiatText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemPriceBitsText), itemPriceBitsText);
    }
#endif
    #endregion 
}
