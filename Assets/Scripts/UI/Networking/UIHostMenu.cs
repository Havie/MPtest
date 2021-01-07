using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHostMenu : MonoBehaviour
{
    [Header("Project Assets")]
    [SerializeField] GameObject _NetworkManagerPREFAB= default;

    [Header("Scene Components")]
    public GameObject _connectionObjects;
    public GameObject _buttonHost;

    //public GameObject _orderFrequency;
    //public GameObject _batchSize;
    //public GameObject _autoSend;
    //public GameObject _addChaotic;
    //public GameObject _isStackable;
    //public GameObject _workStationArrangement;
    //public GameObject _workStationTaskChanging;
    //public GameObject _decreaseChangeOverTime;
    //public GameObject _HUDManagement;
    //public GameObject _HostDefectPausing;

    [SerializeField] List<GameObject> _hostOptions = new List<GameObject>();

    public GameObject _buttonCreateRoom;

    public delegate void ConfirmSettings ();
    public event ConfirmSettings OnConfirmSettings;


    private void Awake()
    {
        ShowMenuOptions(false);
    }

    ///Called from Button
    public void ShowHostCustomization()
    {
        ShowMenuOptions(true);
        _connectionObjects.SetActive(false);
    }

    ///Called from Button
    public void CreateRoom()
    {
        UpdateGameManager();
        HostConnection();


        ///Should put this in a coroutine, that runs this after saying "..Creating Room.."
        /// Because Client was connecting before sNetworkManager ran Start 
        ShowMenuOptions(false);
        UIManager.instance.ConnectToServer();
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
        if (GameObject.FindObjectOfType<sNetworkManager>() == null && _NetworkManagerPREFAB != null)
        {
            var network=  GameObject.Instantiate(_NetworkManagerPREFAB);
            network.transform.parent = UIManager.instance.transform.parent;
        }
    }
}
