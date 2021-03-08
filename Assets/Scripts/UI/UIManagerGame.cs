﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UserInput;


[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManagerGame : MonoSingletonBackwards<UIManagerGame>
{

    [Header("Game Components")]
    public GameObject _inventoryCanvas;  ///TODO fix this ref being public
    [SerializeField] GameObject _normalInInventory = default;
    [SerializeField] GameObject _normalOutInventory = default;
    [SerializeField] GameObject _kittingInventory = default;
    [SerializeField] GameObject _shippingInventory = default;
    [SerializeField] GameObject _endResultsCanvas = default;
    [SerializeField] Button _hand1 = default;
    [SerializeField] Button _hand2 = default;
    [SerializeField] Image _touchPhaseDisplay = default;
    [SerializeField] Image _previewSlot = default;
    ///NB: Dont like how "hardcoded" this is getting

    [Header("Bin Toggles")]
    [SerializeField] ClickToShow _inBinToggle = default;
    [SerializeField] ClickToShow _outBinToggle = default;


    [Header("Bin Components")]
    [SerializeField] GameObject _defectBinInventory = default;
    [SerializeField] GameObject _defectBinObject = default;
    [SerializeField] GameObject _inBinObject = default;
    
    
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
    #endregion
    #region InventoryActions
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

    public void BeginLevel()
    {
        Debug.Log($"<color=blue> BeginLevel:Game </color>GAME:");

        if (_inventoryCanvas)
            _inventoryCanvas.SetActive(true); ///when we turn on the world canvas we should some knowledge of our station and set up the UI accordingly 
        
        if (_endResultsCanvas)
            _endResultsCanvas.SetActive(false);
        //Setup the proper UI for our workStation:
        WorkStation ws = GameManager.Instance._workStation;

        HandleKitting(ws);
        HandleQAStation(ws);
        HandleShippingStation(ws);

        StartCoroutine(ToggleTheInvItemsToLetThemLoad());

    }


    ///This are required to toggle on/off the inventories, to let them run their awake/start functions, 
    ///otherwise adding a component to an inventory that hasnt run yet has undesired results since the InventoryComponent 
    ///didnt get the chance to calculate their starting sizes properly
    IEnumerator ToggleTheInvItemsToLetThemLoad()
    {
        if (_inBinToggle)
            _inBinToggle.ClickToShowObject();
        if (_outBinToggle)
            _outBinToggle.ClickToShowObject();
        yield return new WaitForEndOfFrame();
        if (_inBinToggle)
            _inBinToggle.ClickToShowObject();
        if (_outBinToggle)
            _outBinToggle.ClickToShowObject();
    }

    private void HandleKitting(WorkStation ws)
    {
        //if (!ws.isKittingStation())
        //    return;

        bool cond = ws.isKittingStation();

        //Debug.Log("SwitchToKitting = " + cond);

        if (_kittingInventory != null && cond)
        {
            if (GameManager.instance._batchSize != 1)
                _kittingInventory.SetActive(true);
            else
                _kittingInventory.SetActive(false);
        }

  
        if (_normalInInventory != null)
            _normalInInventory.SetActive(!cond);

        if (_inBinToggle && cond)
            _inBinToggle.ClickToShowObject();

        if (_inBinObject)
            _inBinObject.SetActive(!cond);

        if (cond)
        {
            GameManager.instance.IsStackableChanged(true);
        }

    }
    private void HandleQAStation(WorkStation ws)
    {

        bool isQA = ws.IsQAStation();
        //Debug.Log($"<Color=blue> IS QA </color> = {isQA}");
        if (_defectBinInventory)
            _defectBinInventory.SetActive(isQA);
        if (_defectBinObject)
            _defectBinObject.SetActive(isQA);

    }
    private void HandleShippingStation(WorkStation ws)
    {
        bool cond = ws.IsShippingStation() && GameManager.instance._batchSize == 1;

        if (_shippingInventory)
            _shippingInventory.SetActive(cond);
        if (_normalOutInventory)
            _normalOutInventory.SetActive(!cond);
    }
    public void HideInInventory()
    {
        if (_normalInInventory != null)
            _normalInInventory.SetActive(false);
    }

    public void RoundOutOfTime(float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
        ///SHOW RESULTS MODAL
        if(_endResultsCanvas)
            _endResultsCanvas.SetActive(true);

        if (_inventoryCanvas)
            _inventoryCanvas.SetActive(false); 


        ///TODO figure out how to pass this data into the module
    }
    #endregion



    #region Game Actions
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


}