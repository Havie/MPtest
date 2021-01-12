using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeState : InputState
{


    public FreeState (UserInput input)
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
        /** Player is pressing to begin interaction with an obj or UI item */
        CheckFree();
    }

    /************************************************************************************************************************/


    private bool CheckFree()
    {
        if (_brain.InputDown())
        {
            _brain._lastPos = _brain._inputPos;
            _brain._currentSelection = _brain.CheckForObjectAtLoc(_brain._lastPos);
            _brain._pressTimeCURR = 0;
            if (_brain._currentSelection!=null) ///if you get an obj do rotation
            {
                // Debug.Log("CURR SELC= " + _currentSelection.gameObject);    

                _brain._rotationAmount = Vector2.zero; ///reset our rotation amount before re-entering
                _brain.SwitchState(_brain._rotationState);

            }
            else ///if u get UI do UI 
            {
                UIInventorySlot slot = _brain.RayCastForInvSlot();
                if (slot != null)
                {
                    if (slot.GetInUse())
                    {
                        _brain.SwitchState(_brain._uiState);
                    }
                }
                else
                {
                    UIInstructions instructions = _brain.RayCastForInstructions();
                    if (instructions)
                        instructions.InstructionsClicked();
                }
            }

        }


        return false;
    }

}
