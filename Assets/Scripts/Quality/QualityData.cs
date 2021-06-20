#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using UnityEngine;

[System.Serializable]
public class QualityData
{
    [SerializeField] private int _id;
    [SerializeField] private int _actions;

    public int ID => _id;
    public int Actions => _actions;

    public QualityData(int id, int actions)
    {
        _id = id;
        _actions = actions;
    }
}
