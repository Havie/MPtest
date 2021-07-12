#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class UITutorialModalController : MonoBehaviour
{
    [SerializeField] TutorialLogic _logic = default;
    [SerializeField] UITutorialTabManager _tabManager;
    [SerializeField] GameObject _lockedTutorialDiv = default;
    [SerializeField] GameObject _videoDiv = default;
    private bool _isTutorial;
    /************************************************************************************************************************/

    private void Awake()
    {
        _isTutorial= GameManager.Instance.IsTutorial;
        LoadTutorialMenu(_isTutorial);
    }
    /************************************************************************************************************************/

    ///If we're in the tutorial , show the other menu that replaces the video
    public void LoadTutorialMenu(bool cond)
    {
        _lockedTutorialDiv.SetActive(cond);
        _videoDiv.SetActive(!cond);
    }
    ///Show the main functionality of the widget, which are the steps and videos
    public void LoadTutorialStage(TutorialStage stage)
    {
        _tabManager.LoadSequenece(stage.TutorialSequence);
        this.gameObject.SetActive(true);
        if (_isTutorial)
        {
            LoadTutorialLogic(stage);
            LoadTutorialMenu(false);
        }
    
    }
    /************************************************************************************************************************/

    private void LoadTutorialLogic(TutorialStage stage)
    {
        _logic.InitTutorialStage(stage.TutorialSequence.ToArray());
    }
}
