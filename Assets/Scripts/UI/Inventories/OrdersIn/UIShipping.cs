using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShipping : UIOrdersIn
{
    protected override bool CheckShouldDropParts()
    {
        return false;
    }

}
