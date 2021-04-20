using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchWrapper
{
    public readonly int StationId;
    public readonly int ItemCount;
    public readonly bool IsShipped;


    ///TODO In future we need to include all items in the batch in here, and stop sending individually
    public BatchWrapper(int stationId, int itemCount, bool isShipping)
    {
        StationId = stationId;
        ItemCount = itemCount;
        IsShipped = isShipping;
    }
}
