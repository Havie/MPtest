using HighlightPlus;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class HandManager
{
    public static event Action<Queue<ObjectController>> OrderChanged = delegate { };
    public static Queue<ObjectController> _objects = new Queue<ObjectController>();
    private static int _handSize=2;

    public static void PickUpItem(ObjectController item)
    {
        if (_objects.Contains(item))
            return; ///might have to reorder queue instead if this is possible?

        if (_objects.Count > _handSize - 1)
        {
            //Debug.Log($"Size={_objects.Count} and next in Queue is= {_objects.Peek()}");
            DropItem(_objects.Dequeue());
        }
        Queue<ObjectController> cloned = CloneQueue();
        OrderChanged(cloned);
        item.ToggleRB(true);
        item.PickedUp();
        _objects.Enqueue(item);
        CheckHandPositions();
    }

    public static void DropItem(ObjectController item)
    {
        if (item)
        {
            item.ToggleRB(false);
            item.PutDown();
            if(_objects.Contains(item))
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
            }

            CheckHandPositions();
        }

    }


    /// <summary>
    /// If object is about to be deleted use this
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
        Queue<ObjectController> objects1 = new Queue<ObjectController>();
        Queue<ObjectController> objects2 = new Queue<ObjectController>();
        while (_objects.Count > 0)
        {
            var item = _objects.Dequeue();
            objects1.Enqueue(item);
            objects2.Enqueue(item);
        }
        _objects = objects1;
        return objects2;
    }




}
