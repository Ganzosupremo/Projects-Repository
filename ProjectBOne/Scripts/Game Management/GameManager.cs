using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class GameManager : SingletonMonoBehaviour<GameManager>, IPersistenceData
{
    #region Header Object References
    [Space(10)]
    [Header("Object References")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the Pause Menu UI gameobject")]
    #endregion
    [SerializeField] private GameObject pauseMenuUI;

    #region Tooltip
    [Tooltip("Just to disable the main canvas in scenes when needed")]
    #endregion
    public GameObject parentCanvas;

    #region Tooltip
    [Tooltip("Populate with the text component in the FadeScreen UI")]
    #endregion
    [SerializeField] private TextMeshProUGUI messageText;

    #region Tooltip
    [Tooltip("Populate with the canvas group component in the FadeScreen UI")]
    #endregion
    [SerializeField] private CanvasGroup canvasGroup;

    #region Header Dungeon Levels
    [Space(10)]
    [Header("The Levels Of The Dungeon")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the Dungeon Level SO's")]
    #endregion
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing, first dungeon level = 0")]
    #endregion
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;
    private InstantiatedRoom bossRoom;
    private CharacterInput inputActions;
    private StopWatch stopWatch;

    private PlayerDetailsSO playerDetails;
    private Player player;
    private long gameScore;
    private int scoreMultiplier;
    private bool isScreenFading = false;
    private double fiatCurrency = 0;
    private double bitcoinCurrency = 0;

    [HideInInspector] public GameState currentGameState;
    [HideInInspector] public GameState previousGameState;

    protected override void Awake()
    {
        base.Awake();

        //Set the player details - saved in current player SO from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        InstantiatePlayer();

        inputActions = GetPlayer().playerControl.GetPlayerInputActions();

        // Start a timer
        stopWatch = new();
        stopWatch.StartTimer();
    }

    /// <summary>
    /// Creates The Player In The Scene At Position
    /// </summary>
    private void InstantiatePlayer()
    {
        //Instantiate The Player
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        //Initialize The Player
        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);

        GetPlayer().playerControl.EnablePlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        previousGameState = GameState.gameStarted;
        currentGameState = GameState.gameStarted;

        //PersistenceDataManager.Instance.LoadGame();

        gameScore = 0;
        scoreMultiplier = 1;
        fiatCurrency = 0;

        //Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    // Update is called once per frame
    void Update()
    {
        HandleGameState();
        stopWatch.UpdateTimer();
    }

    private void OnEnable()
    {
        //Subscribe to the room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        //Subscribe to the room change event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        //Subscribe to the points scored event
        StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;

        //Subscribe to the score multiplier event
        StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;

        //Subsribe to the money event
        StaticEventHandler.OnMoneyEvent += StaticEventHandler_OnMoneyPicked;

        //Suscribe to the player destroyed event
        player.destroyEvent.OnDestroy += Player_OnDestroy;

        //PersistenceDataManager.Instance.LoadGame();
    }

    private void OnDisable()
    {
        //Unsubscribe to the room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        //Unsubscribe to the room change event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        //Unsuscribe to the points scored event
        StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;

        //Unsubscribe to the score multiplier event
        StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        //Unsubscribe from the money event
        StaticEventHandler.OnMoneyEvent -= StaticEventHandler_OnMoneyPicked;

        //Unsubscribe to the player destroyed event
        player.destroyEvent.OnDestroy -= Player_OnDestroy;

        //PersistenceDataManager.Instance.SaveGame();
    }

    /// <summary>
    /// Handle the scored points event
    /// </summary>
    private void StaticEventHandler_OnPointsScored(PointsScoredArgs pointsScoredArgs)
    {
        gameScore += pointsScoredArgs.score * scoreMultiplier;

        //Call the score changed event
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }
    
    /// <summary>
    /// Handles the room enemies defeated event
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    /// <summary>
    /// Handle the score multiplier event
    /// </summary>
    private void StaticEventHandler_OnMultiplier(MultiplierArgs multiplierArgs)
    {
        if (multiplierArgs.multiplier)
        {
            scoreMultiplier++;
        }
        else
        {
            scoreMultiplier--;
        }

        //Cap the multiplier of a maximum of 100
        scoreMultiplier = Mathf.Clamp(scoreMultiplier, 1, 100);

        //Call the score changed event to display correctly the score on the UI
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void StaticEventHandler_OnMoneyPicked(MoneyEventArgs moneyEventArgs)
    {
        //Add the value depending on the type of money collected
        if (moneyEventArgs.isBitcoin)
        {
            //Call the money changed event and pass the corresponding type of money
            bitcoinCurrency += moneyEventArgs.value;
            StaticEventHandler.CallMoneyChangedEvent(bitcoinCurrency, true);
            //gameData.satsOnHold += bitcoinValue;
            PersistenceDataManager.Instance.SaveGame();
        }
        else
        {
            //Call the money changed event and pass the corresponding type of money
            fiatCurrency += moneyEventArgs.value;
            StaticEventHandler.CallMoneyChangedEvent(fiatCurrency, false);
        }
    }

    /// <summary>
    /// Handles the destroy event
    /// </summary>
    private void Player_OnDestroy(DestroyEvent destroyEvent, DestroyEventArgs destroyEventArgs)
    {
        previousGameState = currentGameState;
        currentGameState = GameState.gameLost;
    }

    /// <summary>
    /// Handle The Room Changed Event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangeEventArgs)
    {
        SetCurrentRoom(roomChangeEventArgs.room);
    }

    /// <summary>
    /// Handles The Different States Of The Game
    /// </summary>
    private void HandleGameState()
    {
        bool displayOverviewMap = inputActions.Player.DisplayOverviewMap.WasPressedThisFrame();
        bool pauseButtonPressed = inputActions.Player.PauseGame.IsPressed();

        #region Game Management - NOTE: It's a long chunk of code
        switch (currentGameState)
        {
            case GameState.gameStarted:

                PlayDungeonLevel(currentDungeonLevelListIndex);

                currentGameState = GameState.playingLevel;

                //Because we start in a room with no enemies, and just in case there's a level with just a boss room
                RoomEnemiesDefeated();

                break;

            case GameState.playingLevel:
                
                //Pause the game
                if (pauseButtonPressed && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    PauseGameMenu();
                }
                
                //Display the overview map while the game state is playing the level
                if (inputActions.Player.DisplayOverviewMap.WasPressedThisFrame() && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            case GameState.killingEnemies:

                if (pauseButtonPressed && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    PauseGameMenu();
                }

                break;

            case GameState.BossStage:

                //PersistenceDataManager.Instance.SaveGame();

                //Pause the game
                if (pauseButtonPressed && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    PauseGameMenu();
                }

                //Allow the player to activate the dungeon overview map while in this stage
                if (inputActions.Player.DisplayOverviewMap.WasPressedThisFrame() && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying )
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            case GameState.killingBoss:

                //Pause the game
                if (pauseButtonPressed && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    PauseGameMenu();
                }

                break;

            case GameState.levelCompleted:

                StartCoroutine(LevelCompleted());

                break;

            case GameState.gameWon:

                //Just call this only once, we test the previous game state to do so
                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());

                break;

            case GameState.gameLost:

                //Just call this only once, we test the previous game state to do so
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); //Prevent messages if you clear the level and get killed at the same time
                    StartCoroutine(GameLost());
                }

                break;

            case GameState.gamePaused:

                //If the game is already paused, pressing the esc again will unpause it
                if (pauseButtonPressed && !ShopManager.Instance.IsShopOpen && !DialogueManager.Instance.IsDialoguePlaying)
                {
                    PauseGameMenu();
                }

                break;

            case GameState.dungeonOverviewMap:

                GetPlayer().playerControl.GetPlayerInputActions().OverviewMap.Disable();
                GetPlayer().playerControl.GetPlayerInputActions().Player.Enable();

                //Switches from the dungeon overview map back to the normal view
                if (inputActions.Player.DisplayOverviewMap.WasReleasedThisFrame())
                {
                    DungeonMap.Instance.ClearDungeonOverviewMap();
                }

                break;

            case GameState.gameRestarted:

                //Restart the game
                RestartGame();

                break;
        }
        #endregion
    }

    /// <summary>
    /// Set The Current Room The Player Is In
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    /// <summary>
    /// Test if all rooms have been cleared of enemies, if so, load the next dungeon level
    /// </summary>
    private void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfNormalEnemies = true;
        bossRoom = null;

        //See if all rooms have been cleared of enemies
        foreach (KeyValuePair<string,Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            //Skip the boos room for the moment
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            //Check if the other rooms have been cleared of enemies
            if (!keyValuePair.Value.isClearOfEnemies)
            {
                isDungeonClearOfNormalEnemies = false;
                break;
            }
        }

        // Set the game state
        // If the dungeon level has been cleared completely - all the normal rooms have been cleared except the boss room,
        // Or if there's no boss room
        // Or all rooms and the boss room have been cleared
        if ((isDungeonClearOfNormalEnemies && bossRoom == null) || (isDungeonClearOfNormalEnemies && bossRoom.room.isClearOfEnemies))
        {
            //If there are more dungeon level, then 
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                currentGameState = GameState.levelCompleted;
            }
            else
            {
                currentGameState = GameState.gameWon;
            }
        }
        //If just the boss room is not cleared yet
        else if (isDungeonClearOfNormalEnemies)
        {
            currentGameState = GameState.BossStage;

            StartCoroutine(BossStage());
        }
    }

    /// <summary>
    /// Allow to pause the game - can also be called outside the GameManager Class
    /// </summary>
    public void PauseGameMenu()
    {
        if (currentGameState != GameState.gamePaused)
        {
            pauseMenuUI.SetActive(true);
            GetPlayer().playerControl.DisablePlayer();

            //Set the game states
            previousGameState = currentGameState;
            currentGameState = GameState.gamePaused;
        }
        else if (currentGameState == GameState.gamePaused)
        {
            pauseMenuUI.SetActive(false);
            GetPlayer().playerControl.EnablePlayer();

            //Restore the game states
            currentGameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
    }

    /// <summary>
    /// Displays the dungeon overview map view
    /// </summary>
    private void DisplayDungeonOverviewMap()
    {
        if (isScreenFading) return;

        inputActions.Player.Disable();
        //inputActions.OverviewMap.Enable();

        DungeonMap.Instance.DisplayDungeonOverviewMap();
    }

    public void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuiltSuccesfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSuccesfully)
        {
            //Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
            throw new Exception("Couldn't build dungeon from specified rooms and node graphs");
        }

        //Call the static event handler, this tells  that the room has changed
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Reset the fiat currency, maybe I'll implement that later or maybe not
        //fiatCurrency = 0;
        //StaticEventHandler.CallMoneyChangedEvent(fiatCurrency, false);


        //Set the player position in the scene, in mid-room roughly
        player.gameObject.transform.position = new Vector3((currentRoom.worldLowerBounds.x + currentRoom.worldUpperBounds.x) / 2f,
            (currentRoom.worldLowerBounds.y + currentRoom.worldUpperBounds.y) / 2f, 0);

        //Get the nearest spawn point in the room, that is nearest to the player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPointNearestToPlayer(player.gameObject.transform.position);

        StartCoroutine(DisplayDungeonLevelName());
    }

    /// <summary>
    /// Displays the dungeon level name
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisplayDungeonLevelName()
    {
        //Set the screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        GetPlayer().playerControl.DisablePlayer();

        string levelName = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString()
        + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();

        yield return StartCoroutine(DisplayMessageRoutine(levelName, Color.white, 2f));

        GetPlayer().playerControl.EnablePlayer();

        //Fade in the screen again
        StartCoroutine(Fade(1f, 0f, 2f, Color.black));

    }

    /// <summary>
    /// Displays a message on the screen
    /// </summary>
    /// <param name="levelName">The name of the current level</param>
    /// <param name="textColor">The color of the text</param>
    /// <param name="displayTime">The time that the message will appear</param>
    /// <returns>Whatever a Coroutine returns</returns>
    private IEnumerator DisplayMessageRoutine(string levelName, Color textColor, float displayTime)
    {
        messageText.SetText(levelName);
        messageText.color = textColor;

        //Display the text for a given period of time
        if (displayTime > 0f)
        {
            float timer = displayTime;

            while (timer > 0f && !inputActions.Player.NextLevel.WasPressedThisFrame())
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else //Display the message until Enter is pressed
        {
            while (!inputActions.Player.NextLevel.WasPressedThisFrame())
            {
                yield return null;
            }
        }

        yield return null;

        //Clear the text
        messageText.SetText("");
    }

    /// <summary>
    /// Enter the boss stage
    /// </summary>
    private IEnumerator BossStage()
    {
        //Activate the boss room
        bossRoom.gameObject.SetActive(true);

        //Unlock the doors that lead to the boss room
        bossRoom.UnlockTheDoors(0f);

        yield return new WaitForSeconds(2f);

        //Fade in the canvas to display a message
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.5f)));

        //Display the boss message
        yield return StartCoroutine(DisplayMessageRoutine("Well Done Soldier " + GameResources.Instance.currentPlayer.playerName +
            "! You've Survived... Until Now\n\nNow Find And Kill The Boss, Good Luck Soldier!", Color.white, 5f));

        //Fade out the canvas to display a message
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.5f)));
    }

    /// <summary>
    /// Show level as completed, load the next level
    /// </summary>
    private IEnumerator LevelCompleted()
    {
        //Play the next level
        currentGameState = GameState.playingLevel;

        //Play some sound effects to add more life
        int randomInt = Random.Range(0, GameResources.Instance.levelCompleteSoundEffects.Length);
        SoundManager.Instance.PlaySoundEffect(GameResources.Instance.levelCompleteSoundEffects[randomInt]);

        //Fade in the canvas to display a message
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.5f)));

        string name = GameResources.Instance.currentPlayer.playerName;

        if (name == "")
        {
            name = playerDetails.playerCharacterName.ToUpper();
        }

        //Display the level complete message
        yield return StartCoroutine(DisplayMessageRoutine("Well Done Soldier " + name +
            "! You've survived this level, keep it up! \nIt's just gonna get harder from here.", Color.white, 5f));

        yield return StartCoroutine(DisplayMessageRoutine("Collect any loot that you might've left behind. " +
            "\n\nPress 'Enter' to continue your journey", Color.white, 5f));

        //Fade out the canvas to clear the display
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.5f)));

        // When player presses the return key proceed to the next level
        while (!inputActions.Player.NextLevel.WasPressedThisFrame())
        {
            yield return null;
        }

        yield return null; //To avoid being detected twice

        //Increase the index to the next level
        currentDungeonLevelListIndex++;

        //Load the next level
        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    /// <summary>
    /// Fade the screen to black with the canvas group
    /// </summary>
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeTime, Color backgroundColor)
    {
        isScreenFading = true;

        Image image = canvasGroup.GetComponent<Image>();

        image.color = backgroundColor;

        float timer = 0f;

        while (timer <= fadeTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, timer / fadeTime);
            yield return null;
        }
        isScreenFading = false;
    }

    /// <summary>
    /// When the Game is won
    /// </summary>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;
        // Stop the timer
        stopWatch.StopTimer();

        //Play some music
        MusicManager.Instance.PlayMusic(GameResources.Instance.gameCompleteMusic, 2f, 2.5f);

        int rankScore = HighScoreManager.Instance.GetRank(gameScore);

        string rankText;

        //Test to see if the current score gets to the rankings
        if (rankScore > 0 && rankScore <= Settings.maxNumberOfHighScoresToSave)
        {
            rankText = "Your Score This Time Was #" + rankScore.ToString("#0") + "\n\nAnd Is Within The Top " + Settings.maxNumberOfHighScoresToSave.ToString("#0");

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }

            //Update the score
            HighScoreManager.Instance.AddScore(new Score()
            {
                playerName = name,
                levelName = "Level " + currentDungeonLevelListIndex + 1.ToString() +
                " - " + GetCurrentDungeonLevel().levelName.ToUpper(),
                playerScore = gameScore
            }, rankScore);
        }
        else
        {
            rankText = "You Won But Your Score Didn't, Not Even In The Top " + Settings.maxNumberOfHighScoresToSave.ToString("#0");
        }

        yield return new WaitForSeconds(1f);

        //Fade in the canvas to display a message
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        GetPlayer().playerControl.DisablePlayer();

        //Display the game complete message
        yield return StartCoroutine(DisplayMessageRoutine("Good Job Soldier " + GameResources.Instance.currentPlayer.playerName + "!"
            + "\n\nYou've Fulfilled Your Journey!", Color.white, 5.5f));

        yield return StartCoroutine(DisplayMessageRoutine("Your Final Score Was " + gameScore.ToString("###,###,###0") + "\n\n" + rankText, Color.white, 8f));

        yield return StartCoroutine(DisplayMessageRoutine("Congratulations For Completing The Game,\n\nAnd Thanks For Playing", Color.white, 6f));

        // Display the time it took to beat the game
        TimeSpan time = stopWatch.GetCurrentTime();
        string timeText = String.Format("It Took You {0} To Beat The Game, Well Done!", time.ToString(@"mm\:ss"));

        yield return StartCoroutine(DisplayMessageRoutine(timeText, Color.white, 5f));

        
        yield return StartCoroutine(DisplayMessageRoutine("Press 'Enter' To Restart The Game Or Enjoy The Music :)", Color.white, 0f));

        currentDungeonLevelListIndex = 0;

        //Restart the game
        currentGameState = GameState.gameRestarted;

        //Fade out the canvas
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.8f)));
    }

    /// <summary>
    /// When the Game is lost a.k.a the player dies
    /// </summary>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;
        GetPlayer().playerControl.DisablePlayer();
        GetPlayer().playerControl.GetPlayerInputActions().Player.NextLevel.Enable();

        yield return new WaitForSeconds(0.5f);

        //Play a random sound effect when the player dies
        int randomInt = Random.Range(0, GameResources.Instance.playerDeathSoundEffects.Length);
        SoundManager.Instance.PlaySoundEffect(GameResources.Instance.playerDeathSoundEffects[randomInt]);

        yield return new WaitForFixedUpdate();

        MusicManager.Instance.StopMusic();
        MusicManager.Instance.PlayMusic(GameResources.Instance.gameLostMusic, 3f, 1.5f);

        int rankScore = HighScoreManager.Instance.GetRank(gameScore);

        string rankText;
        string name = GameResources.Instance.currentPlayer.playerName;

        if (name == "")
        {
            name = playerDetails.playerCharacterName.ToUpper();
        }

        //Test to see if the current score gets to the rankings
        if (rankScore > 0 && rankScore <= Settings.maxNumberOfHighScoresToSave)
        {
            rankText = "Your Score This Time Was Ranked #" + rankScore.ToString("#0") + "\n\n In The Top " + Settings.maxNumberOfHighScoresToSave.ToString("#0");

            //Update the score
            HighScoreManager.Instance.AddScore(new Score()
            {
                playerName = name,
                levelName = "Level " + currentDungeonLevelListIndex + 1.ToString() +
                " - " + GetCurrentDungeonLevel().levelName.ToUpper(),
                playerScore = gameScore
            }, rankScore);
        }
        else
        {
            rankText = "You Were So Bad That Your Current Score Didn't Even Made It In The Top " + Settings.maxNumberOfHighScoresToSave.ToString("#0");
        }

        yield return new WaitForSeconds(1f);

        //Fade in the canvas to display a message
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        //Disable the enemies that are present(FindObjectOfType requires a lot of resourcess - it's ok in this stage of the game)
        Enemy[] enemiesArray = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemiesArray)
        {
            enemy.gameObject.SetActive(false);
        }

        //Display the game lost message
        yield return StartCoroutine(DisplayMessageRoutine("You Dead " + name +
            "! \n\nYou Failed (Miserably), But Are YOU Gonna Give Up?", Color.white, 3.5f));

        yield return StartCoroutine(DisplayMessageRoutine("Your Final Score Was " + gameScore.ToString("###,###,###0") + "\n\n" + rankText, Color.white, 4f));

        yield return StartCoroutine(DisplayMessageRoutine("Press 'Enter' to try again", Color.white, 0f));

        currentDungeonLevelListIndex = 0;

        //Restart the game
        currentGameState = GameState.gameRestarted;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #region Getters

    /// <summary>
    /// Get The Player For The Cinemachine Camera Target Group
    /// </summary>
    public Player GetPlayer()
    {
        return player;
    }

    public int GetDungeonIndex()
    {
        return currentDungeonLevelListIndex;
    }

    /// <summary>
    /// Gets And Returns The Player Minimap Icon
    /// </summary>
    /// <returns></returns>
    public Sprite GetPlayerMinimapIcon()
    {
        return playerDetails.playerMinimapIcon;
    }

    /// <summary>
    /// Get The Current Room The Player Is In
    /// </summary>
    /// <returns></returns>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get The Current Dungeon Level
    /// </summary>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    /// <summary>
    /// Gets the current amount of bitcoin currency
    /// </summary>
    /// <returns>Returns the amount of bitcoin currency</returns>
    public double GetSatsOnHold()
    {
        return bitcoinCurrency;
    }

    /// <summary>
    /// Gets the amount of current fiat currency
    /// </summary>
    /// <returns>Returns the amount of fiat currency</returns>
    public double GetFiatOnHold()
    {
        return fiatCurrency;
    }

    public void LoadData(GameData data)
    {
        this.bitcoinCurrency = data.satsOnHold;
        StaticEventHandler.CallMoneyChangedEvent(bitcoinCurrency, true);
        this.currentDungeonLevelListIndex = data.currentDungeonIndex;

        if (currentDungeonLevelListIndex >= dungeonLevelList.Count)
        {
            currentDungeonLevelListIndex = 0;
        }
        //this.currentDungeonLevelListIndex = data.currentDungeonIndex;
    }

    #endregion

    #region Setters

    /// <summary>
    /// Updates the amount of bitcoin that the player holds
    /// </summary>
    /// <param name="newBitcoin"></param>
    public void SetBitcoinOnHold(double newBitcoin)
    {
        bitcoinCurrency = newBitcoin;
    }

    /// <summary>
    /// Updates the amount of fiat that the player holds
    /// </summary>
    /// <param name="newFiat"></param>
    public void SetFiatOnHold(double newFiat)
    {
        fiatCurrency = newFiat;
    }

    public void SaveData(GameData data)
    {
        data.satsOnHold = this.bitcoinCurrency;
        data.currentDungeonIndex = this.currentDungeonLevelListIndex;
    }

    #endregion

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageText), messageText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenuUI), pauseMenuUI);

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion
}
