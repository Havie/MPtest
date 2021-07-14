#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class UITutorialStageLockableMenu : UITutorialStageMenu
{

    /************************************************************************************************************************/
    private void OnEnable()
    {
        ///After letting the _buttonSetTut load in awake, 
        ///check with the TutorialManager on which buttons to lock:
        ParseAndSetLockedButtons();
    }

    private void ParseAndSetLockedButtons()
    {
        foreach (UIInGameMenuButton button in _buttonSetTut)
        {
            TutorialStage stage = button.Data as TutorialStage;
            bool isLocked = _tutorialManager.StageIsLocked(stage);
            button.LockButton(isLocked);
        }
    }
}

