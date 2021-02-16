using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sGameStatistics
{
    public static float ORDER_SHIPPING_DELAY = 1000f; ///TODO correlate this with kitting

    public int Defects { get; private set; } = 0;
    public int ShippedOnTime { get; private set; } = 0;
    public int ShippedLate { get; private set; } = 0;


    Queue<ItemOrder> _orders; ///For now we use a QUEUE and dont check item Type since we only have one
    private float totalShippingTime = 0;

    public sGameStatistics()
    {
        _orders = new Queue<ItemOrder>();
    }

    public void AddedADefect() { ++Defects; }

    public void CreatedAnOrder(int itemID, float time)
    {
        ItemOrder order = new ItemOrder(itemID, time, time + ORDER_SHIPPING_DELAY);
        _orders.Enqueue(order);
    }

    public void ShippedAnOrder(float time)
    {
        if (_orders.Count < 1)
            return;

        ItemOrder fifoOrder = _orders.Dequeue();
        float timeToShipThisItem = time - fifoOrder.StartTime;
        totalShippingTime += timeToShipThisItem;

        if (fifoOrder.PromisedTime >= time)
        {
            ++ShippedLate;
            return;
        }
        ++ShippedOnTime;
    }

    public int GetTotalShipped() => (ShippedOnTime + ShippedLate);

    public float GetThroughput() => (totalShippingTime / GetTotalShipped());



    public float GetCycleTime(WorkStation ws)
    {
        return 0;
    }
}
