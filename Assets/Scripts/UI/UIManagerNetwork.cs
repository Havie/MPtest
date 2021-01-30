using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManagerNetwork : MonoSingletonBackwards<UIManagerNetwork>
{
    WorkStationManager _workstationManager;

    [Header("Networking Components")]
    public GameObject _networkingCanvas;
    [SerializeField] GameObject _startMenu;
    public Button _bConnect;
    public Button _bHost;
    public InputField _usernameField;
    public Text _loadingTxt;
    public GameObject _workStationDropDown;
    public Button _tmpConfirmWorkStation;
    public GameObject _tmpObjectPREFAB;






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
            DebugLogWarning("(UIManager): Missing Start objects (if in a test scene without networking this is fine)");

        if (_networkingCanvas)
        {
            _networkingCanvas.SetActive(true);
        }
        else
            DebugLogWarning("(UIManager): Missing BeginLevel Canvases");

        sServer.OnHostIpFound += DisableHostButton;
    }
    private void OnDestroy()
    {
        UIManager.RegisterGameManager(null);
    }

    private void SetUpWorkStationDropDownMenu()
    {
        //DebugLog($"Switching WS::{_workstationManager} to WS::{GameManager.instance.CurrentWorkStationManager}");
        _workstationManager = UIManager.GetWSManager();

        //Set up workstation selection
        if (_workstationManager != null && _workStationDropDown)
            _workstationManager.SetupDropDown(_workStationDropDown.GetComponent<Dropdown>());
        else
            DebugLogWarning("(UIManager): Missing _workstationManager or _workStationDropDown  (if in a test scene without networking this <color=yellow>*might*</color> be fine) ");

    }

    private void EnablePanel(bool cond)
    {
        if (_bConnect && _bHost && _usernameField)
        {
            _bConnect.gameObject.SetActive(cond);
            _bHost.gameObject.SetActive(cond);
            _usernameField.gameObject.SetActive(cond);
        }
        else
            DebugLogWarning("(UIManager): Missing EnablePanel objects");
    }


    public void Connected(bool cond)
    {
        if (!cond)
            DebugLogWarning($"connected to server = <color=red>{cond}</color>");

        if (_loadingTxt)
            StartCoroutine(ConnectionResult(cond));
    }

    IEnumerator ConnectionResult(bool cond)
    {
        if (cond)
        {
            _loadingTxt.text = "Connection Success!";
            yield return new WaitForSeconds(0.5f);
            _loadingTxt.enabled = false;
            SetUpWorkStationDropDownMenu();///resetup incase our host changed the batch size/other settings
            DisplaySelectWorkStation();
        }
        else
        {
            _loadingTxt.text = "Connection Failed!";
            yield return new WaitForSeconds(1f);
            _loadingTxt.enabled = false;
            EnablePanel(true);
        }

    }

    public void DisplaySelectWorkStation()
    {
        if (_tmpConfirmWorkStation && _loadingTxt && _workStationDropDown)
        {
            _tmpConfirmWorkStation.gameObject.SetActive(true);
            _loadingTxt.enabled = true;
            _loadingTxt.text = "Select Work Station";
            _workStationDropDown.SetActive(true);
            Debug.LogWarning("DISPLAYED the Dropdown");
        }
        else
            Debug.LogWarning("(UIManager): Missing DisplaySelectWorkStation objects");

    }


    public void BeginLevel(int itemLevel)
    {
        Debug.Log($"<color=yellow>  BeginLevel:Network </color>{itemLevel}");
        //Debug.Log("called BeginLevel");
        //Setup the proper UI for our workStation
        WorkStation ws = GameManager.Instance._workStation;

        if (_tmpConfirmWorkStation && _loadingTxt && _workStationDropDown)
        {
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _loadingTxt.enabled = false;
            _workStationDropDown.SetActive(false);
        }

        if (_networkingCanvas)
            _networkingCanvas.SetActive(false);

        UIManager.SetStationLevel(itemLevel);

    }


    #endregion


    #region ActionsfromButtons
    public void ConnectToServer()
    {
        EnablePanel(false);
        Client.instance.ConnectToServer();
        if (_loadingTxt)
        {
            _loadingTxt.text = "Trying to find server";
            _loadingTxt.enabled = true;
        }
        else
            Debug.LogWarning("(UIManager): Missing ConnectToServer objects");

    }
    public void ConfirmWorkStation()
    {
        SceneLoader.LoadLevel("Inventory");
        int itemID = _workstationManager.ConfirmStation(_workStationDropDown.GetComponent<Dropdown>());
        BeginLevel(itemID);

    }

    public void SwitchToHost()
    {

    }

    #endregion


    #region RunTime Actions


    public void DisableHostButton(string ignore)
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

        DebugLog(sServer.GetLocalIPAddress());
    }

    #endregion

    private void OnDisable()
    {
        // BroadcastListener.Instance.OnHostIpFound -= DisableHostButton;
    }



    #region Debugger
    public void DebugLog(string text)
    {
        DebugCanvas.Instance.DebugLog(text);
    }
    public void DebugLogWarning(string text)
    {
        DebugCanvas.Instance.DebugLogWarning(text);
    }
    public void DebugLogError(string text)
    {
        DebugCanvas.Instance.DebugLogError(text);
    }

    public void ClearDebugLog()
    {
        DebugCanvas.Instance.ClearDebugLog();
    }

    #endregion
}