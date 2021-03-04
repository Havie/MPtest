using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOrder 
{
    public static int CURRENT_ORDER_NUM =1;
   
    public readonly int Id;
    public readonly int ItemId;
    public readonly float StartTime; //{ get; init; } need newer c# 5.0 ?
    public readonly float PromisedTime;
    public ItemOrder( int itemId, float startTime, float promisedTime)
    {
        Id = CURRENT_ORDER_NUM++;
        ItemId = itemId;
        StartTime = startTime;
        PromisedTime = promisedTime;
    }


}
