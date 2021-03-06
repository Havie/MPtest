﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInput
{
    public class RotationState : InputState
    {
        float _pressTimeCURR = 0;
        float _pressTimeMAX = 0.55f; ///was 1.2f
        float _holdLeniency = 15f; ///was 1.5 but was real sensitive on tablet
        Vector2 _rotationAmount;
        Vector3 _lastPos; ///prior input loc
        Vector3 _lastRotPos;
        bool _cacheInitalPos;

        public RotationState(UserInputManager input, float holdLeniency, float pressTimeMAX)
        {
            _brain = input;
            _holdLeniency = holdLeniency;
            _pressTimeMAX = pressTimeMAX;
        }

        /************************************************************************************************************************/

        public override void EnableState(IInteractable currentSelection)
        {
            _currentSelection = currentSelection;
            _pressTimeCURR = 0;
            _rotationAmount = Vector2.zero;
            _cacheInitalPos = true;
        }

        /************************************************************************************************************************/
        public override void Execute(InputCommand command)
        {
            Vector3 pos = command.Position;
            //Debug.Log($"<color=orange> In RotationStation </color> inputDown={pos}");
            if (_cacheInitalPos)
            {
                _lastRotPos = _lastPos = pos;
                _cacheInitalPos = false;
            }
            TryRotation(command, pos);
        }

        /************************************************************************************************************************/

        /** Player is rotating the object in the scene or pressing and holding to begin displacement */
        private bool TryRotation(InputCommand command, Vector3 inputPos)
        {
            IMoveable moveableObject = _currentSelection as IMoveable;
            bool inputDown = command.DOWN || command.HOLD;
            if (inputDown && moveableObject != null)
            {
                ///if no movement increment time 
                float dis = Vector3.Distance(inputPos, _lastPos);
                var objWhereMouseIs = _brain.CheckForObjectAtLoc(inputPos); ///Prevent bug simon found
                //UIManager.DebugLog($"{dis}<{_holdLeniency} == {dis < _holdLeniency} and sameObj= { objWhereMouseIs == _currentSelection} ");
                if (dis < _holdLeniency && objWhereMouseIs == _currentSelection)
                {
                    _pressTimeCURR += Time.deltaTime;

                    ShowPickupWheel(inputPos, moveableObject);
                }
                else /// not pressing on obj 
                {
                    ResetPickupTimer(inputPos, moveableObject);
                }

                ///if holding down do displacement
                if (_pressTimeCURR >= _pressTimeMAX)
                {
                    SwitchToDisplacement(inputPos);
                }
                else///Do rotation = we're not holding
                {
                    RotatePart(inputPos, moveableObject);
                    return true;
                }


            }
            else if (command.UP)
            {
                HandleExitConditionsOnMouseUp(inputPos, moveableObject);
                _brain.SwitchState(_brain._freeState, _currentSelection);
            }

            return false;

        }

        private void ResetPickupTimer(Vector3 inputPos, IMoveable moveableObject)
        {
            _pressTimeCURR = 0;
            UIManager.HideTouchDisplay();
            moveableObject.HandleInteractionTime(1);
            _lastPos = inputPos;
        }

        private void ShowPickupWheel(Vector3 inputPos, IMoveable moveableObject)
        {
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

        private void RotatePart(Vector3 inputPos, IMoveable moveableObject)
        {
            ///Prevent us from rotating not picked up items
            if (moveableObject.CanRotate())
            {
                ///Store rotation amount
                Vector3 rotation = inputPos - _lastRotPos;
                _rotationAmount += moveableObject.OnRotate(rotation);
                _lastRotPos = inputPos;
                HandleHighlightPreview(moveableObject);

                if (GameManager.Instance.IsTutorial)
                {
                    if (rotation.sqrMagnitude > 7) //5-10 works
                    {
                        TutorialEvents.CallOnPartRotated();
                    }
                }
            }
        }

        private void HandleExitConditionsOnMouseUp(Vector3 inputPos, IMoveable moveableObject)
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
        }
        /************************************************************************************************************************/

        private void SwitchToDisplacement(Vector3 inputPos)
        {
            UIManager.HideTouchDisplay();

            ///Have to do this here because OnEnableState does have inputPos
            _currentSelection = _brain.CheckForObjectAtLoc(inputPos);
            IConstructable constructable = _currentSelection as IConstructable;

            if (constructable != null)
                _currentSelection = FindAbsoluteParent(_currentSelection as ObjectController);

            _brain.SwitchState(_brain._displacementState, _currentSelection);
        }

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
}