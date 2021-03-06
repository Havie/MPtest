﻿using HighlightPlus;
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

    /************************************************************************************************************************/


    public static void PickUpItem(ObjectController item)
    {
        //Debug.Log($"<color=green>pickup item</color> {item}");
        if (HandContains(item))
            return; ///might have to reorder queue instead if this is possible?

        if (CountPickedUpItems() > 1)
            DropItem(_handArray[1]);

        _handArray[1] = _handArray[0];
        _handArray[0] = item;

        if (_handArray[1] != null)
        {
            _handArray[1].PickedUp(2);
            _handArray[1].SetHandPreviewingMode(false);
        }

        _handArray[0].PickedUp(1);
        item.SetHandPreviewingMode(false);

        CheckHandPositions();
        CancelIntensityChangePreview();
        HandleEvents();
    }

    public static void DropItem(ObjectController item)
    {
        if (item)
        {
            bool weHaveItem = false;

            if (_handArray[0] == item)
            {
                weHaveItem = true;
                ///shift other item over to slot 0
                _handArray[0] = _handArray[1];
                if (_handArray[0] != null)
                    _handArray[0].PickedUp(1); ///reset our hand index
                _handArray[1] = null;
                //Debug.Log($"droped item");
            }
            else if (_handArray[1] == item)
            {
                weHaveItem = true;
                _handArray[1] = null;
            }
            if (weHaveItem)
            {
                item.PutDown();
                //Debug.Log($"Dropping item: <color=red>{item.gameObject} </color>");
                item.SetHandPreviewingMode(false);
                CheckHandPositions();
                CancelIntensityChangePreview();
            }

            if (GameManager.Instance && GameManager.Instance.IsTutorial)
            {
                TutorialEvents.CallOnPartDropped();
            }
        }

    }

    public static void DropAllItems()
    {
        Debug.Log($"static call heard");
        for (int i = 0; i < _handArray.Length; i++)
        {
            Debug.Log($"saying to drop: {_handArray[i]}");
            DropItem(_handArray[i]);
        }
    }

    public static void StartToHandleIntensityChange(IHighlightable potentialItemToBePickedUp)
    {
        Debug.Log($"[TODO] <color=yellow>HandManager not set up yet for IHighlightable </color>");
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
        potentialItemToBePickedUp.ChangeHighLightColor(ObjectManager.Instance._colorHand1);

        int numItemsInhand = CountPickedUpItems();

        if (numItemsInhand < 1)
            return;
        ///start to fade the first item in hand to orange 
        ObjectController firstItemInHand = _handArray[0];
        var currentColor = firstItemInHand.GetHighLightColor();
        var orangeColor = ObjectManager.Instance._colorHand2;
        firstItemInHand.ChangeHighLightColor(Color.Lerp(currentColor, orangeColor, 0.05f));
        ///Basing this off of the pickup times from UserInputManagerdoesnt look as good, the colors are too close
        /// so the change happens to fast, might as well just use 0.05f constant as it looks visually appealing
        // (UserInput.Instance._pressTimeCURR/UserInput.Instance._pressTimeMAX)/2));

        ///If we are already picking this item up, we dont wana do anything
        if (numItemsInhand < 2 || potentialItemToBePickedUp == _handArray[0] || potentialItemToBePickedUp == _handArray[1])
            return;
        ///start to fade out next item to be dropped
        ObjectController ItemToBeDroppedNext = _handArray[1];
        var currentIntensity2 = ItemToBeDroppedNext.GetHighlightIntensity();
        ItemToBeDroppedNext.ChangeHighlightAmount(currentIntensity2 - _intensityChange);


        if (!_previewingAChange)
        {
            SetHandPreviewMode(true);
            _previewTime = 0;
        }


        //Debug.Log($"#{numItemsInhand} .. potentialItemToBePickedUp={potentialItemToBePickedUp}, ItemToBeDroppedNext={ItemToBeDroppedNext}");
        //Debug.Log($"#{numItemsInhand} .. 1={_handArray[0]}, 2={_handArray[1]}");

        /// make the HandObject start to move towards this new item
        if (ItemToBeDroppedNext._handLocation != null && potentialItemToBePickedUp._handLocation != null)
        {
            ///weight is used to get the item to be closer to the center of new obj or hand loc in bottom corner. Just looks a bit better if stuff is fallen over
            float weight = Vector3.Distance(ItemToBeDroppedNext.transform.position, potentialItemToBePickedUp.transform.position);
            Vector3 avgPoint = Vector3.Lerp(potentialItemToBePickedUp._handLocation.position, potentialItemToBePickedUp.transform.position, weight);
            Vector3 previewPos = Vector3.Lerp(ItemToBeDroppedNext._handLocation.position, avgPoint, _previewTime);
            UIManager.UpdateHandLocation(2, previewPos);
        }
        _previewTime += Time.deltaTime;

    }


    public static void CancelIntensityChangePreview()
    {
        if (CountPickedUpItems() == 0)
            return;
        if (_previewingAChange)
            SetHandPreviewMode(false);
    }


    /// <summary> If object is about to be deleted use this instead </summary>
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

    /************************************************************************************************************************/


    private static int CountPickedUpItems()
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

    private static bool HandContains(ObjectController newItem)
    {
        for (int i = 0; i < _handSize; i++)
        {
            var item = _handArray[i];
            if (item == newItem)
                return true;
        }

        return false;
    }

    private static void SetHandPreviewMode(bool cond)
    {
        _previewingAChange = cond;
        if (CountPickedUpItems() == 1)
        {
            _handArray[0].SetHandPreviewingMode(cond);
            _handArray[0].ChangeHighlightAmount(_outlineIntensity);
            _handArray[0].ChangeHighLightColor(ObjectManager.Instance._colorHand1);

        }
        else if (CountPickedUpItems() == 2)
        {
            _handArray[0].SetHandPreviewingMode(cond);
            _handArray[0].ChangeHighlightAmount(_outlineIntensity);
            _handArray[1].SetHandPreviewingMode(cond);
            _handArray[1].ChangeHighlightAmount(_outlineIntensity);
            _handArray[0].ChangeHighLightColor(ObjectManager.Instance._colorHand1);
        }
        else
            _previewingAChange = false;

    }

    private static void CheckHandPositions()
    {
        if (CountPickedUpItems() < 2)
            UIManager.ResetHand(2);
        if (CountPickedUpItems() < 1)
            UIManager.ResetHand(1);
    }

    private static void PrintQueue()
    {
        string q = $"Hand[0] = { _handArray[0]} , Hand[1] = { _handArray[1]} ";
        Debug.LogWarning(q);
    }

    private static void HandleEvents()
    {
        if (GameManager.Instance && GameManager.Instance.IsTutorial)
        {
            TutorialEvents.CallOnPartPickedUp();
            ObjectController obj1 = _handArray[0];
            ObjectController obj2 = _handArray[1];
            if (obj1 != null && obj2 != null)
            {
                if (obj2._myID == ObjectRecord.eItemID.GreenRect1
                     && obj1._myID == ObjectRecord.eItemID.BlueBolt)
                {
                    TutorialEvents.CallOnHoldingHandleAndBolt();
                }
            }
        }
    }

}
