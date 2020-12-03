using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sNetworkManager : MonoBehaviour
{
    public static sNetworkManager instance { get; private set; }
    public static int _defaultPort = 26951;

    public GameObject _playerPrefab;

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
        sServer.Start(6, _defaultPort); // Find unused port 
    #else
         sServer.Start(6, _defaultPort); // Find unused port 
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
