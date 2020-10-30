﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class InjectorItems : MonoBehaviour
{
    private UIInventorySlot[] _slots;


    private void Start()
    {
        _slots = this.GetComponentsInChildren<UIInventorySlot>();
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].AssignItem(i, 9999);
        }
    }
}
