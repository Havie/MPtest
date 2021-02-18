using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefectWrapper
{
    public readonly int StationId;
    public readonly int ItemId;

    public DefectWrapper(int stationId, int itemId)
    {
        StationId = stationId;
        ItemId = itemId;
    }
}
