using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIPartCountDisplayDefect : UIPartCountDisplay
{
    ///DEFECT- Shows no Object , shows text if not empty 


    void LateUpdate()
    {
        if(_manager)
        {
            if(_manager.IsInitalized)
            {
                DisableText(false);

                int current = _manager.SlotsInUse();
                int max = _manager.MaxSlots();

                ShowIfNotEmpty(current, max);

                return;
            }
        }

        DisableText(true);
    }
    public void ShowIfNotEmpty(int current, int max)
    {

        if (current == 0)
        {
            DisableText(true);

        }
        else
        {
            UpdateText(current, max); ///Not sure?
        }
    }
    protected override void UpdateText(int current, int max)
    {
       _text.text = $"{current}";
    }




}
