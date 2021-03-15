using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace dataTracking
{
    public class RoundResults
    {
        public int DataSizeBuffer { get; private set; } = 4;
        public readonly float ThruPut;
        public readonly int ShippedOnTime;
        public readonly int ShippedLate;
        public readonly int Wip;
        Dictionary<int, float> _stationCycleTimes;

        public RoundResults(float thruPut, int shippedOnTime, int shippedLate, int wip)
        {
            ThruPut = thruPut;
            ShippedOnTime = shippedOnTime;
            ShippedLate = shippedLate;
            Wip = wip;
            _stationCycleTimes = new Dictionary<int, float>();
        }

        public void SetCycleTime(int stationId, float cycleTime)
        {
            if (stationId == 0)
                return; ///Zero means one of the clients was never assigned a stationID (could be host?)

            if(_stationCycleTimes.ContainsKey(stationId))
            {
                //Replace it
                _stationCycleTimes[stationId] = cycleTime;
            }
            else
            {
                //Add New
                _stationCycleTimes.Add(stationId, cycleTime);
                ++DataSizeBuffer;
            }
        }

        public List<KeyValuePair<int, float>> GetStationCycleTimes()
        {
            ///We want to return a copy, so the user of this class doesnt modify our data outside
            var pairList = new List<KeyValuePair<int, float>>();
            foreach (var item in _stationCycleTimes)
            {
                pairList.Add(item);
            }
            ///IDK?- the order is kind of weird, need to test further:
            pairList.Reverse();
            return pairList;
        }
    }
}
