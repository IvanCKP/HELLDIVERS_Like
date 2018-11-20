﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HELLDIVERS.UI.InGame;

public class GameMain : MonoBehaviour
{
    public PlayerInfo m_PlayerData1;// Test player info.
    public PlayerInfo m_PlayerData2;// Test player info.

    #region Properties

    public static GameMain Instance { get; private set; }
    public CameraFollowing CameraFolloing { get { return m_CameraFollowing; } }

    #endregion Properties

    private AssetManager m_AssetManager = new AssetManager();
    private ResourceManager m_ResourceManager = new ResourceManager();
    private ObjectPool m_ObjectPool = new ObjectPool();
    private InteractiveItemManager m_ItemManager = new InteractiveItemManager();
    private MissionManager m_MissionManager = new MissionManager();
    private InGamePlayerManager m_PlayerManager;
    private InGameRewardManager m_RewardManager;
    private MobManager m_MobSpawner = new MobManager();
    private CameraFollowing m_CameraFollowing;
    [SerializeField] private uint m_NumberOfTowers = 1;

    private delegate void GameStateDelegateFunc();

    private GameStateDelegateFunc DoCheckCondition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        m_AssetManager.Init();
        m_ResourceManager.Init();
        m_ObjectPool.Init();
        UIInGameMain.Instance.Init();
        m_ItemManager.Init();
        m_MissionManager.Init();
        m_MobSpawner.Init();
        m_CameraFollowing = Camera.main.GetComponent<CameraFollowing>();
        m_PlayerManager = this.gameObject.AddComponent<InGamePlayerManager>();
        m_RewardManager = new GameObject("RewardManager").AddComponent<InGameRewardManager>();
    }

    // Use this for initialization
    private void Start()
    {
        m_MissionManager.CreateTowerMissionsOnMap(m_NumberOfTowers);
        GameStart();

        m_MobSpawner.SpawnPatrol(40);
        InvokeRepeating("SpawnMobs", 10.0f, 30.0f);
    }

    [ContextMenu("GameStart")]
    private void GameStart()
    {
        for (int i = 1; i < PlayerManager.Instance.Players.Count + 1; i++)
        {
            m_PlayerManager.CreatePlayer(PlayerManager.Instance.Players[i].info, i);
        }

        //m_PlayerManager.CreatePlayer(m_PlayerData1, 1);
        //m_PlayerManager.CreatePlayer(m_PlayerData2, 2);

        m_PlayerManager.SpawnPlayers();
        UIInGameMain.Instance.DrawGameUI();
        DoCheckCondition = CheckGameCondition;
    }

    // Update is called once per frame
    private void Update()
    {
        if (DoCheckCondition != null) DoCheckCondition();
    }

    [ContextMenu("Mission Abandon")]
    public void MissionAbandon()
    {
        SceneController.Instance.ToLobby();
    }

    private void CheckGameCondition()
    {
        if (InGamePlayerManager.Instance.IsAllPlayerDead())
        {
            MissionFailed();
            DoCheckCondition = null;
        }
    }

    [ContextMenu("Mission Failed")]
    public void MissionFailed()
    {
        UIInGameMain.Instance.DrawMissionFailedUI();
    }

    [ContextMenu("Mission Complete")]
    public void MissionComplete()
    {
        for (int i = 0; i < InGamePlayerManager.Instance.Players.Count; i++)
        {
            Player player = InGamePlayerManager.Instance.Players[i];
            m_RewardManager.SetReward(player.SerialNumber, player.Record);
        }
        SceneController.Instance.ToLobby();
    }

    private void SpawnMobs()
    {
        m_MobSpawner.SpawnFish(4);
    }
}