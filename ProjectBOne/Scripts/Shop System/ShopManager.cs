using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopManager : SingletonMonoBehaviour<ShopManager>
{
    public bool IsShopOpen { get; private set; }

    #region Header
    [Header("Shop UI")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The panel gameobject of the shop")]
    #endregion
    [SerializeField] private GameObject shopPanel;
    #region Tooltip
    [Tooltip("The anchor where all item objects will spawn")]
    #endregion
    [SerializeField] private Transform spawnAnchor;
    #region Tooltip
    [Tooltip("The button that the event system will select at first")]
    #endregion
    [SerializeField] private GameObject firstSelectedButton;
    #region Tooltip
    [Tooltip("The button to make a purchase in Bitcoin")]
    #endregion
    [SerializeField] private GameObject bitcoinButton;
    #region Tooltip
    [Tooltip("The button to make a purchase in fiat")]
    #endregion
    [SerializeField] private GameObject fiatButton;

    [SerializeField] private TextMeshProUGUI messageHint;

    private Player player;
    private GameObject shopItemPrefab;
    private GameObject instantiatedShopPrefab;
    private List<ShopItemSO> shopItemListSO = new();
    private ShopItem instantiatedShopItem;
    private List<GameObject> spawnedItemsList = new();
    private Coroutine coroutine;
    private GameObject minimapUI;

    private int selectedItemIndex = 0;
    private const float offset = 450f;
    /// <summary>
    /// This is the amount of fiat the player has, it's updated every time the shop is open
    /// </summary>
    private double fiatAmount;
    /// <summary>
    /// This is the fixed fiat price for the shop items, because the prices are set at random, 
    /// there was a problem where the internal item price was different from the price showed in the shop. 
    /// </summary>
    private double fixedFiatPrice = 0; // This ensures the price and what is shown in the UI are the same and not diferent values
    /// <summary>
    /// This is the bitcoin the player currently has, it gets pulled from the GameManager
    /// and it's updated every time the shop is open.
    /// </summary>
    private double bitcoinAmount;

    protected override void Awake()
    {
        base.Awake();

        player = GameManager.Instance.GetPlayer();

        // Load the resources for the shop
        shopItemListSO = GameResources.Instance.shopItemsList;
        shopItemPrefab = GameResources.Instance.shopItemPrefab;

        minimapUI = DungeonMap.Instance.MinimapUI;
    }

    private void Start()
    {
        shopPanel.SetActive(false);

        for (int i = 0; i < shopItemListSO.Count; i++)
        {
            instantiatedShopPrefab = Instantiate(shopItemPrefab, spawnAnchor);
            instantiatedShopItem = instantiatedShopPrefab.GetComponent<ShopItem>();
            spawnedItemsList.Add(instantiatedShopPrefab);
            instantiatedShopPrefab.transform.localPosition = new Vector3((offset * i), 0f, 0f);
            instantiatedShopPrefab.SetActive(true);

            LoadShopItems(instantiatedShopItem, shopItemListSO[i]);
        }
    }

    /// <summary>
    /// Enter the shop
    /// </summary>
    public void EnterShop()
    {
        IsShopOpen = true;
        //ShopCamera.Instance.DiplayShop();
        shopPanel.SetActive(true);
        minimapUI.SetActive(false);
        player.playerControl.DisablePlayer();
        player.playerControl.EnableUIControls();

        // Get the amount of currency hold by the player
        bitcoinAmount = GameManager.Instance.GetSatsOnHold();
        fiatAmount = GameManager.Instance.GetFiatOnHold();

        bitcoinButton.SetActive(true);
        fiatButton.SetActive(true);

        // Select the button on the UI
        StartCoroutine(SelectFirstButtonUI());
    }

    /// <summary>
    /// Exit the shop mode - used in a button in the UI
    /// </summary>
    public void ExitShop()
    {
        IsShopOpen = false;
        shopPanel.SetActive(false);
        minimapUI.SetActive(true);
        player.playerControl.EnablePlayer();
        player.playerControl.DisableUIControls();
    }

    /// <summary>
    /// Loads all the ShopItemSO's and shows them in the UI
    /// </summary>
    /// <param name="shopItem"></param>
    /// <param name="shopItemSO"></param>
    private void LoadShopItems(ShopItem shopItem, ShopItemSO shopItemSO)
    {
        // Set the prices of the items
        shopItem.itemPriceBits = shopItemSO.itemPriceBits;
        fixedFiatPrice = shopItemSO.GetRandomFiatPrice();

        // Set the fiat price to a fixed value
        shopItemSO.SetFixedFiatPrice(fixedFiatPrice);
        shopItem.itemPriceFiat = fixedFiatPrice;

        // Display information about the item in the UI
        shopItem.itemImage.sprite = shopItemSO.itemSprite;
        shopItem.itemPriceBitsText.text = shopItemSO.itemPriceBits.ToString();
        shopItem.itemPriceFiatText.text = fixedFiatPrice.ToString("#0");
        shopItem.itemNameUI.text = shopItemSO.itemName.ToString();
        shopItem.itemDescriptionUI.text = shopItemSO.itemDescription.ToString();

        // Define the type of good
        shopItem.itemShopType = shopItemSO.itemShopType;
    }

    /// <summary>
    /// This makes the text on the buttons change depending of the item type
    /// </summary>
    /// <param name="itemShopType"></param>
    private void ChangeTextOnItemType(ItemShopType itemShopType)
    {
        switch (itemShopType)
        {
            case ItemShopType.isWeapon:
                // Set the buttons correctly
                bitcoinButton.SetActive(true);
                bitcoinButton.GetComponent<Button>().interactable = true;

                fiatButton.SetActive(true);
                fiatButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy With\n Fiat";
                break;
            case ItemShopType.isAmmo:
                // Set the buttons correctly
                bitcoinButton.SetActive(true);
                bitcoinButton.GetComponent<Button>().interactable = true;


                fiatButton.SetActive(true);
                fiatButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy With\n Fiat";
                break;
            case ItemShopType.isHealth:
                // Set the buttons correctly
                bitcoinButton.SetActive(true);
                bitcoinButton.GetComponent<Button>().interactable = true;


                fiatButton.SetActive(true);
                fiatButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy With\n Fiat";
                break;
            case ItemShopType.isFiatExchange:  
                // Deactivate the bitcoin button
                bitcoinButton.SetActive(true);
                bitcoinButton.GetComponent<Button>().interactable = false;
                
                // Activate the fiat button
                fiatButton.SetActive(true);
                fiatButton.GetComponentInChildren<TextMeshProUGUI>().text = "Exchange Fiat";
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Makes the exchange of goods in Bitcoin denomination
    /// </summary>
    public void PurchaseInBitcoin()
    {
        if (bitcoinAmount > shopItemListSO[selectedItemIndex].itemPriceBits)
        {
            // If the item is the Exchange fiat for Bitcoin, return
            if (shopItemListSO[selectedItemIndex].itemShopType == ItemShopType.isFiatExchange)
                return;

            // Proceed only if the exchange was completed
            if (!CompletePurchase(shopItemListSO, selectedItemIndex))
                return;

            bitcoinAmount -= shopItemListSO[selectedItemIndex].itemPriceBits;

            GameManager.Instance.SetBitcoinOnHold(bitcoinAmount);
            StaticEventHandler.CallMoneyChangedEvent(bitcoinAmount, true);
        }
        else
        {
            StartCoroutine(DisplayMessage($"Not enough Bitcoin To Buy {shopItemListSO[selectedItemIndex].itemName}. Amount Needed: {shopItemListSO[selectedItemIndex].itemPriceBits} BTC", 3f));
        }
    }

    /// <summary>
    /// Makes the exchange of goods in Fiat denomination
    /// </summary>
    public void PurchaseInFiat()
    {
        if (fiatAmount > shopItemListSO[selectedItemIndex].GetFixedFiatPrice())
        {
            // Proceed only if the exchange was completed
            if (!CompletePurchase(shopItemListSO, selectedItemIndex))
                return;

            fiatAmount -= shopItemListSO[selectedItemIndex].GetFixedFiatPrice();
            GameManager.Instance.SetFiatOnHold(fiatAmount);
            StaticEventHandler.CallMoneyChangedEvent(fiatAmount, false);
        }
        else
        {
            StartCoroutine(DisplayMessage($"Not enough Fiat For {shopItemListSO[selectedItemIndex].itemName}. " +
                $"Amount Needed: {shopItemListSO[selectedItemIndex].GetFixedFiatPrice():#000} Fiat", 3f));
        }
    }

    /// <summary>
    /// Completes the purchase whether in fiat or Bitcoin
    /// </summary>
    /// <param name="shopItemList"></param>
    /// <param name="index"></param>
    /// <returns>Return true if the exchange was completed, false otherwise</returns>
    private bool CompletePurchase(List<ShopItemSO> shopItemList, int index)
    {
        #region Complete Purchase
        switch (shopItemList[index].itemShopType)
        {
            case ItemShopType.isWeapon:
                // Add the weapon to the player
                if (player.IsWeaponHeldByPlayer(shopItemList[index].weaponDetails))
                {
                    // Display a message to the player to let him now, he already has that weapon and it's
                    // not necessary to buy it again
                    StartCoroutine(DisplayMessage("You already have this weapon!", 2.5f));
                    return false;
                }
                else
                {
                    GameManager.Instance.GetPlayer().AddWeaponToPlayer(shopItemList[index].weaponDetails);
                    SoundManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickupSound);
                    return true;
                }

            case ItemShopType.isAmmo:
                // Add the ammo to the current weapon
                player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 100);
                SoundManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickupSound);
                return true;

            case ItemShopType.isHealth:
                // Add the health to the player
                if (player.playerHealth.CurrentHealth == player.playerHealth.StartingHealth)
                {
                    StartCoroutine(DisplayMessage("Max Health - Cannot add more", 3f));
                    return false;
                }
                else
                {
                    player.playerHealth.AddHealth(100);
                    SoundManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickupSound);
                    return true;
                }
                
            case ItemShopType.isFiatExchange:
                // Add the Bitcoin to the player
                bitcoinAmount += shopItemList[selectedItemIndex].itemPriceBits;
                GameManager.Instance.SetBitcoinOnHold(bitcoinAmount);

                // Update the money UI
                StaticEventHandler.CallMoneyChangedEvent(bitcoinAmount, true);
                return true;

            default:
                // If the item is neither of them, just don't complete the exchange
                return false;
        }
        #endregion
    }

    /// <summary>
    /// Selects the next item in the shop - used in a button in the UI
    /// </summary>
    public void SelectNextItem()
    {
        if (selectedItemIndex >= shopItemListSO.Count - 1)
            return;

        selectedItemIndex++;

        MoveToSelectedItem(selectedItemIndex);
    }

    /// <summary>
    /// Selects the previous item in the shop - used in a button in the UI
    /// </summary>
    public void SelectPreviousItem()
    {
        if (selectedItemIndex == 0)
            return;

        selectedItemIndex--;

        MoveToSelectedItem(selectedItemIndex);
    }

    /// <summary>
    /// Moves to the selected item 
    /// </summary>
    /// <param name="index"></param>
    private void MoveToSelectedItem(int index)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(SelectItemRoutine(index));

        ChangeTextOnItemType(shopItemListSO[selectedItemIndex].itemShopType);
    }

    /// <summary>
    /// Makes the items move in the UI
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private IEnumerator SelectItemRoutine(int index)
    {
        float currentLocalXPosition = spawnAnchor.localPosition.x;
        float targetLocalXPosition = index * offset * spawnAnchor.localScale.x * -1f;

        while (Mathf.Abs(currentLocalXPosition - targetLocalXPosition) > 0.01f)
        {
            currentLocalXPosition = Mathf.Lerp(currentLocalXPosition, targetLocalXPosition, Time.deltaTime * 10);

            spawnAnchor.localPosition = new Vector3(targetLocalXPosition, spawnAnchor.localPosition.y, 0f);
            yield return null;
        }

        spawnAnchor.localPosition = new Vector3(targetLocalXPosition, spawnAnchor.localPosition.y, 0f);
    }

    private IEnumerator DisplayMessage(string messageToDisplay, float timeToDisplay)
    {
        messageHint.text = messageToDisplay;

        yield return new WaitForSeconds(timeToDisplay);

        messageHint.text = "Welcome!";
    }

    private IEnumerator SelectFirstButtonUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
