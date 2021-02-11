using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public abstract class UIPartCountDisplay : MonoBehaviour
{

    [SerializeField] protected TextMeshProUGUI _text;
    [SerializeField] protected UIInventoryManager _manager;

    ///IN- Shows drop Arrow when Not Empty, shows text if not empty
    ///OUT- Shows Send Button When Full , always shows text
    ///DEFECT- Shows no Object , shows text if not empty 


   protected virtual void UpdateText(int current, int max)
    {
        _text.text = $"{current}/{max}";
    }

    protected void DisableText(bool cond)
    {
        if (_text)
            _text.enabled = !cond;
    }


}
