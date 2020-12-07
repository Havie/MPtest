﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManager : MonoBehaviour
{

    public static UIManager instance;


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


    [HideInInspector]
    public WorkStationManager _workstationManager;

    [Header("Game Components")]
    public GameObject _inventoryCanvas;
    public GameObject _normalInventory;
    public GameObject _kittingInventory;
    public Button _hand1;
    public Button _hand2;

    [Header("Debugg Console")]
    public Text _debugText;
    public int _maxTextSize = 350;

    #region Init
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate _instance, destroying : " + this.gameObject);
            Destroy(this);
        }
    }

    private void Start()
    {
        SetUpWorkStationDropDownMenu(); ///Will need to be called again when client, but for non network scene need a call here as well

        if (_loadingTxt && _tmpConfirmWorkStation && _workStationDropDown)
        {
            _loadingTxt.enabled = false;
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _workStationDropDown.SetActive(false);
            //Debug.Log(" confirm station off");
        }
        else
            Debug.LogWarning("(UIManager): Missing Start objects (if in a test scene without networking this is fine)");

        if (_inventoryCanvas && _networkingCanvas)
        {
            _inventoryCanvas.SetActive(false);
            _networkingCanvas.SetActive(true);
        }
        else
            Debug.LogWarning("(UIManager): Missing BeginLevel Canvases");


        sServer.OnHostIpFound += DisableHostButton;
    }


    private void SetUpWorkStationDropDownMenu()
    {
        DebugLog($"Switching WS::{_workstationManager} to WS::{GameManager.instance.CurrentWorkStationManager}");
        _workstationManager = GameManager.instance.CurrentWorkStationManager;

        //Set up workstation selection
        if (_workstationManager != null && _workStationDropDown)
            _workstationManager.SetupDropDown(_workStationDropDown.GetComponent<Dropdown>());
        else
            Debug.LogWarning("(UIManager): Missing _workstationManager or _workStationDropDown  (if in a test scene without networking this <color=yellow>*might*</color> be fine) ");

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
            Debug.LogWarning("(UIManager): Missing EnablePanel objects");
    }


    public void Connected(bool cond)
    {
        if(!cond)
            Debug.LogWarning($"connected to server = <color=red>{cond}</color>");

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
        Debug.Log("called BeginLevel");
        //Setup the proper UI for our workStation
        WorkStation ws = GameManager.instance._workStation;

        if (_tmpConfirmWorkStation && _loadingTxt && _workStationDropDown)
        {
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _loadingTxt.enabled = false;
            _workStationDropDown.SetActive(false);
        }

        if (_inventoryCanvas && _networkingCanvas)
        {
            _inventoryCanvas.SetActive(true); ///when we turn on the world canvas we should some knowledge of our station and set up the UI accordingly 
            _networkingCanvas.SetActive(false);
        }
        else
            Debug.LogWarning("(UIManager): <color=red>Missing BeginLevel Canvases </color> , this should be fine if in test scene");


        // Debug.Log($"{ws._stationName} is switching to kiting {ws.isKittingStation()} ");
        if (ws.isKittingStation())
            SwitchToKitting();

        // (TMP) Spawn Object and allow me to rotate it 
        // BuildableObject bo = GameObject.FindObjectOfType<BuildableObject>();
        //bo.SetItemID(itemLevel);


    }

    private void SwitchToKitting()
    {

        if (_normalInventory != null)
            _normalInventory.SetActive(false);
        else
            Debug.LogError("normal??");

        if (_kittingInventory != null)
            _kittingInventory.SetActive(true);

        GameManager.instance._isStackable = true;

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
        int itemID = _workstationManager.ConfirmStation(_workStationDropDown.GetComponent<Dropdown>());
        BeginLevel(itemID);
    }


    public void SwitchToHost()
    {

    }

    #endregion


    #region RunTime Actions

    public void UpdateHandLocation(int index, Vector3 worldLoc)
    {
        Button hand = index == 1 ? _hand1 : _hand2;

        Vector3 screenLoc = UserInput.Instance.GetScreenPointBasedOnWorldLoc(worldLoc);
        if (hand)
            hand.transform.position = screenLoc;

    }
    public void ChangeHandSize(int index, bool smaller)
    {
        Button hand = index == 1 ? _hand1 : _hand2;
        if (hand)
        {
            if (smaller)
                hand.transform.localScale = Vector3.one * 0.75f;
            else
                hand.transform.localScale = Vector3.one;
        }
    }

    public void ResetHand(int index)
    {

        Button hand = index == 1 ? _hand1 : _hand2;
        if (hand)
            hand.transform.position = new Vector3(0, 2000, 0);
    }


    public void DisableHostButton(string ignore)
    {
        if (_bHost)
            _bHost.interactable = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            BroadCastIp();
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
        if (_debugText)
        {
            try
            {
                _debugText.text = _debugText.text + "\n" + text;

                if (_debugText.text.Length > _maxTextSize)
                    _debugText.text = _debugText.text.Substring(_debugText.text.Length - 1 - _maxTextSize);
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugText throwng exepection {e}");
            }

        }
        Debug.Log(text);
    }
    public void DebugLogWarning(string text)
    {
        if (_debugText)
        {
            try
            {
                _debugText.text = _debugText.text + "\n" + text;

                if (_debugText.text.Length > _maxTextSize)
                    _debugText.text = _debugText.text.Substring(_debugText.text.Length - 1 - _maxTextSize);
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugText throwng exepection {e}");
            }
        }
        Debug.LogWarning(text);
    }
    public void DebugLogError(string text)
    {
        try
        {
            _debugText.text = _debugText.text + "\n" + text;
            if (_debugText.text.Length > _maxTextSize)
                _debugText.text = text;
        }
        catch (Exception e)
        {
            Debug.LogError($"DebugText throwng exepection {e}");
        }
        Debug.LogError(text);
    }

    public void ClearDebugLog()
    {
        if (_debugText)
        {
            _debugText.text = "";
        }
    }

    #endregion
}