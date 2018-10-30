﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimationsContorller))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AimLine))]
[RequireComponent(typeof(SpawnMobs))]
public class PlayerController : MonoBehaviour
{
    #region Private Variable
    private CharacterController m_Controller;
    private Transform m_Cam;
    private Vector3 m_CamForward;
    private Vector3 m_Move;
    private Vector3 m_Fall;
    private Vector3 m_Direction;
    private bool bRun = false;
    private bool bInBattle = false;
    private AimLine m_AimLine;

    #endregion Private Variable

    public Player m_Player;
    public bool m_FinishAni = false;
    public string m_MoveMode = "Origin";
    public Animator m_Animator;
    public PlayerAnimationsContorller m_PAC;
    public WeaponController m_WeaponController;
    public StratagemController m_StratagemController;
    public PlayerFSMSystem m_PlayerFSM;
    public float m_fAnimatorTime;
    public bool bIsDead = false;
    public bool bIsAlive = true;

    #region MonoBehaviour
    private void Awake()
    {

    }
    private void Start()
    {
        m_PlayerFSM = new PlayerFSMSystem(this);
        m_Player = this.GetComponent<Player>();
        m_WeaponController = this.GetComponent<WeaponController>();
        m_StratagemController = this.GetComponent<StratagemController>();
        m_PAC = this.GetComponent<PlayerAnimationsContorller>();
        m_Controller = this.GetComponent<CharacterController>();
        m_AimLine = this.GetComponent<AimLine>();
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }

        #region PlayerFSMMap


        m_Animator = m_PAC.Animator;

        PlayerFSMGunState m_GunState = new PlayerFSMGunState();
        PlayerFSMReloadState m_RelodaState = new PlayerFSMReloadState();
        PlayerFSMMeleeAttackState m_MeleeAttackState = new PlayerFSMMeleeAttackState();
        PlayerFSMStratagemState m_StratagemState = new PlayerFSMStratagemState();
        PlayerFSMThrowState m_ThrowState = new PlayerFSMThrowState();
        PlayerFSMSwitchWeaponState m_SwitchWeaponState = new PlayerFSMSwitchWeaponState();
        PlayerFSMPickUpState m_PickUpState = new PlayerFSMPickUpState();

        m_GunState.AddTransition(ePlayerFSMTrans.Go_MeleeAttack, m_MeleeAttackState);
        m_GunState.AddTransition(ePlayerFSMTrans.Go_Reload, m_RelodaState);
        m_GunState.AddTransition(ePlayerFSMTrans.Go_Stratagem, m_StratagemState);
        m_GunState.AddTransition(ePlayerFSMTrans.Go_SwitchWeapon, m_SwitchWeaponState);
        m_GunState.AddTransition(ePlayerFSMTrans.Go_PickUp, m_PickUpState);

        m_RelodaState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_MeleeAttackState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_StratagemState.AddTransition(ePlayerFSMTrans.Go_Throw, m_ThrowState);
        m_StratagemState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_ThrowState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_SwitchWeaponState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_PickUpState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        PlayerFSMRollState m_RollState = new PlayerFSMRollState();
        PlayerFSMVictoryState m_VictoryState = new PlayerFSMVictoryState();
        PlayerFSMDeadState m_DeadState = new PlayerFSMDeadState();
        PlayerFSMReliveState m_ReliveState = new PlayerFSMReliveState();

        m_ReliveState.AddTransition(ePlayerFSMTrans.Go_Gun, m_GunState);

        m_PlayerFSM.AddGlobalTransition(ePlayerFSMTrans.Go_Roll, m_RollState);
        m_PlayerFSM.AddGlobalTransition(ePlayerFSMTrans.Go_Victory, m_VictoryState);
        m_PlayerFSM.AddGlobalTransition(ePlayerFSMTrans.Go_Dead, m_DeadState);
        m_PlayerFSM.AddGlobalTransition(ePlayerFSMTrans.Go_Relive, m_ReliveState);

        m_PlayerFSM.AddState(m_GunState);
        m_PlayerFSM.AddState(m_MeleeAttackState);
        m_PlayerFSM.AddState(m_RelodaState);
        m_PlayerFSM.AddState(m_SwitchWeaponState);
        m_PlayerFSM.AddState(m_StratagemState);
        m_PlayerFSM.AddState(m_ThrowState);
        m_PlayerFSM.AddState(m_PickUpState);
        m_PlayerFSM.AddState(m_VictoryState);
        m_PlayerFSM.AddState(m_DeadState);
        m_PlayerFSM.AddState(m_ReliveState);
        m_PlayerFSM.AddState(m_RollState);

        #endregion
    }

    private void FixedUpdate()
    {
        #region Input
        if (Input.GetKeyDown(KeyCode.O))
        {
            PerformPlayerDead();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            PerformPlayerHurt();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            PerformPlayerVictory();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            PerformPlayerRelive();
        }
        if (Input.GetButtonDown("Roll"))
        {
            if (m_MoveMode.Equals("Dead")) return;

            PerformPlayerRoll();
        }
        #endregion

        SelectMotionState();
        m_PlayerFSM.DoState();
    }

    #endregion MonoBehaviour

    #region Character Behaviour

    private void SelectMotionState()
    {
        if (m_MoveMode.Equals("Origin"))
        {
            BasicMove();
            return;
        }
        else if (m_MoveMode.Equals("Stop"))
        {
            m_PAC.Move(Vector3.zero, this.transform.forward, false, false);
            return;
        }
        else if (m_MoveMode.Equals("Throw"))
        {
            ThrowMove();
            return;
        }
        else if (m_MoveMode.Equals("Throwing"))
        {
            ThrowingMove();
            return;
        }
        else if (m_MoveMode.Equals("Dead"))
        {
            m_PAC.Move(Vector3.zero, this.transform.forward, false, false);
            return;
        }
    }

    private void BasicMove()
    {
        Move();

        if (Input.GetButton("Run")) bRun = true;
        else bRun = false;

        #region Key & Mouse
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            FaceDirection();
            bInBattle = true;
        }
        #endregion

        #region Joystick
        else if (Input.GetAxis("DirectionHorizontal") != 0 || Input.GetAxis("DirectionVertical") != 0)
        {
            FaceDirection();
            m_AimLine.OpenAimLine();
            bInBattle = true;
        }
        else
        {
            m_AimLine.CloseAimLine();
            bInBattle = false;
        }
        #endregion
        m_PAC.Move(m_Move, m_Direction, bRun, bInBattle);
    }

    private void ThrowMove()
    {
        Move();
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            FaceDirection();
            bInBattle = true;
        }

        #region Joystick

        else if (Input.GetAxis("DirectionHorizontal") != 0 || Input.GetAxis("DirectionVertical") != 0)
        {
            FaceDirection();
            bInBattle = true;
        }
        #endregion
        else bInBattle = false;
        bRun = false;

        m_PAC.Move(m_Move, m_Direction, bRun, bInBattle);
    }

    private void ThrowingMove()
    {
        Move();
        FaceDirection();
        bInBattle = true;
        bRun = false;

        m_PAC.Move(m_Move, this.transform.forward, bRun, bInBattle);
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (m_Cam != null)
        {
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;
        }
        else
        {
            m_Move = v * Vector3.forward + h * Vector3.right;
        }
        if (m_Move.magnitude > 1) m_Move.Normalize();

        if (m_Controller.isGrounded == false)
        {
            m_Fall += Physics.gravity * Time.deltaTime;
        }
        m_Controller.Move(m_Fall);
    }

    private void FaceDirection()
    {
        float vHigh = Camera.main.transform.position.y - this.transform.position.y;
        Ray MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 vMouseRay = MouseRay.direction;
        vMouseRay.Normalize();

        float angle = Vector3.Dot(new Vector3(0, -1, 0), vMouseRay);
        float distance = vHigh / angle;
        Vector3 endPoint = MouseRay.GetPoint(distance);

        m_Direction = endPoint - this.transform.position;
        m_Direction.y = 0.0f;
        if (m_Direction.magnitude < 0.1f) return;
        if (m_Direction.magnitude > 1) m_Direction.Normalize();

        if (Input.GetAxis("DirectionHorizontal") != 0 || Input.GetAxis("DirectionVertical") != 0)
        {
            #region Joystick
            float h = Input.GetAxis("DirectionHorizontal");
            float v = Input.GetAxis("DirectionVertical");
            if (m_Cam != null)
            {
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Direction = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                m_Direction = v * Vector3.forward + h * Vector3.right;
            }
            if (m_Direction.magnitude < 0.1f) return;
            if (m_Direction.magnitude > 1) m_Direction.Normalize();
            #endregion
        }



    }

    public void PerformPlayerVictory()
    {
        m_PlayerFSM.PerformGlobalTransition(ePlayerFSMTrans.Go_Victory);
    }
    public void PerformPlayerDead()
    {
        m_PlayerFSM.PerformGlobalTransition(ePlayerFSMTrans.Go_Dead);
    }
    public void PerformPlayerRelive()
    {
        if (m_PlayerFSM == null) return;
        m_PlayerFSM.PerformGlobalTransition(ePlayerFSMTrans.Go_Relive);
    }
    public void PerformPlayerRoll()
    {
        AnimatorStateInfo info = m_Animator.GetCurrentAnimatorStateInfo(3);
        if (info.IsName("Roll"))
        {
            return;
        }
        m_PlayerFSM.PerformGlobalTransition(ePlayerFSMTrans.Go_Roll);
    }
    public bool PerformPlayerHurt()
    {
        AnimatorStateInfo info = m_PAC.Animator.GetCurrentAnimatorStateInfo(2);
        if (info.IsName("GetGurt"))
        {
            return false;
        }
        m_PAC.Animator.SetTrigger("GetHurt");
        return true;
    }
    #endregion Character Behaviour
    


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 2.0f);
    }

#endif
}