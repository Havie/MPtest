using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public class DisplacementState : InputState
    {

        private IAssignable _lastSlot;
        private float _offsetFromFinger;

        public DisplacementState(UserInputManager input, float offsetFromFinger)
        {
            _brain = input;
            _offsetFromFinger = offsetFromFinger;
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

        /************************************************************************************************************************/
        public override void Execute(InputCommand command)
        {
            Vector3 pos = command.Position;
            DoDisplacement(command, pos);
        }

        /************************************************************************************************************************/

        /// <summary>
        ///  Player is moving an object to or from inventory slot
        /// </summary>
        private void DoDisplacement(InputCommand command, Vector3 inputPos)
        {
            IMoveable moveableObject = _currentSelection as IMoveable;
            if (moveableObject == null)
                return;
            IAssignable slot = _brain.RayCastForInvSlot();
            bool inputDown = command.DOWN || command.HOLD;
            if (inputDown)
            {
                MoveObject(inputPos, moveableObject, slot);
            }
            else if (command.UP)
            {
                if (slot != null)
                {
                    //Debug.Log($"FOUND UI SLOT {slot.name}");
                    if (slot.AssignItem(moveableObject as ObjectController, 1))
                    {
                        DestroyObject(moveableObject);
                    }
                    else
                    {
                        HandleDropInInvalidSlot(inputPos, moveableObject, slot);
                    }
                }
                else
                {
                     EnsureObjectIsInValidLocation(moveableObject);
                }

                //Really weird Fix to prevent raycast bug
                FixRayCastBug();
                _brain.SwitchState(_brain._freeState, _currentSelection);
            }
        }

        /// <summary>
        /// Shows the Icon of the picked up obj above your finger when moving an object
        /// </summary>
        private void ShowMovingPreviewIcon(IMoveable moveableObject, Vector3 inputPos)
        {
            ObjectController oc = moveableObject as ObjectController;
            if (oc)
            {
                var offset = Vector3.up * (Screen.height * _offsetFromFinger);
                Sprite img = ObjectManager.Instance.GetSpriteByID((int)oc._myID);
                UIManager.ShowPreviewMovingIcon(true, inputPos + offset, img);
                UIManager.ShowPreviewInvSlot(false, inputPos, img);
            }
        }

        /// <summary>
        /// Shows the UI icon ontop of a UI slot so user still knows what object they are carrying
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

        /// <summary> 
        /// This is a really weird fix I found to prevent the raycast from missing the box 
        /// </summary>
        private void FixRayCastBug()
        {
            if (_currentSelection != null)
            {
                var box = _currentSelection.GetGameObject().GetComponent<Collider>();
                box.enabled = false;
                _currentSelection = null;
                box.enabled = true;
            }
        }
       /// <summary>
       /// Changes the objects appearance to unhidden and undoes any slot previews
       /// </summary>
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
        private void MoveObject(Vector3 inputPos, IMoveable moveableObject, IAssignable slot)
        {
            //Vector3 worldLoc = _brain.GetCurrentWorldLocBasedOnMouse(moveableObject.Transform());
            Vector3 worldLoc = _brain.GetInputWorldPos(_zDepth);
            worldLoc.z = moveableObject.DesiredSceneDepth();
            moveableObject.OnFollowInput(worldLoc);
            ShowMovingPreviewIcon(moveableObject, inputPos);

            if (slot != null) ///we are hovering over a slot 
            {
                MoveOverSlot(inputPos, moveableObject, slot);
            }
            else if (PreviewManager._inPreview)
            {
                _brain.SwitchState(_brain._previewState, _currentSelection); ///dont want to reset the Object while in preview or it wont be hidden
            }
            else
            {
                ///Show the objects mesh again and undo any slot icon previews
                ResetObjectAndSlot(moveableObject as IConstructable, inputPos);
            }
        }
        private void MoveOverSlot(Vector3 inputPos, IMoveable moveableObject, IAssignable slot)
        {
            UIManager.ShowPreviewMovingIcon(false, Vector3.zero, null);
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
                            /*
                                Show an icon of this item in the inventory:
                                Enabling this helps with showing you pulled an item out of a slot,
                                but then makes it less inuitive when you are putting an item over a slot
                                that requires this ID.
                                since this is barely noticeable under the finger on tablet, leave it off
                            */
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
        private void EnsureObjectIsInValidLocation(IMoveable moveableObject)
        {
            var dz = _brain.CheckRayCastForDeadZone();
            if (dz)
            {
                HandleDeadZoneCheck(moveableObject, dz);
            }
            else
            {
                ///Check were not below the table
                if (moveableObject.OutOfBounds())
                {
                    moveableObject.SetResetOnNextChange();
                    moveableObject.ChangeAppearanceNormal(); ///ToDo abstract this somehow
                }
            }
        }
        private void HandleDropInInvalidSlot(Vector3 inputPos, IMoveable moveableObject, IAssignable slot)
        {
            Debug.Log($"<color=red>Try putting it back:</color> {_brain.ObjStartPos}");
            var trans = moveableObject.GetGameObject().transform;
            trans.position = _brain.ObjStartPos;
            trans.rotation = _brain.ObjStartRot;
            UIManager.ShowPreviewInvSlot(false, inputPos, null);
            slot.UndoPreview();

            ///There is a problem where if the object came from UI, it
            /// will return behind it, even with deadzone check below, therefore we check that pos here:
            var placedInDeadZone = _brain.CheckRayCastForDeadZoneAtWorldPos(trans.position);
            if (placedInDeadZone)
            {
                HandleDeadZoneCheck(moveableObject, placedInDeadZone);
            }
            else
            {
                ResetObjectAndSlot(moveableObject as IConstructable, inputPos);
            }    
        }
        private void HandleDeadZoneCheck(IMoveable moveableObject, UIDeadZone dz)
        {

            if (dz.TryAssignItem(moveableObject as ObjectController, 1))
            {
                DestroyObject(moveableObject);
            }
            else
            {
                ///If the item is dropped in a deadzone, reset it to a safe place
                moveableObject.GetGameObject().transform.position = _brain.GetCurrentWorldLocBasedOnPos(dz.GetSafePosition, _currentSelection);
                moveableObject.ChangeAppearanceNormal(); ///ToDo abstract this somehow
            }

        }
        private void DestroyObject(IMoveable moveableObject)
        {
            _brain.Destroy(moveableObject);
        }
    }
}