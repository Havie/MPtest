using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public class InputHandler : MonoBehaviour
    {

        [SerializeField] Texture2D _customMouseCursor;
        private bool _IsMobileMode;
        private UserInputManager _userInput;

        /************************************************************************************************************************/

        void Awake() { _IsMobileMode = Application.isMobilePlatform; }

        void Start()
        { 
            _userInput = UserInputManager.Instance;
            Cursor.SetCursor(_customMouseCursor, Vector2.zero, CursorMode.Auto);
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
            Vector3 inputPos = Vector3.zero;

            if (_IsMobileMode)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    down = touch.phase == TouchPhase.Began;
                    up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                    holding = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                    inputPos = touch.position;
                    if(Input.touchCount>1)
                        UIManager.DebugLog($"count={Input.touchCount} ..down={down} , up={up}, holding={holding}");
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

        /************************************************************************************************************************/

    }
}