using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEndResultsLabel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _headerTxt = default;
    [SerializeField] TextMeshProUGUI _resultTxt = default;


    public void SetResults(string statTracked, float resultInfo)
    {
        if (_headerTxt)
            _headerTxt.text = statTracked;

        if (_resultTxt)
            _resultTxt.text = resultInfo.ToString(); //TODO format
    }
}
