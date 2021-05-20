using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartDropper : MonoSingleton<PartDropper>
{
    [Range(0.1f, 0.5f)]
    [SerializeField] float _dropDelayBetweenParts = 0.2f;
    
    int _ORDERFREQUENCY;
    List<ObjectRecord.eItemID[]> _partOrders = new List<ObjectRecord.eItemID[]>();
    bool _routineIsRunning = false;



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

    public void SendInOrder(ObjectRecord.eItemID[] componentOrder)
    {
        if (!_routineIsRunning && _partOrders.Count == 0)
            StartCoroutine(DropInParts(componentOrder));
        else
            _partOrders.Add(componentOrder);
    }

    IEnumerator DropInParts(ObjectRecord.eItemID[] componentOrder)
    {
        _routineIsRunning = true;
        int count = componentOrder.Length;

        for (int i = 0; i < count; i++)
        {
            ObjectManager.Instance.DropItemInWorld((int)componentOrder[i]);
            yield return new WaitForSeconds(_dropDelayBetweenParts);
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
            List<QualityData> qualityList = slot.RebuildQualities();
            slot.RemoveItem();
            ObjectManager.Instance.DropItemInWorld(itemID, qualityList);
        }

    }

}
