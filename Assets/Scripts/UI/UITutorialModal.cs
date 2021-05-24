#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class UITutorialModal : InstanceMonoBehaviour<UITutorialModal>
{
    [SerializeField] TextMeshProUGUI _txtTitle;
    [SerializeField] TextMeshProUGUI _txtBody;
    [SerializeField] VideoPlayer _video;
    [SerializeField] Image _bgIMG;
    [SerializeField] GameObject _modal;
    [SerializeField] UserInput.UserInputManager _userInput;

    [SerializeField] TutorialItem[] _tutorialSequence = default;
    private int _tutorialIndex = -1; //Start below 0 so we can progress right away
    private bool _firstTimeWelcomeMsg = true;
    TutorialEvents _eventManager;
    private void Start()
    {
        _eventManager = TutorialEvents.Instance;
        if (_userInput==null)
        {
            _userInput = FindObjectOfType<UserInput.UserInputManager>();
        }
        _userInput.AcceptInput = false;
        LoadNextTutorialData();
    }

    /// <summary>
    /// Called from Continue Button
    /// </summary>
    public void ProgressTutorial()
    {
        if (_firstTimeWelcomeMsg)
        {
            _eventManager.CallOnFirstContinueClicked();
            _firstTimeWelcomeMsg = false;
            return;
        }
        ///Close the Menu
        ShowPopup(false);
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
            return;
        TutorialItem t = _tutorialSequence[_tutorialIndex];
        _txtTitle.text = t.TitleTxt;
        _txtBody.text = t.bodyTxt;
        _video.clip = t.VideoGif;
        /// Set next listener for completed action
        _eventManager.RegisterForTutorialEvent(t.EventKey, TutorialActionSuccess);

    }

    private void TutorialActionSuccess(Void cond)
    {
        Debug.Log($"Event happened!");
        ///Give the player a second to see the results of their actions
        var currTutorial = _tutorialSequence[_tutorialIndex];
         _eventManager.UnRegisterForTutorialEvent(currTutorial.EventKey, TutorialActionSuccess);

        StartCoroutine(NextStepDelay(currTutorial.TimeDelayBeforeNextInstruction));

    }

    IEnumerator NextStepDelay(float delayInSeconds)
    {
         yield return new WaitForSeconds(delayInSeconds);
        /// setup for the next event
        LoadNextTutorialData();
        ShowPopup(true);

    }
}
