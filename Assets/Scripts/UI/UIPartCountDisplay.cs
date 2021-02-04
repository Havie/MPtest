using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIPartCountDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] UIInventoryManager _manager;

    [SerializeField] GameObject _optionalItemToShow;


    [SerializeField] bool showIfFull = false;
    [SerializeField] bool isDefect = false;

    void LateUpdate()
    {
        if(_manager)
        {
            if(_manager.IsInitalized)
            {
                DisableText(false);

                int current = _manager.SlotsInUse();
                int max = _manager.MaxSlots();

                if (showIfFull)
                    ShowIfFull(current, max);
                else
                    ShowDefault(current, max);

                return;
            }
        }

        DisableText(true);
    }

    public void ShowIfFull(int current, int max)
    {
        if (current != 0)
        {
            DisableText(true);
            ///Show Button
            if (_optionalItemToShow)
                _optionalItemToShow.SetActive(true);

        }
        else
        {
            UpdateText(current, max);

            ///Don't Show Button
            if (_optionalItemToShow)
                _optionalItemToShow.SetActive(false);

        }
    }

    public void ShowDefault(int current, int max)
    {
        if (current == max)
        {
            DisableText(true);
            ///Show Send Button
            if (_optionalItemToShow)
                _optionalItemToShow.SetActive(true);

        }
        else
        {
            UpdateText(current, max);
            ///Don't Show SendButton
            if (_optionalItemToShow)
                _optionalItemToShow.SetActive(false);

        }
    }

    void UpdateText(int current, int max)
    {
        if (isDefect)
            _text.text = $"{current}";
        else
            _text.text = $"{current}/{max}";
    }

    void DisableText(bool cond)
    {
        if (_text)
            _text.enabled = !cond;
    }


}
