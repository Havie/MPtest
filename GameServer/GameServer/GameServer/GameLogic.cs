using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        public static void Update()
        {
            foreach (Client client in Server._clients.Values)
            {
                if (client._player != null)
                    client._player.Update();
            }
            ThreadManager.UpdateMain();
        }
    }
}
