#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameOptionsMenu : UITutorialStageMenu
{
    [Header("Self Components")]
    [SerializeField] GameObject _menuLabelTxt = default; ///The Text that says "Menu"
    [SerializeField] Button _backButton = default; ///The button hidden in the header

    private List<UIInGameMenuButton> _buttonSetMain = new List<UIInGameMenuButton>();

    private bool _showTutorialSet;
    /************************************************************************************************************************/
    private void OnDisable()
    {
        ///If we get closed, we wana reset our state back to the main menu
        ResetSelf();
    }

    protected override void Awake()
    {
        base.Awake(); ///Load the tutorialButtonSet
        if (_backButton)
        {
            _backButton.onClick.AddListener(delegate { ShowMainMenuSets(); });
            ShowBackButton(false);
        }
        LoadMainMenuButtons();
        _showTutorialSet = false;
        SwitchMenuSets();
    }

    private void LoadMainMenuButtons()
    {
        _buttonSetMain.Add(CreateNewButton("Tutorial", ShowTutorialSets));
        _buttonSetMain.Add(CreateNewButton("Quit", SceneTracker.Instance.ExitScene));
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
        if (_showTutorialSet)
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
    private void ResetSelf()
    {
        ShowMainMenuSets();
    }
}
