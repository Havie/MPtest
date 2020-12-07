
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;



[RequireComponent(typeof(ObjectController))]
public class ObjectQuality : MonoBehaviour
{
    protected static GameObject _qualityVFXPREFAB;
    private ParticleSystem _vfx;
    [SerializeField] QualityStep _qualityStep;

  
    private int _currentActions;
    private float _rotationAmount;
    private bool _isDummy;


    public int MaxQuality => _qualityStep._requiredActions;
    public int ID => _qualityStep.Identifier;
    public int CurrentQuality => _currentActions;
    public QualityStep QualityStep => _qualityStep;


    private void Awake()
    {
        if (_qualityVFXPREFAB == null)
            _qualityVFXPREFAB = Resources.Load<GameObject>("Prefab/VFX/Quality_increase");
    }

    private void OnEnable()
    {
        //Test
    }

    public void InitalizeAsDummy(QualityStep qs, int currentActions)
    {
        _qualityStep = qs;
        AssignCurrentActions(currentActions);
        _isDummy = true;
    }

    /// used between item creations to carry data
    public void AssignCurrentActions(int amount)
    {
        _currentActions = amount;
    }

    public bool PerformAction(QualityAction action)
    {
        if (_isDummy)
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

          return (int)((((float)_currentActions / _qualityStep._requiredActions) *100f));
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

                Debug.Log($"_rotationAmount={_rotationAmount} from ( { action._rotation.x}, { action._rotation.y}) is >= {_qualityStep._requiredRotationThreshold} = { Mathf.Abs(_rotationAmount) >= _qualityStep._requiredRotationThreshold}");
                if( Mathf.Abs(_rotationAmount) >= _qualityStep._requiredRotationThreshold)
                {
                    if (_rotationAmount>0)
                        _rotationAmount -= _qualityStep._requiredRotationThreshold; ///reset 
                    else
                        _rotationAmount += _qualityStep._requiredRotationThreshold; ///reset 

                    IncreaseQuality();
                    Debug.Log($"Successful rotation! reset to {_rotationAmount}");
                }

            }
            else
                Debug.LogWarning("ObjectQuality has no controller");
        }
        else if (action._actionType == QualityAction.eActionType.TAP)
        {
            IncreaseQuality();
            Debug.Log("Successful TAP");
        }
    }
    private void PerformEffect()
    {
        ///do any VFX 
        if (_vfx == null)
        {
            _vfx = GameObject.Instantiate<GameObject>(_qualityVFXPREFAB, this.transform).GetComponent<ParticleSystem>();
        }
        else
        {
            _vfx.Play();
        }
    }

    private void IncreaseQuality()
    {
        ++_currentActions; ///Increase our quality
        PerformEffect();
    }




#if UNITY_EDITOR
    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(ObjectQuality))]
    public class ObjectQualityEditor : Editor
    {
        //[SerializedProperty]
        [SerializeField] ObjectQuality _objQ;
        SerializedProperty _test;
        private void OnEnable()
        {
            _objQ = target as ObjectQuality;
            _test = serializedObject.FindProperty("QualityStep");
        }

        public override void OnInspectorGUI()
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
              
               if(_objQ._qualityStep._qualityAction==QualityAction.eActionType.ROTATE)
                    EditorGUILayout.LabelField("RotationAmount", _objQ._rotationAmount.ToString());

            }

            ///One way to ensure the data is serialized?
            if (EditorGUI.EndChangeCheck())
                EditorSceneManager.MarkSceneDirty(_objQ.gameObject.scene);

            ///serializedObject.ApplyModifiedProperties();
        }
    }

    #endregion
#endif
}




