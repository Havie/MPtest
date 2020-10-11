using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField] GameObject _startMenu;
    public Button _bConnect;
    public Button _bHost;
    public InputField _usernameField;
    public Text _loadingTxt;
    public GameObject _workStationDropDown;

    public Button _tmpConfirmWorkStation;
    public GameObject _tmpObjectPREFAB;

    public GameObject _worldCanvas;
    public GameObject _screenCanvas;

    public GameObject _normalInventory;
    public GameObject _kittingInventory;

    public WorkStationManager _workstationManager;

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

    public void SetNormalMenu(GameObject inv) { _normalInventory = inv; }
    public void SetKittingMenu(GameObject inv) { _kittingInventory = inv; }

    private void Start()
    {
        //Set up workstation selection
        if (_workstationManager != null && _workStationDropDown)
            _workstationManager.SetupDropDown(_workStationDropDown.GetComponent<Dropdown>());
        else
            Debug.LogWarning("(UIManager): Missing _workstationManager ");

        if (_loadingTxt && _tmpConfirmWorkStation && _workStationDropDown)
        {
            _loadingTxt.enabled = false;
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _workStationDropDown.SetActive(false);
            Debug.Log(" confirm station off");
        }
        else
            Debug.LogWarning( "(UIManager): Missing Start objects (if in a test scene without networking this is fine)");



       /* if (true) //TEMP 
            SwitchToKitting(); */
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
        Debug.LogWarning("connected to server =" + cond);
        if(_loadingTxt)
            StartCoroutine(ConnectionResult(cond));
    }

    IEnumerator ConnectionResult(bool cond)
    {
        if (cond)
        {
            _loadingTxt.text = "Connection Success!";
            yield return new WaitForSeconds(0.5f);
            _loadingTxt.enabled = false;
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
        }
        else
            Debug.LogWarning("(UIManager): Missing DisplaySelectWorkStation objects");

    }


    public void BeginLevel(int itemLevel)
    {

        if (_tmpConfirmWorkStation && _loadingTxt && _workStationDropDown)
        {
            _tmpConfirmWorkStation.gameObject.SetActive(false);
            _loadingTxt.enabled = false;
            _workStationDropDown.SetActive(false);
        }

        if (_worldCanvas && _screenCanvas)
        {
            _worldCanvas.SetActive(true); //when we turn on the world canvas we should some knowledge of our station and set up the UI accordingly 
            _screenCanvas.SetActive(false);
        }
        else
            Debug.LogWarning("(UIManager): Missing BeginLevel Canvases");

        //Setup the proper UI for our workStation
        WorkStation ws = GameManager.instance._workStation;
       // Debug.Log($"{ws._stationName} is switching to kiting {ws.isKittingStation()} ");
        if (ws.isKittingStation())
            SwitchToKitting();

         // (TMP) Spawn Object and allow me to rotate it 
         BuildableObject bo = GameObject.FindObjectOfType<BuildableObject>();
         bo.SetItemID(itemLevel);

        
    }

    private void SwitchToKitting()
    {

        if (_normalInventory)
            _normalInventory.SetActive(false);

        if (_kittingInventory !=null)
            _kittingInventory.SetActive(true);

        GameManager.instance._isStackable = true;

    }

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
        int itemID= _workstationManager.ConfirmStation(_workStationDropDown.GetComponent<Dropdown>());
        BeginLevel(itemID);
    }

    #endregion
}
