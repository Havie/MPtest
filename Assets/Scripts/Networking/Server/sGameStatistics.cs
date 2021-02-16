using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sGameStatistics
{
    public static float ORDER_SHIPPING_DELAY = 1000f; ///TODO correlate this with kitting

    public int Defects { get; private set; } = 0;
    public int ShippedOnTime { get; private set; } = 0;
    public int ShippedLate { get; private set; } = 0;


    float _timeGameStarted;
    Queue<ItemOrder> _orders; ///For now we use a QUEUE and dont check item Type since we only have one
    float totalShippingTime = 0;
    Dictionary<int, Queue<float>> _cycleTimes;
    /************************************************************************************************************************/

    public sGameStatistics(float timeGameStarted)
    {
        _timeGameStarted = timeGameStarted;
        _orders = new Queue<ItemOrder>();
        _cycleTimes = new Dictionary<int, Queue<float>>();
    }
    /************************************************************************************************************************/


    public int GetTotalShipped() => (ShippedOnTime + ShippedLate);
    public float GetThroughput() => (totalShippingTime / GetTotalShipped());
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

    public void StationSentBatch(int stationID, int batchSize, float time)
    {

        ///Keep track of cycletime
        if (_cycleTimes.TryGetValue(stationID, out Queue<float> times))
        {
            times.Enqueue(time);
            return;
        }

        ///Creates a new Entry
        Queue<float> newTimesQueue = new Queue<float>();
        //newTimesQueue.Enqueue(_timeGameStarted); //perhaps not?
        newTimesQueue.Enqueue(time);
        _cycleTimes.Add(stationID, newTimesQueue);

    }

    public float GetCycleTimeForStation(int stationID, float endTime)
    {
        bool detailedINFO = false;
        ///Could add up the endTime - the startTime  and divide by cycles?
        int cycles = 1;
        if (_cycleTimes.TryGetValue(stationID, out Queue<float> times))
        {
            cycles = times.Count;
           
            if (detailedINFO) ///Need to play around w this later
            {
                float consequentTime = 0;
                float totalTime = 0;
                while (times.Count != 0)
                {
                    ///could Get More detailed info by subtracting each time by next time
                    float firstTime = times.Dequeue();
                    Debug.Log($"This Cycle took : {firstTime - consequentTime} seconds");
                    consequentTime = firstTime;
                    totalTime += firstTime;
                }

                Debug.Log($"Hoping these #s match: { (endTime - _timeGameStarted) / cycles}  vs { (totalTime) / cycles}");
            }
        }

        //Debug.Log($"# of cycles for station#{stationID} was : {cycles}");
        return (endTime - _timeGameStarted) / cycles;
    }


    public float GetCycleTime(WorkStation ws)
    {
        return 0;
    }

    /************************************************************************************************************************/

}
