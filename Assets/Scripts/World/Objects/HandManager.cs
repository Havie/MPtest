using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HandManager
{
    public static Queue<ObjectController> _objects = new Queue<ObjectController>();
    private static int _handSize=2;

    public static void PickUpItem(ObjectController item)
    {
        if (_objects.Contains(item))
            return;

        if (_objects.Count > _handSize - 1)
        {
            //Debug.Log($"Size={_objects.Count} and next in Queue is= {_objects.Peek()}");
            DropItem(_objects.Dequeue());
        }

        item.ToggleRB(true);
        _objects.Enqueue(item);
    }

    public static void DropItem(ObjectController item)
    {
        if (item)
        {
            item.ToggleRB(false);
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
                    _objects.Enqueue(qItem);
                }
                    
            }
        }
    }

    /// <summary>
    /// If object is about to be deleted use this
    /// </summary>
    /// <param name="item"></param>
    public static void RemoveItem(ObjectController item)
    {
        if (item)
        {
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

            }
        }
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


  

}
