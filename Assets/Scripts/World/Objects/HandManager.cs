using HighlightPlus;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class HandManager
{
    public static event Action<Queue<ObjectController>> OrderChanged = delegate { };

    private static int _handSize = 2;
    public static ObjectController[] _handArray = new ObjectController[_handSize];


    private static float _outlineIntensity = 0.4f; /// little awkward to not use the cached color profile on the prefab profile
    private static float _intensityChange = 0.01f;
    private static bool _previewingAChange = false;
    private static float _previewTime;


    public static int CountPickedUpItems()
    {
        int count = 0;
        for (int i = 0; i < _handSize; i++)
        {
            var item = _handArray[i];
            if (item != null)
                ++count;
        }
        return count;
    }

    public static bool HandContains(ObjectController newItem)
    {
        for (int i = 0; i < _handSize; i++)
        {
            var item = _handArray[i];
            if (item == newItem)
              return true;
        }

        return false;
    }

    public static void PickUpItem(ObjectController item)
    {
        if (HandContains(item))
            return; ///might have to reorder queue instead if this is possible?

        if (CountPickedUpItems() > 1)
            DropItem(_handArray[1]);

        _handArray[1] = _handArray[0];
        _handArray[0] = item;

        if (_handArray[1])
        {
            _handArray[1].PickedUp(2);
            _handArray[1].HandPreviewingMode = false;
        }

        _handArray[0].PickedUp(1);
        item.HandPreviewingMode = false;

        item.ToggleRB(true);


        CheckHandPositions();
        CancelIntensityChangePreview();
    }


   
    public static void StartToHandleIntensityChange(ObjectController potentialItemToBePickedUp)
    {
        if (potentialItemToBePickedUp == null)
        {
            Debug.LogWarning("Incoming item is null, todo change to IInteractble");
            return;
        }

        ///start to fade in next item to be picked up
        var currentIntensity = potentialItemToBePickedUp.GetHighlightIntensity();
        potentialItemToBePickedUp.ChangeHighlightAmount(currentIntensity + _intensityChange);
        potentialItemToBePickedUp.ChangeHighLightColor(BuildableObject.Instance._colorHand1);

        int numItemsInhand = CountPickedUpItems();

        if (numItemsInhand < 1)
            return;
        ///start to fade the first item in hand to orange 
        ObjectController firstItemInHand = _handArray[0];
        var currentColor = firstItemInHand.GetHighLightColor();
        var orangeColor = BuildableObject.Instance._colorHand2;
        firstItemInHand.ChangeHighLightColor(Color.Lerp(currentColor, orangeColor, 0.05f));
        ///Basing this off of the pickup times from UserInput doesnt look as good, the colors are too close
        /// so the change happens to fast, might as well just use 0.05f constant as it looks visually appealing
        // (UserInput.Instance._pressTimeCURR/UserInput.Instance._pressTimeMAX)/2));


        if (numItemsInhand < 2)
            return;

        ///start to fade out next item to be dropped
        ObjectController ItemToBeDroppedNext = _handArray[1];
        var currentIntensity2 = ItemToBeDroppedNext.GetHighlightIntensity();
        ItemToBeDroppedNext.ChangeHighlightAmount(currentIntensity2 - _intensityChange);

 



        ///We could also make the HandObject start to move towards this new item
        if (!_previewingAChange)
        {
            SetHandPreviewMode(true);
            _previewTime = 0;
        }

        if (ItemToBeDroppedNext._handLocation != null && potentialItemToBePickedUp._handLocation != null)
        {
            ///weight is used to get the item to be closer to the center of new obj or hand loc in bottom corner. Just looks a bit better if stuff is fallen over
            float weight = Vector3.Distance(ItemToBeDroppedNext.transform.position, potentialItemToBePickedUp.transform.position);
            Vector3 avgPoint = Vector3.Lerp(potentialItemToBePickedUp._handLocation.position, potentialItemToBePickedUp.transform.position, weight);
            Vector3 previewPos = Vector3.Lerp(ItemToBeDroppedNext._handLocation.position, avgPoint, _previewTime);
            UIManager.instance.UpdateHandLocation(2, previewPos);
        }
        _previewTime += Time.deltaTime;

    }



    public static void CancelIntensityChangePreview()
    {
        if (CountPickedUpItems()==0)
            return;
        if(_previewingAChange)
            SetHandPreviewMode(false);
    }

    private static void SetHandPreviewMode(bool cond)
    {
        _previewingAChange = cond;
        if (CountPickedUpItems() == 1)
        {
            _handArray[0].HandPreviewingMode = cond;
            _handArray[0].ChangeHighlightAmount(_outlineIntensity);
            _handArray[0].ChangeHighLightColor(BuildableObject.Instance._colorHand1);

        }
        else if (CountPickedUpItems() == 2)
        {
            _handArray[0].HandPreviewingMode = cond;
            _handArray[0].ChangeHighlightAmount(_outlineIntensity);
            _handArray[1].HandPreviewingMode = cond;
            _handArray[1].ChangeHighlightAmount(_outlineIntensity);
            _handArray[0].ChangeHighLightColor(BuildableObject.Instance._colorHand1);
        }
        else
            _previewingAChange = false;

    }


    public static void DropItem(ObjectController item)
    {
        if (item)
        {
            item.ToggleRB(false);
            item.PutDown();
           // Debug.Log($"Dropping item: <color=red>{item.gameObject} </color>");
            item.HandPreviewingMode = false;
            if (_handArray[0] == item)
            {
                ///shift other item over to slot 0
                _handArray[0] = _handArray[1];
                if (_handArray != null)
                    _handArray[0].PickedUp(1); ///reset our hand index
                _handArray[1] = null;
            }
            else if(_handArray[1] == item)
            {
                _handArray[1] = null;
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
           // if (_handArray[0] == item)
            {
              ///Seems we just need to remove everything when we make a new object now
                _handArray[0] = null;
                _handArray[1] = null;
            }
            //else if (_handArray[1] == item)
            //{
            //    _handArray[1] = null;
            //}

            CheckHandPositions();
        }
    }



    public static void PrintQueue()
    {
        string q = $"Hand[0] = { _handArray[0]} , Hand[1] = { _handArray[1]} ";
        Debug.LogWarning(q);
    }

    public static void CheckHandPositions()
    {
        if (CountPickedUpItems() < 2)
            UIManager.instance.ResetHand(2);
        if (CountPickedUpItems() < 1)
            UIManager.instance.ResetHand(1);
    }




}
