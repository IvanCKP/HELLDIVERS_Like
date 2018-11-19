﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponInfo : MonoBehaviour {

    public Dictionary<int, List<int>> weapons = new Dictionary<int, List<int>>();
    [SerializeField] int m_iType;
    [SerializeField] int m_iCurrentID;

    public int ID { get { return m_iCurrentID; } }
    public void SetID(int id) { m_iCurrentID = id; }
    public void SetType(int type) { m_iType = type; }

    public void SetWeapon()
    {
        float cur;
        float next;
        float max;
        string mode;
        SetUI(m_iCurrentID);
        Get_Power(out cur, out next, out max);
        m_Power.SetAbility(m_Power.name, GetLength(cur, max), GetLength(next, max));
        Get_FireRate(out cur, out next, out max);
        m_Magazine.SetAbility(m_Magazine.name, GetLength(cur, max), GetLength(next, max));
        Get_Stability(out cur, out next, out max);
        m_FireRate.SetAbility(m_FireRate.name, GetLength(cur, max), GetLength(next, max));
        Get_Magazine(out cur, out next, out max);
        m_Range.SetAbility(m_Range.name, GetLength(cur, max), GetLength(next, max));
        Get_Range(out cur, out next, out max);
        m_Stability.SetAbility(m_Stability.name, GetLength(cur, max), GetLength(next, max));
        Get_FireMode(out mode);
        m_FireMode.SetWord(m_FireMode.name, mode);
    }
    
    private void Awake()
    {
        CreateDictionary();
        CreateObject(ref m_Power, "Power");
        CreateObject(ref m_Magazine, "Magazine");
        CreateObject(ref m_FireRate, "FireRate");
        CreateObject(ref m_Range, "Range");
        CreateObject(ref m_Stability, "Stability");
        CreateObject(ref m_FireMode, "FireMode");
    }

    #region UIWeaponAbility Field
    UIWeaponAbility m_Power;
    UIWeaponAbility m_Magazine;
    UIWeaponAbility m_FireRate;
    UIWeaponAbility m_Range;
    UIWeaponAbility m_Stability;
    UIWeaponAbility m_FireMode;
    #endregion

    #region Get Value Method
    private void Get_Power(out float cur, out float next, out float max)
    {
        List<int> pList = weapons[m_iType];
        int MaxLevel = pList[pList.Count - 1];
        cur = GameData.Instance.WeaponInfoTable[m_iCurrentID].Damage;
        if (m_iCurrentID != MaxLevel)
        {
            max = GameData.Instance.WeaponInfoTable[MaxLevel].Damage;
            next = GameData.Instance.WeaponInfoTable[m_iCurrentID + 1].Damage;
        }else { max = next = cur; }
    }
    private void Get_FireRate(out float cur, out float next, out float max)
    {
        List<int> pList = weapons[m_iType];
        int MaxLevel = pList[pList.Count - 1];
        cur = GameData.Instance.WeaponInfoTable[m_iCurrentID].FirePerMinute;
        if (m_iCurrentID != MaxLevel)
        {
            max = GameData.Instance.WeaponInfoTable[MaxLevel].FirePerMinute;
            next = GameData.Instance.WeaponInfoTable[m_iCurrentID + 1].FirePerMinute;
        } else { max = next = cur; }
    }
    private void Get_Stability(out float cur, out float next, out float max)
    {
        List<int> pList = weapons[m_iType];
        int MaxLevel = pList[pList.Count - 1];
        cur = GameData.Instance.WeaponInfoTable[m_iCurrentID].Max_Spread;
        if (m_iCurrentID != MaxLevel)
        {
            max = GameData.Instance.WeaponInfoTable[MaxLevel].Max_Spread;
            next = GameData.Instance.WeaponInfoTable[m_iCurrentID + 1].Max_Spread;
        }
        else { max = next = cur; }

    }
    private void Get_Magazine(out float cur, out float next, out float max)
    {
        List<int> pList = weapons[m_iType];
        int MaxLevel = pList[pList.Count - 1];
        cur = GameData.Instance.WeaponInfoTable[m_iCurrentID].Max_Mags;
        if (m_iCurrentID != MaxLevel)
        {
            max = GameData.Instance.WeaponInfoTable[MaxLevel].Max_Mags;
            next = GameData.Instance.WeaponInfoTable[m_iCurrentID + 1].Max_Mags;
        }
        else { max = next = cur; }

    }
    private void Get_Range(out float cur, out float next, out float max)
    {
        List<int> pList = weapons[m_iType];
        int MaxLevel = pList[pList.Count - 1];
        cur = GameData.Instance.WeaponInfoTable[m_iCurrentID].Range;
        if (m_iCurrentID != MaxLevel)
        {
            max = GameData.Instance.WeaponInfoTable[MaxLevel].Range;
            next = GameData.Instance.WeaponInfoTable[m_iCurrentID + 1].Range;
        }
        else { max = next = cur; }

    }
    private void Get_FireMode(out string mode)
    {
        float i = GameData.Instance.WeaponInfoTable[m_iCurrentID].FireMode;
        mode = (i == 0) ? "SEMI - AUTO" : "FULL - AUTO";
    }
    #endregion

    #region Create Method
    private void CreateDictionary()
    {
        foreach (var item in GameData.Instance.WeaponInfoTable)
        {
            if (weapons.ContainsKey(GameData.Instance.WeaponInfoTable[item.Key].Type) == false)
            {
                List<int> pList = new List<int>();
                pList.Add(item.Key);
                weapons.Add(GameData.Instance.WeaponInfoTable[item.Key].Type, pList);
            }
            else
            {
                weapons[GameData.Instance.WeaponInfoTable[item.Key].Type].Add(item.Key);
            }
        }
    }

    private void CreateObject(ref UIWeaponAbility ability, string name)
    {
        ability = Instantiate(m_WeaponAbility, m_WeaponAbilities.transform).GetComponent<UIWeaponAbility>();
        ability.name = name;        
    }

    private void SetUI(int id)
    {
        WeaponInfo info = GameData.Instance.WeaponInfoTable[id];
        m_WeaponName.text = info.Name;
        m_IWeaponTexture.sprite = ResourceManager.m_Instance.LoadSprite(typeof(Sprite), HELLDIVERS.UI.UIHelper.WeaponIconFolder, "icon_" + info.Image, false);
    }

    private void GetCurrentID(LobbyUI_Weapon uI_Weapon)
    {
        m_iType = uI_Weapon.Type;
        m_iCurrentID = uI_Weapon.ID;
    }

    private float GetLength(float target, float max)
    {
        float length = (target / max) * 200;
        return length;
    }
    #endregion Create Method

    #region SerializeField
    [Header("== Set Current UI ==")]
    [SerializeField] Text m_WeaponName;
    [SerializeField] Image m_IWeaponTexture;
    [SerializeField] GameObject m_WeaponAbilities;
    [SerializeField] GameObject m_WeaponAbility;
    # endregion SerializeField
}
