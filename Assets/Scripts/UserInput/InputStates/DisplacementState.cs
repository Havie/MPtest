﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementState : InputState
{
    public DisplacementState(UserInput input)
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
        CheckDisplacement();
    }

    /************************************************************************************************************************/


    /** Player is moving an object to or from inventory slot*/
    private bool CheckDisplacement()
    {

        UIInventorySlot slot = _brain.RayCastForInvSlot();
        if (_brain.InputDown())
        {
            if (_brain._currentSelection)
            {
                Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(_brain._currentSelection.transform);
                _brain._currentSelection.Follow(worldLoc + _brain._mOffset);

                if (slot != null) ///we are hovering over a slot 
                {
                    if (!slot.GetInUse())
                    {
                        ///The slot can accept this item
                        if (slot.PreviewSlot(BuildableObject.Instance.GetSpriteByID((int)_brain._currentSelection._myID)))
                        {
                            _brain._currentSelection.ChangeAppearanceHidden(true);
                            UIManager.instance.ShowPreviewInvSlot(false, _brain._inputPos, null);
                        }
                        else ///the slot can not accept this item so continue to show the dummy preview
                            ShowDummyPreviewSlot();

                        if (slot != _brain._lastSlot && _brain._lastSlot != null)
                            _brain._lastSlot.UndoPreview();
                        _brain._lastSlot = slot;
                    }
                    else
                    {
                        ///show a preview of just the icon floating around
                        if (slot != _brain._lastSlot && _brain._lastSlot != null)
                            _brain._lastSlot.UndoPreview();
                        _brain._lastSlot = slot;
                        ShowDummyPreviewSlot();
                    }
                }
                else if (PreviewManager._inPreview)
                    _brain.SwitchState(_brain._previewState); ///dont want to reset the Object while in preview or it wont be hidden
                else
                   ResetObjectAndSlot();
            }
        }
        else ///Input UP
        {
            if (_brain._currentSelection)
            {
                bool assigned = false;
                if (slot != null)
                {
                    //Debug.Log($"FOUND UI SLOT {slot.name}");
                    slot.SetNormal();
                    assigned = slot.AssignItem(_brain._currentSelection, 1);
                    if (assigned)
                        _brain.Destroy(_brain._currentSelection);
                }
                if (!assigned)
                {
                    ///put it back to where we picked it up 
                    if (slot) // we tried dropping in incompatible slot
                    {
                        _brain._currentSelection.transform.position = _brain._objStartPos;
                        _brain._currentSelection.transform.rotation = _brain._objStartRot;
                        UIManager.instance.ShowPreviewInvSlot(false, _brain._inputPos, null);
                    }
                    else
                    {
                        var dz = _brain.CheckRayCastForDeadZone();
                        if (dz)
                        {
                            ///If the item is dropped in a deadzone, reset it to a safe place
                            _brain._currentSelection.transform.position = _brain.GetCurrentWorldLocBasedOnPos(dz.GetSafePosition);
                        }
                        else
                        {
                            ///Check were not below the table
                            if (_brain._currentSelection._hittingTable)
                                _brain._currentSelection.SetResetOnNextChange();

                            // Debug.Log($"curr: {_currentSelection.transform.position.y} vs table {_tableHeight}");

                        }
                    }
                    _brain._currentSelection.ChangeAppearanceNormal();
                    // HandManager.DropItem(_currentSelection);
                    //Really weird Fix to prevent raycast bug
                    _brain.FixRayCastBug();
                }
            }

            // _justPulledOutOfUI = false;

            _brain.SwitchState(_brain._freeState);
        }

        return false;
    }



    private void ShowDummyPreviewSlot()
    {
        Sprite img = BuildableObject.Instance.GetSpriteByID((int)_brain._currentSelection._myID);
        _brain._currentSelection.ChangeAppearanceHidden(true);
        UIManager.instance.ShowPreviewInvSlot(true, _brain._inputPos, img);
    }

    private void ResetObjectAndSlot()
    {
        if (_brain._currentSelection)
        {
            _brain._currentSelection.ChangeAppearanceHidden(false);
        }
        if (_brain._lastSlot)
        {
            _brain._lastSlot.UndoPreview();
            _brain._lastSlot = null;
        }

        UIManager.instance.ShowPreviewInvSlot(false, _brain._inputPos, null);
    }
}