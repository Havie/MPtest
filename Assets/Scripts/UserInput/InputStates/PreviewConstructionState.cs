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
        IMoveable moveableObject = _brain._currentSelection as IMoveable;

        if (_brain.InputDown())
        {
            if (moveableObject != null)
            {
                Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(moveableObject.GetGameObject().transform);
                moveableObject.OnFollowInput(worldLoc + _brain._mOffset);
            }

            if (!PreviewManager._inPreview)
                _brain.SwitchState(_brain._displacementState);
        }
        else //Input UP
        {
            if (moveableObject != null)
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
