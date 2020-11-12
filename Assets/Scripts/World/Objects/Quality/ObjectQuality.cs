using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;


[RequireComponent(typeof(ObjectController))]
public class ObjectQuality : MonoBehaviour
{

    [SerializeField] int _requiredActions;
    private int _currentActions;

    [SerializeField] QualityAction.eActionType _qualityAction;
    [SerializeField] float _requiredRotationThreshold;
    private float _rotationAmount;


   public bool PerformAction(QualityAction action)
    {
        if (action._actionType == _qualityAction)
        {
         
            HandleAction(action);
            PerformEffect();

            Debug.Log(GetQuality()+ "%");
            return true;
        }

        return false;
    }

    public int GetQuality()
    {
        if (_currentActions > _requiredActions)
            return -1;

          return (int)((((float)_currentActions /_requiredActions)*100f));
    }

    private void HandleAction(QualityAction action)
    {
        if(action._actionType == QualityAction.eActionType.ROTATE)
        {
            ///figure out the object we are ons rotation axis;
            ObjectController controller = GetComponent<ObjectController>();
            if (controller)
            {
                if (controller._rotationAxis == ObjectController.eRotationAxis.XAXIS)
                {
                    _rotationAmount += action._rotation.x;
                }
                else if (controller._rotationAxis == ObjectController.eRotationAxis.YAXIS)
                {
                    _rotationAmount += action._rotation.y;
                }
                else
                    Debug.LogWarning("No implementation for keeping track of both rotations at the moment, shouldnt need to be a mechanic");
           
                if(_rotationAmount>=_requiredRotationThreshold)
                {
                    ++_currentActions; ///Increase our quality
                    _rotationAmount = _rotationAmount - _requiredRotationThreshold; ///reset 

                    Debug.Log("Successful rotation");
                }

            }
            else
                Debug.LogWarning("ObjectQuality has no controller");
        }
        else if (action._actionType == QualityAction.eActionType.TAP)
        {
            ++_currentActions; ///Increase our quality
            Debug.Log("Successful TAP");
        }
    }
    private void PerformEffect()
    {
        ///do any VFX 
    }





    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(ObjectQuality))]
    public class ObjectQualityEditor : Editor
    {
        ObjectQuality _objQ;
        private void OnEnable()
        {
            _objQ = target as ObjectQuality;
        }

        public override void OnInspectorGUI()
        {
            _objQ._currentActions = EditorGUILayout.IntField("Required Actions", _objQ._currentActions);
            _objQ._qualityAction = (QualityAction.eActionType)EditorGUILayout.EnumPopup("Quality Action Type", _objQ._qualityAction);

            switch (_objQ._qualityAction)
            {
                case QualityAction.eActionType.TAP:
                    {
                        break;
                    }
                case QualityAction.eActionType.ROTATE:
                    {
                        _objQ._requiredRotationThreshold = EditorGUILayout.FloatField("Required Rotation Threshold", _objQ._requiredRotationThreshold);
                        break;
                    }
            }
        }
    }

    #endregion
}




