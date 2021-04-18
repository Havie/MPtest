using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sGameStatistics
{

    public int Defects { get; private set; } = 0;
    public int ShippedOnTime { get; private set; } = 0;
    public int ShippedLate { get; private set; } = 0;


    float _timeServerStarted; ///Just incase I wana do something w this?
    float _currentRoundTimeStart; /// Will correlate with when the host loaded in
    float _currentRoundEndtime;
    Queue<ItemOrder> _orders; ///For now we use a QUEUE and dont check item Type since we only have one
    float totalShippingTime = 0;
    Dictionary<int, Queue<float>> _cycleTimes;
    /************************************************************************************************************************/

    public sGameStatistics(float timeServerStarted)
    {
        _timeServerStarted = timeServerStarted;
        _orders = new Queue<ItemOrder>();
        _cycleTimes = new Dictionary<int, Queue<float>>();
    }
    /************************************************************************************************************************/
    public void RoundBegin(float startTime)
    {
        _currentRoundTimeStart = startTime;
    }
    public void RoundEnded(float endTime)
    {
        ///Not sure, TODO
        _currentRoundEndtime = endTime;
    }

    public int GetTotalShipped() => (ShippedOnTime + ShippedLate);
    public int GetShippedOnTime() => ShippedOnTime;
    public int GetShippedLate() => ShippedLate;

    public float GetThroughput()
    {
        var totalShipped = GetTotalShipped();

        return totalShipped > 0 ? ( totalShippingTime / totalShipped) : (_currentRoundEndtime - _currentRoundTimeStart);
    }
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
    public float GetCycleTimeForStation(int stationID)
    {
        bool detailedINFO = false;
        ///Could add up the endTime - the startTime  and divide by cycles?
        if (_cycleTimes.TryGetValue(stationID, out Queue<float> times))
        {
            int cycles = times.Count;
           
            if (detailedINFO) ///Need to play around w this later
            {
                float consequentTime = 0;
                float totalTime = 0;
                while (times.Count != 0)
                {
                    ///could Get More detailed info by subtracting each time by next time
                    float firstTime = times.Dequeue();
                    //Debug.Log($"This Cycle took : {firstTime - consequentTime} seconds");
                    consequentTime = firstTime;
                    totalTime += firstTime;
                }

                Debug.Log($"Hoping these #s match: { (_currentRoundEndtime - _currentRoundTimeStart) / cycles}  vs { (totalTime) / cycles}");
            }
            return (_currentRoundEndtime - _currentRoundTimeStart) / cycles;
        }

        Debug.Log($"no Cycles for station ");
        return (_currentRoundEndtime - _currentRoundTimeStart);
    }
    public int GetWIP()
    {
        ///TED: we do not count individual parts as WIP, but rather the amount of kits
        /// in the simulation

        ///OLD and wrong:
        //var gm = GameManager.instance;
        //int batchSize = gm._batchSize; ///If we change batch sizes mid game, move Batch inside CreatedOrder wrapper
        //var config = gm.AssemblyBook;
        //int totalWip = 0;
        //foreach (var order in _orders)
        //{
        //    totalWip += config.GetRequiredComponentsForPart(order.ItemId).Count * batchSize;
        //}


        ///TODO - In the future, real LEAN says
        ///WIP doesnt start till kitting pushes first batch
        ///or shipping pulls an item 
        
        ///For now just return the # of orders
        return _orders.Count;
    }

    /************************************************************************************************************************/

    private void ShippedAnOrder(float time)
    {
        ///This is gonna get tricky for debugging without full MP 
        if (_orders.Count < 1)
            return;

        ItemOrder fifoOrder = _orders.Dequeue();
        float timeToShipThisItem = time - fifoOrder.StartTime;
        totalShippingTime += timeToShipThisItem;

        if (fifoOrder.PromisedTime > time)
        {
            ++ShippedLate;
            return;
        }
        ++ShippedOnTime;
    }
}
