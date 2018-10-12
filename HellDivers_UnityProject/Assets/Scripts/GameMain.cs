using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    /// <summary>
    /// Testing player info.
    /// </summary>
    public PlayerInfo m_PlayerData;

#pragma warning disable
    private AssetManager assetManager = new AssetManager();
    private ResourceManager resourceManager = new ResourceManager();
    private GameData gameData = new GameData();
    private ObjectPool objectPool = new ObjectPool();
#pragma warning disable

    private void Awake()
    {
        objectPool.Init();
        assetManager.Init();
        resourceManager.Init();
        gameData.Init();
    }

    // Use this for initialization
    private void Start()
    {
        if (m_PlayerData != null)
        {
            PlayerCreater.CreatMainPlayer(m_PlayerData);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}