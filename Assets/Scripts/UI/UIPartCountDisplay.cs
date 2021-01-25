using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIPartCountDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] UIInventoryManager _manager;

    [SerializeField] GameObject _optionalItemToShow;



    void LateUpdate()
    {
        if(_text && _manager)
        {
            if(_manager.IsInitalized)
            {
                DisableText(false);

                var current = _manager.SlotsInUse();
                var max = _manager.MaxSlots();
                if (current == max)
                {
                    _text.enabled = false;
                    ///Show Send Button
                    if (_optionalItemToShow)
                        _optionalItemToShow.SetActive(true);

                }
                else
                {
                    _text.enabled = true;
                    _text.text = $"{current}/{max}";
                    ///Don't Show SendButton
                    if (_optionalItemToShow)
                     _optionalItemToShow.SetActive(false);
 
                }
                return;
            }
        }

        DisableText(true);
    }

    void DisableText(bool cond)
    {
        if (_text)
            _text.enabled = !cond;
    }


}
