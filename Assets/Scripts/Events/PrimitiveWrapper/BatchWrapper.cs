using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchWrapper
{
    public readonly int StationId;
    public readonly int ItemCount;

    public BatchWrapper(int stationId, int itemCount)
    {
        StationId = stationId;
        ItemCount = itemCount;
    }
}
