#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class QualityData
{
    public int ID { get; private set; }
    public int Actions { get; private set; }

    public QualityData(int iD, int actions)
    {
        ID = iD;
        Actions = actions;
    }
}
