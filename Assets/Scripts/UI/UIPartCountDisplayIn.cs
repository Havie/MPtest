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
        if (current != 0)
        {
            DisableText(true);
            ///Show Button
            if (_dropPartsArrow)
                _dropPartsArrow.SetActive(true);

        }
        else
        {
            UpdateText(current, max); ///Not sure?
            ///Don't Show Button
            if (_dropPartsArrow)
                _dropPartsArrow.SetActive(false);

        }
    }





}
