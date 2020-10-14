using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIKitting : MonoBehaviour
{

    public GameObject _bORDERPREFAB;

    private int _ORDERFREQUENCY;
    private float _timeToOrder;

    private List<OrderButton> _orderList = new List<OrderButton>();
    private int _startingY = 350;
    private int _yOffset = -65;

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
        GameManager.instance.SetInventroyKitting(this);
        if (_bORDERPREFAB == null)
            _bORDERPREFAB = Resources.Load<GameObject>("Prefab/UI/bOrder");
       _ORDERFREQUENCY = GameManager.instance._orderFrequency;
    }

    public void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.R))
             RemoveOrder(_OrderList[_OrderList.Count / 2]);*/

        if (_timeToOrder > _ORDERFREQUENCY)
            SendInNewOrder();
        else
            _timeToOrder += Time.deltaTime;
    }

    /**Kittings "Final ItemIDs should be the final item(s) that go to shipping */
    private void SendInNewOrder()
    {
        _timeToOrder = 0;
        var workStation = GameManager.instance._workStation;
        //Get the kitting task (should be only task)
        foreach(Task t in workStation._tasks)
        {
            if(t.isKittingStation)
            {
                //Remove the count-1 when we get sprites for every type
               int rng= Random.Range(0, t._finalItemID.Count);
               int itemid= (int)t._finalItemID[rng];
               //Debug.Log($"rng={rng} from max of {t._finalItemID.Count } , thus final itemID={itemid}");
               AddOrder(itemid);
            }
        }
      
    }

    public void AddOrder(int itemID)
    {
        var bOrder = GameObject.Instantiate(_bORDERPREFAB);
        OrderButton ob = bOrder.GetComponent<OrderButton>();
        //Debug.Log("assign item with ID:" + itemID);
        ob.SetOrder(itemID);
        _orderList.Add(ob);
        bOrder.transform.SetParent(this.transform);
        bOrder.transform.localPosition = FindPosition(_orderList.Count - 1);

        //Get data based off of the incoming value
    }

    public void RemoveOrder(int itemID)
    {
        //Debug.Log(_orderList.Count +" size  , Remove order with ItemID : " + itemID);
        /* for (int i = _orderList.Count-1; i >0 ; i--)*/
        for (int i =0; i < this.transform.childCount ; i++)
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

    private Vector3 FindPosition(int index)
    {
        return new Vector3(0, _startingY + (_yOffset *index), 0);
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
