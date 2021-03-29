using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIPartCountDisplayIn : UIPartCountDisplay
{
    ///IN- Shows drop Arrow when Not Empty, shows text if not empty

    [SerializeField] GameObject _dropPartsArrow;

    void LateUpdate()
    {
        if (_manager)
        {
            if (_manager.IsInitalized)
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
        UpdateText(current, max);
        if (current == 0)
        {
            DisableText(true);

        }
    }

    public void ShowIfPartArrowIfNotEmpty(int current, int max)
    {
        if (current != 0)
        {
            DisableText(true);
            ///Show Button
            ShowDropPartsArrow(true);
        }
        else
        {
            ShowDropPartsArrow(false);
            UpdateText(current, max);
        }
    }

    private void ShowDropPartsArrow(bool cond)
    {
        if (_dropPartsArrow)
        {
            ///Off since feedback from UX and users always abusing dropping parts into
            ///scene when it wasn't advantageous to them
           // _dropPartsArrow.SetActive(cond);
        }
    }





}
