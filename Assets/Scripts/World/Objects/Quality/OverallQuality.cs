using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OverallQuality : MonoBehaviour
{
    public List<ObjectQuality> _qualities = new List<ObjectQuality>();
    [SerializeField] float _lastKnownQuality; ///TMP for read outs in UI

    [SerializeField] int _currentQuality =0;
    [SerializeField] int _maxQuality=0;

    private void Awake()
    {
        CacheMaxQuality();

    }


    private void LateUpdate()
    {
        GetPercent();///TMP for read outs in UI
    }
    private void CacheMaxQuality()
    {
        foreach (var q in _qualities)
        {
            IncrementQuality(0, q.MaxQuality);
        }
    }

    public void ChangeQuality(int curr, int max)
    {
        _currentQuality = curr;
        _maxQuality = max;
    }

    public void IncrementQuality(int currAdditive, int maxAdditive)
    {
        _currentQuality += currAdditive;
        _maxQuality += maxAdditive;
    }

    public float GetPercent()
    {
        _lastKnownQuality = (float)_currentQuality / (float)_maxQuality;   ///TMP for read outs in UI
        return (float)_currentQuality / (float)_maxQuality;
    }

    /**Update our current quality */
    public void ReadOutQuality(ObjectQuality pastObject)
    {
        Debug.Log($"We aer reading out : {pastObject} its not null? {pastObject.ID}, {pastObject.CurrentQuality}/{pastObject.MaxQuality}");

        foreach (var item in _qualities)
        {
            if(item.ID == pastObject.ID) ///gets the ID from shared scriptable asset
            {
                if(pastObject.CurrentQuality != item.MaxQuality)
                {
                    item.AssignCurrentActions(pastObject.CurrentQuality);
                    if(pastObject.CurrentQuality <= item.MaxQuality)
                        IncrementQuality(pastObject.CurrentQuality, 0); ///keep track of where we are at along the way
                                                                        ///else we leave it alone at 0, the item is a defect we dont want to count it
                    return;
                }
               /* else
                {
                    IncrementQuality(pastObject.CurrentActions, 0);
                    RemoveFinishedQuality(item);
                    return;
                }
               */
            }
        }

        ///We never found a match so assume you can no longer perform this actions and lets increment ourselves:
        IncrementQuality(pastObject.CurrentQuality, pastObject.MaxQuality);
        /// spoof the preview manager by adding this dummy to our list so it keeps getting passed along
        AddAsNewQuality(pastObject);
      
    }

    /** Have to add a new instance as its getting destroyed on last OBJ*/
    private void AddAsNewQuality(ObjectQuality pastObject)
    {
       var qs= this.transform.gameObject.AddComponent<ObjectQuality>();
        qs.InitalizeAsDummy(pastObject.QualityStep, pastObject.CurrentQuality);

        _qualities.Add(qs);
    }

}
