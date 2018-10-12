using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerCreater
{
    public static bool CreatMainPlayer(PlayerInfo data)
    {
        if (data == null) return false;

        GameObject playerGo = Resources.Load("Characters/Ch01/ch01") as GameObject;
        playerGo = GameObject.Instantiate(playerGo);
        Player p = playerGo.AddComponent<Player>();
        p.Initialize(data);

        return true;
    }
}