using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public override void Execute(bool inputDown, Vector3 pos)
    {
        CheckUI(inputDown, pos);
    }


    /************************************************************************************************************************/

    private bool CheckUI(bool inputDown, Vector3 pos)
    {
        if (inputDown)
        {
            ///If found slot in use spawn obj and go to displacement 
            var slot = _brain.RayCastForInvSlot();
            if (slot)
            {
                //Debug.LogWarning($"Slot found= {slot.name}");
                int itemID = slot.GetItemID();
                // Debug.Log($"Removing ItemID{itemID} from {slot.name}");
                var qualityList = RebuildQualities(slot.Qualities);
                slot.RemoveItem();
                Vector3 slotLoc = slot.transform.position;
                slotLoc.z = _tmpZfix;
                float zCoord = _brain.WorldToScreenPoint(slotLoc).z;
                var obj = BuildableObject.Instance.SpawnObject(itemID, _brain.GetInputWorldPos(zCoord), qualityList).GetComponent<ObjectController>();
                _currentSelection = obj;
                //HandManager.PickUpItem(_currentSelection as ObjectController); ///Abstraced now when displacement calls OnBeginFollow()
                //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                if (_currentSelection!=null)
                {
                    _brain._mOffset = Vector3.zero; /// it spawns here so no difference
                    _brain._objStartPos = new Vector3(0, 0, _tmpZfix);
                    _brain._objStartRot = Quaternion.identity;

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
        else
            _brain.SwitchState(_brain._freeState, _currentSelection);

        return false;
    }


    /************************************************************************************************************************/

    public List<QualityObject> RebuildQualities(List<QualityObject> toCopy)
    {
        List<QualityObject> newList = new List<QualityObject>();
        if (toCopy != null)
        {
            foreach (var q in toCopy)
                newList.Add(q);
        }

        return newList;
    }
}
