using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsUI : MonoBehaviour
{
    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ReturnToPreviousScene()
    {
        GameManager.Instance.parentCanvas.SetActive(true);

        SceneManager.UnloadSceneAsync("Test");
    }
}
