using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OverallQuality : MonoBehaviour
{
    private List<ObjectQuality> _qualities = new List<ObjectQuality>();
    public List<ObjectQuality> Qualities => _qualities;

    [SerializeField] float _lastKnownQuality; ///TMP for read outs in UI
    [SerializeField] int _currentQuality =0; /// Serialize for read outs in UI
    [SerializeField] int _maxQuality=0;  /// Serialize for read outs in UI

   

    private void Awake()
    {
       _qualities= FindQualities();


    }

    private List<ObjectQuality> FindQualities()
    {
        var qs = new List<ObjectQuality>();
        foreach (ObjectQuality q in this.GetComponentsInChildren<ObjectQuality>())
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
    public void ReadOutQuality(ObjectQuality pastObject)
    {
        Debug.Log($"We are reading out : {pastObject} its not null? {pastObject.ID}, {pastObject.CurrentQuality}/{pastObject.MaxQuality}");

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

        Debug.LogWarning($"Couldnt find {pastObject.QualityStep } #{pastObject.ID} on new item {this.gameObject.name}'s children");

        ///We never found a match so assume you can no longer perform this actions ,
        /// spoof the preview manager by adding this dummy to our list so it keeps getting passed along
        AddAsNewQuality(pastObject);
      
    }

    /** Have to add a new instance as its getting destroyed on last OBJ*/
    /** An example of this would be the blue bolts are only available on the first prefabs */
    private void AddAsNewQuality(ObjectQuality pastObject)
    {
        var qs = this.transform.gameObject.AddComponent<ObjectQuality>();
        qs.InitalizeAsDummy(pastObject.QualityStep, pastObject.CurrentQuality);
        _qualities.Add(qs);
    }

    public ObjectQuality FindObjectQualityOfType(int id)
    {
        foreach(ObjectQuality q in this.GetComponentsInChildren<ObjectQuality>())
        {
            if (q.ID == id)
                return q;
        }    

        return null;
    }

}
