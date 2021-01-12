using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewConstructionState : InputState
{
    public PreviewConstructionState(UserInput input)
    {
        _brain = input;
    }

    /************************************************************************************************************************/

    public override bool CanExitState(InputState nextState) { return true; }

    public override void DisableState()
    {

    }

    public override void EnableState()
    {

    }

    /************************************************************************************************************************/
    public override void Execute()
    {
        CheckPreviewConstruction();
    }
    /************************************************************************************************************************/


    public bool CheckPreviewConstruction()
    {

        if (_brain.InputDown())
        {
            if (_brain._currentSelection)
            {
                Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(_brain._currentSelection.transform);
                _brain._currentSelection.Follow(worldLoc + _brain._mOffset);
            }

            if (!PreviewManager._inPreview)
                _brain.SwitchState(_brain._displacementState);
        }
        else //Input UP
        {
            if (_brain._currentSelection)
            {
                if (PreviewManager._inPreview)
                    PreviewManager.ConfirmCreation();

                _brain._currentSelection = null;
            }
            _brain.SwitchState(_brain._freeState);
        }
        return false;
    }
}
