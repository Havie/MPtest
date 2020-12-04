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
            Debug.LogWarning("Duplicate Clients, destroying");
            Destroy(this);
        }
    }

    private void Start()
    {
        //ToDo increase port # if failed or use scriptable Obj 
#if UNITY_EDITOR
        sServer.Start(6, _defaultPort); // Find unused port 
#else
         sServer.Start(6, _defaultPort); // Find unused port 
#endif
    }

    public sPlayer InstantiatePlayer()
    {
        return Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<sPlayer>();
    }
}
