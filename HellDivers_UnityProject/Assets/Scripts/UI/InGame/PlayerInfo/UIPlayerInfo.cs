﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfo : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// Represent of the player which is observing.
    /// </summary>
    public Player CurrentPlayer { get { return m_Player; } }

    #endregion Properties

    #region Private Variable

    private Player m_Player;
    private Dictionary<int, UIPlayerWeaponInfo> m_UIWeapons;
    private Dictionary<int, UIPlayerStratagemInfo> m_UIStratagems;
    [SerializeField] private List<Color> m_PlayerColors;
    [SerializeField] private Text m_PlayerName;
    [SerializeField] private Text m_PlayerRank;
    [SerializeField] private Image m_PlayerRankImg;
    [SerializeField] private Image m_PlayerDeathImg;
    [SerializeField] private GameObject m_WeaponInfoPrefab;
    [SerializeField] private GameObject m_StratagemInfoPrefab;
    [SerializeField] private GameObject m_StratagemCDIconPrefab;
    [SerializeField] private Transform m_StratagemCDPanel;

    #endregion Private Variable

    #region Initializer

    public void Initialize(Player player)
    {
        m_Player = player;

        InitTitleDisplay();
        InitWeaponUI();
        InitStratagemDisplay();
    }

    #endregion Initializer

    #region UI Title

    private void InitTitleDisplay()
    {
        if (m_Player == null) return;

        m_PlayerName.text = m_Player.Info.Username;
        m_PlayerRank.text = m_Player.Info.Rank.ToString();

        string rankImgFile = "icon_rank_";
        if (m_Player.Info.Rank < 1)
        {
            rankImgFile += "01";
        }
        else if (m_Player.Info.Rank > 40)
        {
            rankImgFile += "40";
        }
        else
        {
            rankImgFile += m_Player.Info.Rank.ToString("00");
        }

        string rankImgPath = string.Format("UI/Resource/Icons/Rank/{0}", rankImgFile);
        Sprite rankImg = Resources.Load<Sprite>(rankImgPath);
        m_PlayerRankImg.sprite = rankImg;

        m_Player.OnStartSpawnNotify += DrawAliveTitle;
        m_Player.OnStartDeathNotify += DrawDathTitle;
    }

    private void DrawAliveTitle()
    {
        m_PlayerDeathImg.gameObject.SetActive(false);
        m_PlayerRankImg.gameObject.SetActive(true);
        m_PlayerRank.gameObject.SetActive(true);
    }

    private void DrawDathTitle()
    {
        m_PlayerDeathImg.gameObject.SetActive(true);
        m_PlayerRankImg.gameObject.SetActive(false);
        m_PlayerRank.gameObject.SetActive(false);
    }

    #endregion UI Title

    #region UI Weapon

    private void InitWeaponUI()
    {
        if (m_Player == null) return;

        m_UIWeapons = new Dictionary<int, UIPlayerWeaponInfo>();
        foreach (KeyValuePair<int, IWeaponBehaviour> weapon in m_Player.WeaponController.ActiveWeapon)
        {
            UIPlayerWeaponInfo weaponUI = Instantiate(m_WeaponInfoPrefab, this.transform).GetComponent<UIPlayerWeaponInfo>();
            weaponUI.Initialize(weapon.Value.weaponInfo);
            weaponUI.gameObject.SetActive(false);

            m_Player.WeaponController.OnFire += weaponUI.UpdateAmmoDisplay;
            m_Player.WeaponController.OnPickMags += weaponUI.UpdateAmmoDisplay;
            m_Player.WeaponController.OnReload += weaponUI.StartReload;
            m_Player.WeaponController.OnReloadEnd += weaponUI.UpdateAmmoDisplay;

            m_UIWeapons.Add(weapon.Value.weaponInfo.ID, weaponUI);
        }

        m_Player.OnStartSpawnNotify += DrawCurrentWeaponUI;
        m_Player.OnStartDeathNotify += HideWeaponUI;
        m_Player.WeaponController.OnSwitch += DrawCurrentWeaponUI;
        DrawCurrentWeaponUI();
    }

    private void DrawCurrentWeaponUI()
    {
        if (m_UIWeapons.Count <= 0) return;

        foreach (KeyValuePair<int, UIPlayerWeaponInfo> weaponUI in m_UIWeapons)
        {
            if (weaponUI.Key == m_Player.WeaponController._CurrentWeapon)
            {
                weaponUI.Value.gameObject.SetActive(true);
                weaponUI.Value.UpdateAmmoDisplay();
            }
            else
            {
                if (weaponUI.Value.isActiveAndEnabled) weaponUI.Value.gameObject.SetActive(false);
            }
        }
    }

    private void HideWeaponUI()
    {
        if (m_UIWeapons.Count <= 0) return;

        foreach (KeyValuePair<int, UIPlayerWeaponInfo> weaponUI in m_UIWeapons)
        {
            if (weaponUI.Value.isActiveAndEnabled) weaponUI.Value.gameObject.SetActive(false);
        }
    }

    #endregion UI Weapon

    #region UI Stratagem

    private void InitStratagemDisplay()
    {
        if (m_Player == null) return;

        m_UIStratagems = new Dictionary<int, UIPlayerStratagemInfo>();
        for (int i = 0; i < m_Player.StratagemController.Stratagems.Count; i++)
        {
            Stratagem currentStratagem = m_Player.StratagemController.Stratagems[i];

            UIPlayerStratagemInfo stratagemUI = Instantiate(m_StratagemInfoPrefab, this.transform).GetComponent<UIPlayerStratagemInfo>();
            stratagemUI.Initialize(currentStratagem);
            stratagemUI.gameObject.SetActive(false);

            currentStratagem.OnThrow += StopStratagemUI;
            m_Player.StratagemController.OnStartCheckingCode += StartStratagemUI;
            m_Player.StratagemController.OnStopCheckingCode += StopStratagemUI;
            m_Player.StratagemController.OnCheckingCode += CheckingCodes;
            m_Player.StratagemController.OnGetReady += GetReady;

            UIPlayerStratagemCD stratagemCD = Instantiate(m_StratagemCDIconPrefab, m_StratagemCDPanel).GetComponent<UIPlayerStratagemCD>();
            stratagemCD.Initialize(currentStratagem);
            m_Player.OnStartSpawnNotify += stratagemCD.RefreshDisplay;
            m_Player.OnStartDeathNotify += stratagemCD.RefreshDisplay;

            m_UIStratagems.Add(currentStratagem.Info.ID, stratagemUI);
        }
    }

    private void StartStratagemUI()
    {
        HideWeaponUI();

        foreach (KeyValuePair<int, UIPlayerStratagemInfo> stratagemUI in m_UIStratagems)
        {
            if (stratagemUI.Value.CurrentStratagem.IsCooling || stratagemUI.Value.CurrentStratagem.IsOutOfUses)
            {
                stratagemUI.Value.DarkDisplay();
            }
            else
            {
                stratagemUI.Value.BrightDisplay();
            }
            stratagemUI.Value.StartUI();
        }
    }

    private void CheckingCodes()
    {
        foreach (KeyValuePair<int, UIPlayerStratagemInfo> stratagemUI in m_UIStratagems)
        {
            if (m_Player.StratagemController.StratagemsOnCheckingCode.Contains(stratagemUI.Value.CurrentStratagem))
            {
                stratagemUI.Value.HilightArrow(m_Player.StratagemController.InputCodeStep);
            }
            else
            {
                stratagemUI.Value.DarkDisplay();
            }
        }
    }

    private void GetReady()
    {
        foreach (KeyValuePair<int, UIPlayerStratagemInfo> stratagemUI in m_UIStratagems)
        {
            if (stratagemUI.Value.CurrentStratagem == m_Player.StratagemController.CurrentStratagem)
            {
                stratagemUI.Value.GetReady();
            }
            else
            {
                stratagemUI.Value.CloseUI();
            }
        }
    }

    private void StopStratagemUI()
    {
        foreach (KeyValuePair<int, UIPlayerStratagemInfo> stratagemUI in m_UIStratagems)
        {
            stratagemUI.Value.CloseUI();
        }

        DrawCurrentWeaponUI();
    }

    #endregion UI Stratagem
}