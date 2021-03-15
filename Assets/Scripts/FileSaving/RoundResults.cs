using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace dataTracking
{
    public class RoundResults
    {
        float thruPut;
        int shippedOnTime;
        int shippedLate;
        int wip;
        Dictionary<int, float> _stationCycleTimes;

        public RoundResults(float thruPut, int shippedOnTime, int shippedLate, int wip)
        {
            this.thruPut = thruPut;
            this.shippedOnTime = shippedOnTime;
            this.shippedLate = shippedLate;
            this.wip = wip;
            _stationCycleTimes = new Dictionary<int, float>();
        }

        public void appendCycleTime(int stationId, float cycleTime)
        {

        }
    }
}
