using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //singleton
    public static GameManager instance { get; private set; }

    #region GameVariables
    public bool _autoSend = true;
    public int _inventorySize = 50;
    public bool _isStackable = false;
    public bool _addChaotic = false;
    #endregion


    public WorkStation _workStation;
    public UIInventoryManager _invIN;
    public UIInventoryManager _invOUT;



    /**Singleton*/
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }
    /** Work station is used to identify what items are produced here and where items are sent to */
    public void AssignWorkStation(WorkStation station)
    {
        _workStation = station;
        BuildableObject.Instance.SetLevel((int)_workStation._myStation);
    }
    public void SetInventoryIn(UIInventoryManager inv) { _invIN = inv; }
    public void SetInventoryOut(UIInventoryManager inv) { _invOUT = inv; }








    #region oldTutorialStuff
    //old
    public static Dictionary<int, PlayerManager> _players = new Dictionary<int, PlayerManager>();
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
    }
    #endregion
}
