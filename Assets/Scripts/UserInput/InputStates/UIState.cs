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
                if (slot != null)
                {
                    //Debug.LogWarning($"Slot found= {slot.name}");
                    int itemID = slot.GetItemID();
                    // Debug.Log($"Removing ItemID{itemID} from {slot.name}");
                    var qualityList = slot.RebuildQualities();
                    slot.RemoveItem();
                    Vector3 slotLoc = slot.transform.position; ///This world space val where the canvas is wayyy off in no where land

                    var nv3 = new Vector3(slotLoc.x, slotLoc.y, slotLoc.z);

                    //slotLoc.z = _zDepth;  ///Might want to get a new depth based on viewing angle
                    //slotLoc.z = -4;
                    ///WorldToScreenPoint doesn't work when angled, this WorldLoc isn't anywhere near
                    ///where the Camera is (the x/y vals are insane), and somehow
                    ///when titled it makes the Zdepth wrong 
                    //float zCoord = _brain.WorldToScreenPoint(slotLoc).z;

                    // Debug.Log($"Thinks slotLoc is : <color=yellow> {nv3}  </color>and when we change it to append {_zDepth}- we get : <color=orange>{_brain.WorldToScreenPoint(slotLoc)}</color>  ,, so finalzCoord =<color=red>{_zDepth}</color> , final result=<color=green>{_brain.GetInputWorldPos(_zDepth)}</color>");


                    var obj = BuildableObject.Instance.SpawnObject(itemID, _brain.GetInputWorldPos(_zDepth), qualityList).GetComponent<ObjectController>();
                    _currentSelection = obj;
                    //HandManager.PickUpItem(_currentSelection as ObjectController); ///Abstraced now when displacement calls OnBeginFollow()
                    //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                    if (_currentSelection != null)
                    {
                        _brain._mOffset = Vector3.zero; /// it spawns here so no difference
                        _brain.SetObjectStartPos(new Vector3(0, 0, _zDepth));
                        _brain.SetObjectStartRot(Quaternion.identity);

                        IConstructable moveableObject = _currentSelection as IConstructable;
                        if (moveableObject != null)
                            moveableObject.ChangeAppearanceHidden(true); ///spawn it invisible till were not hovering over UI

                        _brain.SwitchState(_brain._displacementState, _currentSelection);

                    }
                    else
                        Debug.LogWarning("This happened?1");

                }
                else
                    Debug.LogWarning("This happened?2");
            }
            else if (command.UP)
                _brain.SwitchState(_brain._freeState, _currentSelection);

            return false;
        }


        /************************************************************************************************************************/


    }
}