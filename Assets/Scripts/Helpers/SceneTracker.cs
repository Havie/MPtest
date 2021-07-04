#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class SceneTracker : MonoSingleton<SceneTracker>
{
    public enum eSceneName { Main_Menu, MP_Lobby, Work_Station }
    private eSceneName _currentScene = eSceneName.Main_Menu;



    private void Update()
    {
        ///TMP 
        ///
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitScene();
        }
    }

    public void LoadScene(eSceneName sceneName)
    {
        SceneLoader.LoadLevel(sceneName.ToString());
        _currentScene = sceneName;
    }

    /// <summary> Exits the Current Scene </summary>
    public void ExitScene()
    {
        switch (_currentScene)
        {
            case eSceneName.Main_Menu:
                {
                    ///Ask to quit the game
                    break;
                }
            case eSceneName.MP_Lobby:
                {
                    ///Return to main menu 
                    LoadScene(eSceneName.Main_Menu);
                    /// disconnect from server
                    DisconnectFromServer();
                    break;
                }
            case eSceneName.Work_Station:
                {
                    ///TODO return to MP_Lobby and wait for next round?
                    ///.. instead for now : 
                    LoadScene(eSceneName.Main_Menu);
                    DisconnectFromServer();
                    break;
                }
        }
    }

    private void DisconnectFromServer()
    {
        Client client = Client.Instance;
        if(client)
        {
            client.Disconnect();
        }
    }

    private void OnApplicationQuit()
    {
        DisconnectFromServer();
    }
}
