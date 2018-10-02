using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    /// <summary>
    /// Testing player info.
    /// </summary>
    public PlayerInfo m_playerInfo;

#pragma warning disable
    private AssetManager assetManager = new AssetManager();
    private ResourceManager resourceManager = new ResourceManager();
    private GameData gameData = new GameData();
    private ObjectPool objectPool;
#pragma warning disable

    private void Awake()
    {
        objectPool = this.gameObject.AddComponent<ObjectPool>();
        assetManager.Init();
        resourceManager.Init();
        gameData.LoadGameData();
    }

    // Use this for initialization
    private void Start()
    {
        if (m_playerInfo != null)
        {
            GameObject playerGo = new GameObject("Player", typeof(Player));
            playerGo.GetComponent<Player>().Info = m_playerInfo;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}