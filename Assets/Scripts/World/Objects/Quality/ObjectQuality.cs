using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectQuality : MonoBehaviour
{

    [SerializeField] int _requiredActions;
    private int _currentActions;

    [SerializeField] QualityAction _qualityAction;


   public bool PerformAction(QualityAction action)
    {

        if (action == _qualityAction)
        {
            ++_currentActions;
            PerformEffect();
            return true;
        }

        return false;
    }

    public int GetQuality()
    {
        if (_currentActions > _requiredActions)
            return 0;

        return (int)(_currentActions/_requiredActions);
    }


    private void PerformEffect()
    {
        ///do any VFX 
    }
}
