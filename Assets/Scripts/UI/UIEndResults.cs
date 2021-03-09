using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEndResults : MonoBehaviour
{
    [SerializeField] UIEndResultsLabel[] _labels = default;



    public void SetResults(float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
        _labels[0].SetResults("Cycle Time", cycleTime);
        _labels[1].SetResults("Through Put", thruPut);
        _labels[2].SetResults("Shipped: On Time", shippedOnTime);
        _labels[3].SetResults("Shipped: Late", shippedLate);
        _labels[4].SetResults("WIP", wip);
    }
}
