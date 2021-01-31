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
                    UIManager.ShowTouchDisplay(_pressTimeCURR, _pressTimeMAX, inputPos);
    
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
                UIManager.HideTouchDisplay();
                moveableObject.HandleInteractionTime(1);
            }

            ///if holding down do displacement
            if (_pressTimeCURR >= _pressTimeMAX)
            {
                UIManager.HideTouchDisplay();

                ///Have to do this here because OnEnableState does have inputPos
                _currentSelection = _brain.CheckForObjectAtLoc(inputPos);
                IConstructable constructable = _currentSelection as IConstructable;

                if (constructable != null)
                    _currentSelection = FindAbsoluteParent(_currentSelection as ObjectController);

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
                //Debug.Log($"<color=blue> TryQualityAction</color>-->{_currentSelection} ");
                TryPerformAction(QualityAction.eActionType.ROTATE, inputPos, _rotationAmount);
                TryPerformAction(QualityAction.eActionType.TAP, inputPos, _rotationAmount);
                _currentSelection.OnInteract();
                if (moveableObject != null)
                    CancelHighLightPreview(moveableObject);

                UIManager.HideTouchDisplay();
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
        IHighlightable highlightableObj = moveableObject as IHighlightable;
        if (highlightableObj != null)
            highlightableObj.HandleHighlightPreview();


    }

    private void CancelHighLightPreview(IMoveable moveableObject)
    {
        IHighlightable highlightableObj = moveableObject as IHighlightable;
        if (highlightableObj != null)
            highlightableObj.CancelHighLightPreview();

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
