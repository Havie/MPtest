#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Helpers;

public class UITutorialTab : UIInGameMenuButton
{
    [SerializeField] Image _image;
    private static Color _colorOn;
    private static Color _colorOff;
    private static bool _colorsLoaded;
    private string _myIndexNumber;
    private string _myTitle;
    private int _textScaleFactor = 25; ///how much a character roughly equals in rectTransform.Width?
    private bool _isFocused = false;
    private bool _isInit = false;
    /************************************************************************************************************************/
    protected override void Awake()
    {
        base.Awake();
        FindImage();
        LoadStaticColors();
        _isInit = true;
    }

    public float Width()
    {
        float val = 0; //_rect.sizeDelta.x;
        ///This is sort of a mess, because  the rect.sizeDelta is not updating properly in time for the call
        ///So we have to estimate the width manually
        ///This is still not working as expected seems to be returning all sorts of old/wrong info
        if (val == 0)
        {
            ///It seems 1 char = 85 width, however multiple characters seems to be something like 1char = 25px. Must be some kind of base padding im not willing to spend more time on
            val = (_isFocused ? _myTitle.Length * _textScaleFactor : 85f);
        }
        //Debug.Log($"<color=red>{val}</color>{this.gameObject.name}<color=yellow>STATE:</color> _isFocused={_isFocused} _myTitle[{_myTitle}, {_myTitle.Length}]  , _myIndexNumber[{_myIndexNumber}, {_myIndexNumber.Length}]");
        return val;
    }
    public void SetInfo(int stepNumber, string label)
    {
        _myTitle = label;
        AssignNumberString(stepNumber);
    }


    public void SetFocused(bool cond)
    {
        //Debug.Log($"<color=white>{this.gameObject.name}</color> set Focused =  {cond}");
        if (!_isInit)
            Awake();

        if (_image) ///Change the color of our tab based on if were focused or not
        {
            Color bColor = cond ? _colorOn : _colorOff;
            _image.color = bColor;
        }
        SetDisplay(cond);
        _isFocused = cond;
    }

    /************************************************************************************************************************/
    private void AssignNumberString(int stepNumber)
    {
        if (stepNumber == -1) //This is a gross hack i shud fix later
        {
            _myTitle = _myIndexNumber = "Finished";
        }
        else if (stepNumber == -2)
        {
            _myTitle = _myIndexNumber = "...";
        }
        else
        {
            _myIndexNumber = stepNumber.ToString();
        }
    }
    private void SetDisplay(bool cond)
    {
        if (cond)
        {
            _buttonTxt.text = _myTitle;
        }
        else
        {
            _buttonTxt.text = _myIndexNumber;
        }
    }
    private void FindImage()
    {
        if (_image == null)
        {
            _image = this.GetComponent<Image>();
        }
    }

    private static void LoadStaticColors()
    {
        if (!_colorsLoaded)
        {
            ColorManager cm = Resources.Load<ColorManager>("ColorManager");
            if (cm)
            {
                _colorOn = cm.TutorialTabOn;
                _colorOff = cm.TutorialTabOff;
                _colorsLoaded = true;
            }
            else
                Debug.Log($"<color=red> Missing ColorManager in Resouce Folder</color>");
        }
    }

}

