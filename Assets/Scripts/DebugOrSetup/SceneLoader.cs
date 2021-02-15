using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public static class SceneLoader 
{
    public static void LoadLevel(string level)
    {
        var loadingOperation= SceneManager.LoadSceneAsync(level);
    }



}
