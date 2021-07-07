using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerifyInputINT : GameEventListener<IntWrapper, IntEvent, UnityIntEvent>
{
    public int _defaultValue = 2;

    private InputField _inField; ///Will find in child

    ///Have to do this as Start, otherwise gameEventListeners RegisterListeners OnEnable are too slow
    private void Start()
    {
        _inField = this.GetComponentInChildren<InputField>();
        if (_inField)
        {
            SetupMonitorAndValidator();
            AssignPreferredDefaultValue();
        }
        else
            Debug.LogWarning($"Missing InputField for {this.gameObject.name}");

    }
    /// <summary> Manually assign our listeners</summary>
    private void SetupMonitorAndValidator()
    {
        //Debug.Log($"INT: SetupMonitorAndValidator: <color=blue>{this.gameObject.name}</color>!");
        ///Ensures that only valid characters can be entered into this box  , Always set since its Something we dont have access to via the inspector
        _inField.onValidateInput += ValidateUserInputChar;
        ///Failsafe: If we forgot to assign the ref in the Inspector, assign it here
        if (_inField.onValueChanged == null)
        {
            _inField.onValueChanged.AddListener(delegate
            {
                VerifyUserInput();
            });
        }
     }

    /// <summary> Make sure our starting value meets the expected setting visually</summary>
    private void AssignPreferredDefaultValue()
    {
        //Debug.Log($"INT: AssignPreferredDefaultValue: <color=blue>{this.gameObject.name}</color>!");
        _inField.text = _defaultValue.ToString();
    }

    /// <summary> Signature which matches the UnityEvent to ensure what the user enters, is a valid char for an int</summary>
    private char ValidateUserInputChar(string input, int charIndex, char addedChar)
    {
         return ValidateINT(addedChar);
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
       // Debug.Log($"INT: VerifyUserInput: <color=blue>{this.gameObject.name}</color> => {_inField.text}");
        ///Convert to an Int and Update the GameManager 
        if (int.TryParse(_inField.text, out int val))
        {
            _gameEvent.Raise(new IntWrapper(val));
        }
        else
            Debug.Log($"Recieved invalid input:<color=red>{_inField.text} </color> from {_inField}");

    }

}
