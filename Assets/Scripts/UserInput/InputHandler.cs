using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public class InputHandler : MonoBehaviour
    {
        private bool _IsMobileMode;
        

        private UserInputManager _userInput;

        /************************************************************************************************************************/

        void Awake()
        {
            _IsMobileMode = Application.isMobilePlatform;
        }

        void Start()
        {
            _userInput = UserInputManager.Instance;
        }
        /************************************************************************************************************************/

        void Update()
        {
            if (_userInput)
            {
                _userInput.SetInputCommand(GenerateInput());
            }
        }
        /************************************************************************************************************************/

        private InputCommand GenerateInput()
        {
            bool  down = false, up = false, holding = false;
            Vector3 inputPos;

            if (_IsMobileMode)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    down = touch.phase == TouchPhase.Began;
                    up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                    holding = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                    inputPos = touch.position;
                }
                else
                {
                    inputPos = Vector3.zero; /// will this work?
                }
            }
            else
            {
                inputPos = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                up = Input.GetMouseButtonUp(0);
                holding = Input.GetMouseButton(0);
            }
            return new InputCommand(down, up, holding, inputPos);
        }

        ///OLD
        private bool CheckInput()
        {
            Vector3 _inputPos; ///current input loc
            if (!_IsMobileMode)
            {
                _inputPos = Input.mousePosition;
                return Input.GetMouseButton(0);
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    bool touching = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                    _inputPos = touch.position;
                    return touching;
                }
                else
                {
                    _inputPos = Vector3.zero; /// will this work?
                    return false;
                }
            }
        }
        /************************************************************************************************************************/

    }
}