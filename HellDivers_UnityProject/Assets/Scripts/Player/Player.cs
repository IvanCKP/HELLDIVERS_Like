using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Weapon_Battle))]
[RequireComponent(typeof(StratagemController))]
[RequireComponent(typeof(PlayerAnimationsContorller))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    #region Private Variable

#pragma warning disable
    private PlayerInfo m_Data;
    private PlayerParts m_Parts;
    private PlayerController m_PlayerController;
    private StratagemController m_StratagemController;
    //private Weapon_Battle m_WeapoonBattle;
#pragma warning disable

    #endregion Private Variable

    public void Initialize(PlayerInfo data)
    {
        data.CopyTo(m_Data);

        if (m_StratagemController.Stratagems.Count > 0) m_StratagemController.Clear();
        m_StratagemController.AddStratagems(m_Data.stratagems.ToArray(), m_Parts.RightHand);
    }

    #region MonoBehaviour

    private void Awake()
    {
        this.tag = "Player";
        m_Data = new PlayerInfo();
        m_Parts = GetComponent<PlayerParts>();
        m_PlayerController = GetComponent<PlayerController>();
        //m_WeapoonBattle = GetComponent<Weapon_Battle>();
        m_StratagemController = GetComponent<StratagemController>();
    }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #endregion MonoBehaviour
}