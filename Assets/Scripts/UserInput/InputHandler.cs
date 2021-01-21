using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private bool _IsMobileMode;
    private Vector3 _inputPos; ///current input loc

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
            _userInput.SetInputDown(CheckInput(), _inputPos);
    }

    public bool CheckInput()
    {
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
