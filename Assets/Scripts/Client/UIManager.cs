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

    public Button _tmpConfirmWorkStation;
    public GameObject _tmpObjectPREFAB;

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
        _loadingTxt.enabled = false;
        _tmpConfirmWorkStation.gameObject.SetActive(false);
        Debug.Log(" confirm station off");
    }

    public void ConnectToServer()
    {
        EnablePanel(false);
        Client.instance.ConnectToServer();
        _loadingTxt.text = "Trying to find server";
        _loadingTxt.enabled = true;

    }

    private void EnablePanel(bool cond)
    {
        _bConnect.gameObject.SetActive(cond);
        _bHost.gameObject.SetActive(cond);
        _usernameField.gameObject.SetActive(cond);
    }


    public void Connected(bool cond)
    {
        Debug.LogWarning("connected to server =" + cond);
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
        _tmpConfirmWorkStation.gameObject.SetActive(true);
        _loadingTxt.enabled = true;
        _loadingTxt.text = "Select Work Station (TODO)";

    }

    public void Test()
    {
        _tmpConfirmWorkStation.gameObject.SetActive(false);
        _loadingTxt.enabled = false;

        //Spawn Object and allow me to rotate it 
        GameObject.Instantiate(_tmpObjectPREFAB, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
