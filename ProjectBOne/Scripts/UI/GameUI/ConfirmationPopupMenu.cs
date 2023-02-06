using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

public class ConfirmationPopupMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelectedButton;

    #region Header
    [Header("Components")]
    #endregion
    [SerializeField] private TextMeshProUGUI confirmationText;

    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private void Start()
    {
        StartCoroutine(SelectFirstButton());
    }

    public void ActivateMenu(string displayText, UnityAction confirmAction, UnityAction cancelAction)
    {
        this.gameObject.SetActive(true);

        this.confirmationText.text = displayText;

        //Remove listener to make sure there are no previous listeners
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        //Assign the onClick listeners
        confirmButton.onClick.AddListener(() =>
        {
            DeactivateMenu();
            confirmAction();
        });

        cancelButton.onClick.AddListener(() =>
        {
            DeactivateMenu();
            cancelAction();
        });
    }

    private void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }

    private IEnumerator SelectFirstButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
