using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
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
        GameObject playerGo = new GameObject();
        playerGo.name = "Player";
        playerGo.AddComponent<Player>();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}