using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class DisplayCreditsUI : MonoBehaviour
{
    [SerializeField] private Transform creditsAnchor;
    //private PlayerInput inputActions;
    private List<GameObject> creditPrefabList = new();
    private List<CreditSO> creditSOList = new();
    private int creditsNumber;

    private void Start()
    {
        SetCreditsText();
    }

    private void SetCreditsText()
    {
        creditsNumber = GameResources.Instance.creditSOs.Length;
        GameObject creditsGameObject;
        CreditSO[] creditSOs = GameResources.Instance.creditSOs;
        for (int index = 0; index < creditsNumber; index++)
        {
            creditsGameObject = Instantiate(GameResources.Instance.creditsPrefab, creditsAnchor);

            CreditsPrefab creditsPrefab = creditsGameObject.GetComponent<CreditsPrefab>();

            creditPrefabList.Insert(index, creditsGameObject);

            //Populate the two fields in the credit prefab
            creditsPrefab.nameText.text = GameResources.Instance.creditSOs[index].creditName;
            creditsPrefab.descriptionText.text = GameResources.Instance.creditSOs[index].creditDescription;
        }

        //foreach (CreditSO credits in creditSOs)
        //{
        //    rank++;
        //    creditsGameObject = Instantiate(GameResources.Instance.creditsPrefab, creditsAnchor);

        //    CreditsPrefab creditsPrefab = creditsGameObject.GetComponent<CreditsPrefab>();

        //    creditPrefabList.Insert(rank - 1, creditsGameObject);

        //    //Populate the two fields in the credit prefab
        //    creditsPrefab.nameText.text = GameResources.Instance.creditSOs[rank].creditName;
        //    creditsPrefab.descriptionText.text = GameResources.Instance.creditSOs[rank].creditDescription;
        //}

        //Add a blank line
        creditsGameObject = Instantiate(GameResources.Instance.creditsPrefab, creditsAnchor);
    }
}
