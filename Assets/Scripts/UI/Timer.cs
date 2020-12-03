using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class Timer : MonoBehaviour
{
    TextMeshProUGUI _text;
    private float _time = 90;
    private int _tLow = 60;
    private int _tBad =10;
    [SerializeField] Color _cGood;
    [SerializeField] Color _cLow;
    [SerializeField] Color _cBad;


    private bool test=true;

    private void Awake()
    {
        if (_text == null)
            _text = this.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        _text.color = _cGood;
        UpdateTime(_time);
       
    }


    private void FixedUpdate()
    {
        if (!test)
        {
            _time -= Time.fixedDeltaTime;
            UpdateTime(_time);
            CheckTimeColor();
        }
        else
        {
            _text.text=UserInput.Instance._pressTimeCURR.ToString();
        }
    }

    private void UpdateTime(float time)
    {
        if(_text && time>=0)
        {
            _text.text = FormatTime(time);
        }
    }

    private string FormatTime(float time)
    {
        string min= ((int)time / 60).ToString();
        string sec = (time%60).ToString("0");

        if (time < 60)
            min = "0";
        if (sec.Length == 1)
            sec = "0" + sec;

        return ($"{min}:{sec}");
    }

    private void CheckTimeColor()
    {
        if (_time < 0)
        {
            return;
        }
        else if (_time < _tBad)
        {
            _text.color = _cBad;
        }
        else if (_time < _tLow)
        {
            _text.color = _cLow;
        }
    }
}
