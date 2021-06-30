using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerifyInputINT : GameEventListener<IntWrapper, IntEvent, UnityIntEvent>
{
    public UIHostMenu _hostMenuManager; ///Will find in parent if null
   // public GameObject _myComponent;
    public int _defaultValue = 2;

    private InputField _inField; ///Will find in child

    private void Awake()
    {
        _inField = this.GetComponentInChildren<InputField>();
        if (_inField)
        {
            SetupMonitorAndValidator();
            AssignPreferredDefaultValue();
            SetUpHostCallBack();
        }
        else
            Debug.LogWarning($"Missing InputField for {this.gameObject.name}");

         }
    /// <summary> Make sure our starting value meets the expected setting visually</summary>
    private void AssignPreferredDefaultValue()
    {
        _inField.text = _defaultValue.ToString();
    }

    /// <summary> Manually assign our listeners</summary>
    private void SetupMonitorAndValidator()
    {
        _inField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidateINT(addedChar); };

        if (_inField.onValueChanged == null)
        {
            _inField.onValueChanged.AddListener(delegate
            {
                VerifyUserInput();
            });
        }
    }

    /// <summary> When the host wants to confirm settings, lock in our changes</summary>
    private void SetUpHostCallBack()
    {

        if (_hostMenuManager == null)
            _hostMenuManager = this.GetComponentInParent<UIHostMenu>();

        if (_hostMenuManager)
            _hostMenuManager.OnConfirmSettings += VerifyUserInput;
        else
            Debug.LogWarning($"no _hostMenuManager for {this.gameObject.name}  , will not update settings properly");

    }

    /// <summary> Ensures User input is a valid char #</summary>
    private char ValidateINT(char charToValidate)
    {
        if (!int.TryParse(charToValidate.ToString(), out int val))
        {
            /// ... if its not a match change it to an empty character.
            charToValidate = '\0';
        }
        return charToValidate;
    }


    /// <summary> Also called from button </summary>
    public void VerifyUserInput()
    {
        ///Convert to an Int and Update the GameManager 
        if (int.TryParse(_inField.text, out int val))
            _gameEvent.Raise(new IntWrapper(val));
        else
            Debug.LogWarning($"Recieved invalid input:<color=red>{_inField.text} </color> from {_inField}");

    }

}
