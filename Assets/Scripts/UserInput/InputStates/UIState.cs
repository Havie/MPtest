using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public class UIState : InputState
    {

        public UIState(UserInputManager input)
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
            CheckUI(command, pos);
        }


        /************************************************************************************************************************/

        private bool CheckUI(InputCommand command, Vector3 pos)
        {
            bool inputDown = command.DOWN || command.HOLD;
            if (inputDown)
            {
                ///If found slot in use spawn obj and go to displacement 
                var slot = _brain.RayCastForInvSlot() as UIInventorySlot;
                if (slot != null && slot.GetInUse())
                {
                    //Debug.LogWarning($"Slot found= {slot.name}");
                    int itemID = slot.GetItemID();
                    var qualityList = slot.RebuildQualities();
                    DebugQualities.DebugQuality(qualityList);
                    slot.RemoveItem(); 
                    var obj = ObjectManager.Instance.SpawnObject(itemID, _brain.GetInputWorldPos(_zDepth), qualityList).GetComponent<ObjectController>();
                    _currentSelection = obj;
                    //HandManager.PickUpItem(_currentSelection as ObjectController); ///Abstraced now when displacement calls OnBeginFollow()
                    //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                    if (_currentSelection != null)
                    {
                        _brain._mOffset = Vector3.zero; /// it spawns here so no difference
                        _brain.SetObjectStartPos(new Vector3(0, 0, _zDepth));
                        _brain.SetObjectStartRot(obj.transform.rotation);  //Quaternion.identity

                        IConstructable moveableObject = _currentSelection as IConstructable;
                        if (moveableObject != null)
                            moveableObject.ChangeAppearanceHidden(true); ///spawn it invisible till were not hovering over UI

                        if(GameManager.Instance.IsTutorial)
                        {
                            TutorialEvents.CallOnPartRemovedFromSlot();
                        }

                        _brain.SwitchState(_brain._displacementState, _currentSelection);

                    }

                }
            }
            else if (command.UP)
                _brain.SwitchState(_brain._freeState, _currentSelection);

            return false;
        }


        /************************************************************************************************************************/


    }
}