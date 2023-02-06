using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DungeonMap : SingletonMonoBehaviour<DungeonMap>
{
    #region Header GameObject References
    [Space(10)]
    [Header("References")]
    #endregion

    #region Tooltip
    [Tooltip("The Canvas where the image of the minimap is")]
    #endregion
    public GameObject MinimapUI;
    
    private Camera dungeonMapCamera;
    private Camera mainCamera;
    private Player player;
    /// <summary>
    /// This character Input get populated with the same from the Player Controler script
    /// </summary>
    private CharacterInput characterInput;
    private float waitBeforeTeleporting = 0.7f;
    private float counter = 0.7f;

    protected override void Awake()
    {
        base.Awake();
        // Get the player
        player = GameManager.Instance.GetPlayer();
        characterInput = new();
        characterInput.OverviewMap.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        //Populate the cinemachine camera target with the player
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        dungeonMapCamera = GetComponentInChildren<Camera>();
        dungeonMapCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        waitBeforeTeleporting -= Time.deltaTime;

        //Get the room that was clicked
        if (characterInput.OverviewMap.Click.IsPressed() && GameManager.Instance.currentGameState == GameState.dungeonOverviewMap && waitBeforeTeleporting <= 0)
        {
            GetRoomClicked();
        }
    }

    /// <summary>
    /// Get the room that was clicked on the overview dungeon map
    /// </summary>
    private void GetRoomClicked()
    {
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        //Check for collisions at the mouse position
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        foreach (Collider2D collider2D in collider2Ds)
        {
            if (collider2D.GetComponent<InstantiatedRoom>() != null)
            {
                InstantiatedRoom instantiatedRoom = collider2D.GetComponent<InstantiatedRoom>();

                //If the room has been cleared of enemies an has been previously visited, the player can be teleported there
                if (instantiatedRoom.room.isClearOfEnemies && instantiatedRoom.room.isPreviouslyVisited)
                {
                    StartCoroutine(TeleportPlayerToRoom(worldPosition, instantiatedRoom.room));
                }
            }
        }

        waitBeforeTeleporting = counter;
    }

    /// <summary>
    /// Teleport the player to the clicked room
    /// </summary>
    private IEnumerator TeleportPlayerToRoom(Vector3 worldPosition, Room room)
    {
        StaticEventHandler.CallRoomChangedEvent(room);

        //Fade the screen to black
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0.1f, Color.black));

        ClearDungeonOverviewMap();

        //Disable the player during the fade
        player.playerControl.DisablePlayer();

        //Get the nearest spawn point to the player that is in the room
        Vector3 nearestSpawnPoint = HelperUtilities.GetSpawnPointNearestToPlayer(worldPosition);

        //Teleport the player to that new position
        GameManager.Instance.GetPlayer().transform.position = nearestSpawnPoint;

        //Return the screen to normal
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        //Enable the player again
        player.playerControl.EnablePlayer();
    }

    /// <summary>
    /// Displays the map on all the screen
    /// </summary>
    public void DisplayDungeonOverviewMap()
    {
        //player.playerControl.GetPlayerInputActions().OverviewMap.Enable();
        characterInput.OverviewMap.Enable();

        //Set the game states
        GameManager.Instance.previousGameState = GameManager.Instance.currentGameState;
        GameManager.Instance.currentGameState = GameState.dungeonOverviewMap;

        //Disable the player
        player.playerControl.DisablePlayer();

        //Disable the main camera and display the overview map
        mainCamera.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        //Ensure all room are active
        ActivateDungeonRoomsForDisplay();

        //Disable the small minimap UI component
        MinimapUI.SetActive(false);
    }

    public void ClearDungeonOverviewMap()
    {
        characterInput.OverviewMap.Disable();
        characterInput.Disable();

        //Restore the game states
        GameManager.Instance.currentGameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;

        //Reenable the player
        player.playerControl.EnablePlayer();

        //Renable the main camera and disable the overview map
        mainCamera.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);
        // Enable again the small minimap UI component
        MinimapUI.SetActive(true);
    }

    private void ActivateDungeonRoomsForDisplay()
    {
        foreach (KeyValuePair<string,Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }
}
