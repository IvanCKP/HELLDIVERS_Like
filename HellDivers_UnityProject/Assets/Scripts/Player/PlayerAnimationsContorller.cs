using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePlayerAnimationState
{
    ANI_IDLE,
    ANI_WALK,
    ANI_RUN,
    ANI_WALKSHOOT,
    ANI_DEATH,
    ANI_ROLL
};

public enum ePlayerAttack
{
    ANI_IDLE,
    ANI_GUNPLAY,
    ANI_THROW,
    ANI_RELOAD,
    ANI_ORI,
    ANI_GETHURT
}

public class PlayerAnimationsContorller : MonoBehaviour
{
    private Animator m_Animator;
    public static ePlayerAnimationState m_MoveState;
    public static ePlayerAttack m_AttackState;
    private Transform m_Cam;
    private Vector3 m_CamFoward;
    private Vector3 m_Direction;
    private bool bThrow;
    private float m_Right;
    private float m_Forward;

    private void Start()
    {
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
        m_Animator = this.GetComponent<Player>().Anima;
        m_AttackState = ePlayerAttack.ANI_GUNPLAY;
    }

    private void Update()
    {
        MovrDirection();
        DisplayMoveState();
        DisplayAttackState();
        CheckState();
        if (m_MoveState == ePlayerAnimationState.ANI_DEATH)
        {
            m_Animator.SetBool("Death", true);
        }
        if (m_AttackState == ePlayerAttack.ANI_GETHURT)
        {
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetTrigger("GetHurt");
        }
        if (m_MoveState == ePlayerAnimationState.ANI_ROLL)
        {
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetTrigger("Roll");
        }
    }

    private void MovrDirection()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (m_Cam != null)
        {
            m_CamFoward = Vector3.Scale(m_Cam.forward, Vector3.forward + Vector3.right);
            m_Direction = m_CamFoward.normalized * v + m_Cam.right * h;
        }
        else
        {
            m_Direction = Vector3.forward * v + Vector3.right * h;
        }

        m_Right = Vector3.Dot(this.transform.right, m_Direction.normalized);
        m_Forward = Vector3.Dot(this.transform.forward, m_Direction.normalized);
    }

    private void DisplayMoveState()
    {
        if (m_MoveState == ePlayerAnimationState.ANI_IDLE)
        {
            m_Animator.SetFloat("Move", 0);
        }
        else if (m_MoveState == ePlayerAnimationState.ANI_WALK)
        {
            m_Animator.SetFloat("Move", 1);
        }
        else if (m_MoveState == ePlayerAnimationState.ANI_RUN)
        {
            m_Animator.SetFloat("Move", 2);
        }
        if (m_MoveState == ePlayerAnimationState.ANI_WALKSHOOT)
        {
            m_Animator.SetBool("WalkShoot", true);
            m_Animator.SetFloat("WalkForward", m_Forward);
            m_Animator.SetFloat("WalkRight", m_Right);
        }
    }

    private void DisplayAttackState()
    {
        if (m_AttackState == ePlayerAttack.ANI_THROW)
        {
            m_AttackState = ePlayerAttack.ANI_IDLE;
            m_Animator.SetTrigger("ThrowStandBy");
            bThrow = true;
        }
        else if (m_AttackState == ePlayerAttack.ANI_RELOAD)
        {
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetTrigger("Reload");
        }
        else if (Input.GetMouseButton(0))
        {
            if (m_AttackState == ePlayerAttack.ANI_GUNPLAY)
            {
                m_Animator.SetBool("Shoot", true);
            }
            else if (bThrow)
            {
                bThrow = false;
                m_Animator.SetTrigger("ThrowOut");
            }
        }
    }

    private void CheckState()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            m_Animator.SetBool("WalkShoot", false);
            m_Animator.SetBool("Shoot", false);
        }
        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
        {
            m_Animator.SetBool("WalkShoot", false);
        }
    }
}