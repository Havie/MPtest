#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class UIOrdersIn : MonoBehaviour, IInventoryManager
{
    ///TODO This class probably needs to be re-written to not use "Vertical LayoutGrp and content Size fitter
    ///to match our other inventories,
    ///or a lot of debugging needs to be done to get these to play nicely together as its just a total
    ///hail mary right now
    [SerializeField] OrderReceivedEvent _orderCreated;

    [Header("Vertical Layout Overrides These..")]
    [SerializeField] int _startingX = 16;   ///0
    [SerializeField] int _startingY = 350;
    [SerializeField] int _yOffset = -39;  ///-65

    [Header("Text Components")]
    [SerializeField] TextMeshProUGUI _orderCountTxt = default;
    [SerializeField] TextMeshProUGUI _shippedCountTxt = default;

    private GameObject _bORDERPREFAB;

    private int _ORDERFREQUENCY;
    private float _timeToOrder = float.MaxValue;
    protected bool _shouldDropParts = true;

    private List<OrderButton> _orderList = new List<OrderButton>();
    private int _shippedCount = 0;

    private ComponentList _componentList;
    List<int> _usedIndicies = new List<int>();

    /************************************************************************************************************************/


    private void Awake()
    {
        //Get rid of our test button incase it gets renabled on accident cuz I keep doingthis
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            Destroy(transform.GetChild(0).gameObject);
        }
    }

    private void OnEnable()
    {
        if (_bORDERPREFAB == null)
            _bORDERPREFAB = Resources.Load<GameObject>("Prefab/UI/bOrder_slot");

        _ORDERFREQUENCY = GameManager.Instance._orderFrequency;
        _componentList = GameManager.Instance._componentList;
        _shouldDropParts = CheckShouldDropParts();
    }

    protected abstract bool CheckShouldDropParts();
    /************************************************************************************************************************/


    protected void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.R))
             RemoveOrder(_OrderList[_OrderList.Count / 2]);*/

        if (_timeToOrder > _ORDERFREQUENCY)
            SendInNewOrder();
        else
            _timeToOrder += Time.deltaTime;
    }


    public void AddOrder(int itemID)
    {
        var bOrder = GameObject.Instantiate(_bORDERPREFAB);
        OrderButton ob = bOrder.GetComponent<OrderButton>();
        //Debug.Log("assign item with ID:" + itemID);
        float deliveryTime = GetEstimatedDeliveryTime();
        ob.SetOrder(itemID, deliveryTime);
        _orderList.Add(ob);
        bOrder.transform.SetParent(this.transform);
        bOrder.transform.localPosition = FindPosition(_orderList.Count - 1);
        bOrder.transform.localScale = new Vector3(1, 1, 1); /// no idea why these come in at 1.5, when the prefab and parent are at 1
        ob.SetManager(this);
        UpdateOrderText();
        //Get data based off of the incoming value
        if (_orderCreated)
        {
            _orderCreated.Raise(new OrderWrapper(itemID, Time.time, deliveryTime));
        }
    }


    public bool RemoveOrder(int itemID)
    {
        //Debug.Log(_orderList.Count +" size  , Remove order with ItemID : " + itemID);
        /* for (int i = _orderList.Count-1; i >0 ; i--)*/
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            var order = child.GetComponent<OrderButton>();
            if (order != null)
            {
                if (order.ItemID == itemID)
                {
                    //Debug.LogWarning($"Item ID {itemID} found match");
                    return RemoveOrder(order);
                }
            }
        }
        return false;
    }

    public bool RemoveOrder(OrderButton orderButton)
    {
        bool removed = _orderList.Contains(orderButton);
        Debug.Log($"..<color=blue>This is happening for: </color> {orderButton.Slot.gameObject.name}");
        if (removed)
        {
            _orderList.Remove(orderButton);
            UpdateOrderText();
            // play animation then give the anim on.finish() this callback
            StartCoroutine(ButtonShipped(orderButton));
        }
        else
            Debug.LogError("how is this not in the list");

        return removed;
    }


    /************************************************************************************************************************/

    /**Kittings "Final ItemIDs should be the final item(s) that go to shipping */

    private int PickAnItemIDFromFinalTask()
    {
        var manager = GameManager.instance.CurrentWorkStationManager;
        var list = manager.GetStationList();
        var lastStation = list[list.Count - 1];
        var lastTaskList = lastStation.Tasks;
        var lastTask = lastTaskList[lastTaskList.Count - 1];
        var finalItemList = lastTask._finalItemID;
        var finalItem = finalItemList[Random.Range(0, finalItemList.Count)];

        return (int)finalItem;
    }

    private void SendInNewOrder()
    {
        _timeToOrder = 0;
        _usedIndicies.Clear();

        var finalItemId = PickAnItemIDFromFinalTask();
        List<ObjectRecord.eItemID> componentsNeeded = _componentList.GetComponentListByItemID(finalItemId);
        int size = componentsNeeded.Count;
        ObjectRecord.eItemID[] componentOrder = new ObjectRecord.eItemID[size];

        foreach (var item in componentsNeeded)
        {
            componentOrder[GetUnusedIndex(size)] = item;
        }

        // printOrderList(componentOrder);
        if (_shouldDropParts)
        {
            PartDropper.Instance.SendInOrder(componentOrder);
        }
        AddOrder(finalItemId);
    }

    private int GetUnusedIndex(int size)
    {
        int index = Random.Range(0, size);
        if (_usedIndicies.Contains(index))
            return GetUnusedIndex(size);
        else
        {
            _usedIndicies.Add(index);
            return index;
        }
    }

    private float GetEstimatedDeliveryTime()
    {
        return Time.time + 600;  ///10min 
    }


    private Vector3 FindPosition(int index)
    {
        return new Vector3(_startingX, _startingY + (_yOffset * index), 0);
    }
    private void printOrderList(ObjectRecord.eItemID[] componentOrder)
    {
        string s = "";
        for (int i = 0; i < componentOrder.Length; ++i)
        {
            s += $"#{i} = ItemID:{componentOrder[i]} , ";
        }

        Debug.LogWarning($"NEW LIST: {s}");
    }

    private void GetRandomItemIDFromKitting()
    {
        var workStation = GameManager.instance._workStation;
        //Get the kitting task (should be only task)
        foreach (Task t in workStation.Tasks)
        {
            if (t._stationType == Task.eStationType.Kitting)
            {
                //Remove the count-1 when we get sprites for every type
                int rng = Random.Range(0, t._finalItemID.Count);
                int itemid = (int)t._finalItemID[rng];
                //Debug.Log($"rng={rng} from max of {t._finalItemID.Count } , thus final itemID={itemid}");
                AddOrder(itemid);
            }
        }
    }

    private void UpdateOrderText()
    {
        if (_orderCountTxt)
        {
            _orderCountTxt.text = _orderList.Count.ToString();
        }
    }
    private void UpdateShippedText()
    {
        if (_shippedCountTxt)
        {
            _shippedCountTxt.text = (++_shippedCount).ToString();
        }
    }

    /// <summary> Item is assigned manually by the bSlot, shuffle it for FIFO</summary>
    public void SlotStateChanged(UIInventorySlot slot)
    {
        var itemID = slot.GetItemID();
        if (slot)
            slot.RemoveItem();  ///prevent the anim from playing on wrong slot for FIFO, see ButtonShipped()
        RemoveOrder(itemID);

    }
    protected virtual IEnumerator ButtonShipped(OrderButton orderButton)
    {
        ///Let animation play:
        var slot = orderButton.Slot;
        ///Need to fake in use, calling assign item will infite loop
        ///A work around to our FIFO order system,
        ///if someone assigns the item and tries to ship an order, it still takes the first order in queue out
        ///so visually we have to fix this since we are only removing per ID, not physical order,
        ///would have to re-write network layer to not be FIFO in version2
        slot.FakeSetSpriteAsInUse(orderButton.ItemID);
        slot.PlayCheckMarkAnim(true);
        yield return new WaitForSeconds(1f);
        UpdateShippedText();
        ButtonDestroyedCallback(orderButton);
    }

    protected void ButtonDestroyedCallback(OrderButton orderButton)
    {
        //Debug.Log($"Destroy: {orderButton}");
        Destroy(orderButton.gameObject);

        for (int i = 0; i < _orderList.Count; i++)
        {
            _orderList[i].transform.localPosition = FindPosition(i);
        }
    }

    /// <summary>
    /// Interface version of assigning an item into the general inventory space, called from deadzone most likely
    /// </summary>
    public bool TryAssignItem(int id, int count, List<QualityData> qualities)
    {
        return RemoveOrder(id);
    }
}

