using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCamera : SingletonMonoBehaviour<ShopCamera>
{
    private Camera mainCamera;
    [SerializeField] private Camera shopCamera;
    private GameObject minimapUI;

    private void Start()
    {
        mainCamera = Camera.main;

        shopCamera.gameObject.SetActive(false);

        minimapUI = DungeonMap.Instance.MinimapUI;
    }

    public void DiplayShop()
    {
        mainCamera.gameObject.SetActive(false);
        shopCamera.gameObject.SetActive(true);

        minimapUI.SetActive(false);
    }

    public void ClearShop()
    {
        mainCamera.gameObject.SetActive(true);
        shopCamera.gameObject.SetActive(false);

        minimapUI.SetActive(true);
    }
}
