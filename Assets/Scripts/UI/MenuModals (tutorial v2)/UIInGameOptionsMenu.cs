#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameOptionsMenu : UIMenuController
{
    [Header("Scene Components")]
    [SerializeField] TutorialManager _tutorialManager = default;
    [Header("Self Components")]
    [SerializeField] GameObject _menuLabelTxt = default; ///The Text that says "Menu"
    [SerializeField] Button _backButton = default; ///The button hidden in the header

    private List<UIInGameMenuButton> _buttonSetMain = new List<UIInGameMenuButton>();
    private List<UIInGameMenuButton> _buttonSetTut = new List<UIInGameMenuButton>();

    private bool _showTutorialSet;
    /************************************************************************************************************************/
    private void OnDisable()
    {
        ///If we get closed, we wana reset our state back to the main menu
        ResetSelf();
    }

    private void Start()
    {
        if (_backButton)
        {
            _backButton.onClick.AddListener(delegate { ShowMainMenuSets(); });
            ShowBackButton(false);
        }
        LoadMainMenuButtons();
        LoadTutorialButtons();
        _showTutorialSet = false;
        SwitchMenuSets();
    }

    private void LoadMainMenuButtons()
    {
        _buttonSetMain.Add(CreateNewButton("Tutorial", ShowTutorialSets));
        _buttonSetMain.Add(CreateNewButton("Quit", SceneTracker.Instance.ExitScene));
    }
    private void LoadTutorialButtons()
    {
        if (!_tutorialManager)
            return;
        foreach ( var stage in _tutorialManager.GetStages())
        {
            _buttonSetTut.Add(CreateNewButton(stage.StageName, stage, AssignButtonData,TutorialModalChosen));
        }
    }

    private void ShowTutorialSets()
    {
        _showTutorialSet = true;
        SwitchMenuSets();
        ShowBackButton(true);
    }
    private void ShowMainMenuSets()
    {
        _showTutorialSet = false;
        SwitchMenuSets();
        ShowBackButton(false);
    }
    private void ShowBackButton(bool cond)
    {
        _backButton.gameObject.SetActive(cond);
        _menuLabelTxt.SetActive(!cond);
    }
    private void SwitchMenuSets()
    {
        if(_showTutorialSet)
        {
            ToggleASet(_buttonSetMain, false);
            ToggleASet(_buttonSetTut, true);
        }
        else
        {
            ToggleASet(_buttonSetMain, true);
            ToggleASet(_buttonSetTut, false);
        }
    }
    private void ToggleASet(List<UIInGameMenuButton> set, bool cond)
    {
        foreach (var item in set)
        {
            item.gameObject.SetActive(cond);
        }
    }
    private void TutorialModalChosen(UIInGameMenuButton fromButton)
    {
        /// get data off the button from a data wrapper class 
        TutorialStage stage = (TutorialStage)fromButton.Data;
        ///Instead we load the widget that shows stage  info
        ShowTutorialWidget(stage);
    }
    private void AssignButtonData(UIInGameMenuButton button, IButtonData stage )
    {
        button.AssignData(stage);
    }
    private void ShowTutorialWidget(TutorialStage stage)
    {
        this.gameObject.SetActive(false);
        _tutorialManager.LoadStage(stage);
    }

    private void ResetSelf()
    {
        ShowMainMenuSets();
    }
}
