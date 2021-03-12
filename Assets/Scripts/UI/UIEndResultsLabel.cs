using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEndResultsLabel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _headerTxt = default;
    [SerializeField] TextMeshProUGUI _resultTxt = default;


    public void SetResults(string statTracked, float resultInfo, bool formatAsTime)
    {
        if (_headerTxt)
            _headerTxt.text = statTracked;

        if (_resultTxt)
        {
            if (formatAsTime)
            {
                _resultTxt.text = Timer.FormatTime(resultInfo);
            }
            else
            {
                _resultTxt.text = resultInfo.ToString();
            }
        }
    }
}
