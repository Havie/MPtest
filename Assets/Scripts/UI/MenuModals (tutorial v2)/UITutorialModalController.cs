#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class UITutorialModalController : MonoBehaviour
{
    [SerializeField] UITutorialTabManager _tabManager;
    [SerializeField] GameObject _lockedTutorialDiv = default;
    [SerializeField] GameObject _videoDiv = default;
    [SerializeField] TextMeshProUGUI _descriptionTxtArea = default;
    [SerializeField] [ResizableTextArea] string _stageDescription = "Please select from one of the following unlocked stages.\n\n\nNew stages can be unlocked by completing previous ones";

    private bool _isTutorial;
    /************************************************************************************************************************/

    public void Init(bool isTutorial)
    {
        _isTutorial = isTutorial;
        ShowStageMenu(_isTutorial);
    }
    /************************************************************************************************************************/

    ///If we're in the tutorial , show the other menu that replaces the video
    public void ShowStageMenu(bool cond)
    {
        _lockedTutorialDiv.SetActive(cond);
        _videoDiv.SetActive(!cond);
        if (cond)
        {
            ///Set the default text for stage selection
            _descriptionTxtArea.text = _stageDescription;
        }
    }
    ///Show the main functionality of the widget, which are the steps and videos
    public void LoadTutorialStage(TutorialStage stage)
    {
        _tabManager.LoadSequenece(stage.TutorialSequence);
        this.gameObject.SetActive(true);
        if (_isTutorial)
        {
            ShowStageMenu(false);
        }

    }
    /************************************************************************************************************************/

}
