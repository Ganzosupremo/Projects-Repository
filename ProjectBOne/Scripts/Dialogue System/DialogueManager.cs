using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class DialogueManager : SingletonMonoBehaviour<DialogueManager>
{
    public bool IsDialoguePlaying { get; private set; }

    #region Header
    [Header("Parameters")]
    [Space(10)]
    #endregion
    [SerializeField] private float typingSpeed = 0.04f;

    #region Header
    [Header("Dialogue UI")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The panel where the dialogue will show")]
    #endregion
    [SerializeField] private GameObject dialoguePanel;
    #region Tooltip
    [Tooltip("The TMPpro text for the dialogue")]
    #endregion
    [SerializeField] private TextMeshProUGUI dialogueText;
    #region Tooltip
    [Tooltip("The name of the current speaker")]
    #endregion
    [SerializeField] private TextMeshProUGUI speakerNameText;
    #region Tooltip
    [Tooltip("The Transform of the gameobject with the layout group")]
    #endregion
    [SerializeField] private Transform layoutTransform;
    #region Tooltip
    [Tooltip("The animator for the portrait in the UI")]
    #endregion
    [SerializeField] private Animator portraitAnimator; // I think with more characters it's gonna get dificult to manage, but this serves the purpose for now

    #region Header
    [Header("Choices UI")]
    [Space(10)]
    #endregion
    [SerializeField] private GameObject[] choices;

    private TextMeshProUGUI[] choicesText;
    private Story currentStory;
    private Player player;
    private CharacterInput characterInput;
    private Coroutine exitDialogueCoroutine;
    private Coroutine typingEffectRoutine;
    private bool canContinueToNextDialogue = false;

    //Define some const for the dialogue tags
    private const string Speaker_Tag = "speaker";
    private const string Portrait_Tag = "portrait";
    private const string Layout_Tag = "layout";

    protected override void Awake()
    {
        base.Awake();
        player = GameManager.Instance.GetPlayer();
        characterInput = player.playerControl.GetPlayerInputActions();
    }

    private void Start()
    {
        IsDialoguePlaying = false;
        dialoguePanel.SetActive(false);
        choicesText = new TextMeshProUGUI[choices.Length];
        
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        // If there's no dialogue just return
        if (!IsDialoguePlaying)
            return;

        // Enable the UI scheme if for some reason it wasn't enabled in the EnterDialogueMode Method
        if (!player.playerControl.GetPlayerInputActions().UI.enabled)
        {
            Debug.LogWarning("The UI Scheme wasn't active... Activating it now...");
            player.playerControl.EnableUIControls();
        }

        if (!canContinueToNextDialogue) return;
        if (currentStory.currentChoices.Count != 0) return;
        //if (typingEffectRoutine != null) return;
        if (!characterInput.UI.PressEnter.WasPressedThisFrame()) return;

        ContinueStory();
    }

    /// <summary>
    /// Enters in the dialogue mode - in this mode the player can't move
    /// </summary>
    /// <param name="inkJSON">The JSON where the text is stored</param>
    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        IsDialoguePlaying = true;
        Tween(dialoguePanel, new Vector3(0f, 100f, 0f), 0.8f, LeanTweenType.easeInOutBounce, false, true, 0.2f);


        player.playerControl.DisablePlayer();
        player.playerControl.EnableUIControls();

        InputSystem.DisableDevice(Mouse.current);

        // Reset portrait, speaker values
        speakerNameText.text = "???";
        portraitAnimator.Play("Portrait_default");

        canContinueToNextDialogue = true;

        ContinueStory();
    }

    /// <summary>
    /// Continues, if any, with the current story
    /// </summary>
    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            Debug.Log("Story Continued");
            // Continue with the story
            if (typingEffectRoutine != null)
                StopCoroutine(typingEffectRoutine);
            
            typingEffectRoutine = StartCoroutine(CreateTypingEffect(currentStory.Continue()));

            //Handle the tags of this dialogue
            HandleTags(currentStory.currentTags);
        }
        else
        {
            if (exitDialogueCoroutine != null)
                StopCoroutine(exitDialogueCoroutine);

            exitDialogueCoroutine = StartCoroutine(ExitDialogueMode());
        }
    }

    /// <summary>
    /// Creates a typing effect for the dialogues
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private IEnumerator CreateTypingEffect(string text)
    {
        // Set the text to the full line, but max visible characters to zero
        yield return null;
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        canContinueToNextDialogue = false;
        HideChoices();

        // Write each letter at a time
        foreach (char letter in text.ToCharArray())
        {
            // If the submit button is pressed skip the text
            if (characterInput.UI.PressEnter.WasReleasedThisFrame())
            {
                Debug.Log("Button F pressed");
                dialogueText.maxVisibleCharacters = text.Length;
                break;
            }
             
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(typingSpeed);
        }

        //Display choices, if any, for this line of dialogue
        DisplayChoices();

        canContinueToNextDialogue = true;
    }

    /// <summary>
    /// Exits the dialogue mode - the player can move again
    /// </summary>
    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.3f);

        IsDialoguePlaying = false;
        Tween(dialoguePanel, new Vector3(0f, 250f, 0f), 0.8f, LeanTweenType.easeInOutBounce, false, false, 0.2f, DeactivatePanel);
        dialogueText.text = "";
        player.playerControl.DisableUIControls();
        player.playerControl.EnablePlayer();
        exitDialogueCoroutine = null;

        yield return new WaitForSeconds(0.3f);

        InputSystem.EnableDevice(Mouse.current);
    }

    /// <summary>
    /// Display the choices,if any, in the UI
    /// </summary>
    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // If there are more choices than the UI can support
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogWarning($"More Choices were giving than the UI can support. Number of choices given: {currentChoices.Count}");
        }

        int index = 0;

        // Enable and initialize the choices up to the amount of choices given for this line of dialogue
        foreach (Choice choice in currentChoices)
        {
            this.choices[index].SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        // Go through the remaining choices and make them dissapear
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private void HideChoices()
    {
        foreach (GameObject choiceButton in choices)
        {
            choiceButton.SetActive(false);
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        // Loop through each tag and handle it
        foreach (string tag in currentTags)
        {
            string[] splitedTags = tag.Split(':');

            if (splitedTags.Length != 2)
            {
                Debug.LogError($"Tag couldn't be parsed: {tag}");
            }

            string tagKey = splitedTags[0].Trim();
            string tagValue = splitedTags[1].Trim();

            switch (tagKey)
            {
                case Speaker_Tag:
                    speakerNameText.text = tagValue;
                    break;
                case Portrait_Tag:
                    portraitAnimator.Play(tagValue);
                    break;
                case Layout_Tag:
                    Debug.Log("Layout = " + tagValue);
                    break;
                default:
                    Debug.LogError("Tag came in but couldn't be handled" + tag);
                    break;
            }
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0]);
    }

    /// <summary>
    /// Makes the choice so the dialogue can continue
    /// </summary>
    /// <param name="choiceindex">The index of the choice</param>
    public void MakeChoice(int choiceindex)
    {
        if (canContinueToNextDialogue)
        {
            currentStory.ChooseChoiceIndex(choiceindex);

            ContinueStory();
        }
    }

    /// <summary>
    /// Moves the <paramref name="objectToTween"/> to the <paramref name="positionToTween"/> specified
    /// </summary>
    /// <param name="objectToTween">The object to apply the tween to</param>
    /// <param name="positionToTween">The new position of the object</param>
    /// <param name="time">The time it takes to make the tween</param>
    /// <param name="tweenType"></param>
    /// <param name="canContinueNextDialogue">True if it can continue to the next dialogue of the story, false otherwise</param>
    /// <param name="objectActive">True activates the object, false deactivates it</param>
    /// <param name="delay">Delay of the tween, if any</param>
    private void Tween(GameObject objectToTween, Vector3 positionToTween, float time, LeanTweenType tweenType, bool canContinueNextDialogue, bool objectActive, float delay = 0f, Action onComplete = null)
    {
        canContinueToNextDialogue = canContinueNextDialogue;
        if (onComplete != null)
            LeanTween.moveLocal(objectToTween, positionToTween, time).setDelay(delay).setEase(tweenType).setOnComplete(onComplete);
        else
            LeanTween.moveLocal(objectToTween, positionToTween, time).setDelay(delay).setEase(tweenType);
        
        objectToTween.SetActive(objectActive);
    }

    private void DeactivatePanel()
    {
        dialoguePanel.SetActive(false);
    }
}
