using System.IO;
using System;
using UnityEngine;

namespace dataTracking
{
    public static class FileSaver
    {
        private static string FILEPATH = Application.persistentDataPath; //+ "/settings.txt";
        private static string LEAN_HEADER = "/LeanData_";
        private static string ROUND = "round_";

        public static void WriteToFileTest(string word)
        {
            //UIManager.DebugLog(Application.persistentDataPath);

            try
            {
                string[] fileArr = new string[1];
                fileArr[0] = word;

                string path = findValidFileName();
                UIManager.DebugLog(path);
                File.WriteAllLines(path, fileArr);

            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
        public static void WriteToFile(RoundResults result)
        {
            UIManager.DebugLog(Application.persistentDataPath);
            //IF PC LEAN_HEADER="";

            try
            {
                string[] fileArr = MakeLinesFromData(result);
                string path = findValidFileName();
                UIManager.DebugLog(path);
                File.WriteAllLines(path, fileArr);
            }
            catch (System.Exception e)
            {
                UIManager.DebugLogError(e.ToString());
            }
        }

        private static string[] MakeLinesFromData(RoundResults result)
        {
            int addedLines = 2;
            string header = "---Round Results:---";
            string[] fileArr = new string[result.DataSizeBuffer + addedLines];
            fileArr[0] = header;

            ///Start at 1 because of the header
            int index = 1;
            string thruPut = $"Throughput= {result.ThruPut.ToString("0.00")}";
            string shippedOnTime = $"ShippedOnTime= {result.ShippedOnTime}";
            string shippedLate = $"ShippedLate= {result.ShippedLate}";
            string wip = $"WIP= {result.Wip}";
            string cycleHeader = "----Cycle Times:----";
            ///Assign them to our outputArray
            fileArr[index++] = thruPut;
            fileArr[index++] = shippedOnTime;
            fileArr[index++] = shippedLate;
            fileArr[index++] = wip;
            fileArr[index++] = cycleHeader;

            foreach (var pair in result.GetStationCycleTimes())
            {
                string formattedTime = pair.Value.ToString("0.00");
                string stationCycleInfo = $"   Station#:{pair.Key} CycleTime: {formattedTime} sec";
                fileArr[index++] = stationCycleInfo;
            }

            return fileArr;
        }

        private static string GetDateForFileName()
        {
            var dt = DateTime.Today;
            string date = $"{dt.ToString("yyyy")}_{dt.ToString("MM")}_{dt.ToString("dd")}_";

            //UIManager.DebugLog(date);

            return date;
        }


        private static string findValidFileName()
        {
            int roundIndex = 1;
            string date = GetDateForFileName();
            bool valid = false;
            while (!valid)
            {
                try
                {
                    if (!File.Exists(FILEPATH + date + ROUND + roundIndex + ".txt"))
                    {
                        valid = true;
                    }
                    else
                    {
                        ++roundIndex;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    valid = true;//IDK?
                    return Application.persistentDataPath + date + roundIndex + "/invalid.txt";
                }

            }
            return FILEPATH + LEAN_HEADER + date + ROUND + roundIndex + ".txt";
        }

    }
}
