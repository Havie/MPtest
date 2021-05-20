
using UnityEditor;
using UnityEngine;
using System.Collections;


[RequireComponent(typeof(ObjectController))]
public class QualityObject : MonoBehaviour
{
    protected static GameObject _qualityVFXPREFAB;
    private ParticleSystem _vfx;
    [SerializeField] QualityStep _qualityStep;


    private int _currentActions;
    private float _rotationAmount;
    public bool IsDummy { get; private set; }


    public int MaxQuality => _qualityStep._requiredActions;
    public int ID => _qualityStep.Identifier;
    public int CurrentQuality => _currentActions <= MaxQuality ? _currentActions : 0;
    public int CurrentActions => _currentActions;
    public QualityStep QualityStep => _qualityStep;


    void Awake()
    {
        if (_qualityVFXPREFAB == null)
            _qualityVFXPREFAB = Resources.Load<GameObject>("Prefab/VFX/Quality_increase");

    }

    public void InitalizeAsDummy(QualityStep qs, int currentActions)
    {
        _qualityStep = qs;
        AssignCurrentActions(currentActions);
        IsDummy = true;
    }
    public void InitalizeAsDummy(int qualityStepID, int currentActions)
    {
        _qualityStep = ObjectManager.Instance._qualityPresets[qualityStepID];
        Debug.Log($"<color=red> TODO</color> Get QS via ID");
        AssignCurrentActions(currentActions);
        IsDummy = true;
    }
    public void CloneQuality(QualityObject toCopy)
    {
        // Debug.Log($"{this.gameObject.name} copying {toCopy}");
        AssignCurrentActions(toCopy.CurrentQuality);
    }
    public void CloneQuality(QualityData toCopy)
    {
        // Debug.Log($"{this.gameObject.name} copying {toCopy}");
        AssignCurrentActions(toCopy.Actions);
    }
    /// used between item creations to carry data
    public void AssignCurrentActions(int amount)
    {
        _currentActions = amount;
        //Debug.Log($"{this.gameObject.name}..Set {_qualityStep} actions to : {amount}");
    }

    public bool PerformAction(QualityAction action)
    {
        //Debug.Log($"Perform {action._actionType} on {this.gameObject.name} _isDummy={_isDummy}");
        if (IsDummy)
            return false;

        if (action._actionType == _qualityStep._qualityAction)
        {

            HandleAction(action);

            //Debug.Log(GetQuality()+ "%");
            return true;
        }

        return false;
    }

    public int GetQuality()
    {
        if (_currentActions > _qualityStep._requiredActions)///we might want to require some type of tool is equipt
            return -1;

        return (int)((((float)_currentActions / _qualityStep._requiredActions) * 100f));
    }

    private void HandleAction(QualityAction action)
    {
        //Debug.Log($"Handle Action : " + action._rotation);

        if (action._actionType == QualityAction.eActionType.ROTATE)
        {
            ///figure out the object we are ons rotation axis;
            ObjectController controller = GetComponent<ObjectController>();
            if (controller)
            {
                if (controller._rotateAroundAxis == ObjectController.eRotationAxis.XAXIS)
                {
                    _rotationAmount += action._rotation.x;
                }
                else if (controller._rotateAroundAxis == ObjectController.eRotationAxis.YAXIS)
                {
                    _rotationAmount += action._rotation.y;
                }
                else
                    Debug.LogWarning("No implementation for keeping track of both rotations at the moment, shouldnt need to be a mechanic");

                // Debug.Log($"_rotationAmount={_rotationAmount} from ( { action._rotation.x}, { action._rotation.y}) is >= {_qualityStep._requiredRotationThreshold} = { Mathf.Abs(_rotationAmount) >= _qualityStep._requiredRotationThreshold}");
                if (Mathf.Abs(_rotationAmount) >= _qualityStep._requiredRotationThreshold)
                {
                    if (_rotationAmount > 0)
                        _rotationAmount -= _qualityStep._requiredRotationThreshold; ///reset 
                    else
                        _rotationAmount += _qualityStep._requiredRotationThreshold; ///reset 

                    IncreaseQuality();
                    //   Debug.Log($"Successful rotation! reset to {_rotationAmount}");
                }

            }
            else
                Debug.LogWarning("ObjectQuality has no controller");
        }
        else if (action._actionType == QualityAction.eActionType.TAP)
        {
            IncreaseQuality();
            //Debug.Log("Successful TAP");
        }
    }
    private void PerformEffect()
    {
        var vfxIn = VFXManager.Instance;
        if (vfxIn) ///TODO cache the instance?
        {
            vfxIn.PerformEffect(_qualityVFXPREFAB, this.transform, false);
        }
        else
            Debug.Log("<color=yellow>no vfx?</color>");
    }

    private void IncreaseQuality()
    {
        //Debug.Log($"<color=green>IncreaseQuality</color>!  on {this.gameObject.name} ");
        ++_currentActions; ///Increase our quality
        PerformEffect();
    }



#if UNITY_EDITOR

    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(QualityObject))]
    public class ObjectQualityEditor : Editor
    {
        //[SerializedProperty]
        [SerializeField] QualityObject _objQ;
        SerializedProperty QualityStep;
        private void OnEnable()
        {
            _objQ = target as QualityObject;
            QualityStep = serializedObject.FindProperty(nameof(QualityObject._qualityStep));
        }

        public override void OnInspectorGUI()
        {
            //OldWay();
            NewWay();
        }

        private void NewWay()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(QualityStep);
            serializedObject.ApplyModifiedProperties();

            if (_objQ._qualityStep != null)
            {
                EditorGUILayout.LabelField("Read Only (for debugging):", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Current Actions", _objQ._currentActions.ToString());
                EditorGUILayout.LabelField("Required Actions", _objQ.MaxQuality.ToString());

                if (_objQ._qualityStep._qualityAction == QualityAction.eActionType.ROTATE)
                    EditorGUILayout.LabelField("RotationAmount", _objQ._rotationAmount.ToString());

            }
        }

        private void OldWay()
        {
            //serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.PropertyField(_test);


            _objQ._qualityStep = (QualityStep)EditorGUILayout.ObjectField("Quality Step", _objQ._qualityStep, typeof(QualityStep), true);
            ///Expose but do not make editable 
            if (_objQ._qualityStep != null)
            {
                EditorGUILayout.LabelField("Read Only (for debugging):", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Current Actions", _objQ._currentActions.ToString());
                EditorGUILayout.LabelField("Required Actions", _objQ.MaxQuality.ToString());

                if (_objQ._qualityStep._qualityAction == QualityAction.eActionType.ROTATE)
                    EditorGUILayout.LabelField("RotationAmount", _objQ._rotationAmount.ToString());

            }

            ///One way to ensure the data is serialized?
            if (EditorGUI.EndChangeCheck())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_objQ.gameObject.scene);

        }
    }



    #endregion
#endif
}




