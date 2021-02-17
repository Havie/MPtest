using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchWrapper
{
    public readonly int StationId;
    public readonly int ItemCount;
    public readonly bool IsShipped;

    public BatchWrapper(int stationId, int itemCount, bool isShipping)
    {
        StationId = stationId;
        ItemCount = itemCount;
        IsShipped = isShipping;
    }
}
