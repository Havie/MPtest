using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QualityOverall : MonoBehaviour
{
    public List<QualityObject> _qualities = new List<QualityObject>();
    public List<QualityObject> Qualities => _qualities;

    [SerializeField] float _lastKnownQuality; ///TMP for read outs in UI
    [SerializeField] int _currentQuality =0; /// Serialize for read outs in UI
    [SerializeField] int _maxQuality=0;  /// Serialize for read outs in UI


    private void Awake()
    {
       _qualities= FindQualities();


    }

    private List<QualityObject> FindQualities()
    {
        var qs = new List<QualityObject>();
        foreach (QualityObject q in this.GetComponentsInChildren<QualityObject>())
            qs.Add(q);
        return qs;
    }

    private void LateUpdate()
    {
        _lastKnownQuality= GetPercent();///TMP for read outs in UI
    }

    public void ChangeQuality(int curr, int max)
    {
        _currentQuality = curr;
        _maxQuality = max;
    }


    public float GetPercent()
    {
        _currentQuality = 0;
        _maxQuality = 0;
        foreach (var q in _qualities)
        {
            _currentQuality += q.CurrentQuality;
            _maxQuality += q.MaxQuality;
        }
        _lastKnownQuality = (float)_currentQuality / (float)_maxQuality;   ///TMP for read outs in UI
        return (float)_currentQuality / (float)_maxQuality;
    }

    /**Update our current quality */
    public void ReadOutQuality(QualityObject pastObject)
    {
        //Debug.Log($"We are reading out : {pastObject} its not null? {pastObject.ID}, {pastObject.CurrentQuality}/{pastObject.MaxQuality}");

        foreach (var item in _qualities)
        {
            if(item.ID == pastObject.ID) ///gets the ID from shared scriptable asset
            {
                if (pastObject.CurrentQuality != item.MaxQuality)
                {
                    item.CloneQuality(pastObject);
                    return;
                }
            }
        }

        ///This if fine, just a warning, it will keep track of it on the base obj
       // Debug.LogWarning($"Couldnt find {pastObject.QualityStep } #{pastObject.ID} on new item {this.gameObject.name}'s children");

        ///We never found a match so assume you can no longer perform this actions ,
        /// spoof the preview manager by adding this dummy to our list so it keeps getting passed along
        AddAsNewQuality(pastObject);
      
    }
    public void ReadOutQuality(QualityData pastObject)
    {
        //Debug.Log($"We are reading out : {pastObject} its not null? {pastObject.ID}, {pastObject.CurrentQuality}/{pastObject.MaxQuality}");

        foreach (var item in _qualities)
        {
            if (item.ID == pastObject.ID) ///gets the ID from shared scriptable asset
            {
                if (pastObject.Actions != item.MaxQuality)
                {
                    item.CloneQuality(pastObject);
                    return;
                }
            }
        }

        ///This if fine, just a warning, it will keep track of it on the base obj
       // Debug.LogWarning($"Couldnt find {pastObject.QualityStep } #{pastObject.ID} on new item {this.gameObject.name}'s children");

        ///We never found a match so assume you can no longer perform this actions ,
        /// spoof the preview manager by adding this dummy to our list so it keeps getting passed along
        AddAsNewQuality(pastObject);

    }


    /** Have to add a new instance as its getting destroyed on last OBJ*/
    /** An example of this would be the blue bolts are only available on the first prefabs */
    private void AddAsNewQuality(QualityObject pastObject)
    {
        var qs = this.transform.gameObject.AddComponent<QualityObject>();
        qs.InitalizeAsDummy(pastObject.QualityStep, pastObject.CurrentQuality);
        _qualities.Add(qs);
    }
    private void AddAsNewQuality(QualityData pastObject)
    {
        var qs = this.transform.gameObject.AddComponent<QualityObject>();
        qs.InitalizeAsDummy(pastObject.ID, pastObject.Actions);
        _qualities.Add(qs);
    }

    public QualityObject FindObjectQualityOfType(int id)
    {
        foreach(QualityObject q in this.GetComponentsInChildren<QualityObject>())
        {
            if (q.ID == id)
                return q;
        }    

        return null;
    }

}
