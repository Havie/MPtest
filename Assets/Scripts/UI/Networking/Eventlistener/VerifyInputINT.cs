using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerifyInputINT : GameEventListener<IntWrapper, IntEvent, UnityIntEvent>
{
    public UIHostMenu _hostMenuManager;
    public GameObject _myComponent;
    public int _defaultValue = 2;

    private InputField _inField;

    private void Awake()
    {
        _inField = _myComponent.GetComponent<InputField>();
        if (_inField)
        {
             _inField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidateINT(addedChar); };

            if (_inField.onValueChanged == null)
            {
                _inField.onValueChanged.AddListener(delegate {
                    VerifyUserInput();
                });
            }
            _inField.text = _defaultValue.ToString();
            SetUpHostCallBack();
        }
        else
            Debug.LogWarning($"Missing InputField for {this.gameObject.name}");

         }

    private void SetUpHostCallBack()
    {

        if (_hostMenuManager == null)
            _hostMenuManager = this.GetComponentInParent<UIHostMenu>();

        if (_hostMenuManager)
            _hostMenuManager.OnConfirmSettings += VerifyUserInput;
        else
            Debug.LogWarning($"no _hostMenuManager for {this.gameObject.name}  , will not update settings properly");

    }


    private char ValidateINT(char charToValidate)
    {
        ///Checks if input is an #
        int val;
        if (!int.TryParse(charToValidate.ToString(), out val))
        {
            /// ... if its not a match change it to an empty character.
            charToValidate = '\0';
        }
        return charToValidate;
    }





    public void VerifyUserInput()
    {
        ///Convert to an Int and Update the GameManager 
        if (int.TryParse(_inField.text, out int val))
            _gameEvent.Raise(new IntWrapper(val));
        else
            Debug.LogWarning($"Recieved invalid input:<color=red>{_inField.text} </color> from {_inField}");

    }

}
