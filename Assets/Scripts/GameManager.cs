using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[DefaultExecutionOrder(-1000000)] ///load early to beat UIManager 
public class GameManager : MonoSingleton<GameManager>
{

    [Header("Game Modifiers")]
    ///TODO make these all accessors with priv set:
    #region Game Modifiers
    public int _roundDuration = 10; //In SECONDS  --> 60 =1 min
    public int _orderFrequency = 3;
    public int _batchSize = 10;
    public bool _autoSend = true;
    public bool _addChaotic = false;
    public bool _isStackable = false;
    public bool _workStationArrangement = false;
    public bool _workStationTaskChanging = false;
    public bool _decreaseChangeOverTime = false;
    public bool _HUDManagement = false;
    public bool _HostDefectPausing = false;
    #endregion

    public bool RoundShouldStart { get; private set; } = false;
    public WorkStation _workStation { get; private set; }
    public UIInventoryManager _invIN { get; private set; }
    public UIInventoryManager _invOUT { get; private set; }
    public UIInventoryManager _invSTATION { get; private set; }
    public UIKitting _invKITTING { get; private set; }
    public UIShipping _invShipping { get; private set; }

    [Header("Resources")]
    public ComponentList _componentList;
    [SerializeField] WorkStationManager _batchWorkStationManager = default;
    [SerializeField] WorkStationManager _stackBatchWorkStationManager = default;
    [SerializeField] WorkStationManager _pullWorkStationManager = default;
    [SerializeField] PartAssemblyBook _assemblyBook = default;
    public WorkStationManager CurrentWorkStationManager { get; private set; }
    public PartAssemblyBook AssemblyBook => _assemblyBook;
   
    
    private bool _isMobileMode;


    protected override void Awake()
    {
        base.Awake();
        MobileSetUp();
        AutomaticChecks();
        DetermineCurrentWorkStation(); ///have to call on Awake for test scenes not run by networking UI

        //Test and see if this works for all scripts?
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled=false;
#endif
    }

    private void MobileSetUp()
    {
        _isMobileMode = Application.isMobilePlatform;
        //if (_isMobileMode)  ///let the build settings dictate
        //{
        //    Screen.orientation = ScreenOrientation.Landscape;
        //    Screen.autorotateToPortrait = false;
        //    Screen.autorotateToLandscapeRight = true;
        //    Screen.autorotateToPortraitUpsideDown = false;
        //}
    }
    private void AutomaticChecks()
    {
        if (_batchSize == 1)
        {
            _isStackable = true;
            _autoSend = true;
            _addChaotic = false;
        }
    }

    ///Needs to be called everytime the batch size changes, to keep as current as possible
    private void DetermineCurrentWorkStation()
    {
        if (_batchSize == 1)
            CurrentWorkStationManager = _pullWorkStationManager;
        else if (_isStackable)
            CurrentWorkStationManager = _stackBatchWorkStationManager;
        else
            CurrentWorkStationManager = _batchWorkStationManager;

    }
    /** Work station is used to identify what items are produced here and where items are sent to */
    public void AssignWorkStation(WorkStation station)
    {
        _workStation = station;
        UIManager.DebugLog($"assigned WS <color=green>{station}</color>");
    }

    ///Things are reliant on batchsize
    private void ValidateBatchSize(int amnt)
    {
        _batchSize = amnt;
        ValidateAutoSend();

        DetermineCurrentWorkStation();
    }
    ///We need to base auto send off batch
    private void ValidateAutoSend()
    {
        AutomaticChecks();
    }

    public void SetInventoryIn(UIInventoryManager inv) { _invIN = inv; }
    public void SetInventoryOut(UIInventoryManager inv) { _invOUT = inv; }
    public void SetInventoryStation(UIInventoryManager inv) { _invSTATION = inv; }
    public void SetInventoryKitting(UIKitting inv) { _invKITTING = inv; }
    public void SetInventoryShipping(UIShipping inv) { _invShipping = inv; }
    #region Setters for Host Changes 
    /// These Are from Button VerifyInput Events and from ClientHandle
    public void RoundDurationChanged(IntWrapper val) => RoundDurationChanged(val._value);
    public void RoundDurationChanged(int duration) { _roundDuration = duration; }
    public void OrderFreqChanged(IntWrapper val) { _orderFrequency = val._value; }
    public void BatchChanged(IntWrapper val) { ValidateBatchSize(val._value); } ///from button Events
    public void BatchChanged(int val) { ValidateBatchSize(val); } ///from ClientHandle
    public void AutoSendChanged(bool cond) { _autoSend = cond; ValidateAutoSend(); }
    public void AddChaoticChanged(bool cond) { _addChaotic = cond; }
    public void IsStackableChanged(bool cond) { _isStackable = cond; }
    public void WorkStationArrangementChanged(bool cond) { _workStationArrangement = cond; }
    public void WorkStationTaskChanged(bool cond) { _workStationTaskChanging = cond; }
    public void DecreasedChangedOverTimeChanged(bool cond) { _decreaseChangeOverTime = cond; }
    public void HUDManagementChanged(bool cond) { _HUDManagement = cond; }
    public void HostDefectPausingChanged(bool cond) { _HostDefectPausing = cond; }

    #endregion

    public void SetRoundShouldStart(bool cond)
    {
        RoundShouldStart = cond;
    }

    private void LinqTest()
    {
        //var nearestSometing = FindObjectOfType<ObjectController>()
        //.Where(t => t!=this)
        //.OrderBy(mbox => Vector3.Distance(m.transform.position, transform.position))
        //.FirstOrDefault();
    }

}
