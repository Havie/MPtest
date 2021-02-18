using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sGameStatistics
{

    public int Defects { get; private set; } = 0;
    public int ShippedOnTime { get; private set; } = 0;
    public int ShippedLate { get; private set; } = 0;


    float _timeGameStarted; ///TODO correlate this properly w actual start, not station select?
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
    public void AddedADefect(int stationID, int itemID) { ++Defects; }
    public void CreatedAnOrder(int itemID, float createdTime, float expectedTime)
    {
        ItemOrder order = new ItemOrder(itemID, createdTime, expectedTime);
        _orders.Enqueue(order);
    }
    public void StationSentBatch(int stationID, int batchSize, bool wasShipped, float time)
    {
        if (wasShipped)
            ShippedAnOrder(time);

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

    public int GetWIP()
    {
        ///TODO can query the final items in the _orders and add up their req Items
        return 0;
    }
    /************************************************************************************************************************/

    private void ShippedAnOrder(float time)
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
}
