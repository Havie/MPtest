using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public class FreeState : InputState
    {

        public FreeState(UserInputManager input)
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
        public override void Execute(InputCommand command)
        {
            Vector3 pos = command.Position;
            /** Player is pressing to begin interaction with an obj or UI item */
            CheckFree(command, pos);
        }

        /************************************************************************************************************************/


        private bool CheckFree(InputCommand command, Vector3 pos)
        {
            bool inputDown = command.DOWN || command.HOLD;
            if (inputDown)
            {

                _currentSelection = _brain.CheckForObjectAtLoc(pos);
                //_brain._pressTimeCURR = 0;
                if (_currentSelection != null) ///if you get an obj do rotation
                {
                    // Debug.Log("CURR SELC= " + _currentSelection.gameObject);    
                    _brain.SwitchState(_brain._rotationState, _currentSelection);
                }
                else ///if u get UI do UI 
                {
                    IAssignable slot = _brain.RayCastForInvSlot();
                    if (slot != null)
                    {
                        if (slot.GetInUse())
                        {
                            _brain.SwitchState(_brain._uiState, _currentSelection);
                        }
                    }
                }

            }


            return false;
        }

    }
}