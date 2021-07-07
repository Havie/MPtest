using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.yougetsignal.com/tools/open-ports/

public class sNetworkManager : MonoSingleton<sNetworkManager>
{

    public readonly static int _defaultPort = 26951;
    public readonly int maxPlayers = 6;

    private bool  _started = false;
    public void HostNetwork()
    {
        ///Is it possible this port fails? If so we need to iterate through ports and tell others
#if UNITY_EDITOR
        sServer.Start(maxPlayers, _defaultPort);
#else
         sServer.Start(maxPlayers, _defaultPort);  
#endif
        _started = true;
    }

    private void Update()
    {
        ///Need a way to tick our static server class as it will need to keep track of time
        if(_started)
        {
            sServer.Tick();
        }
    }
}



