using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;

public class MainMenuUI : SingletonMonoBehaviour<MainMenuUI>
{
    #region Header Buttons
    [Header("UI Buttons")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject returnButton;
    [SerializeField] private GameObject highScoresButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject instructionsButton;
    [SerializeField] private GameObject quitConfirmationObject;
    [SerializeField] private GameObject settingsButton; 
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject loadSaveSlotsButton;
    [SerializeField] private GameObject saveSlotsMenuGameObject;
    #endregion

    [Space(10)]

    [SerializeField] private SaveSlotsMenu saveSlotsMenu;
    [SerializeField] private ConfirmationPopupMenu ConfirmationPopupMenu;

    private bool isHighScoreSceneLoaded = false;
    private bool isInstructionsSceneLoaded = false;
    private bool isSettingsSceneLoaded = false;
    private bool isCreditsSceneLoaded = false;
    private CharacterInput inputActions;

    protected override void Awake()
    {
        base.Awake();

        inputActions = new();
        inputActions.Player.Disable();
        inputActions.UI.Enable();
        InputSystem.EnableDevice(Mouse.current);

        StartCoroutine(SelectFirstButton(playButton));
    }

    private void Start()
    {
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);
        SceneManager.LoadScene("CharacterSelector", LoadSceneMode.Additive);

        returnButton.SetActive(false);
        quitConfirmationObject.SetActive(false);
        saveSlotsMenuGameObject.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("MainGame");
        inputActions.UI.Disable();
        inputActions.Player.Enable();
    }

    /// <summary>
    /// Loads the high scores when the button is pressed
    /// </summary>
    public void LoadHighScores()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);
        isHighScoreSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelector");
        SceneManager.LoadScene("HighScore", LoadSceneMode.Additive);

        returnButton.SetActive(true);
        StartCoroutine(SelectFirstButton(returnButton));
    }

    /// <summary>
    /// Loads back the character selector scene when a button is pressed
    /// </summary>
    public void LoadCharacterSelectorScene()
    {
        returnButton.SetActive(false);

        if (isHighScoreSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("HighScore");
            isHighScoreSceneLoaded = false;
        }
        else if (isInstructionsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("Instructions");
            isInstructionsSceneLoaded = false;
        }
        else if (isSettingsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("Settings");
            isSettingsSceneLoaded = false;
        }
        else if (isCreditsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("Credits");
            isCreditsSceneLoaded = false;
        }

        SceneManager.LoadScene("CharacterSelector", LoadSceneMode.Additive);

        playButton.SetActive(true);
        quitButton.SetActive(true);
        instructionsButton.SetActive(true);
        highScoresButton.SetActive(true);
        settingsButton.SetActive(true);
        creditsButton.SetActive(true);
        loadSaveSlotsButton.SetActive(true);
        saveSlotsMenuGameObject.SetActive(false);

        StartCoroutine(SelectFirstButton(playButton));
    }

    public void LoadSettings()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        instructionsButton.SetActive(false);
        highScoresButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);
        isSettingsSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelector");
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    public void LoadCredits()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        instructionsButton.SetActive(false);
        highScoresButton.SetActive(false);
        settingsButton.SetActive(false);
        returnButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);
        creditsButton.SetActive(false);
        isCreditsSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelector");
        SceneManager.LoadScene("Credits", LoadSceneMode.Additive);

        returnButton.SetActive(true);
        StartCoroutine(SelectFirstButton(returnButton));
    }

    public void ShowQuitConfirmation()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);

        SceneManager.UnloadSceneAsync("CharacterSelector");

        MusicManager.Instance.PlayMusic(GameResources.Instance.quitMenuMusic, 1f, 1f);

        ConfirmationPopupMenu.ActivateMenu("You sure wanna quit?",
            () =>
            //Function to execute when selecting 'confirm'
            {
                QuitGame();
            },
            () =>
            //Function to execute when selecting 'cancel'
            {
                ReturnMainMenu();
            });
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("CharacterSelector", LoadSceneMode.Additive);

        playButton.SetActive(true);
        quitButton.SetActive(true);
        highScoresButton.SetActive(true);
        instructionsButton.SetActive(true);
        settingsButton.SetActive(true);
        creditsButton.SetActive(true);
        loadSaveSlotsButton.SetActive(true);

        quitConfirmationObject.SetActive(false);

        StartCoroutine(SelectFirstButton(playButton));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadInstructions()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);
        isInstructionsSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelector");
        SceneManager.LoadScene("Instructions", LoadSceneMode.Additive);

        returnButton.SetActive(true);
        StartCoroutine(SelectFirstButton(returnButton));
    }

    public void NewGameSaveSlots()
    {
        saveSlotsMenu.ActivateMenu(false);
        saveSlotsMenuGameObject.SetActive(true);

        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);

        SceneManager.UnloadSceneAsync("CharacterSelector");

        returnButton.SetActive(true);
        StartCoroutine(SelectFirstButton(returnButton));
    }

    public void LoadSaveSlots()
    {
        saveSlotsMenu.ActivateMenu(true);
        saveSlotsMenuGameObject.SetActive(true);

        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false);
        loadSaveSlotsButton.SetActive(false);

        SceneManager.UnloadSceneAsync("CharacterSelector");

        returnButton.SetActive(true);
        StartCoroutine(SelectFirstButton(returnButton));
    }

    public CharacterInput GetCharacterInput()
    {
        return inputActions;
    }

    private IEnumerator SelectFirstButton(GameObject buttonToSelect)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(returnButton), returnButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(highScoresButton), highScoresButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(instructionsButton), instructionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(settingsButton), settingsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(creditsButton), creditsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(loadSaveSlotsButton), loadSaveSlotsButton);
    }
#endif
    #endregion
}
