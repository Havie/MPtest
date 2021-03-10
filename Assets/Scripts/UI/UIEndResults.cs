using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEndResults : MonoBehaviour
{
    [SerializeField] UIEndResultsLabel[] _labels = default;



    public void SetResults(float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
        _labels[0].SetResults("Cycle Time", cycleTime, true);
        _labels[1].SetResults("Throughput", thruPut, true); //Was better when it returned NaN imo
        _labels[2].SetResults("Shipped: On Time", shippedOnTime, false);
        _labels[3].SetResults("Shipped: Late", shippedLate, false);
        _labels[4].SetResults("WIP", wip, false);
    }
}
