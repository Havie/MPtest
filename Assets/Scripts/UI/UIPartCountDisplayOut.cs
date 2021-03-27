using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIPartCountDisplayOut : UIPartCountDisplay
{
    ///OUT- Shows Send Button When Full , always shows text

    [SerializeField] GameObject _sendButton;

    void LateUpdate()
    {
        if (_manager)
        {
            if (_manager.IsInitalized)
            {
                DisableText(false);

                int current = _manager.SlotsInUse();
                int max = _manager.MaxSlots();

                ShowButtonIfFull(current, max);

                return;
            }
        }

        DisableText(true);
    }



    public void ShowButtonIfFull(int current, int max)
    {
        if (current == max)
        {
            DisableText(true);
            ///Show Send Button
            if (_sendButton)
                _sendButton.SetActive(true);

        }
        else
        {
            UpdateText(current, max); ///not sure?
            ///Don't Show SendButton
            if (_sendButton)
                _sendButton.SetActive(false);

        }
    }




}
