using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StationItemParser
{
    public static List<int> ParseItemsAsOUT(int batchSize, WorkStationManager wm, WorkStation myWS)
    {
        List<int> itemIDs = new List<int>();

        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (batchSize == 1) /// PULL SYSTEM
        {
            ///get last task at my station and put in its final item:
            Task t = myWS._tasks[myWS._tasks.Count - 1];

            foreach (var item in t._finalItemID)
            {
                itemIDs.Add((int)item);
            }

            return itemIDs;
        }

        ///BATCH>1 :
        ///Sum the total required items (not self) of all subseqential workstations, and * BATCH_SIZE
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        ///Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        bool isKittingStation = myWS.isKittingStation();
        ///shud only b the final item of last task
        for (int i = startingIndex; i < stationSequence.Length; i++)
        {
            WorkStation ws = stationList[i];
            Debug.Log($"<color=white>Looking at workstation:</color> {ws}::{ws._stationName}");
            for (int taskIndex = 0; taskIndex < ws._tasks.Count; ++taskIndex)
            {
                Task t = ws._tasks[taskIndex]; ///get the current task 
                if (!isKittingStation)  ///Exit early
                {
                    ///look at the immediate output for next station final ID, then pass on basic items for others
                    if (ws == myWS) //add my own output
                    {
                        AddSelfItems(ws, taskIndex, t);
                    }
                    else
                    {
                        ParseRequiredItems(t);
                    }

                }
            }

        }

        return itemIDs;

        ///LOCAL FUNCTIONS:
        void AddSelfItems(WorkStation local_ws, int local_count, Task local_task)
        {
            Dictionary<int, List<int>> _finalItemsPairedWithReqItems = new Dictionary<int, List<int>>();


            Debug.Log($"..<color=green>We are parsing self items for task:</color> {local_task}");
            if (local_count == local_ws._tasks.Count - 1) // look at the last task at this station and add its produced items
            {
                foreach (var item in local_task._finalItemID) /// final produced items
                {
                    if (!BuildableObject.Instance.IsBasicItem(item)) /// only add non-basic items
                    {
                        int itemId = (int)item;
                        Debug.Log($"..<color=yellow>adding non-basic item:</color> {itemId} from {local_task}");
                        for (int i = 0; i < batchSize; i++)
                        {
                            itemIDs.Add((int)item);
                        }
                    }
                }
                foreach (var reqItem in local_task._requiredItemIDs) /// final produced items
                {

                }
            }
            else //add the batch items to pass along to other stations
            {
                foreach (var item in local_task._requiredItemIDs) /// look at all of its required items
                {
                    if (BuildableObject.Instance.IsBasicItem(item)) ///only add basic items
                    {
                        int itemId = (int)item;
                        Debug.Log($"..<color=red>adding basic item:</color> {itemId} from {local_task}");
                        for (int j = 0; j < batchSize; j++)
                        {
                            itemIDs.Add((int)item);
                        }
                        // Debug.LogWarning($" (2)...Task::{t} adding item:{item} #{itemId}");
                    }
                }
            }
        }

        void ParseRequiredItems(Task local_task)
        {
            Debug.Log($"..<color=blue>We are parsing required basic items for task:</color> {local_task}");
            foreach (var item in local_task._requiredItemIDs) /// look at all of its _requiredItemIDs items
            {
                if (BuildableObject.Instance.IsBasicItem(item)) ///decide if basic item 
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
    public static List<int> ParseItemsAsIN(int batchSize, WorkStationManager wm, WorkStation myWS)
    {

        List<int> itemIDs = new List<int>();
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        ///Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        // Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (batchSize == 1)
        {
            ///look at the last tasks final items in the station before mine
            if (startingIndex > 1) /// no kitting on pull, so second station
            {
                WorkStation ws = stationList[startingIndex - 1];
                Task lastTask = ws._tasks[ws._tasks.Count - 1];
                // Debug.Log($"# of items at Task:{lastTask} is {lastTask._finalItemID.Count}");


                foreach (var item in lastTask._finalItemID)
                {
                    itemIDs.Add((int)item);
                }

            }
        }
        else  ///look at all final items for station before me , and the basic items from kitting[1]
        {
            var listItems = FindObjectsAtKittingStation(stationList[1]);
            // Debug.Log("Staring index=+" + startingIndex);
            ///foreach station between us and kitting, if listItem contains a requiredItem, remove it
            if (startingIndex > 2) //1 = kitting
            {
                for (int i = 0; i < startingIndex; i++)
                {
                    WorkStation ws = stationList[i];
                    foreach (Task t in ws._tasks)
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
                                // Debug.Log($"_finalItems....Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");

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
                    itemIDs.Add((int)item);
                }
            }


        }
        //Debug.Log($"The # of INV items will be : {itemIDs.count}");
        return itemIDs;

        ///LOCAL FUNCTIONS:
        List<int> FindObjectsAtKittingStation(WorkStation ws)
        {
            if (!ws.isKittingStation())
                Debug.LogError("Wrong order, kitting isnt @ index 1");

            List<int> items = new List<int>();
            foreach (Task t in ws._tasks)
            {
                foreach (var item in t._finalItemID)
                    items.Add((int)item);
            }

            return items;
        }
    }
    public static List<int> ParseItemsAsStation(int batchSize, WorkStationManager wm, WorkStation myWS)
    {
        List<int> seenItems = new List<int>();
        if (batchSize == 1)
        {
            foreach (Task t in myWS._tasks)
            {
                foreach (var item in t._requiredItemIDs)
                {
                    seenItems.Add((int)item);
                }

                Debug.Log($"<color=yellow>batch size is 1 and itemCount </color> ={t._requiredItemIDs.Count} for Task:{t}");
            }
        }
        else
        {

            int[] stationSequence = getProperSequence(wm, myWS);
            var stationList = wm.GetStationList();
            //Figure out myplace in Sequence 
            int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);


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
                foreach (Task t in ws._tasks)
                {
                    //verify no duplicates
                    foreach (var item in t._requiredItemIDs)
                    {
                        if (BuildableObject.Instance.IsBasicItem(item)) //only want basic parts
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
            foreach (var task in ws._tasks)
            {
                foreach (var item in task._requiredItemIDs)
                {
                    if (BuildableObject.Instance.IsBasicItem(item))
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
        return ParseItemsAsOUT(batchSize, wm, myWS);
    }


    /************************************************************************************************************************/

    static void PrintASequence(int[] sequence, string seqName)
    {
        string p = "";
        for (int i = 0; i < sequence.Length; ++i)
        {
            p += $" , {sequence[i]}";
        }
        //Debug.Log(seqName+ ": " + p);
    }

    /** This is kind of a mess, thinking of making a doubly linked list class at some point*/
    static int[] getProperSequence(WorkStationManager wm, WorkStation myWS)
    {
        int[] sequence = new int[wm.GetStationCount() + 1];
        //Debug.LogWarning("sequence size=" + wm.GetStationCount() + 1);
        foreach (WorkStation ws in wm.GetStationList())
        {
            //figure out sequence (*Exclude staiton 0 cuz SELF testing*)
            //each ws knows where its sending stuff , so we need to build the path?
            //Debug.Log($"{(int)ws._myStation} = {(int)ws._sendOutputToStation}");
            if ((int)ws._myStation != 0)
                sequence[(int)ws._myStation] = (int)ws._sendOutputToStation;

        }
        PrintASequence(sequence, "initial");
        //Find End:
        int endIndex = -1;
        for (int i = 1; i < sequence.Length - 1; ++i)
        {
            if (sequence[i] == (int)WorkStation.eStation.NONE)
            {
                endIndex = i;
                break;
            }
        }
        //Debug.Log("endIndex=" + endIndex);
        //Rebuild from backwards:
        int[] backwardSequence = new int[wm.GetStationCount()];
        backwardSequence[0] = endIndex;
        int totalSeen = 1;
        while (totalSeen != backwardSequence.Length)
        {
            // Debug.Log($"(while)::endIndex= {endIndex}  and val at that index= {sequence[endIndex]} " );
            for (int i = 1; i < sequence.Length - 1; i++)
            {
                if (sequence[i] == endIndex)
                {
                    backwardSequence[totalSeen] = i;
                    endIndex = i;
                    break;
                }
            }

            ++totalSeen;
        }
        PrintASequence(backwardSequence, "backwards");
        int[] finalSequence = new int[wm.GetStationCount()];
        for (int i = 0; i < backwardSequence.Length; i++)
        {
            //Debug.Log($"final:{i} = bs({backwardSequence.Length - 1 - i}):{backwardSequence[backwardSequence.Length - 1 - i]} ");
            finalSequence[i] = backwardSequence[backwardSequence.Length - 1 - i];
        }
        PrintASequence(finalSequence, "final");
        return finalSequence;
    }

    static int FindPlaceInSequence(int[] sequence, int stationID)
    {
        int index = 0;
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] == stationID)
                return i;
        }

        return index;
    }

    static int SumSequence(WorkStationManager wm, WorkStation myWS, bool reqItemsOverFinalItems, bool includeSelf, bool excludeDuplicates)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        List<int> seenItems = new List<int>();
        if (!includeSelf)
            ++startingIndex;
        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        for (int i = startingIndex; i < stationSequence.Length; i++)
        {
            WorkStation ws = stationList[i]; /// think this is in order?
            foreach (Task t in ws._tasks)
            {
                if (reqItemsOverFinalItems)
                {
                    if (!excludeDuplicates)
                        count += t._requiredItemIDs.Count;
                    else   //verify no duplicates
                    {
                        foreach (var item in t._requiredItemIDs)
                        {
                            if (BuildableObject.Instance.IsBasicItem(item)) // cant do across board, will cause issue w OUT/IN
                            {
                                int itemId = (int)item;

                                if (!seenItems.Contains(itemId))
                                {
                                    seenItems.Add(itemId);
                                    ++count;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!excludeDuplicates)
                        count += t._finalItemID.Count;
                    else  //verify no duplicates
                    {
                        foreach (var item in t._finalItemID)
                        {
                            int itemId = (int)item;
                            if (!seenItems.Contains(itemId))
                            {
                                seenItems.Add(itemId);
                                ++count;
                            }
                        }
                    }
                }
            }
        }
        // Debug.Log($"The # of INV items will be : {count}");
        return count;
    }


}
