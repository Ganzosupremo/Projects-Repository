using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTrigger : MonoBehaviour, IUseable
{
    #region Tooltip
    [Header("Visual Cue")]
    [Tooltip("The Visual Cue of the NPC")]
    #endregion
    [SerializeField] private GameObject visualCue;
    #region Tooltip
    [Header("InkJSON")]
    [Tooltip("The InkJSON file for the dialogs")]
    #endregion
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;

    private void Start()
    {
        visualCue.SetActive(false);
        playerInRange = false;
    }

    private void Update()
    {
        HideVisualClue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Settings.playerTag)) 
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Settings.playerTag))
        {
            playerInRange = false;
        }
    }

    private void HideVisualClue()
    {
        if (playerInRange)
        {
            visualCue.SetActive(true);
        }
        else
        {
            visualCue.SetActive(false);
        }
    }

    public void UseItem()
    {
        if (playerInRange && !ShopManager.Instance.IsShopOpen)
        {
            ShopManager.Instance.EnterShop();
        }
    }
}
