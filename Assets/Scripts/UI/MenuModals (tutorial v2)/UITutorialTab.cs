#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UITutorialTab : UIInGameMenuButton
{
    [SerializeField] Image _image;

    private string _myIndexNumber;
    private string _myTitle;
    /************************************************************************************************************************/
    protected override void Awake()
    {
        base.Awake();
        FindImage();
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
        if (_image)
        {
            _image.enabled = cond;
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

}

