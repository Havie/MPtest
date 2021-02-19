﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShipping : MonoBehaviour
{
    ///TODO This class probably needs to be re-written to not use "Vertical LayoutGrp and content Size fitter
    ///to match our other inventories,
    ///or a lot of debugging needs to be done to get these to play nicely together as its just a total
    ///hail mary right now

    [Header("Vertical Layout Overrides These..")]
    [SerializeField] int _startingX = 16;   ///0
    [SerializeField] int _startingY = 350;
    [SerializeField] int _yOffset = -39;  ///-65


    private GameObject _bORDERPREFAB;

    private int _ORDERFREQUENCY;
    private float _timeToOrder = float.MaxValue;

    private List<OrderButton> _orderList = new List<OrderButton>();


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
        GameManager.Instance.SetInventoryShipping(this);
        if (_bORDERPREFAB == null)
            _bORDERPREFAB = Resources.Load<GameObject>("Prefab/UI/bOrder_slot");

        _ORDERFREQUENCY = GameManager.Instance._orderFrequency;
        _componentList = GameManager.Instance._componentList;

    }
    /************************************************************************************************************************/


    public void Update()
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
        ob.SetOrder(itemID, GetEstimatedDeliveryTime());
        _orderList.Add(ob);
        bOrder.transform.SetParent(this.transform);
        bOrder.transform.localPosition = FindPosition(_orderList.Count - 1);
        bOrder.transform.localScale = new Vector3(1, 1, 1); /// no idea why these come in at 1.5, when the prefab and parent are at 1

        //Get data based off of the incoming value
    }


    public void RemoveOrder(int itemID)
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
                    RemoveOrder(order);
                    return;
                }
            }
        }
    }

    public void RemoveOrder(OrderButton orderButton)
    {
        if (_orderList.Contains(orderButton))
        {
            _orderList.Remove(orderButton);
        }
        else
            Debug.LogError("how is this not in the list");

        //TODO play animation then give the anim on.finish() this callback
        ButtonDestroyedCallback(orderButton);
    }


    /************************************************************************************************************************/

    /**Kittings "Final ItemIDs should be the final item(s) that go to shipping */

    private int PickAnItemIDFromFinalTask()
    {
        var manager = GameManager.instance.CurrentWorkStationManager;
        var list = manager.GetStationList();
        var lastStation = list[list.Count - 1];
        var lastTaskList = lastStation._tasks;
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
        List<ObjectManager.eItemID> componentsNeeded = _componentList.GetComponentListByItemID(finalItemId);
        int size = componentsNeeded.Count;
        ObjectManager.eItemID[] componentOrder = new ObjectManager.eItemID[size];

        foreach (var item in componentsNeeded)
        {
            componentOrder[GetUnusedIndex(size)] = item;
        }

        // printOrderList(componentOrder);
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

    private void ButtonDestroyedCallback(OrderButton orderButton)
    {
        Destroy(orderButton.gameObject);

        for (int i = 0; i < _orderList.Count; i++)
        {
            _orderList[i].transform.localPosition = FindPosition(i);
        }
    }
}