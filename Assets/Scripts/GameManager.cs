﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[DefaultExecutionOrder(-1000000)] ///load early to beat UIManager 
public class GameManager : MonoSingleton<GameManager>
{
    //singleton


    [Header("Game Modifiers")]
    #region Game Modifiers
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
 


     public WorkStation _workStation { get; private set; }
     public UIInventoryManager _invIN { get; private set; }
     public UIInventoryManager _invOUT { get; private set; }
     public UIInventoryManager _invSTATION { get; private set; }
     public UIKitting _invKITTING { get; private set; }
    public UIShipping _invShipping { get; private set; }
  
    [Header("Components")]
    public ComponentList _componentList;
    [SerializeField] WorkStationManager _batchWorkStationManager = default;
    [SerializeField] WorkStationManager _pullWorkStationManager = default;

    public WorkStationManager CurrentWorkStationManager { get; private set; }

    private bool _isMobileMode;


    /**Singleton*/
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
        CurrentWorkStationManager = _batchSize > 1 ? _batchWorkStationManager : _pullWorkStationManager;
    
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
    public void SetInventoryShipping(UIShipping inv) { _invShipping=inv; }
    #region Setters for Host Changes 
    /// These Are from Button VerifyInput Events and from ClientHandle
    public void OrderFreqChanged(IntWrapper val) { _orderFrequency = val._value; }
    public void BatchChanged(IntWrapper val) { ValidateBatchSize(val._value); } ///from button Events
    public void BatchChanged(int val) { ValidateBatchSize(val); } ///from ClientHandle
    public void AutoSendChanged(bool cond) { _autoSend = cond; ValidateAutoSend(); }
    public void AddChaoticChanged(bool cond) { _addChaotic = cond; }
    public void IsStackableChanged(bool cond)  { _isStackable = cond; }
    public void WorkStationArrangementChanged(bool cond) { _workStationArrangement = cond; }
    public void WorkStationTaskChanged(bool cond) { _workStationTaskChanging = cond; }
    public void DecreasedChangedOverTimeChanged(bool cond) { _decreaseChangeOverTime = cond; }
    public void HUDManagementChanged(bool cond) { _HUDManagement = cond; }
    public void HostDefectPausingChanged(bool cond) { _HostDefectPausing = cond; }

    #endregion


    private void LinqTest()
    {
        //var nearestSometing = FindObjectOfType<ObjectController>()
        //.Where(t => t!=this)
        //.OrderBy(mbox => Vector3.Distance(m.transform.position, transform.position))
        //.FirstOrDefault();
    }





#region oldTutorialStuff
    //old
    /*public static Dictionary<int, PlayerManager> _players = new Dictionary<int, PlayerManager>();
    public GameObject _localPlayerPREFAB;
    public GameObject _playerPREFAB;
    //UNUSED for reference
    public void SpawnPlayer(int id, string username, Vector3 pos, Quaternion rot)
    {
        GameObject player;
        if (id == Client.instance._myId)
            player = Instantiate(_localPlayerPREFAB, pos, rot);
        else
            player = Instantiate(_playerPREFAB, pos, rot);

        PlayerManager pm = player.GetComponent<PlayerManager>();
        if (pm)
        {
            pm._id = id;
            pm._username = username;

            _players.Add(id, pm);
        }
    }*/
#endregion
}
