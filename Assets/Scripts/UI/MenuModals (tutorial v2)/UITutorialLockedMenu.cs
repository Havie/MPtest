﻿#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialLockedMenu : UIMenuController
{
    [Header("Scene Components")]
    [SerializeField] TutorialManager _tutorialManager = default;

    private List<UIInGameMenuButton> _buttonSetTut = new List<UIInGameMenuButton>();
    /************************************************************************************************************************/

    private void Awake()
    {
        LoadTutorialButtons();
    }

    private void LoadTutorialButtons()
    {
        if (!_tutorialManager)
            return;
        foreach (var stage in _tutorialManager.GetStages())
        {
            _buttonSetTut.Add(CreateNewButton(stage.StageName, stage, AssignButtonData, TutorialModalChosen));
        }
    }

    private void AssignButtonData(UIInGameMenuButton button, IButtonData stage)
    {
        button.AssignData(stage);
    }
    private void TutorialModalChosen(UIInGameMenuButton fromButton)
    {
        /// get data off the button from a data wrapper class 
        TutorialStage stage = (TutorialStage)fromButton.Data;
        ShowTutorialVideo(stage);
    }
    private void ShowTutorialVideo(TutorialStage stage)
    {
        this.gameObject.SetActive(false);
        _tutorialManager.LoadStage(stage);
    }
}
