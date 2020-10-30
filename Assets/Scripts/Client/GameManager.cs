using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    //singleton
    public static GameManager instance { get; private set; }

    #region Game Modifiers
    public bool _autoSend = true;
    public int _batchSize = 10;
    public int _inventorySize = 50; //This will have to become batch size once stations are set
    public bool _addChaotic = false;
    public bool _isStackable = false;
    public bool _workStationArrangement = false;
    public bool _workStationTaskChanging = false;
    public bool _decreaseChangeOverTime = false;
    public bool _HUDManagement = false;
    public bool _HostDefectPausing = false;
    public int _orderFrequency = 3;

    #endregion


    public WorkStation _workStation;
    public UIInventoryManager _invIN;
    public UIInventoryManager _invOUT;
    public UIInventoryManager _invSTATION;
    public UIKitting _invKITTING;



    /**Singleton*/
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);

        //Test and see if this works for all scripts?
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled=false;
#endif
    }
    /** Work station is used to identify what items are produced here and where items are sent to */
    public void AssignWorkStation(WorkStation station)
    {
        _workStation = station;
        //BuildableObject.Instance.SetLevel((int)_workStation._myStation); // better to do this elsewhere when level starts , driven from UIManager
    }
    public void SetInventoryIn(UIInventoryManager inv) { _invIN = inv; }
    public void SetInventoryOut(UIInventoryManager inv) { _invOUT = inv; }
    public void SetInventoryStation(UIInventoryManager inv) { _invSTATION = inv; }
    public void SetInventroyKitting(UIKitting inv) { _invKITTING = inv; }


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
