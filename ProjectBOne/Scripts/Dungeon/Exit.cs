using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Exit : MonoBehaviour, IUseable
{
    [SerializeField] private GameObject visualCue;

    private int dungeonIndex;
    private bool playerInRange;

    // Start is called before the first frame update
    void Start()
    {
        playerInRange = false;
    }

    private void Update()
    {
        // Update the dungeon index every time the player is in range
        dungeonIndex = GameManager.Instance.GetDungeonIndex();
        Debug.Log("The current index is " + dungeonIndex);

        if (playerInRange)
            visualCue.SetActive(true);
        else
            visualCue.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag))
        {
            playerInRange = true;

            // Update the dungeon index every time the player is in range
            dungeonIndex = GameManager.Instance.GetDungeonIndex();
            Debug.Log("The current index is " + dungeonIndex);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag))
        {
            playerInRange = false;
        }
    }

    public void UseItem()
    {
        if (playerInRange)
        {
            // Update the dungeon index every time the player is in range
            dungeonIndex = GameManager.Instance.GetDungeonIndex();

            GameManager.Instance.PlayDungeonLevel(dungeonIndex);
        }
    }
}
