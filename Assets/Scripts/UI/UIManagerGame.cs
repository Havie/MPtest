using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UserInput;
using TMPro;
using System.Collections.Generic;

[DefaultExecutionOrder(-9999)] ///Load early to beat Injector
public class UIManagerGame : MonoSingletonBackwards<UIManagerGame>
{
    //[Header("Scene Loading Info")]
    const string _lobbyScene = "MP_Lobby";
    const string _mainMentSceneName = "Networking Scene";

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
    [SerializeField] UIPreviewMovementIcon _previewMovingIcon = default;
    ///NB: Dont like how "hardcoded" this is getting

    [Header("Bin Toggles")]
    [SerializeField] ClickToShow _inBinToggle = default;
    [SerializeField] ClickToShow _outBinToggle = default;


    [Header("Bin Components")]
    [SerializeField] GameObject _defectBinInventory = default;
    [SerializeField] GameObject _defectBinObject = default;
    [SerializeField] GameObject _inBinObject = default;
    [SerializeField] GameObject _outBinObject = default;
    [SerializeField] TextMeshProUGUI _inPlayerLabel = default;
    [SerializeField] TextMeshProUGUI _outPlayerLabel = default;

    [Header("Buttons")]
    [SerializeField] TextMeshProUGUI _sendButtonOut = default;
    [SerializeField] TextMeshProUGUI _sendButtonSmall = default;

    [Header("Events")]
    [SerializeField] BatchEvent _itemRecievedEvent = default;

    ///TODO , why dont I just seralize these like everything else so its not circular?
    public UIInventoryManager _invIN { get; private set; }
    public UIInventoryManager _invOUT { get; private set; }

    ///Load the dynamically from  _kittingInventory / _shippingInventory when needed
    UIOrdersIn _invKITTING;
    UIOrdersIn _invShipping;


    /************************************************************************************************************************/

    #region Init
    private void Start()
    {
        FindAndCacheInventories();
        UIManager.RegisterGameManager(this);
        ShowPreviewInvSlot(false, Vector3.zero, null);
        ShowPreviewMovingIcon(false, Vector3.zero, null);
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

    public void ShowPreviewMovingIcon(bool cond, Vector3 pos, Sprite img)
    {
        if (_previewMovingIcon)
        {
            _previewMovingIcon.ShowPreviewMovingIcon(cond, pos, img);
        }
    }

    public void BeginLevel()
    {
        if (_inventoryCanvas)
            _inventoryCanvas.SetActive(true); ///when we turn on the world canvas we should some knowledge of our station and set up the UI accordingly 

        if (_endResultsCanvas)
            _endResultsCanvas.SetActive(false);
        var gm = GameManager.Instance;

        //Setup the proper UI for our workStation:
        WorkStationManager wm = gm.CurrentWorkStationManager;
        WorkStation ws = gm._workStation;

        int[] stationSequence = StationSequenceReader.GetProperSequence(wm);
        int mySequenceIndex = StationSequenceReader.FindPlaceInSequence(stationSequence, (int)ws._myStation);

        if (mySequenceIndex > 0)
        {
            var InPlayerStationID = stationSequence[mySequenceIndex - 1];
            _inPlayerLabel.text = $"Station {InPlayerStationID}";
        }
        if (mySequenceIndex+1 < stationSequence.Length ) 
        {
            var OutPlayerStationID = stationSequence[mySequenceIndex + 1];
            _outPlayerLabel.text = $"Station {OutPlayerStationID}";
        }
        else
        {
            _outPlayerLabel.text ="Shipping";
        }
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
            {
                _kittingInventory.SetActive(true);
                _invKITTING = _kittingInventory.GetComponentInChildren<UIOrdersIn>();
            }
            else
            {
                _kittingInventory.SetActive(false);
            }
        }


        ShowInInventory(!cond);

        if (_inBinToggle && cond)
        {
            _inBinToggle.ClickToShowObject();
        }
        if (_inBinObject)
        {
            _inBinObject.SetActive(!cond);
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
        bool isShipping = ws.IsShippingStation();
        if (isShipping)
        {
            _sendButtonOut.text = "Ship";
            _sendButtonSmall.text = "Ship";
        }
        HandlePullShipping(isShipping && GameManager.instance._batchSize == 1);
    }

    private void HandlePullShipping(bool cond)
    {
        if (_shippingInventory)
        {
            _shippingInventory.SetActive(cond);
            _invShipping = _shippingInventory.GetComponentInChildren<UIOrdersIn>();
        }
        if (_normalOutInventory)
            _normalOutInventory.SetActive(!cond);
        if (_outBinObject)
        {
            _outBinObject.SetActive(!cond);
        }
    }

    public void ShowInInventory(bool cond)
    {
        if (_normalInInventory != null)
        {
            _normalInInventory.SetActive(cond);
        }

    }

    private void FindAndCacheInventories()
    {
        if (_normalInInventory)
            _invIN = _normalInInventory.GetComponentInChildren<UIInventoryManager>(true);
        if (_normalOutInventory)
            _invOUT = _normalOutInventory.GetComponentInChildren<UIInventoryManager>(true);
    }

    public void RoundOutOfTime(float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
        ///SHOW RESULTS MODAL
        if (_endResultsCanvas)
        {
            _endResultsCanvas.SetActive(true);
            UIEndResults endResults = _endResultsCanvas.GetComponentInChildren<UIEndResults>();
            if (endResults)
            {
                endResults.SetResults(cycleTime, thruPut, shippedOnTime, shippedLate, wip);
            }
        }

        if (_inventoryCanvas)
            _inventoryCanvas.SetActive(false);
    }

    ///Called from end results modal 
    public void ContinueFromEndResults()
    {
        //TODO?- reload to main menu and let them reconnect with new host settings / start over?
        SceneLoader.LoadLevel(_lobbyScene);

    }

    public void ReturnToMainMenu()
    {
        SceneLoader.LoadLevel(_mainMentSceneName);
    }

    public void KanbanUpdateInventory(bool isInInventory, bool isEmpty, int itemID, List<QualityData> qualityData)
    {
        var whichInventory = isInInventory ? _invIN : _invOUT;
        if (whichInventory)
        {
            whichInventory.KanbanInventoryChanged(isEmpty, itemID, qualityData);
        }
        else
            UIManager.DebugLog($"..fatal no Inventory for : isIN={isInInventory} ");
    }

    public void ItemReceived(int itemID, List<QualityData> qualities)
    {
        _invIN.AddItemToSlot(itemID, qualities, false);
        InvokeDummyBatchEvent();
    }
    /// <summary> Used for PartBin to be able to listen for 1 event type, instead of a Batch and Void Event</summary>
    private void InvokeDummyBatchEvent()
    {
        if (_itemRecievedEvent)
            _itemRecievedEvent.Raise(new BatchWrapper(-1, 1, false));
    }
    #endregion


    /************************************************************************************************************************/


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
    public void OrderShipped(int itemdID)
    {
        if (_invKITTING)
        {
            _invKITTING.RemoveOrder(itemdID);
        }
        if (_invShipping)
        {
            // the Inv Shipping is manually handling the removal of items in its menu.
            // _invShipping.RemoveOrder(itemdID);
        }
    }
    ///Extra button for clear when in debug mode for Inv scene
    public void ClearDebugLogger()
    {
        UIManager.ClearDebugLog();
    }
    ///Called from HudButton under Timer on Canvas
    public void UIInstructionsClicked()
    {
        TutorialEvents.CallOnStationInstructionsClicked();
    }
    #endregion
    /************************************************************************************************************************/

}