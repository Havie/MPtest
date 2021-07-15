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
    /************************************************************************************************************************/
    protected override void Awake()
    {
        base.Awake();
        FindImage();
        LoadStaticColors();
    }


    public void SetInfo(int stepNumber, string label)
    {
        AssignNumberString(stepNumber);
        _myTitle = label;
    }

    private void AssignNumberString(int stepNumber)
    {
        if (stepNumber == -1) //we always want the end tab to say "Finished"
        {
            _myIndexNumber = "Finished";
        }
        else
        {
            _myIndexNumber = stepNumber.ToString();
        }
    }

    public void SetFocused(bool cond)
    {
        Debug.Log($"{this.gameObject.name} set Focused =  {cond}");
        if (_image) ///Change the color of our tab based on if were focused or not
        {
            Color bColor = cond ? _colorOn : _colorOff;
            _image .color= bColor;
        }
        SetDisplay(cond);
    }

    /************************************************************************************************************************/

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

