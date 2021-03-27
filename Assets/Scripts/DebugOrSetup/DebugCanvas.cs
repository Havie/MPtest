using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class DebugCanvas : MonoSingleton<DebugCanvas>
{
    [Header("Debugg Console")]
    [SerializeField] int _maxTextSize = 350;
    [SerializeField] Text _debugText;




    public void DebugLog(string text)
    {
        if (_debugText)
        {
            try
            {
                _debugText.text = _debugText.text + "\n" + text;

                if (_debugText.text.Length > _maxTextSize)
                    _debugText.text = _debugText.text.Substring(_debugText.text.Length - 1 - _maxTextSize);
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugText throwng exepection {e}");
            }

        }
        Debug.Log(text);
    }
    public void DebugLogWarning(string text)
    {
        if (_debugText)
        {
            try
            {
                _debugText.text = _debugText.text + "\n" + text;

                if (_debugText.text.Length > _maxTextSize)
                    _debugText.text = _debugText.text.Substring(_debugText.text.Length - 1 - _maxTextSize);
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugText throwng exepection {e}");
            }
        }
        Debug.LogWarning(text);
    }
    public void DebugLogError(string text)
    {
        try
        {
            _debugText.text = _debugText.text + "\n" + text;
            if (_debugText.text.Length > _maxTextSize)
                _debugText.text = text;
        }
        catch (Exception e)
        {
            Debug.LogError($"DebugText throwng exepection {e}");
        }
        Debug.LogError(text);
    }

    public void ClearDebugLog()
    {
        if (_debugText)
        {
            _debugText.text = "";
        }
    }
}
