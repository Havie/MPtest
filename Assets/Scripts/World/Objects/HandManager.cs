using HighlightPlus;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class HandManager
{
    public static event Action<Queue<ObjectController>> OrderChanged = delegate { };
    public static Queue<ObjectController> _objects = new Queue<ObjectController>();
    private static int _handSize=2;
    private static ObjectController _firstInQueue;

    private static float _outlineIntensity = 0.4f; /// little awkward to not use the cached color profile on the prefab profile
    private static float _intensityChange = 0.01f;
    private static bool _previewingAChange = false;
    private static bool _isCloning = false;
    public static void PickUpItem(ObjectController item)
    {
        if (_objects.Contains(item))
            return; ///might have to reorder queue instead if this is possible?

        int handIndex = _objects.Count ==2 ? 2 : 1;

        // if (_objects.Count > _handSize - 1)
        if (handIndex == 2)
        {
            //Debug.Log($"Size={_objects.Count} and next in Queue is= {_objects.Peek()}");
            DropItem(_objects.Dequeue());
            _firstInQueue.PickedUp(2);
        }

        if(handIndex==1)
        {
            _firstInQueue = item;
        }
        Queue<ObjectController> cloned = CloneQueue();
        OrderChanged(cloned);
        item.ToggleRB(true);
        item.HandPreviewingMode = false;
        _objects.Enqueue(item);

       
        item.PickedUp(handIndex);

        CheckHandPositions();
        CancelIntensityChangePreview();
    }


   
    public static void StartToHandleIntensityChange(ObjectController potentialItemToBePickedUp)
    {
        var currentIntensity = potentialItemToBePickedUp.GetHighlightIntensity();
        potentialItemToBePickedUp.ChangeHighlightAmount(currentIntensity + _intensityChange);

        if (_objects.Count == 0)
            return;

        ObjectController ItemToBeDroppedNext = _objects.Peek();
        currentIntensity = ItemToBeDroppedNext.GetHighlightIntensity();
        ItemToBeDroppedNext.ChangeHighlightAmount(currentIntensity - _intensityChange);



        ///We could also make the HandObject start to move towards this new item
       if(!_previewingAChange)
            SetHandPreviewMode(true);
       ///weight is used to get the item to be closer to the center of new obj or hand loc in bottom corner. Just looks a bit better if stuff is fallen over
        float weight = Vector3.Distance(ItemToBeDroppedNext.transform.position, potentialItemToBePickedUp.transform.position);
        Vector3 avgPoint = Vector3.Lerp( potentialItemToBePickedUp._handLocation.position, potentialItemToBePickedUp.transform.position, weight);
        Vector3 previewPos = Vector3.Lerp(avgPoint , ItemToBeDroppedNext._handLocation.position, currentIntensity);
        UIManager.instance.UpdateHandLocation(2, previewPos);

    }

    public static void CancelIntensityChangePreview()
    {
        if (_objects.Count==0)
            return;
        if(_previewingAChange)
            SetHandPreviewMode(false);
    }

    private static void SetHandPreviewMode(bool cond)
    {
        var clonedQ = CloneQueue();
        while (clonedQ.Count > 0) ///not sure why this never happens???
        {
            var item = clonedQ.Dequeue();
            //if(cond)
            //    Debug.Log($"Setting hand preview mode to <color=green>{cond}</color> for {item.gameObject.name}");
            //else
            //    Debug.Log($"Setting hand preview mode to <color=red>{cond}</color> for {item.gameObject.name}");

            item.HandPreviewingMode = cond;
            item.ChangeHighlightAmount(_outlineIntensity);
        }
        _previewingAChange = cond;
    }


    public static void DropItem(ObjectController item)
    {
        if (item)
        {
            item.ToggleRB(false);
            item.PutDown();
            item.HandPreviewingMode = false;
            if (_objects.Contains(item))
            {
                var qItem = _objects.Dequeue();
                if (qItem == item)
                {
                    return;
                }
                else
                {
                    _objects.Clear();
                    _objects.Enqueue(qItem); ///our queue can only have two items so resetting the order is ez
                }
                Queue<ObjectController> cloned = CloneQueue() ;
                OrderChanged(cloned);
                if(_objects.Count>0)
                    _objects.Peek().PickedUp(1); ///resets the handIndex
            }

            CheckHandPositions();
            CancelIntensityChangePreview();
        }

    }


    /// <summary>
    /// If object is about to be deleted use this instead 
    /// </summary>
    /// <param name="item"></param>
    public static void RemoveDeletedItem(ObjectController item)
    {
        if (item)
        {
            item.PutDown();
            if (_objects.Contains(item))
            {
                var qItem = _objects.Dequeue();
                if (qItem == item)
                {
                    return;
                }
                else
                {
                    _objects.Clear();
                    _objects.Enqueue(qItem);
                }
                Queue<ObjectController> cloned = CloneQueue();
                OrderChanged(cloned);
            }
        }

        CheckHandPositions();
    }


    /// <summary>
    /// Consider using  if (_currentSelection.IsPickedUp) instead
    /// </summary>
    /// <param name="objectController"></param>
    /// <returns></returns>
    public static bool Contains(ObjectController objectController)
    {
        Queue<ObjectController> cloned = CloneQueue();
        while(cloned.Count>0)
        {
            var item = cloned.Dequeue();
            if (item == objectController)
                return true;
        }

        return false;
    }

    public static void PrintQueue()
    {
        Queue<ObjectController> objects2 = new Queue<ObjectController>();
        string q = "qList=  ";
        while(_objects.Count>0)
        {
            var item = _objects.Dequeue();
            q += " item:" + item.name;
            objects2.Enqueue(item);
        }

        _objects = objects2;
        Debug.LogWarning(q);
    }

    public static void CheckHandPositions()
    {
        if (_objects.Count < 2)
            UIManager.instance.ResetHand(2);
        if (_objects.Count < 1)
            UIManager.instance.ResetHand(1);
    }

    private static Queue<ObjectController> CloneQueue()
    {
        if (_isCloning) ///wait for it to finish?
            return CloneQueue();  // return Invoke("CloneQueue", .1f);

        _isCloning = true;
        Queue<ObjectController> objects1 = new Queue<ObjectController>();
        Queue<ObjectController> objects2 = new Queue<ObjectController>();
        Queue<ObjectController> reversed = new Queue<ObjectController>();

        while (_objects.Count > 0)
        {
            var item = _objects.Dequeue();
            reversed.Enqueue(item);
        }

        while (reversed.Count > 0)
        {
            var item = reversed.Dequeue();
            objects1.Enqueue(item);
            objects2.Enqueue(item);
        }
        _objects = objects1;
        _isCloning = false;
        return objects2;
    }




}
