using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    #region Header Profile ID
    [Header("Profile ID")]
    #endregion
    [SerializeField] private string profileID = "";

    #region Header Content
    [Header("Content")]
    #endregion
    [SerializeField] private GameObject noContentObject;
    [SerializeField] private GameObject hasContentObject;

    [SerializeField] private TextMeshProUGUI numberOfSatsOnHold;
    [SerializeField] private TextMeshProUGUI currentLevel;

    private Button saveSlotButton;

    public bool HasData { get; private set; } = false;

    private void Awake()
    {
        saveSlotButton = this.GetComponent<Button>();
    }

    public void SetData(GameData data)
    {
        //If there's no data
        if (data == null)
        {
            HasData = false;
            noContentObject.SetActive(true);
            hasContentObject.SetActive(false);
        }
        //When there's data
        else 
        {
            HasData = true;
            noContentObject.SetActive(false);
            hasContentObject.SetActive(true);

            numberOfSatsOnHold.text = data.GetSatsOnHold().ToString("#0 Bits");
            currentLevel.text = (data.currentDungeonIndex + 1).ToString();
        }
    }

    /// <summary>
    /// Gets the ID of a profile
    /// </summary>
    /// <returns>Return the profile as a string</returns>
    public string GetProfileID()
    {
        return this.profileID;
    }

    public void SetInteractable(bool interactable)
    {
        saveSlotButton.interactable = interactable;
    }
}
