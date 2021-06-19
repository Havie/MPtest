#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

	public static class StationSequenceReader 
	{
    /************************************************************************************************************************/

    public static void PrintASequence(int[] sequence, string seqName)
    {
        string p = "";
        for (int i = 0; i < sequence.Length; ++i)
        {
            p += $" , {sequence[i]}";
        }
        //Debug.Log(seqName+ ": " + p);
    }

    /** This is kind of a mess, thinking of making a doubly linked list class at some point*/
    public static int[] GetProperSequence(WorkStationManager wm)
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

    public static int FindPlaceInSequence(int[] sequence, int stationID)
    {
        int index = 0;
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] == stationID)
                return i;
        }

        return index;
    }

    public static int SumSequence(WorkStationManager wm, WorkStation myWS, bool reqItemsOverFinalItems, bool includeSelf, bool excludeDuplicates)
    {
        int count = 0;
        int[] stationSequence = GetProperSequence(wm);
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
            foreach (Task t in ws.Tasks)
            {
                if (reqItemsOverFinalItems)
                {
                    if (!excludeDuplicates)
                        count += t._requiredItemIDs.Count;
                    else   //verify no duplicates
                    {
                        foreach (var item in t._requiredItemIDs)
                        {
                            if (ObjectManager.Instance.IsBasicItem(item)) // cant do across board, will cause issue w OUT/IN
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

