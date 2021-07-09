#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameOptionsMenu : UIMenuController
{
    [SerializeField] GameObject _menuLabelTxt = default; ///The Text that says "Menu"
    [SerializeField] Button _backButton = default; ///The button hidden in the header

    private List<UIInGameMenuButton> _buttonSetMain = new List<UIInGameMenuButton>();
    private List<UIInGameMenuButton> _buttonSetTut = new List<UIInGameMenuButton>();

    private bool _showTutorialSet;
    /************************************************************************************************************************/
    private void Start()
    {
        if (_backButton)
        {
            _backButton.onClick.AddListener(delegate { ShowMainMenu(); });
            ShowBackButton(false);
        }
        LoadMainMenuButtons();
        LoadTutorialButtons();
        _showTutorialSet = false;
        SwitchMenuSets();
    }

    private void LoadMainMenuButtons()
    {
        _buttonSetMain.Add(CreateNewButton("Tutorial", ShowTutorial));
        _buttonSetMain.Add(CreateNewButton("Quit", SceneTracker.Instance.ExitScene));
    }
    private void LoadTutorialButtons()
    {
        ///TODO load the story branches from the tutorial somehow
        _buttonSetTut.Add(CreateNewButton("Tutorial 1", UnknownAction));
        _buttonSetTut.Add(CreateNewButton("Tutorial 2", UnknownAction));
        _buttonSetTut.Add(CreateNewButton("Tutorial 3", UnknownAction));
    }

    private void ShowTutorial()
    {
        _showTutorialSet = true;
        SwitchMenuSets();
        ShowBackButton(true);
    }
    private void ShowMainMenu()
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

    private void UnknownAction()
    {

    }
}
