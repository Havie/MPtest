using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using dataTracking;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManagerNetwork : MonoSingletonBackwards<UIManagerNetwork>
{
    WorkStationManager _workstationManager;

    [Header("Scene Loading Info")]
    [SerializeField] string _inventorySceneName = "Inventory";
    [SerializeField] string _mpLobbySceneName = "MP_Lobby";


    [Header("Networking Components")]
    public GameObject _networkingCanvas;
    public Button _bConnect;
    public Button _bHost;
    public InputField _usernameField;
    public Text _loadingTxt;
    public GameObject _workStationDropDown;
    public Button _tmpConfirmWorkStation;


    [Header("MPLobby Components")]
    [SerializeField] LobbyMenu _lobbyMenu;

    [Header("Tutorial")]
    [SerializeField] WorkStationManager _tutorialWSManager;


    public delegate void ConnectionResult(bool cond);
    public ConnectionResult OnConnectionResult;

    #region Init

    private void Start()
    {
        UIManager.RegisterNetworkManager(this);

        SetUpWorkStationDropDownMenu(); ///Will need to be called again when client, but for non network scene need a call here as well

        if (_loadingTxt && _tmpConfirmWorkStation && _workStationDropDown)
        {
            _loadingTxt.enabled = false;
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _workStationDropDown.SetActive(false);
            //Debug.Log(" confirm station off");
        }
        else
            UIManager.DebugLogWarning("(UIManager): Missing Start objects (if in a test scene without networking this is fine)");

        if (_networkingCanvas)
        {
            _networkingCanvas.SetActive(true);
        }
        else
            UIManager.DebugLogWarning("(UIManager): Missing BeginLevel Canvases");

        sServer.OnHostIpFound += DisableHostButtonHack;
    }
    private void OnDestroy()
    {
        UIManager.RegisterGameManager(null);
    }

    private void SetUpWorkStationDropDownMenu()
    {
        //DebugLog($"Switching WS::{_workstationManager} to WS::{GameManager.instance.CurrentWorkStationManager}");
        _workstationManager = GameManager.Instance.CurrentWorkStationManager;

        //Set up workstation selection
        if (_workstationManager != null && _workStationDropDown)
            _workstationManager.SetupDropDown(_workStationDropDown.GetComponent<Dropdown>());
        else
            UIManager.DebugLogWarning("(UIManager): Missing _workstationManager or _workStationDropDown  (if in a test scene without networking this <color=yellow>*might*</color> be fine) ");

    }

    private void EnablePanel(bool cond)
    {
        if (_bConnect && _bHost && _usernameField)
        {
            _bConnect.gameObject.SetActive(cond);
            _bHost.gameObject.SetActive(cond);
            _usernameField.gameObject.SetActive(cond);
            _loadingTxt.gameObject.SetActive(!cond);
        }
        else
            UIManager.DebugLogWarning("(UIManager): Missing EnablePanel objects");
    }

    public void Connected(bool cond)
    {
        if (!cond)
            UIManager.DebugLogWarning($"connected to server = <color=red>{cond}</color>");

        if (_loadingTxt)
            StartCoroutine(ConnectionResultRoutine(cond));
    }

    IEnumerator ConnectionResultRoutine(bool cond)
    {
        if (cond)
        {
            _loadingTxt.text = "Connection Success!";
            yield return new WaitForSeconds(0.5f);
            _loadingTxt.enabled = false;
            ///OLD 
            //SetUpWorkStationDropDownMenu();///resetup incase our host changed the batch size/other settings
            //DisplaySelectWorkStation();
            LoadLobbyScene();

        }
        else
        {
            _loadingTxt.color = Color.red;
            _loadingTxt.text = "Connection Failed! \nCheck Tablet is connected to internet";
            yield return new WaitForSeconds(2f);
            _loadingTxt.enabled = false;
            _loadingTxt.color = Color.black;
            EnablePanel(true);
        }

        OnConnectionResult?.Invoke(cond);

    }

    private void BeginLevel(int stationID)
    {
        Debug.LogWarning($"<color=yellow>  BeginLevel:Network </color>{stationID}");

        if (_networkingCanvas)
            _networkingCanvas.SetActive(false);

        UIManager.SetStationLevel(stationID);
    }

    public void HostStartsRound()
    {
        ///Send message to the network that we want to begin
        ClientSend.Instance.HostWantsToBeginRound();
    }

    public void ConfirmWorkStation()
    {
        ///Get the ID before leaving the Scene 
        var stationID = _lobbyMenu.GetStationSelectionID();
        LoadInventoryScene(stationID, false);
    }
    public void RequestRefresh() 
    {
        ClientSend.Instance.RequestMPData();
    }
    public void ReceieveMPData(List<LobbyPlayer> playerData)
    {
        if(_lobbyMenu)
            _lobbyMenu.ReceieveRefreshData(playerData);
    }
    public bool RegisterLobbyMenu(LobbyMenu menu)
    {
        //Slightly circular but needs to be set between scenes...
        _lobbyMenu = menu;

        return sServer._iAmHost;
    }

    #endregion

    ///called from button
    public void ConnectToServer()
    {
        ConnectToServer("Trying to find server");
    }

    public void ConnectToServer(string msg)
    {
        EnablePanel(false);
        Client.instance.ConnectToServer();
        if (_loadingTxt)
        {
            _loadingTxt.text = msg;
            _loadingTxt.enabled = true;
        }
        else
            Debug.LogWarning("(UIManager): Missing ConnectToServer objects");
        
    }

    public void LoadLobbyScene()
    {
        Debug.Log("load scene");
        SceneLoader.LoadLevel(_mpLobbySceneName);
    }

    public void EnableHostButton(bool cond)
    {
        if (_bHost)
            _bHost.interactable = cond;
    }

    #region RunTime Actions
    public void DisableHostButtonHack(string ignore)
    {
        if (_bHost)
            _bHost.interactable = false;
    }

    public void BroadCastIp()
    {
        // BroadcastListener.Instance.BroadCastIP();
        sServer.BroadCastIP();
    }

    public void PrintMyIp()
    {

        UIManager.DebugLog(sServer.GetLocalIPAddress());
    }

    public void TestFile()
    {
        FileSaver.WriteToFileTest("test");
    }
    
    public void LoadTutorial()
    {
        GameManager.Instance.BatchChanged(2);
        GameManager.Instance.ForceTutorialWSM();
        var stationID = 2; ///tutorial station needs to not be kitting
        LoadInventoryScene(stationID, true);

    }
    #endregion


    private void LoadInventoryScene(int stationIndex, bool isTutorial)
    {
        GameManager.Instance.IsTutorial = isTutorial;
        GameManager.Instance.StartWithWIP = isTutorial;
        SceneLoader.LoadLevel(_inventorySceneName);
        BeginLevel(stationIndex);
        GameManager.instance.SetRoundShouldStart(!isTutorial); ///No timer? not needed?
    }

    private void OnDisable()
    {
        // BroadcastListener.Instance.OnHostIpFound -= DisableHostButton;
    }

}

