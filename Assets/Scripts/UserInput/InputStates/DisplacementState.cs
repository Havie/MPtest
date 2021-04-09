using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
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
            UIManager.ShowPreviewMovingIcon(false, Vector3.zero, null);
            UIManager.ShowPreviewInvSlot(false, Vector3.zero, null);
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
                _brain.SetObjectStartPos(transform.position);

                moveableObject.OnBeginFollow(); ///Might mess up objectCntroller
                //HandManager.PickUpItem(_currentSelection as ObjectController); //might have moved to the wrong spot
            }
        }
        private void ResetObjectOrigin(IMoveable moveableObject)
        {

            ///Reset the object to have the right orientation for construction when picked back up
            if (moveableObject != null)
            {
                Vector3 mouseLocWorld = _brain.GetInputWorldPos(_zDepth); ///Get the right x/y based on zDepth
                                                                          ///Find new starting position based off of where the input is :
                _brain.SetObjectStartPos(new Vector3(mouseLocWorld.x, mouseLocWorld.y, _partDepth)); ///Put the obj at the zDepth of parts, which is different than zDepth for Camera
                //Debug.Log($"mouseLocWorld={mouseLocWorld} , _objStartPos={_brain._objStartPos} ");
                _brain._mOffset = Vector3.zero; ///Reset the offset, since its dead on w the input location
                _brain.SetObjectStartRot(Quaternion.identity);
                ///new
                moveableObject.ResetPositionHard(_brain.ObjStartPos, _brain.ObjStartRot);
                ///Start moving the object
               // Debug.Log($"Object was on table reset Zdepth to : <color=orange>{new Vector3(mouseLocWorld.x, mouseLocWorld.y, _partDepth)}</color>");
            }
            else
                Debug.LogWarning("This happened?1");
        }


        /************************************************************************************************************************/
        public override void Execute(InputCommand command)
        {
            Vector3 pos = command.Position;
            CheckDisplacement(command, pos);
        }

        /************************************************************************************************************************/


        /** Player is moving an object to or from inventory slot*/
        private bool CheckDisplacement(InputCommand command, Vector3 inputPos)
        {
            IMoveable moveableObject = _currentSelection as IMoveable;
            IAssignable slot = _brain.RayCastForInvSlot();
            bool inputDown = command.DOWN || command.HOLD;
            if (inputDown)
            {
                if (moveableObject != null)
                {
                    //Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(moveableObject.Transform());

                    Vector3 worldLoc = _brain.GetInputWorldPos(_zDepth);
                    worldLoc.z = moveableObject.DesiredSceneDepth();
                    moveableObject.OnFollowInput(worldLoc); //+ _brain._mOffset
                                                            //Debug.Log($" {worldLoc} + _mOffset={_brain._mOffset}  = {(worldLoc + _brain._mOffset)}");
                    ShowMovingPreviewIcon(moveableObject, inputPos);
                    if (slot != null) ///we are hovering over a slot 
                    {
                        UIManager.ShowPreviewMovingIcon(false, Vector3.zero, null);
                        // Debug.Log("WE are hovering over a slot!");
                        if (!slot.GetInUse())
                        {
                            ObjectController oc = moveableObject as ObjectController;
                            if (oc)
                            {
                                bool didPreview = slot.PreviewSlot(ObjectManager.Instance.GetSpriteByID((int)oc._myID));
                                ///The slot can accept this item
                                if (didPreview) //|| !slot.RequiresCertainID()
                                {
                                    IConstructable constructable = moveableObject as IConstructable;
                                    if (constructable != null)
                                    {
                                        constructable.ChangeAppearanceHidden(true);
                                        ///Show an icon of this item in the inventory:
                                        ///Enabling this helps with showing you pulled an item out of a slot,
                                        ///but then makes it less inuitive when you are putting an item over a slot
                                        ///that requires this ID.
                                        /// since this is barely noticeable under the finger on tablet, leave it off
                                        //ShowDummyPreviewSlot(constructable, inputPos);
                                        UIManager.ShowPreviewInvSlot(false, Vector3.zero, null);
                                    }
                                }
                                else
                                {
                                    ShowDummyPreviewSlot(moveableObject as IConstructable, inputPos);
                                }

                                if (slot != _lastSlot && _lastSlot != null)
                                    _lastSlot.UndoPreview();

                                _lastSlot = slot;

                            }
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
            else if (command.UP)
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
                            //Debug.Log($"Try putting it back: {_brain.ObjStartPos}");
                            var trans = moveableObject.GetGameObject().transform;
                            trans.position = _brain.ObjStartPos;
                            trans.rotation = _brain.ObjStartRot;
                            UIManager.ShowPreviewInvSlot(false, inputPos, null);
                            slot.UndoPreview();

                            ///There is a problem where if the object came from UI, it
                            /// still gets set back behind it, even with deadzone check below?
                            var placedInDeadZone = _brain.CheckRayCastForDeadZoneAtWorldPos(trans.position);
                            if(placedInDeadZone)
                            {
                                trans.position = _brain.GetCurrentWorldLocBasedOnPos(placedInDeadZone.GetSafePosition, _currentSelection);
                            }

                        }
                        ///Since its possible for the above to reset it to where it came out of UI
                        ///Do this everytime
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
                            {
                                moveableObject.SetResetOnNextChange();
                            }
                            // Debug.Log($"curr: {_currentSelection.transform.position.y} vs table {_tableHeight}");
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


        /// <summary>
        /// Shows the Icon of the picked up obj above your finger when moving an object
        /// </summary>
        private void ShowMovingPreviewIcon(IMoveable moveableObject, Vector3 inputPos)
        {
            ObjectController oc = moveableObject as ObjectController;
            if (oc)
            {
                ///TODO -decide if each obj uses same offset (cache this) or
                ///could make each item contain its own offset
                var offset = Vector3.up * 175; ///doesnt make any sense how 175 on PC is nowhere near obj, and on tablet its right above finger
                //Debug.Log($"..in={inputPos}  --> offset={inputPos + offset}");
                Sprite img = ObjectManager.Instance.GetSpriteByID((int)oc._myID);
                UIManager.ShowPreviewMovingIcon(true, inputPos + offset, img);
                UIManager.ShowPreviewInvSlot(false, inputPos, img);
            }
        }

        /// <summary>
        /// Shows the UI icon ontop of an invalid slot so user still knows what object they are carrying
        /// </summary>
        private void ShowDummyPreviewSlot(IConstructable moveableObject, Vector3 inputPos)
        {
            ObjectController oc = moveableObject as ObjectController;
            if (oc)
            {
                Sprite img = ObjectManager.Instance.GetSpriteByID((int)oc._myID);
                moveableObject.ChangeAppearanceHidden(true);
                UIManager.ShowPreviewInvSlot(true, inputPos, img);
                UIManager.ShowPreviewMovingIcon(false, inputPos, null);
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

            UIManager.ShowPreviewInvSlot(false, inputPos, null);
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
}