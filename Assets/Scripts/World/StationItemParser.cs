﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StationItemParser
{


    public static List<int> ParseItemsAsOUT(int batchSize, bool isStackable, WorkStationManager wm, WorkStation myWS)
    {
        List<int> itemIDs = new List<int>();
        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (batchSize == 1 || isStackable) /// PULL SYSTEM
        {
            ///get last task at my station and put in its final item:
            Task t = myWS.Tasks[myWS.TaskCount - 1];
            ///Have to wrap this incase stackable batch inventory is enabled:
            for (int i = 0; i < batchSize; ++i)
            {
                foreach (var item in t._finalItemID)
                {
                    itemIDs.Add((int)item);
                }
            }

            return itemIDs;
        }

        ///BATCH>1 :
        ///Sum the total required items (not self) of all subseqential workstations, and * BATCH_SIZE
        int[] stationSequence = StationSequenceReader.GetProperSequence(wm);
        var stationList = wm.GetStationList();
        ///Figure out myplace in Sequence 
        int startingIndex = StationSequenceReader.FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        bool isKittingStation = myWS.isKittingStation();
        ///shud only b the final item of last task
        for (int i = startingIndex; i < stationSequence.Length; i++)
        {
            WorkStation ws = stationList[i];
            //Debug.Log($"<color=white>Looking at workstation:</color> {ws}::{ws.StationName}");
            for (int taskIndex = 0; taskIndex < ws.TaskCount; ++taskIndex)
            {
                Task t = ws.Tasks[taskIndex]; ///get the current task 
                if (!isKittingStation && ws == myWS)  ///Exit early
                {
                    ///look at the immediate output for next station final ID, then pass on basic items for others
                    //add my own output
                    AddSelfItems(ws, taskIndex, t);
                }
                else if (i == startingIndex + 1 && isStackable)
                {
                    ParseNextStationsRequiredItems(isKittingStation, taskIndex, t);
                }
                else
                {
                    ParseRequiredItems(t, isKittingStation);
                }
            }

        }

        return itemIDs;

        ///LOCAL FUNCTIONS:
        void AddSelfItems(WorkStation local_ws, int local_count, Task local_task)
        {
            //Debug.Log($"..<color=green>We are parsing self items for task:</color> {local_task}");
            if (local_count == local_ws.TaskCount - 1 && !isStackable) // look at the last task at this station and add its produced items
            {
                foreach (var item in local_task._finalItemID) /// final produced items
                {
                    //Debug.Log($"..<color=orange>LOOK:</color> {item} from {local_task}");
                    if (!ObjectManager.Instance.IsBasicItem(item)) /// only add non-basic items
                    {
                        int itemId = (int)item;
                        // Debug.Log($"..<color=yellow>adding non-basic item:</color> {itemId} from {local_task}");
                        for (int i = 0; i < batchSize; i++)
                        {
                            itemIDs.Add((int)item);
                        }
                    }
                }
            }
        }
        void ParseNextStationsRequiredItems(bool isKitting, int local_count, Task local_task)
        {
            if (local_count == 0) // look at the last task at this station and add its produced items
            {
                Debug.Log($"..<color=orange>We are parsing required  items for task:</color> {local_task}");
                foreach (var item in local_task._requiredItemIDs) /// look at all of its _requiredItemIDs items
                {
                    if (isKitting)
                    {
                        if (ObjectManager.Instance.IsBasicItem(item)) ///decide if basic item 
                        {
                            int itemId = (int)item;

                            for (int j = 0; j < batchSize; j++)
                            {
                                itemIDs.Add((int)item);
                            }
                        }
                    }
                    else
                    {
                        if (!ObjectManager.Instance.IsBasicItem(item)) ///decide if basic item 
                        {
                            int itemId = (int)item;

                            for (int j = 0; j < batchSize; j++)
                            {
                                itemIDs.Add((int)item);
                            }
                        }
                    }
                }
            }
        }
        void ParseRequiredItems(Task local_task, bool isKitting)
        {
            //Debug.Log($"..<color=blue>We are parsing required basic items for task:</color> {local_task}");
            foreach (var item in local_task._requiredItemIDs) /// look at all of its _requiredItemIDs items
            {
                if (!isStackable || isKitting)
                {
                    if (ObjectManager.Instance.IsBasicItem(item)) ///decide if basic item 
                    {
                        int itemId = (int)item;
                        //Debug.Log($"{local_task}<color=orange> Addng basic item : </color> {itemId}");
                        for (int j = 0; j < batchSize; j++)
                        {
                            itemIDs.Add((int)item);
                        }

                    }
                }
            }
        }
    }
    public static List<int> ParseItemsAsIN(int batchSize, bool isStackable, WorkStationManager wm, WorkStation myWS)
    {

        List<int> itemIDs = new List<int>();
        int[] stationSequence = StationSequenceReader.GetProperSequence(wm);
        var stationList = wm.GetStationList();
        ///Figure out myplace in Sequence 
        int startingIndex = StationSequenceReader.FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        // Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (batchSize == 1 || isStackable)
        {
            ///look at the last tasks final items in the station before mine
            AddLastTasksFinalitemsToList();
        }
        else  
        {   ///look at all final items for station before me , and the basic items from kitting[1]
            AddLastTasksFinalitemsAndBasicItemsToList();
        }
        //Debug.Log($"The # of INV items will be : {itemIDs.count}");
        return itemIDs;

        ///**********************LOCAL FUNCTIONS: *******************************
        List<int> FindObjectsAtKittingStation(WorkStation ws)
        {
            if (!ws.isKittingStation())
                Debug.LogError("Wrong order, kitting isnt @ index 1");

            List<int> items = new List<int>();
            foreach (Task t in ws.Tasks)
            {
                foreach (var item in t._finalItemID)
                    items.Add((int)item);
            }

            return items;
        }

        void AddLastTasksFinalitemsToList()
        {
            if (startingIndex > 1) /// no kitting on pull, so second station
            {
                WorkStation ws = stationList[startingIndex - 1];
                Task lastTask = ws.Tasks[ws.TaskCount - 1];
                // Debug.Log($"# of items at Task:{lastTask} is {lastTask._finalItemID.Count}");
                ///Have to wrap this incase stackable batch inventory is enabled:
                for (int i = 0; i < batchSize; ++i)
                {
                    foreach (var item in lastTask._finalItemID)
                    {
                        itemIDs.Add((int)item);
                    }
                }

            }
        }

        void AddLastTasksFinalitemsAndBasicItemsToList()
        {
            var listItems = FindObjectsAtKittingStation(stationList[1]);
            // Debug.Log("Staring index=+" + startingIndex);
            ///foreach station between us and kitting, if listItem contains a requiredItem, remove it
            if (startingIndex > 2) //1 = kitting
            {
                for (int i = 0; i < startingIndex; i++)
                {
                    WorkStation ws = stationList[i];
                    foreach (Task t in ws.Tasks)
                    {
                        foreach (var item in t._requiredItemIDs)
                        {
                            int itemId = (int)item;
                            // Debug.Log($"_requiredItems.. Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                            if (listItems.Contains(itemId))
                                listItems.Remove(itemId);
                        }
                        ///were at prior station
                        if (i == startingIndex - 1)
                        {
                            ///Add the final items from station prior to me
                            foreach (var item in t._finalItemID)
                            {
                                int itemId = (int)item;
                                listItems.Add(itemId);
                                //Debug.Log($"_finalItems....Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                            }
                        }
                    }
                }
            }
            ///finally we can add what we found
            foreach (var item in listItems)
            {
                for (int j = 0; j < batchSize; j++)
                {
                    if (!isStackable)
                    {
                        itemIDs.Add((int)item);
                    }
                    else
                    {
                        if (!ObjectManager.Instance.IsBasicItem((ObjectRecord.eItemID)item))
                        {
                            itemIDs.Add((int)item);
                        }
                    }
                }
            }
        }
    }
    public static List<int> ParseItemsAsStation(int batchSize, bool isStackable, WorkStationManager wm, WorkStation myWS)
    {
        List<int> seenItems = new List<int>();
        if (batchSize == 1 || isStackable) ///PULL & Stackable Batch, works for now if 1 task per station
        {
            foreach (Task t in myWS.Tasks)
            {
                foreach (var item in t._requiredItemIDs)
                {
                    seenItems.Add((int)item);
                }

                //Debug.Log($"<color=yellow>batch size is 1 and itemCount </color> ={t._requiredItemIDs.Count} for Task:{t}");
            }
        }
        //else if ( isStackable) ///STACKABLE BATCH
        //{
        //Found a bit of a shortcut / hack.Since we are not going to allow users to customize the tasks lists this version of LEAN, we can just cut the tasks lists down to 1 per station, then StationItemParser can treat batch == 1 or isStackable the same
        //This will have to change if we revisit the idea of modular task assignments
        //    /// if WS .TASK list > 1 we have to look at all our tasks and figure out what parts
        //    /// we can assemble with our in inventory, 
        //    /// then only includethe parts not included to do final item ID on last Task
        //}
        else ///BATCH
        {

            int[] stationSequence = StationSequenceReader.GetProperSequence(wm);
            var stationList = wm.GetStationList();
            //Figure out myplace in Sequence 
            int startingIndex = StationSequenceReader.FindPlaceInSequence(stationSequence, (int)myWS._myStation);


            if (myWS.isKittingStation()) // look at everyones required items
            {
                //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
                LookAtEveryStationsItems(stationSequence, stationList, startingIndex, seenItems);
            }
            else // just look at my own items
            {
                LookAtOwnStationItems(myWS, seenItems);

            }
        }

        return seenItems;

        ///LOCAL FUNCTIONS:
        void LookAtEveryStationsItems(int[] sSequence, List<WorkStation> sList, int sindex, List<int> itemList)
        {
            for (int i = sindex; i < sSequence.Length; i++)
            {
                WorkStation ws = sList[i]; /// think this is in order?
                foreach (Task t in ws.Tasks)
                {
                    //verify no duplicates
                    foreach (var item in t._requiredItemIDs)
                    {
                        if (ObjectManager.Instance.IsBasicItem(item)) //only want basic parts
                        {
                            int itemId = (int)item;
                            if (!itemList.Contains(itemId))
                            {
                                itemList.Add(itemId);
                            }
                        }
                    }

                }

            }
        }
        void LookAtOwnStationItems(WorkStation ws, List<int> sItems)
        {
            foreach (var task in ws.Tasks)
            {
                foreach (var item in task._requiredItemIDs)
                {
                    if (ObjectManager.Instance.IsBasicItem(item))
                    {
                        int itemId = (int)item;
                        if (!sItems.Contains(itemId))
                        {
                            sItems.Add(itemId);
                        }
                    }
                }
            }
        }
    }
    public static List<int> ParseItemsAsDefect(int batchSize, WorkStationManager wm, WorkStation myWS)
    {
        return ParseItemsAsOUT(batchSize, false, wm, myWS);
    }


    /************************************************************************************************************************/

 

}
