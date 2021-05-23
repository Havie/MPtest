#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

public class UITutorialModal : InstanceMonoBehaviour<UITutorialModal>
{
    [SerializeField] TextMeshProUGUI _txtTitle;
    [SerializeField] TextMeshProUGUI _txtBody;
    [SerializeField] VideoPlayer _video;
    [SerializeField] Image _bgIMG;
    [SerializeField] GameObject _modal;
    [SerializeField] UserInput.UserInputManager _userInput;

    [SerializeField] TutorialItem[] _tutorialSequence = default;
    private int _tutorialIndex = 0;

    private void Start()
    {
        if(_userInput==null)
        {
            _userInput = FindObjectOfType<UserInput.UserInputManager>();
        }
        _userInput.AcceptInput = false;
        LoadTutorialData();
    }

    /// <summary>
    /// Called from Continue Button
    /// </summary>
    public void ProgressTutorial()
    {

        ///TOD Set next listener for completed action

        ///Increase the index
        ++_tutorialIndex;
        ///Close the Menu
        ShowPopup(false);


    }

    public void ShowPopup(bool cond)
    {
        _modal.SetActive(cond);
        _bgIMG.enabled = cond;
        _userInput.AcceptInput = !cond;
    }


    private void LoadTutorialData()
    {
        if (_tutorialIndex >= _tutorialSequence.Length)
            return;

        TutorialItem t = _tutorialSequence[_tutorialIndex];
        _txtTitle.text = t.TitleTxt;
        _txtBody.text = t.bodyTxt;
        _video.clip = t.VideoGif;

    }
}
