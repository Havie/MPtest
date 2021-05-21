using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoSingleton<ThreadManager>
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    private void Update()
    {
        UpdateMain();
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    private static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }


    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="_action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }


    public void ExecuteOnMainThreadWithDelay(Action action, float delay)
    {
        StartCoroutine(RunActionWithDelay(delay, action)); 
    }

    IEnumerator RunActionWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        ExecuteOnMainThread(action);
        Debug.Log($"<color=green>Finished delay</color>");
    }

}