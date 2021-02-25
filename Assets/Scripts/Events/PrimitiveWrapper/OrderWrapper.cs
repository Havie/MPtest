using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderWrapper
{
    //public readonly int StationId;
    public readonly int ItemID;
    public readonly float CreatedTime;
    public readonly float DueTime;

    public OrderWrapper( int itemID, float createdTime, float dueTime)
    {
        //StationId = stationId;
        ItemID = itemID;
        CreatedTime = createdTime;
        DueTime = dueTime;
    }
}
