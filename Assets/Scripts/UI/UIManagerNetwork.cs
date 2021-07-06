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

    [Header("Networking Components")]
    public GameObject _networkingCanvas;
    public Button _bConnect;
    public Button _bHost;
    public Button _bTutorial;
    public Button _bQuit;
    public InputField _usernameField;
    public Text _loadingTxt;
    public Text _orTxt;


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

        if (_loadingTxt)
        {
            _loadingTxt.enabled = false;
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


    private void EnablePanel(bool cond)
    {
        if (_bConnect && _bHost && _usernameField && _bTutorial && _bQuit && _orTxt)
        {
            _bConnect.gameObject.SetActive(cond);
            _bHost.gameObject.SetActive(cond);
            _usernameField.gameObject.SetActive(cond);
            _loadingTxt.gameObject.SetActive(!cond);
            _bTutorial.gameObject.SetActive(cond);
            _bQuit.gameObject.SetActive(cond);
            _orTxt.gameObject.SetActive(cond);
        }
        else
            UIManager.DebugLogWarning("(UIManager): Missing EnablePanel objects");
    }

    public void Connected(bool cond)
    {
        StartCoroutine(ConnectionResultRoutine(cond));
    }

    IEnumerator ConnectionResultRoutine(bool cond)
    {
        if (cond)
        {
            if (_loadingTxt)
            {
                _loadingTxt.text = "Connection Success!";
                yield return new WaitForSeconds(0.5f);
                _loadingTxt.enabled = false;
            }
            LoadLobbyScene();

        }
        else
        {
            UIManager.DebugLogWarning($"connected to server = <color=red>{cond}</color>");
            if (_loadingTxt)
            {
                _loadingTxt.color = Color.red;
                _loadingTxt.text = "Connection Failed! \nCheck Tablet is connected to internet";
                yield return new WaitForSeconds(2f);
                _loadingTxt.enabled = false;
                _loadingTxt.color = Color.black;
            }
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
        if (_lobbyMenu)
            _lobbyMenu.ReceieveRefreshData(playerData);
    }
    public void GameManagerVarsChanged(GameManager gm)
    {
        if (_lobbyMenu)
        {
            _lobbyMenu.WorkStationManagerChanged(gm.CurrentWorkStationManager);
        }
    }
    public bool RegisterLobbyMenu(LobbyMenu menu)
    {
        //Slightly circular but needs to be set between scenes...
        _lobbyMenu = menu;
        ///Menu needs to know if its the host
        return sServer._iAmHost;
    }
    public void RegisterHostMenu(UIHostMenu menu)
    {
        if(sServer._iAmHost)
        {
             menu.OnConfirmSettings += sServerSend.HostChangedGMValues;
        }
    }
    public void UnRegisterHostMenu(UIHostMenu menu)
    {
        if (sServer._iAmHost)
        {
            menu.OnConfirmSettings -= sServerSend.HostChangedGMValues;
        }
    }

    #endregion
    public void HostConnection()
    {
        Client.instance.IWillBeHost = true;
        sNetworkManager.Instance.HostNetwork();
        ConnectToServer("Trying to host connection");
    }

    ///called from button
    public void ConnectToServer()
    {
        EnablePanel(false);
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
        SceneTracker.Instance.LoadScene(SceneTracker.eSceneName.MP_Lobby);
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
        var gm = GameManager.Instance;

        gm.IsTutorial = isTutorial;
        gm.StartWithWIP = isTutorial ? true : gm._batchSize == 1 ? true : false;
        SceneTracker.Instance.LoadScene(SceneTracker.eSceneName.Work_Station);
        BeginLevel(stationIndex);
        gm.SetRoundShouldStart(!isTutorial); ///No timer? not needed?
    }

    private void OnDisable()
    {
        // BroadcastListener.Instance.OnHostIpFound -= DisableHostButton;
    }

}

