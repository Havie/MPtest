using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public bool _IsMobileMode { get; private set; }
    public Vector3 _inputPos; ///current input loc

    public bool InputDown()
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

}
