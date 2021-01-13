using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIState : InputState
{
    public UIState(UserInput input)
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
        CheckUI();
    }


    /************************************************************************************************************************/

    private bool CheckUI()
    {
        if (_brain .InputDown())
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
                slotLoc.z = _brain._tmpZfix;
                float zCoord = _brain._mainCamera.WorldToScreenPoint(slotLoc).z;
                var obj = BuildableObject.Instance.SpawnObject(itemID, _brain.GetInputWorldPos(zCoord), qualityList).GetComponent<ObjectController>();
                _brain._currentSelection = obj;
                HandManager.PickUpItem(_brain._currentSelection as ObjectController);
                //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                if (_brain._currentSelection!=null)
                {
                    _brain._mOffset = Vector3.zero; /// it spawns here so no difference
                    _brain._objStartPos = new Vector3(0, 0, _brain._tmpZfix);
                    _brain._objStartRot = Quaternion.identity;
                    //_justPulledOutOfUI = true;
                    _brain.SwitchState(_brain._displacementState);
                    IMoveable moveableObject = _brain._currentSelection as IMoveable;
                    if(moveableObject!=null)
                        moveableObject.ChangeAppearanceHidden(true); ///spawn it invisible till were not hovering over UI
                }
                else
                    Debug.LogWarning("This happened?1");

            }
            else
                Debug.LogWarning("This happened?2");
        }
        else
            _brain.SwitchState(_brain._freeState);

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
