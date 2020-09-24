using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }


    public static Dictionary<int, PlayerManager> _players = new Dictionary<int, PlayerManager>();

    public GameObject _localPlayerPREFAB;
    public GameObject _playerPREFAB;



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }



    public void SpawnPlayer(int id, string username, Vector3 pos, Quaternion rot)
    {
        GameObject player;
        if (id == Client.instance._myId)
            player = Instantiate(_localPlayerPREFAB, pos, rot);
        else
            player = Instantiate(_playerPREFAB, pos, rot);

        PlayerManager pm = player.GetComponent<PlayerManager>();
        if (pm)
        {
            pm._id = id;
            pm._username = username;

            _players.Add(id, pm);
        }
    }
}
