﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerifyInputBOOL : GameEventListener<bool, BoolEvent, UnityBoolEvent>
{
    public bool _defaultValue = false;

    [SerializeField]private Toggle _inField;

    ///Have to do this as Start, otherwise gameEventListeners RegisterListeners OnEnable are too slow

    private void Start()
    {
        if (_inField == null)
        {
            _inField = this.GetComponentInChildren<Toggle>();
        }
        if (_inField)
        {
            AssignValidatorListener();
            AssignPreferredDefaultValue();
        }
        else
            Debug.LogWarning($"Missing InputField for {this.gameObject.name}");


    }
    /// <summary> Manually assign our listener if not assigned in inspector</summary>
    private void AssignValidatorListener()
    {
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
        _inField.isOn = _defaultValue;
    }

    public void VerifyUserInput()
    {
        ///Convert to a bool and Update the GameManager 
        _gameEvent.Raise(_inField.isOn);

    }

}
