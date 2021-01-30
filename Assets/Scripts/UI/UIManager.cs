using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


public static class UIManager
{
    static UIManagerNetwork _networkManager;
    static UIManagerGame _invManager;

    public static bool LoadedFromMenu { get; private set; } = false;
    public static int StationToInject { get; private set; } = 0;

    #region Init

    public static void RegisterNetworkManager(UIManagerNetwork manager) { _networkManager = manager; }

    public static void RegisterGameManager(UIManagerGame manager) { _invManager = manager; }



    public static string GetUserName()
    {
        return _networkManager == null ? "NoName" : _networkManager._usernameField.text;
    }

    public static WorkStationManager GetWSManager()
    {
        return GameManager.Instance.CurrentWorkStationManager;
    }

    public static GameObject GetInventoryCanvas()
    {
        return _invManager == null ? null : _invManager._inventoryCanvas;
    }

    public static void ShowPreviewInvSlot(bool cond, Vector3 pos, Sprite img)
    {
        if (_invManager)
            _invManager.ShowPreviewInvSlot(cond, pos, img);
    }





    public static void Connected(bool cond)
    {
        if (_networkManager)
            _networkManager.Connected(cond);
    }

    public static void SetStationLevel(int itemLevel)
    {
        ///THIS is Circular >.< 
        LoadedFromMenu = true;
        StationToInject = itemLevel;
    }

    public static void BeginLevel(int itemLevel)
    {
        LoadedFromMenu = true;
        Debug.Log($"<color=orange> LoadedFromMenu</color> ={LoadedFromMenu}");
     
        if (_invManager)
            _invManager.BeginLevel(itemLevel);
    }


    public static void HideInInventory()
    {
        if (_invManager)
            _invManager.HideInInventory();
    }

    #endregion


    #region ActionsfromButtons
    ///Indirect from UIHostMenu button call:
    public static void ConnectToServer()
    {
        if (_networkManager)
            _networkManager.ConnectToServer();

    }
    #endregion


    #region RunTime Actions
    public static void ShowTouchDisplay(float pressTime, float pressTimeMax, Vector3 pos)
    {
        if (_invManager)
            _invManager.ShowTouchDisplay(pressTime, pressTimeMax, pos);
    }
    private static Color SetTouchPhaseOpacity(float perct)
    {
        return Color.white;
    }

    public static void HideTouchDisplay()
    {
        if (_invManager)
            _invManager.HideTouchDisplay();
    }


    public static void UpdateHandLocation(int index, Vector3 worldLoc)
    {
        if (_invManager)
            _invManager.UpdateHandLocation(index, worldLoc);
    }
    public static void ChangeHandSize(int index, bool smaller)
    {
        if (_invManager)
            _invManager.ChangeHandSize(index, smaller);

    }

    public static void ResetHand(int index)
    {
        if (_invManager)
            _invManager.ResetHand(index);
    }


    #endregion




    #region Debugger
    public static void DebugLog(string text)
    {
        DebugCanvas.Instance.DebugLog(text);
    }
    public static void DebugLogWarning(string text)
    {
        DebugCanvas.Instance.DebugLogWarning(text);
    }
    public static void DebugLogError(string text)
    {
        DebugCanvas.Instance.DebugLogError(text);
    }

    public static void ClearDebugLog()
    {
        DebugCanvas.Instance.ClearDebugLog();
    }

    #endregion
}