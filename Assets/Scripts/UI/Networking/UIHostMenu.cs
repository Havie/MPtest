using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIHostMenu : MonoBehaviour
{

    [Header("Scene Components")]
    public GameObject _connectionObjects;
    public GameObject _buttonHost;

    [SerializeField] List<GameObject> _hostOptions = new List<GameObject>();

    public GameObject _buttonCreateRoom;

    public delegate void ConfirmSettings();
    public event ConfirmSettings OnConfirmSettings;


    private void Awake()
    {
        ShowMenuOptions(false);
    }
    public void OnConnection(bool cond)
    {
        if(!cond)
        {
            HideHostCustomization();
            UIManagerNetwork.Instance.EnableHostButton(true);
        }
        UIManagerNetwork.Instance.OnConnectionResult -= OnConnection;
    }

    ///Called from Button
    public void ShowHostCustomization()
    {
        ShowMenuOptions(true);
        _connectionObjects.SetActive(false);
    }

    public void HideHostCustomization()
    {
        ShowMenuOptions(false);
        _connectionObjects.SetActive(true);
    }

    ///Called from Button
    public void CreateRoom()
    {
        UpdateGameManager();
        HostConnection();


        ///Should put this in a coroutine, that runs this after saying "..Creating Room.."
        /// Because Client was connecting before sNetworkManager ran Start 
        ShowMenuOptions(false);
        UIManagerNetwork.Instance.OnConnectionResult += OnConnection;
        UIManagerNetwork.Instance.ConnectToServer("Trying to host connection");
    }

    private void ShowMenuOptions(bool cond)
    {
        _buttonHost.SetActive(!cond);

        foreach (var item in _hostOptions)
        {
            item.SetActive(cond);
        }

        _buttonCreateRoom.SetActive(cond);
    }

    private void UpdateGameManager()
    {
        OnConfirmSettings?.Invoke();
    }



    private void HostConnection()
    {
        Client.instance.IWillBeHost = true;
        sNetworkManager.Instance.HostNetwork();
    }
}
