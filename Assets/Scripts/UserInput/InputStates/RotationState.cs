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
        if (_brain.InputDown() && _brain._currentSelection!=null)
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
                    _brain._currentSelection.HandleInteractionTime(changeVal);

                    //Vibration.Vibrate(100); ///No haptic feedback on WiFi version of TabS5E :(
                }
            }
            else ///reset pickup timer
            {
                _brain._pressTimeCURR = 0;
                UIManager.instance.HideTouchDisplay();
                _brain._currentSelection.HandleInteractionTime(1);
            }

            ///if holding down do displacement
            if (_brain._pressTimeCURR >= _brain._pressTimeMAX)
            {
                UIManager.instance.HideTouchDisplay();

                _brain._currentSelection = _brain.CheckForObjectAtLoc(_inputPos);
                _brain._currentSelection = _brain.FindAbsoluteParent(_brain._currentSelection as ObjectController);
                if (_brain._currentSelection !=null)
                {
   
                    _brain._currentSelection.ChangeAppearanceMoving(); ///TODO abstract to handle inside interface
                    float zCoord = _brain._mainCamera.WorldToScreenPoint(_brain._currentSelection.GetGameObject().transform.position).z;
                    _brain._mOffset = _brain._currentSelection.GetGameObject().transform.position - _brain.GetInputWorldPos(zCoord);
                    _brain._objStartPos = _brain._currentSelection.GetGameObject().transform.position;
                    _brain._objStartRot = _brain._currentSelection.GetGameObject().transform.rotation;

                    ///only if on table
                    // if (_currentSelection.SetOnTable())  
                   // if (_brain._currentSelection._hittingTable)
                   if(_brain._currentSelection.OutOfBounds())
                        ResetObjectOrigin(zCoord);

                    HandManager.PickUpItem(_brain._currentSelection as ObjectController); //might have moved to the wrong spot
                }
                _brain.SwitchState(_brain._displacementState);
            }
            else ///Do rotation
            {
                ///Store rotation amount
                Vector3 rotation = _inputPos - _brain._lastPos;
                _brain._rotationAmount += _brain._currentSelection.OnRotate(rotation);
                _brain._lastPos = _inputPos;
                HandleHighlightPreview();
                return true;
            }


        }
        else /// this input UP
        {
            if (_brain._currentSelection !=null)
            {
                _brain.TryPerformAction(QualityAction.eActionType.ROTATE);
                _brain.TryPerformAction(QualityAction.eActionType.TAP);
                _brain.CheckForSwitch();
               CancelHighLightPreview();

                UIManager.instance.HideTouchDisplay();
                _brain._currentSelection.HandleInteractionTime(1);
            }
            _brain.SwitchState(_brain._freeState);
        }

        return false;

    }

    /************************************************************************************************************************/


    private void HandleHighlightPreview()
    {
        ///if its a current item being held in hand , return
        if (_brain._currentSelection.IsPickedUp())
            return;

        ///if its not highlighting turn it on 
        if (!_brain._currentSelection.IsHighlighted())
        {
            _brain._currentSelection.SetHighlighted(true);
            _brain._currentSelection.ChangeHighlightAmount(0);
        }

        ///TODO make this take in IInteractable
        HandManager.StartToHandleIntensityChange(_brain._currentSelection as ObjectController);

    }

    private void CancelHighLightPreview()
    {
        _brain._currentSelection.SetHandPreviewingMode(false);

        if (_brain._currentSelection.IsPickedUp())
            return;

        HandManager.CancelIntensityChangePreview();

        if (_brain._currentSelection.IsHighlighted())
            _brain._currentSelection.SetHighlighted(false);


    }


    private void ResetObjectOrigin(float zCoord)
    {
        ///Reset the object to have the right orientation for construction when picked back up
        if (_brain._currentSelection !=null)
        {
            _brain._currentSelection.ChangeAppearanceMoving();///TODO abstract to handle inside interface
            Vector3 mouseLocWorld = _brain.GetInputWorldPos(zCoord);
            _brain._objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _brain._tmpZfix);
            //Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_objStartPos}   _currentSelection.transform.position={_currentSelection.transform.position}");
            _brain._objStartRot = Quaternion.identity;
            _brain._mOffset = Vector3.zero;
            ///new
            _brain._currentSelection.GetGameObject().transform.position = _brain._objStartPos;
            _brain._currentSelection.GetGameObject().transform.rotation = _brain._objStartRot;
            ///Start moving the object
            _brain._currentSelection.ResetHittingTable(); // so we can pick it up again
            _brain.SwitchState(_brain._displacementState); ///I switched this down 1 line incase things break
        }
        else
            Debug.LogWarning("This happened?1");
    }



}
