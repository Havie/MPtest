using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField] GameObject _startMenu;
   public InputField _usernameField;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate _instance, destroying : " +this.gameObject);
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        _startMenu.SetActive(false);
        _usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
}
