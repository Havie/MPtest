﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementState : InputState
{

    private IAssignable _lastSlot;

    public DisplacementState(UserInputManager input)
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
        var moveableObject = _currentSelection as IMoveable;
        if (moveableObject != null)
        {

            moveableObject.ChangeAppearanceMoving(); ///TODO abstract to handle inside interface
            Transform transform = moveableObject.Transform();
            float zCoord = _brain.WorldToScreenPoint(transform.position).z;
            _brain._mOffset = transform.position - _brain.GetInputWorldPos(zCoord);
            //Debug.LogWarning($" (1st) _mOffset= <color=red> {_brain._mOffset} </color>");
            _brain._objStartPos = transform.position;
            _brain._objStartRot = transform.rotation;

            ///only if on table
            // if (_currentSelection.SetOnTable())  
            // if (_brain._currentSelection._hittingTable)
            if (moveableObject.OutOfBounds())
                ResetObjectOrigin(moveableObject, _zDepth); ///zCoord

            moveableObject.OnBeginFollow(); ///Might mess up objectCntroller
            //HandManager.PickUpItem(_currentSelection as ObjectController); //might have moved to the wrong spot
        }
    }
    private void ResetObjectOrigin(IMoveable moveableObject, float zCoord)
    {
        ///Reset the object to have the right orientation for construction when picked back up
        if (moveableObject != null)
        {
            Vector3 mouseLocWorld = _brain.GetInputWorldPos(zCoord);
            _brain._objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _zDepth);
            Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_brain._objStartPos} ");
            _brain._mOffset = Vector3.zero;
            ///new
            var trans = moveableObject.GetGameObject().transform;
            trans.position = _brain._objStartPos;
            trans.rotation = _brain._objStartRot;
            ///Start moving the object
        }
        else
            Debug.LogWarning("This happened?1");
    }


    /************************************************************************************************************************/
    public override void Execute(bool inputDown, Vector3 inputPos)
    {
        CheckDisplacement(inputDown, inputPos);
    }

    /************************************************************************************************************************/


    /** Player is moving an object to or from inventory slot*/
    private bool CheckDisplacement(bool inputDown, Vector3 inputPos)
    {
        IMoveable moveableObject = _currentSelection as IMoveable;

        IAssignable slot = _brain.RayCastForInvSlot();
        if (inputDown)
        {
            if (moveableObject != null)
            {
                //Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(moveableObject.Transform());

                Vector3 worldLoc = _brain.GetInputWorldPos(_zDepth);
                worldLoc.z = moveableObject.DesiredSceneDepth();
                moveableObject.OnFollowInput(worldLoc); //+ _brain._mOffset
                //Debug.Log($" {worldLoc} + _mOffset={_brain._mOffset}  = {(worldLoc + _brain._mOffset)}");

                if (slot != null) ///we are hovering over a slot 
                {
                    if (!slot.GetInUse())
                    {
                        ObjectController oc = moveableObject as ObjectController;
                        if (oc)
                        {
                            ///The slot can accept this item
                            if (slot.PreviewSlot(BuildableObject.Instance.GetSpriteByID((int)oc._myID)))
                            {
                                IConstructable constructable = moveableObject as IConstructable;
                                if (constructable != null)
                                {
                                    constructable.ChangeAppearanceHidden(true);
                                    UIManager.instance.ShowPreviewInvSlot(false, inputPos, null);
                                }
                            }
                            else ///the slot can not accept this item so continue to show the dummy preview
                                ShowDummyPreviewSlot(moveableObject as IConstructable, inputPos);

                            if (slot != _lastSlot && _lastSlot != null)
                                _lastSlot.UndoPreview();

                            _lastSlot = slot;

                        }///Might be the wrong place to close this bracket
                    }
                    else
                    {
                        ///show a preview of just the icon floating around
                        if (slot != _lastSlot && _lastSlot != null)
                            _lastSlot.UndoPreview();

                        _lastSlot = slot;
                        ShowDummyPreviewSlot(moveableObject as IConstructable, inputPos);
                    }
                }
                else if (PreviewManager._inPreview)
                    _brain.SwitchState(_brain._previewState, _currentSelection); ///dont want to reset the Object while in preview or it wont be hidden
                else
                    ResetObjectAndSlot(moveableObject as IConstructable, inputPos);
            }
        }
        else ///Input UP
        {
            if (moveableObject != null)
            {
                bool assigned = false;
                if (slot != null)
                {
                    //Debug.Log($"FOUND UI SLOT {slot.name}");
                    //slot.SetNormal();
                    assigned = slot.AssignItem(moveableObject as ObjectController, 1); ///TODO verify this somehow
                    if (assigned)
                        _brain.Destroy(moveableObject);
                }
                if (!assigned)
                {
                    ///put it back to where we picked it up 
                    if (slot != null) // we tried dropping in incompatible slot
                    {
                        var trans = moveableObject.GetGameObject().transform;
                        trans.position = _brain._objStartPos;
                        trans.rotation = _brain._objStartRot;
                        UIManager.instance.ShowPreviewInvSlot(false, inputPos, null);
                    }
                    else
                    {
                        var dz = _brain.CheckRayCastForDeadZone();
                        if (dz)
                        {
                            ///If the item is dropped in a deadzone, reset it to a safe place
                            moveableObject.GetGameObject().transform.position = _brain.GetCurrentWorldLocBasedOnPos(dz.GetSafePosition, _currentSelection);
                        }
                        else
                        {
                            ///Check were not below the table
                            if (moveableObject.OutOfBounds())
                                moveableObject.SetResetOnNextChange();

                            // Debug.Log($"curr: {_currentSelection.transform.position.y} vs table {_tableHeight}");

                        }
                    }
                    moveableObject.ChangeAppearanceNormal(); ///ToDo abstract this somehow
                    // HandManager.DropItem(_currentSelection);
                    //Really weird Fix to prevent raycast bug
                    FixRayCastBug();
                }
            }

            // _justPulledOutOfUI = false;

            _brain.SwitchState(_brain._freeState, _currentSelection);
        }

        return false;
    }



    private void ShowDummyPreviewSlot(IConstructable moveableObject, Vector3 inputPos)
    {
        ObjectController oc = moveableObject as ObjectController;
        if (oc)
        {
            Sprite img = BuildableObject.Instance.GetSpriteByID((int)oc._myID);
            moveableObject.ChangeAppearanceHidden(true);
            UIManager.instance.ShowPreviewInvSlot(true, inputPos, img);
        }
    }

    private void ResetObjectAndSlot(IConstructable moveableObject, Vector3 inputPos)
    {
        if (moveableObject != null)
        {
            moveableObject.ChangeAppearanceHidden(false);
        }
        if (_lastSlot != null)
        {
            _lastSlot.UndoPreview();
            _lastSlot = null;
        }

        UIManager.instance.ShowPreviewInvSlot(false, inputPos, null);
    }

    /**This is a really weird fix I found to prevent the raycast from missing the box */
    public void FixRayCastBug()
    {
        if (_currentSelection != null)
        {
            var box = _currentSelection.GetGameObject().GetComponent<Collider>();
            box.enabled = false;
            _currentSelection = null;
            box.enabled = true;
        }
    }

}