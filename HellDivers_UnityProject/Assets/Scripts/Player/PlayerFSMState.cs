﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePlayerFSMTrans
{
    NullTransition,
    Go_Gun,
    Go_MeleeAttack,
    Go_Reload,
    Go_Stratagem,
    Go_Throw,
    Go_SwitchWeapon,
    Go_PickUp,
    Go_Victory,
    Go_Dead,
    Go_Relive,
    Go_Roll,
}
public enum ePlayerFSMStateID
{
    NullStateID,
    GunStateID,
    MeleeAttackID,
    ReloadStateID,
    StratagemStateID,
    ThrowStateID,
    SwitchWeaponID,
    PickUpID,
    VictoryID,
    DeadStateID,
    ReliveStateID,
    RollStateID,
}

public class PlayerFSMState
{

    public ePlayerFSMStateID m_StateID;
    public Dictionary<ePlayerFSMTrans, PlayerFSMState> m_Map;
    public float m_fCurrentTime;

    public PlayerFSMState()
    {
        m_StateID = ePlayerFSMStateID.NullStateID;
        m_fCurrentTime = 0.0f;
        m_Map = new Dictionary<ePlayerFSMTrans, PlayerFSMState>();
    }

    public void AddTransition(ePlayerFSMTrans trans, PlayerFSMState toState)
    {
        if (m_Map.ContainsKey(trans))
        {
            return;
        }

        m_Map.Add(trans, toState);
    }
    public void DelTransition(ePlayerFSMTrans trans)
    {
        if (m_Map.ContainsKey(trans))
        {
            m_Map.Remove(trans);
        }

    }

    public PlayerFSMState TransitionTo(ePlayerFSMTrans trans)
    {
        if (m_Map.ContainsKey(trans) == false)
        {
            return null;
        }
        return m_Map[trans];
    }

    public virtual void DoBeforeEnter(PlayerController data)
    {

    }

    public virtual void DoBeforeLeave(PlayerController data)
    {

    }

    public virtual void Do(PlayerController data)
    {

    }

    public virtual void CheckCondition(PlayerController data)
    {

    }
}

public class PlayerFSMGunState : PlayerFSMState
{
    bool shoot;
    public PlayerFSMGunState()
    {
        m_StateID = ePlayerFSMStateID.GunStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {

    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {
        if (GameData.Instance.WeaponInfoTable[data.m_WeaponController._CurrentWeapon].FireMode == 0)
        {
            if (Input.GetAxis("Fire1") < 0 || Input.GetButton("Fire1"))
            {
                if (data.m_WeaponController.ShootState()) shoot = true;
                else shoot = false;
            }
            else shoot = false;
        }
        else
        {
            if (Input.GetAxis("Fire1") < 0 || Input.GetButton("Fire1"))
            {
                if (data.m_WeaponController.ShootState()) shoot = true;
            }
            else shoot = false;
        }
        data.m_PAC.SetAnimator(m_StateID, shoot);
    }

    public override void CheckCondition(PlayerController data)
    {
        if (Input.GetButtonDown("Reload"))
        {
            if (data.m_WeaponController.ReloadState())
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Reload);
            }
        }
        if (Input.GetButton("Stratagem"))
        {
            if (data.m_StratagemController.Stratagems.Count > 0)
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Stratagem);
            }
        }
        if (Input.GetButtonDown("WeaponSwitch"))
        {
            if (data.m_WeaponController.SwitchWeaponState())
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_SwitchWeapon);
            }
        }
        if (Input.GetButtonDown("Interactive"))
        {
            data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_PickUp);
        }
        if (Input.GetButtonDown("MeleeAttack"))
        {
            data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_MeleeAttack);
        }
    }
}

public class PlayerFSMMeleeAttackState : PlayerFSMState
{
    bool shoot;
    public PlayerFSMMeleeAttackState()
    {
        m_StateID = ePlayerFSMStateID.MeleeAttackID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
        data.m_NowAnimation = "Stratagem";
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {
      
    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("MeleeAttack"))
        {
            if (info.normalizedTime > 0.8f)
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
            }
        }
    }
}

public class PlayerFSMReloadState : PlayerFSMState
{
    public PlayerFSMReloadState()
    {
        m_StateID = ePlayerFSMStateID.ReloadStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {

    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(1);
        if (info.IsName("Reload"))
        {
            if (data.m_PAC.FinishAnimator(data))
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
            }
        }
    }
}

public class PlayerFSMStratagemState : PlayerFSMState
{
    public PlayerFSMStratagemState()
    {
        m_StateID = ePlayerFSMStateID.StratagemStateID;
    }
    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID, true);
    }

    public override void DoBeforeLeave(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID, false);
    }

    public override void Do(PlayerController data)
    {
        data.m_NowAnimation = "Stratagem";

        if (data.m_StratagemController.IsCheckingCode == false)
        {
            data.m_StratagemController.StartCheckCodes();
        }
    }

    public override void CheckCondition(PlayerController data)
    {
        if (data.m_StratagemController.IsReady)
        {
            data.m_PAC.SetAnimator(m_StateID);
            data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Throw);
        }

        else if (Input.GetButtonUp("Stratagem"))
        {
            data.m_StratagemController.StopCheckCodes();
            data.m_PAC.SetAnimator(m_StateID, data.m_StratagemController.IsReady);
            data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
        }
    }
}

public class PlayerFSMThrowState : PlayerFSMState
{

    public PlayerFSMThrowState()
    {
        m_StateID = ePlayerFSMStateID.ThrowStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
      
    }

    public override void DoBeforeLeave(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID, false);
    }

    public override void Do(PlayerController data)
    {
        data.m_NowAnimation = "Throw";

        if (Input.GetAxis("Fire1") < 0 || Input.GetButton("Fire1"))
        {
            data.m_NowAnimation = "Throwing";
            data.m_PAC.SetAnimator(m_StateID, true);
        }
        
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(1);
        if (info.IsName("ThrowOut"))
        {
            if (info.normalizedTime > 0.7f)
            {
                data.m_StratagemController.Throw();
            }
        }
    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(1);
        if (info.IsName("ThrowOut"))
        {
            if (data.m_PAC.FinishAnimator(data))
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
            }
        }
    }
}

public class PlayerFSMSwitchWeaponState : PlayerFSMState
{
    public PlayerFSMSwitchWeaponState()
    {
        m_StateID = ePlayerFSMStateID.SwitchWeaponID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {

    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(1);
        if (info.IsName("SwitchWeapon"))
        {
            if (data.m_PAC.FinishAnimator(data))
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
            }
        }
    }
}

public class PlayerFSMPickUpState : PlayerFSMState
{
    public PlayerFSMPickUpState()
    {
        m_StateID = ePlayerFSMStateID.PickUpID;
    }
    
    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {
        
    }

    public override void Do(PlayerController data)
    {
        data.m_NowAnimation = "Stratagem";
    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("PickUp"))
        {
            if (info.normalizedTime > 0.8f)
            {
                data.m_PlayerFSM.PerformTransition(ePlayerFSMTrans.Go_Gun);
            }
        }
    }
}

public class PlayerFSMVictoryState : PlayerFSMState
{
    public PlayerFSMVictoryState()
    {
        m_StateID = ePlayerFSMStateID.VictoryID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {

    }

    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(1);
        if (info.IsName("Victory"))
        {
            if (info.normalizedTime > 0.8f)
            {
                data.m_PlayerFSM.PerformPreviousTransition();
            }
        }
    }
}

public class PlayerFSMDeadState : PlayerFSMState
{
    public PlayerFSMDeadState()
    {
        m_StateID = ePlayerFSMStateID.DeadStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_NowAnimation = "Dead";
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {
        AnimatorStateInfo info = data.m_Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Death"))
        {
            if (info.normalizedTime < 0.95f) data.bIsDead = false;
            else
            {
                data.bIsDead = true;
                data.bIsAlive = false;
            }
            return;
        }
    }

    public override void CheckCondition(PlayerController data)
    {

    }
}
  
public class PlayerFSMReliveState : PlayerFSMState
{
    public PlayerFSMReliveState()
    {
        m_StateID = ePlayerFSMStateID.ReliveStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {

    }

    public override void Do(PlayerController data)
    {
        AnimatorStateInfo info = data.m_Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Relive"))
        {
            if (info.normalizedTime < 0.9f) data.bIsAlive = false;
            else
            {
                data.bIsAlive = true;
                data.bIsDead = false;
            }
            return;
        }
    }
    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Relive"))
        {
            if (info.normalizedTime > 0.95f)
            {
                data.m_PlayerFSM.PerformPreviousTransition();
            }
        }
    }
}

public class PlayerFSMRollState : PlayerFSMState
{
    public PlayerFSMRollState()
    {
        m_StateID = ePlayerFSMStateID.RollStateID;
    }


    public override void DoBeforeEnter(PlayerController data)
    {
        data.m_PAC.SetAnimator(m_StateID);
    }

    public override void DoBeforeLeave(PlayerController data)
    {
        data.m_NowAnimation = "Origin";
    }

    public override void Do(PlayerController data)
    {
        data.m_NowAnimation = "Stratagem";
    }


    public override void CheckCondition(PlayerController data)
    {
        AnimatorStateInfo info = data.m_PAC.Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Roll"))
        {
            if (info.normalizedTime > 0.8f)
            {
                Debug.Log("enter");
                data.m_PlayerFSM.PerformPreviousTransition();
            }
        }
    }
}