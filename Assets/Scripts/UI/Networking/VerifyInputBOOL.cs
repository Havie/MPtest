using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerifyInputBOOL : GameEventListener<bool, BoolEvent, UnityBoolEvent>
{
    public UIHostMenu _hostMenuManager;
    public GameObject _myComponent;
    public bool _defaultValue = false;

    private Toggle _inField;

    private void Awake()
    {
        _inField = _myComponent.GetComponent<Toggle>();
        if (_inField)
        {
            if (_inField.onValueChanged == null)
            {
                _inField.onValueChanged.AddListener(delegate
                {
                    VerifyUserInput();
                });
            }
            _inField.isOn = _defaultValue;
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


    public void VerifyUserInput()
    {
        //Debug.Log($"The new value is :{_inField.isOn}");

        ///Convert to an Int and Update the GameManager 
        _gameEvent.Raise(_inField.isOn);

    }

}
