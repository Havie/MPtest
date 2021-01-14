using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationState : InputState
{
    public RotationState(UserInput input)
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
        CheckRotation();
    }

    /************************************************************************************************************************/


    /** Player is rotating the object in the scene or pressing and holding to begin displacement */
    private bool CheckRotation()
    {
        IMoveable moveableObject = _brain._currentSelection as IMoveable;

        if (_brain.InputDown() && moveableObject != null)
        {
            var _inputPos = _brain._inputPos;


            ///if no movement increment time 
            float dis = Vector3.Distance(_inputPos, _brain._lastPos);
            var objWhereMouseIs = _brain.CheckForObjectAtLoc(_inputPos); ///Prevent bug simon found
            if (dis < _brain._holdLeniency && objWhereMouseIs == _brain._currentSelection)
            {
                _brain._pressTimeCURR += Time.deltaTime;

                ///Try Show Pickup Wheel
                if (_brain._pressTimeCURR > _brain._pressTimeMAX / 10) ///dont show this instantly 10%filled
                {
                    ///Show the UI wheel for our TouchPhase 
                    UIManager.instance.ShowTouchDisplay(_brain._pressTimeCURR, _brain._pressTimeMAX,
                         new Vector3(_inputPos.x, _inputPos.y, _inputPos.z)
                         );


                    ///Cap our mats transparency fade to 0.5f
                    float changeVal = (_brain._pressTimeMAX - _brain._pressTimeCURR) / _brain._pressTimeMAX;
                    changeVal = Mathf.Lerp(1, changeVal, 0.5f);
                    moveableObject.HandleInteractionTime(changeVal);

                    //Vibration.Vibrate(100); ///No haptic feedback on WiFi version of TabS5E :(
                }
            }
            else ///reset pickup timer
            {
                _brain._pressTimeCURR = 0;
                UIManager.instance.HideTouchDisplay();
                moveableObject.HandleInteractionTime(1);
            }

            ///if holding down do displacement
            if (_brain._pressTimeCURR >= _brain._pressTimeMAX)
            {
                UIManager.instance.HideTouchDisplay();

                _brain._currentSelection = _brain.CheckForObjectAtLoc(_inputPos);
                IConstructable constructable = _brain._currentSelection as IConstructable;
                if(constructable!=null)
                    _brain._currentSelection = _brain.FindAbsoluteParent(_brain._currentSelection as ObjectController);
                moveableObject = _brain._currentSelection as IMoveable;
                if (moveableObject != null)
                {

                    moveableObject.ChangeAppearanceMoving(); ///TODO abstract to handle inside interface
                    Transform transform = moveableObject.Transform();
                    float zCoord = _brain._mainCamera.WorldToScreenPoint(transform.position).z;
                    _brain._mOffset = transform.position - _brain.GetInputWorldPos(zCoord);
                    _brain._objStartPos = transform.position;
                    _brain._objStartRot = transform.rotation;

                    ///only if on table
                    // if (_currentSelection.SetOnTable())  
                    // if (_brain._currentSelection._hittingTable)
                    if (moveableObject.OutOfBounds())
                        ResetObjectOrigin(moveableObject, zCoord);

                    moveableObject.AllowFollow(); ///Might mess up objectCntroller
                    HandManager.PickUpItem(_brain._currentSelection as ObjectController); //might have moved to the wrong spot
                }

                _brain.SwitchState(_brain._displacementState);
            }
            else ///Do rotation
            {
                ///Store rotation amount
                Vector3 rotation = _inputPos - _brain._lastPos;
                _brain._rotationAmount += moveableObject.OnRotate(rotation);
                _brain._lastPos = _inputPos;
                HandleHighlightPreview(moveableObject);
                return true;
            }


        }
        else /// this input UP
        {
            if (_brain._currentSelection != null)
            {
                _brain.TryPerformAction(QualityAction.eActionType.ROTATE);
                _brain.TryPerformAction(QualityAction.eActionType.TAP);
                _brain._currentSelection.OnInteract();
                if (moveableObject != null)
                    CancelHighLightPreview(moveableObject);

                UIManager.instance.HideTouchDisplay();
                _brain._currentSelection.HandleInteractionTime(1);
            }
            _brain.SwitchState(_brain._freeState);
        }

        return false;

    }

    /************************************************************************************************************************/


    private void HandleHighlightPreview(IMoveable moveableObject)
    {
        ///if its a current item being held in hand , return
        if (moveableObject.IsPickedUp())
            return;

        ///if its not highlighting turn it on 
        if (!moveableObject.IsHighlighted())
        {
            moveableObject.SetHighlighted(true);
            moveableObject.ChangeHighlightAmount(0);
        }

        ///TODO make this take in IInteractable
        HandManager.StartToHandleIntensityChange(moveableObject as ObjectController);

    }

    private void CancelHighLightPreview(IMoveable moveableObject)
    {
        IConstructable constructable = moveableObject as IConstructable; ///This is a mess
        if(constructable!=null)
            constructable.SetHandPreviewingMode(false);

        if (moveableObject.IsPickedUp())
            return;

        HandManager.CancelIntensityChangePreview();

        if (moveableObject.IsHighlighted())
            moveableObject.SetHighlighted(false);


    }


    private void ResetObjectOrigin(IMoveable moveableObject, float zCoord)
    {
        ///Reset the object to have the right orientation for construction when picked back up
        if (moveableObject != null)
        {
            moveableObject.ChangeAppearanceMoving();///TODO abstract to handle inside interface
            Vector3 mouseLocWorld = _brain.GetInputWorldPos(zCoord);
            _brain._objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _brain._tmpZfix);
            //Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_objStartPos}   _currentSelection.transform.position={_currentSelection.transform.position}");
            _brain._objStartRot = Quaternion.identity;
            _brain._mOffset = Vector3.zero;
            ///new
            var trans = moveableObject.GetGameObject().transform;
            trans.position = _brain._objStartPos;
            trans.rotation = _brain._objStartRot;
            ///Start moving the object
            moveableObject.AllowFollow(); /// so we can pick it up again
            _brain.SwitchState(_brain._displacementState); ///I switched this down 1 line incase things break
        }
        else
            Debug.LogWarning("This happened?1");
    }



}
