using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartDropper : MonoBehaviour
{
    public static PartDropper Instance;

    ///Exposed for inspector debugging
    //[SerializeField]
    List<ObjectManager.eItemID[]> _partOrders = new List<ObjectManager.eItemID[]>();
    float _dropDelay = 0;
    bool _routineIsRunning = false;

    private int _ORDERFREQUENCY;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    void Start()
    {
        _ORDERFREQUENCY = GameManager.instance._orderFrequency;
    }

   void LateUpdate()
    {
        if(!_routineIsRunning && _partOrders.Count!=0)
        {
            var nextList = _partOrders[0];
            _partOrders.Remove(nextList);
            StartCoroutine(DropInParts(nextList));
        }
    }

    public void SendInOrder(ObjectManager.eItemID[] componentOrder)
    {
        if (!_routineIsRunning && _partOrders.Count == 0)
            StartCoroutine(DropInParts(componentOrder));
        else
            _partOrders.Add(componentOrder);
    }

    IEnumerator DropInParts(ObjectManager.eItemID[] componentOrder)
    {
        _routineIsRunning = true;
        int count = componentOrder.Length;
        _dropDelay = _ORDERFREQUENCY/((float)count) /5 ;
       // Debug.Log($"The Drop Delay is :{_dropDelay}  becuz {_ORDERFREQUENCY} / {(float)count} ");

        for (int i = 0; i < count; i++)
        {
            BuildableObject.Instance.DropItemInWorld((int)componentOrder[i]);
            yield return new WaitForSeconds(_dropDelay);
        }
        _routineIsRunning = false;
    }


    public void DropPartsOnDemand(List<UIInventorySlot> inUseSlots)
    {
        if (inUseSlots == null || inUseSlots.Count == 0)
            return;

        foreach (UIInventorySlot slot  in inUseSlots)
        {
            int itemID = slot.GetItemID();
            var qualityList = slot.RebuildQualities();
            slot.RemoveItem();
            BuildableObject.Instance.DropItemInWorld(itemID, qualityList);
        }

    }

}
