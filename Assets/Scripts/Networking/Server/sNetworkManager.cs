using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.yougetsignal.com/tools/open-ports/

public class sNetworkManager : MonoSingleton<sNetworkManager>
{

    public static int _defaultPort = 26951;
    private int _maxPlayers = 6;

    public void HostNetwork()
    {
        ///Is it possible this port fails? If so we need to iterate through ports and tell others
#if UNITY_EDITOR
        sServer.Start(_maxPlayers, _defaultPort);
#else
         sServer.Start(_maxPlayers, _defaultPort);  
#endif
    }
}



