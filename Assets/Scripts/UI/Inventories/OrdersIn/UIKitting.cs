using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIKitting : UIOrdersIn
{

    [Header("Prevent stack batch from dropping")]
    [SerializeField]  WorkStationManager _stackBatchHack = default;



    protected override bool CheckShouldDropParts()
    {
        return GameManager.instance.CurrentWorkStationManager != _stackBatchHack;
    }
}
