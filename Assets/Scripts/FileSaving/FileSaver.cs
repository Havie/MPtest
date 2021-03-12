using System.IO;
using System;
using UnityEngine;

public static class FileSaver
{
    private static string FILEPATH = Application.persistentDataPath; //+ "/settings.txt";
    private static string ROUND = "round";

    public static void WriteToFile(string word)
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


    private static string GetDateForFileName()
    {
        var dt = DateTime.Today;
        string date = $"_{dt.ToString("yyyy")}_{dt.ToString("MM")}_{dt.ToString("dd")}_";

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
                if (!File.Exists(FILEPATH + date + ROUND+ roundIndex + ".txt"))
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
        return FILEPATH + date + ROUND + roundIndex + ".txt";
    }

}
