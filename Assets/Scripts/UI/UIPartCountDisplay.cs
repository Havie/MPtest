using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPartCountDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] UIInventoryManager _manager;

    void Awake()
    {

    }

    void LateUpdate()
    {
        if(_text && _manager)
        {
            if(_manager.IsInitalized)
            {
                DisableText(false);
                _text.text = $"{_manager.SlotsInUse()}/{_manager.MaxSlots()} ";
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
