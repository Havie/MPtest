using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeState : InputState
{

    public FreeState (UserInputManager input)
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
        /** Player is pressing to begin interaction with an obj or UI item */
        CheckFree(inputDown, pos);
    }

    /************************************************************************************************************************/


    private bool CheckFree(bool inputDown, Vector3 pos)
    {
        if (inputDown)
        {

            _currentSelection = _brain.CheckForObjectAtLoc(pos);
           //_brain._pressTimeCURR = 0;
            if (_currentSelection!=null) ///if you get an obj do rotation
            {
                // Debug.Log("CURR SELC= " + _currentSelection.gameObject);    

                //_rotationAmount = Vector2.zero; ///reset our rotation amount before re-entering
                _brain.SwitchState(_brain._rotationState, _currentSelection);

            }
            else ///if u get UI do UI 
            {
                UIInventorySlot slot = _brain.RayCastForInvSlot();
                if (slot != null)
                {
                    if (slot.GetInUse())
                    {
                        _brain.SwitchState(_brain._uiState, _currentSelection);
                    }
                }
                else
                {
                    ///TODO get these under the interactable umbrella
                    UIInstructions instructions = _brain.RayCastForInstructions();
                    if (instructions)
                        instructions.InstructionsClicked();
                }
            }

        }


        return false;
    }

}
