﻿#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class UITutorialModal : InstanceMonoBehaviour<UITutorialModal>
{
    public enum eFollowUpActions { NONE, SPAWNPARTS, MAINMENU , LOCK_CONSTRUCTION, UNLOCK_CONSTRUCTION, LOCK_BINS, UNLOCK_BINS, DISABLE_SWITCH, ENABLE_SWITCH}
    
    [SerializeField] TextMeshProUGUI _txtTitle;
    [SerializeField] TextMeshProUGUI _txtBody;
    [SerializeField] VideoPlayer _video;
    [SerializeField] Image _bgIMG;
    [SerializeField] GameObject _modal;
    [SerializeField] GameObject _tab;
    [SerializeField] UserInput.UserInputManager _userInput;
    [SerializeField] TutorialItem[] _tutorialSequence = default;
    private int _tutorialIndex = -1; //Start below 0 so we can progress right away


    public bool DISABLED = false;

    private void Start()
    {
        if (GameManager.Instance.IsTutorial)
        {
            if (_userInput == null)
            {
                _userInput = FindObjectOfType<UserInput.UserInputManager>();
            }
            _userInput.AcceptInput = false;
            LoadNextTutorialData();
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Called from Continue Button
    /// </summary>
    public void ProgressTutorial()
    {
        ///Close the Menu
        ShowPopup(false);
        TutorialEvents.CallOnContinueClicked();
    }

    public void ShowPopup(bool cond)
    {
        _modal.SetActive(cond);
        _bgIMG.enabled = cond;
        _userInput.AcceptInput = !cond;
    }

    private void LoadNextTutorialData()
    {
        ///Increase the index
        ++_tutorialIndex;

        if (_tutorialIndex >= _tutorialSequence.Length)
        {
            //Debug.Log($"END OF TUTORIAL");
            Destroy(this);
            return;
        }
        TutorialItem t = _tutorialSequence[_tutorialIndex];
        _txtTitle.text = t.TitleTxt;
        _txtBody.text = t.bodyTxt;
        _video.clip = t.VideoGif;
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
        ///Give the player a fixed duration to see the results of their actions
        StartCoroutine(NextStepDelay(currTutorial.TimeDelayBeforeNextInstruction));

    }

    IEnumerator NextStepDelay(float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);
        /// setup for the next event
        ShowPopup(true);
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
        ShowPopup(false);
        if (_tab)
            Destroy(_tab);
    }
}
