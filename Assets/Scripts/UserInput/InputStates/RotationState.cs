using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationState : InputState
{
    float _pressTimeCURR = 0;
    float _pressTimeMAX = 0.55f; ///was 1.2f
    float _holdLeniency = 1.5f;
    Vector2 _rotationAmount;
    Vector3 _lastPos; ///prior input loc
    bool _cacheInitalPos;

    public RotationState(UserInputManager input, float holdLeniency, float pressTimeMAX)
    {
        _brain = input;
        _holdLeniency = holdLeniency;
        _pressTimeMAX = pressTimeMAX;
    }

    /************************************************************************************************************************/

    public override bool CanExitState(InputState nextState) { return true; }

    public override void DisableState()
    {

    }

    public override void EnableState(IInteractable currentSelection)
    {
        _currentSelection = currentSelection;
        _pressTimeCURR = 0;
        _rotationAmount = Vector2.zero;
        _cacheInitalPos = true;
    }

    /************************************************************************************************************************/
    public override void Execute(bool inputDown, Vector3 pos)
    {
        if (_cacheInitalPos)
        {
            _lastPos = pos;
            _cacheInitalPos = false;
        }
        CheckRotation(inputDown, pos);
    }

    /************************************************************************************************************************/


    /** Player is rotating the object in the scene or pressing and holding to begin displacement */
    private bool CheckRotation(bool inputDown, Vector3 inputPos)
    {
        IMoveable moveableObject = _currentSelection as IMoveable;

        if (inputDown && moveableObject != null)
        {

            ///if no movement increment time 
            float dis = Vector3.Distance(inputPos, _lastPos);
            var objWhereMouseIs = _brain.CheckForObjectAtLoc(inputPos); ///Prevent bug simon found
            if (dis < _holdLeniency && objWhereMouseIs == _currentSelection)
            {
                _pressTimeCURR += Time.deltaTime;

                ///Try Show Pickup Wheel
                if (_pressTimeCURR > _pressTimeMAX / 10) ///dont show this instantly 10%filled
                {
                    ///Show the UI wheel for our TouchPhase 
                    UIManager.instance.ShowTouchDisplay(_pressTimeCURR, _pressTimeMAX,
                         new Vector3(inputPos.x, inputPos.y, inputPos.z)
                         );


                    ///Cap our mats transparency fade to 0.5f
                    float changeVal = (_pressTimeMAX - _pressTimeCURR) / _pressTimeMAX;
                    changeVal = Mathf.Lerp(1, changeVal, 0.5f);
                    moveableObject.HandleInteractionTime(changeVal);

                    //Vibration.Vibrate(100); ///No haptic feedback on WiFi version of TabS5E :(
                }
            }
            else ///reset pickup timer
            {
                _pressTimeCURR = 0;
                UIManager.instance.HideTouchDisplay();
                moveableObject.HandleInteractionTime(1);
            }

            ///if holding down do displacement
            if (_pressTimeCURR >= _pressTimeMAX)
            {
                UIManager.instance.HideTouchDisplay();

                _currentSelection = _brain.CheckForObjectAtLoc(inputPos);
                IConstructable constructable = _currentSelection as IConstructable;

                if (constructable != null)
                    _currentSelection = FindAbsoluteParent(_currentSelection as ObjectController);

                moveableObject = _currentSelection as IMoveable;
                if (moveableObject != null)
                {

                    moveableObject.ChangeAppearanceMoving(); ///TODO abstract to handle inside interface
                    Transform transform = moveableObject.Transform();
                    float zCoord = _brain.WorldToScreenPoint(transform.position).z;
                    _brain._mOffset = transform.position - _brain.GetInputWorldPos(zCoord);
                    _brain._objStartPos = transform.position;
                    _brain._objStartRot = transform.rotation;

                    ///only if on table
                    // if (_currentSelection.SetOnTable())  
                    // if (_brain._currentSelection._hittingTable)
                    if (moveableObject.OutOfBounds())
                        ResetObjectOrigin(moveableObject, zCoord);

                    moveableObject.AllowFollow(); ///Might mess up objectCntroller
                    HandManager.PickUpItem(_currentSelection as ObjectController); //might have moved to the wrong spot
                }

                _brain.SwitchState(_brain._displacementState, _currentSelection);
            }
            else ///Do rotation = we're not holding
            {
                ///Store rotation amount
                Vector3 rotation = inputPos - _lastPos;
                _rotationAmount += moveableObject.OnRotate(rotation);
                _lastPos = inputPos;
                HandleHighlightPreview(moveableObject);
                return true;
            }


        }
        else /// this input UP
        {
            if (_currentSelection != null)
            {
                TryPerformAction(QualityAction.eActionType.ROTATE, inputPos, _rotationAmount);
                TryPerformAction(QualityAction.eActionType.TAP, inputPos, _rotationAmount);
                _currentSelection.OnInteract();
                if (moveableObject != null)
                    CancelHighLightPreview(moveableObject);

                UIManager.instance.HideTouchDisplay();
                _currentSelection.HandleInteractionTime(1);
            }
            _brain.SwitchState(_brain._freeState, _currentSelection);
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
        if (constructable != null)
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
            _brain._objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _tmpZfix);
            //Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_objStartPos}   _currentSelection.transform.position={_currentSelection.transform.position}");
            _brain._objStartRot = Quaternion.identity;
            _brain._mOffset = Vector3.zero;
            ///new
            var trans = moveableObject.GetGameObject().transform;
            trans.position = _brain._objStartPos;
            trans.rotation = _brain._objStartRot;
            ///Start moving the object
            moveableObject.AllowFollow(); /// so we can pick it up again
            _brain.SwitchState(_brain._displacementState, _currentSelection); ///I switched this down 1 line incase things break
        }
        else
            Debug.LogWarning("This happened?1");
    }



    #region QualityActions
    public bool TryPerformAction(QualityAction.eActionType type, Vector3 inputPos, Vector2 rotationAmount)
    {
        var objectQuality = _currentSelection.GetGameObject().GetComponent<QualityObject>();
        if (objectQuality != null)
        {
            QualityAction action = new QualityAction(type, inputPos, rotationAmount);
            if (objectQuality.PerformAction(action))
                return true;
        }

        return false;
    }

    public ObjectController FindAbsoluteParent(ObjectController startingObj)
    {
        if (startingObj == null)
        {
            Debug.LogWarning($"Somehow we are passing ina  null startingObj???");
            return null;
        }

        ObjectController parent = startingObj._parent;
        ObjectController child = startingObj;
        while (parent != null)
        {
            child = parent;
            parent = child._parent;
        }

        return child;
    }
    #endregion

}
