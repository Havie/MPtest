using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class sGameStatistics
{

    public int Defects { get; private set; } = 0;
    public int ShippedOnTime { get; private set; } = 0;
    public int ShippedLate { get; private set; } = 0;


    float _currentRoundTimeStart; /// Will correlate with when the host loaded in
    float _currentRoundEndtime;
    Queue<ItemOrder> _orders; ///For now we use a QUEUE and dont check item Type since we only have one
    float totalShippingTime = 0;
    Dictionary<int, Queue<float>> _cycleTimes;
    /************************************************************************************************************************/

    public sGameStatistics()
    {
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

    /// <summary>
    /// The avg time it takes to get an item(/batch?) through the simulation to shipping
    /// </summary>
    public float GetThroughput()
    {
        var totalShipped = GetTotalShipped();
        if(totalShipped>0)
            UIManager.DebugLog($"result= <color=green> {totalShippingTime} / {totalShipped}  </color>");
        else
            UIManager.DebugLog($"result= <color=red> {_currentRoundEndtime} / {_currentRoundTimeStart}  </color>");
        return totalShipped > 0 ? ( totalShippingTime / totalShipped) : (_currentRoundEndtime - _currentRoundTimeStart);
    }
    /// <summary>
    /// The time it takes a station to move work through (time it takes to do station task)
    /// </summary>
    public float GetCycleTimeForStation(int stationID)
    {
        bool detailedINFO = false;
        ///Could add up the endTime - the startTime  and divide by cycles?
        if (_cycleTimes.TryGetValue(stationID, out Queue<float> times))
        {
            int cycles = times.Count;
            //Debug.Log($"{stationID}:: Number of cycles<color=yellow> {cycles} </color>  end:{_currentRoundEndtime} - start:{_currentRoundTimeStart}");
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

        //Debug.Log($"!!..no Cycles for station ");
        return (_currentRoundEndtime - _currentRoundTimeStart);
    }
    /// <summary>
    /// Total # of kits not yet shipped
    /// TED: we do not count individual parts as WIP, but rather the amount of kits in the simulation
    /// </summary>
    public int GetWIP()
    {
        ///TODO - In the future, real LEAN says
        ///WIP doesnt start till kitting pushes first batch
        ///or shipping pulls an item 

        ///For now just return the # of orders since this matches the # of kits 
        return _orders.Count;
    }
    public void AddedADefect(int stationID, int itemID) { ++Defects; }
    public void CreatedAnOrder(int itemID, float createdTime, float expectedTime)
    {
        ItemOrder order = new ItemOrder(itemID, createdTime, expectedTime);
        _orders.Enqueue(order);
    }
    public void StationSentBatch(int stationID, int batchSize, bool wasShipped, float time)
    {
       //Debug.Log($"{stationID}::<color=White> Batch Sent Somewhere </color> = {wasShipped}  --> ORDERCOUNT= {_orders.Count}");
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

    /************************************************************************************************************************/

    private void ShippedAnOrder(float currTime)
    {
        ///This is gonna get tricky for debugging without full MP 
        if (_orders.Count < 1)
            return;

        ItemOrder fifoOrder = _orders.Dequeue();
        float timeToShipThisItem = currTime - fifoOrder.StartTime;
        if(timeToShipThisItem < 0)
            Debug.Log($"<color=red>negative...timeToShipThisItem = </color> {timeToShipThisItem} from CurrTime:{currTime} - {fifoOrder.StartTime}");
        totalShippingTime += timeToShipThisItem;
        //Debug.Log($"...PromisedTime = {fifoOrder.PromisedTime} vs CurrTime = {currTime}");
        if (currTime > fifoOrder.PromisedTime )
        {
            ++ShippedLate;
            return;
        }
        ++ShippedOnTime;

        ///This is kind of hacky for now, becuz we arent verifying the itemID in the batch,
        ///since we only have 1 type of item to ship in this iteration of the game
        sServerSend.OrderShipped(fifoOrder.ItemId);
    }
}
