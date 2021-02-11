using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.yougetsignal.com/tools/open-ports/

public class sNetworkManager : MonoBehaviour
{
    public static sNetworkManager instance { get; private set; }
    public static int _defaultPort = 26951;


    private int _maxPlayers = 6;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate NetworkManagers, destroying");
            Destroy(this);
        }
    }

    private void Start()
    {
    ///Is it possible this port fails? If so we need to iterate through ports and tell others
    #if UNITY_EDITOR
        sServer.Start(_maxPlayers, _defaultPort); 
#else
         sServer.Start(_maxPlayers, _defaultPort);  
#endif
    }

    void BroadCastGameManagerChanges()
    {

    }


    #region oldTutorialCode
    //public sPlayer InstantiatePlayer()
    //{
    //    return Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<sPlayer>();
    //}
    #endregion
}
