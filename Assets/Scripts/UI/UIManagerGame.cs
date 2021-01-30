using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManagerGame : MonoSingletonBackwards<UIManagerGame>
{

    [Header("Game Components")]
    public GameObject _inventoryCanvas;
    public GameObject _normalInventory;
    public GameObject _kittingInventory;
    public Button _hand1;
    public Button _hand2;
    public Image _touchPhaseDisplay;
    public Image _previewSlot;



    #region Init


    private void Start()
    {
        UIManager.RegisterGameManager(this);
        ShowPreviewInvSlot(false, Vector3.zero, null);
    }

    private void OnDestroy()
    {
        UIManager.RegisterGameManager(null);
    }

    public void ShowPreviewInvSlot(bool cond, Vector3 pos, Sprite img)
    {
        if (_previewSlot)
        {
            _previewSlot.gameObject.SetActive(cond);
            if (cond)
            {
                _previewSlot.gameObject.transform.position = pos;
                _previewSlot.sprite = img;
            }
        }
    }



    public void BeginLevel(int itemLevel)
    {
        Debug.Log($"<color=green> BeginLevel:Game </color>GAME:{itemLevel}");


        //Debug.Log("called BeginLevel");
        //Setup the proper UI for our workStation
        WorkStation ws = GameManager.Instance._workStation;

        if (_inventoryCanvas)
            _inventoryCanvas.SetActive(true); ///when we turn on the world canvas we should some knowledge of our station and set up the UI accordingly 

        // Debug.Log($"{ws._stationName} is switching to kiting {ws.isKittingStation()} ");
        if (ws.isKittingStation())
            SwitchToKitting();



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

    public void HideInInventory()
    {
        if (_normalInventory != null)
            _normalInventory.SetActive(false);
    }

    #endregion



    #region RunTime Actions
    public void ShowTouchDisplay(float pressTime, float pressTimeMax, Vector3 pos)
    {
        if (_touchPhaseDisplay)
        {
            if (!_touchPhaseDisplay.transform.gameObject.activeSelf)
                _touchPhaseDisplay.transform.gameObject.SetActive(true);

            _touchPhaseDisplay.fillAmount = pressTime / pressTimeMax;
            _touchPhaseDisplay.transform.position = pos;
            _touchPhaseDisplay.color = SetTouchPhaseOpacity(_touchPhaseDisplay.fillAmount);
        }
    }
    private Color SetTouchPhaseOpacity(float perct)
    {
        Color curr = _touchPhaseDisplay.color;
        Color newColor = new Color(curr.r, curr.g, curr.b, perct);
        return newColor;
    }

    public void HideTouchDisplay()
    {
        if (_touchPhaseDisplay)
            _touchPhaseDisplay.gameObject.SetActive(false);
    }


    public void UpdateHandLocation(int index, Vector3 worldLoc)
    {
        Button hand = index == 1 ? _hand1 : _hand2;

        Vector3 screenLoc = UserInputManager.Instance.WorldToScreenPoint(worldLoc);
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


    #endregion


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