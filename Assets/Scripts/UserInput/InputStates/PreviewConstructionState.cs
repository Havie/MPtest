﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewConstructionState : InputState
{
    public PreviewConstructionState(UserInputManager input)
    {
        _brain = input;
    }

    /************************************************************************************************************************/

    public override bool CanExitState(InputState nextState) { return true; }

    public override void DisableState()
    {

    }

    public override void EnableState(IInteractable currentSelection)
    {
        _currentSelection = currentSelection;
    }

    /************************************************************************************************************************/
    public override void Execute(bool inputDown, Vector3 pos)
    {
        CheckPreviewConstruction(inputDown, pos);
    }
    /************************************************************************************************************************/


    public bool CheckPreviewConstruction(bool inputDown, Vector3 pos)
    {
        IConstructable moveableObject = _currentSelection as IConstructable;

        if (inputDown)
        {
            if (moveableObject != null)
            {
                //Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(moveableObject.GetGameObject().transform);
                //moveableObject.OnFollowInput(worldLoc + _brain._mOffset);
                Vector3 worldLoc = _brain.GetInputWorldPos(_zDepth);
                worldLoc.z = moveableObject.DesiredSceneDepth();
                moveableObject.OnFollowInput(worldLoc);
            }

            if (!PreviewManager._inPreview)
                _brain.SwitchState(_brain._displacementState, _currentSelection);
        }
        else //Input UP
        {
            if (moveableObject != null)
            {
                if (PreviewManager._inPreview)
                    PreviewManager.ConfirmCreation();

                _currentSelection = null;
            }
            _brain.SwitchState(_brain._freeState, _currentSelection);
        }
        return false;
    }
}