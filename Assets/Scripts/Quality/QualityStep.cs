using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName ="Quality/Quality Step")]
public class QualityStep : ScriptableObject
{
    public int Identifier;
    public int _requiredActions;

    public QualityAction.eActionType _qualityAction;
    public float _requiredRotationThreshold;


    #if UNITY_EDITOR
    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(QualityStep))]
    public class ObjectQualityEditor : Editor
    {
        QualityStep _objQ;
        private void OnEnable()
        {
            _objQ = target as QualityStep;
        }

        public override void OnInspectorGUI()
        {
            _objQ.Identifier = EditorGUILayout.IntField("Identifier", _objQ.Identifier);
            _objQ._qualityAction = (QualityAction.eActionType)EditorGUILayout.EnumPopup("Quality Action Type", _objQ._qualityAction);

            switch (_objQ._qualityAction)
            {
                case QualityAction.eActionType.TAP:
                    {
                        _objQ._requiredActions = EditorGUILayout.IntField("Required Taps", _objQ._requiredActions);

                        break;
                    }
                case QualityAction.eActionType.ROTATE:
                    {
                        _objQ._requiredActions = EditorGUILayout.IntField("Required Rotations", _objQ._requiredActions);
                        _objQ._requiredRotationThreshold = EditorGUILayout.Slider("Required Rotation Threshold", _objQ._requiredRotationThreshold, 0f, 359f);
                      
                        break;
                    }
            }
        }
}

    #endregion

    #endif
}
