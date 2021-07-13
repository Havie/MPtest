#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialLogic : MonoBehaviour 
{
    public enum eFollowUpActions { NONE, SPAWNPARTS, MAINMENU, LOCK_CONSTRUCTION, UNLOCK_CONSTRUCTION, LOCK_BINS, UNLOCK_BINS, DISABLE_SWITCH, ENABLE_SWITCH }
    public System.Action OnSequenceFinished;
    [SerializeField] private UserInput.UserInputManager _userInput = default;
    [SerializeField] private GameObject _tutorialModal = default;
    [SerializeField] private Button _continueButton = default;
    private TutorialItem[] _tutorialSequence = default;
    private int _tutorialIndex = -1; //Start below 0 so we can progress right away
    private bool DISABLED = false;

    private void Start()
    {
        ///Set up our listener w the continue button to advance the tutorial
        _continueButton.onClick.AddListener(delegate { ProgressTutorial(); });
        ShowContinueButton(false);
        if (_userInput == null)
        {
            _userInput = FindObjectOfType<UserInput.UserInputManager>();
        }
    }
    public void InitTutorialStage(TutorialItem[] sequence)
    {
        _tutorialSequence = sequence;
        LoadNextTutorialData();
        ShowContinueButton(true);
    }

    /// <summary>
    /// Called from Continue Button
    /// </summary>
    public void ProgressTutorial()
    {
        var currTutorial = _tutorialSequence[_tutorialIndex];

        ///Close the Menu
        ShowContinueButton(false);
        TutorialEvents.CallOnContinueClicked();
    }


    /// We are going to need each step to dictate if it has to show continue button, or close module?

    public void ShowContinueButton(bool cond)
    {
        if (_continueButton)
        {
            _continueButton.gameObject.SetActive(cond);
        }
        _userInput.AcceptInput = !cond;
    }

    private void LoadNextTutorialData()
    {
        ///Increase the index
        ++_tutorialIndex;

        if (_tutorialIndex >= _tutorialSequence.Length)
        {
            Debug.Log($"END OF TUTORIAL Seq");
            OnSequenceFinished?.Invoke();
            return;
        }
        TutorialItem t = _tutorialSequence[_tutorialIndex];
        /// Set next listener for completed action
        TutorialEvents.RegisterForTutorialEvent(t.EventKey, TutorialActionSuccess);

    }

    private void TutorialActionSuccess(Void cond)
    {
        if (DISABLED)
            return;
        var currTutorial = _tutorialSequence[_tutorialIndex];
        TutorialEvents.UnRegisterForTutorialEvent(currTutorial.EventKey, TutorialActionSuccess);
        HandleFollowUpActions(currTutorial.FollowUpResponse);
        Debug.Log($"Trying to enact followUpResponse : {currTutorial.FollowUpResponse} ");
        ///Give the player a fixed duration to see the results of their actions
        StartCoroutine(NextStepDelay(currTutorial.TimeDelayBeforeNextInstruction));

    }

    IEnumerator NextStepDelay(float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);
        /// setup for the next event
        ShowContinueButton(true);
        LoadNextTutorialData();
    }

    private void HandleFollowUpActions(eFollowUpActions action)
    {
        switch (action)
        {
            case eFollowUpActions.SPAWNPARTS:
                {
                    ObjectRecord.eItemID[] tutorialOrder = new ObjectRecord.eItemID[2]
                    {
                        ObjectRecord.eItemID.PinkwPurplePlug,
                        ObjectRecord.eItemID.RedBot
                    };
                    PartDropper.Instance.SendInOrder(tutorialOrder);
                    break;
                }
            case eFollowUpActions.MAINMENU:
                {
                    GameManager.Instance.IsTutorial = false;
                    UIManagerGame.Instance.ReturnToMainMenu();
                    break;
                }
            case eFollowUpActions.LOCK_CONSTRUCTION:
                {
                    TutorialEvents.LockConstruction(true);
                    break;
                }
            case eFollowUpActions.UNLOCK_CONSTRUCTION:
                {
                    TutorialEvents.LockConstruction(false);
                    break;
                }
            case eFollowUpActions.LOCK_BINS:
                {
                    TutorialEvents.LockBins(true);
                    break;
                }
            case eFollowUpActions.UNLOCK_BINS:
                {
                    TutorialEvents.LockBins(false);
                    break;
                }
            case eFollowUpActions.DISABLE_SWITCH:
                {
                    TutorialEvents.LockSwitch(true);
                    break;
                }
            case eFollowUpActions.ENABLE_SWITCH:
                {
                    TutorialEvents.LockSwitch(false);
                    break;
                }
        }
    }

    private void OnDestroy()
    {
        ShowContinueButton(false);
    }
}
