﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[DefaultExecutionOrder(-1000000)] ///load early to beat UIManager 
public class GameManager : MonoSingleton<GameManager>
{

    [Header("Game Modifiers")]
    ///TODO make these all accessors with priv set or Serialized:
    ///Left like this because its way easier to change in the test scene for now
    #region Game Modifiers
    public int _roundDuration = 10; //In SECONDS  --> 60 =1 min
    public int _orderFrequency = 3;
    public int ExpectedDeliveryDelay = 400; //seconds
    public int _batchSize = 10;
    public bool _autoSend = true;
    public bool _addChaotic = false;
    public bool _isStackable = false;
    public bool _workStationArrangement = false;
    public bool _workStationTaskChanging = false;
    public bool _decreaseChangeOverTime = false;
    public bool _HUDManagement = false;
    public bool _HostDefectPausing = false;
    public bool StartWithWIP = false;
    public bool IsTutorial = false;
    #endregion

    public bool RoundShouldStart { get; private set; } = false;
    public WorkStation _workStation { get; private set; }

    [Header("Resources")]
    public ComponentList _componentList;
    [SerializeField] WorkStationManager _batchWorkStationManager = default;
    [SerializeField] WorkStationManager _stackBatchWorkStationManager = default;
    [SerializeField] WorkStationManager _pullWorkStationManager = default;
    [SerializeField] WorkStationManager _tutorialWorkStationManager = default;
    [SerializeField] PartAssemblyBook _assemblyBook = default;
    public PartAssemblyBook AssemblyBook => _assemblyBook;
    public WorkStationManager CurrentWorkStationManager { get; private set; }
   
    
    protected override void Awake()
    {
        base.Awake();
        AutomaticChecks();
        DetermineCurrentWorkStation(); ///have to call on Awake for test scenes not run by networking UI

        //Test and see if this works for all scripts?
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled=false;
#endif
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
        else if (!IsTutorial)
            CurrentWorkStationManager = _batchWorkStationManager;
        else
            CurrentWorkStationManager = _tutorialWorkStationManager;

    }
    public void ForceTutorialWSM()
    {
        CurrentWorkStationManager = _tutorialWorkStationManager;
    }
    /** Work station is used to identify what items are produced here and where items are sent to */
    public void AssignWorkStation(WorkStation station)
    {
        _workStation = station;
        //UIManager.DebugLog($"assigned WS <color=green>{station}</color>");
    }

    ///Things are reliant on batchsize
    private void ValidateBatchSize(int amnt)
    {
        //Debug.Log($"<color=orange>GM BATCH CHANGED TO:</color> {amnt}");
        _batchSize = amnt;
        ValidateAutoSend();
        DetermineCurrentWorkStation();
     }
    ///We need to base auto send off batch
    private void ValidateAutoSend()
    {
        AutomaticChecks();
    }


    #region Setters for Host Changes 
    /// These Are from Button VerifyInput Events and from ClientHandle
    public void RoundDurationChanged(IntWrapper val) => RoundDurationChanged(val._value);
    public void RoundDurationChanged(int duration) { _roundDuration = duration; }
    public void OrderFreqChanged(IntWrapper val) { _orderFrequency = val._value; }
    public void BatchChanged(IntWrapper val) { ValidateBatchSize(val._value); } ///from button Events
    public void DeliveryTimeChanged(IntWrapper val) { ExpectedDeliveryDelay = val._value; }
    public void BatchChanged(int val) { ValidateBatchSize(val); } ///from ClientHandle
    public void AutoSendChanged(bool cond) { _autoSend = cond; ValidateAutoSend(); }
    public void AddChaoticChanged(bool cond) { _addChaotic = cond; }
    public void IsStackableChanged(bool cond) {_isStackable = cond; }
    public void WorkStationArrangementChanged(bool cond) { _workStationArrangement = cond; }
    public void WorkStationTaskChanged(bool cond) { _workStationTaskChanging = cond; }
    public void DecreasedChangedOverTimeChanged(bool cond) { _decreaseChangeOverTime = cond; }
    public void HUDManagementChanged(bool cond) { _HUDManagement = cond; }
    public void HostDefectPausingChanged(bool cond) { _HostDefectPausing = cond; }
    public void StartWithWIPChanged(bool cond)  { StartWithWIP = cond; }
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
