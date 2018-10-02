using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePlayerAnimationState
{
    ANI_IDLE,
    ANI_WALK,
    ANI_RUN,
    ANI_WALKSHOOT,
    ANI_TURNRIGHT90,
    ANI_TURNLEFT90,
    ANI_DEATH
};

public enum ePlayerAttack
{
    ANI_IDLE,
    ANI_GUNPLAY,
    ANI_SHOOT,
    ANI_SWITCHWEAPON,
    ANI_THROW,
    ANI_THROWSTANDBY,
    ANI_THROWOUT,
    ANI_RELOAD
};

public enum ePlayerAnyState
{
    ANI_IDLE,
    ANI_GETHURT,
    ANI_ROLL
};

public class PlayerAnimationsContorller : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    public static ePlayerAnimationState m_MoveState;
    public static ePlayerAttack m_AttackState;
    public static ePlayerAnyState m_AnyState;
    private Transform m_Cam;
    private Vector3 m_CamFoward;
    private Vector3 m_Direction;
    private float m_Right;
    private float m_Forward;
    private float cSpeed = 0f;

    private void Start()
    {
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }

        if (m_Animator == null) m_Animator = this.GetComponent<Player>().Anima;

        m_AttackState = ePlayerAttack.ANI_GUNPLAY;
        m_AnyState = ePlayerAnyState.ANI_IDLE;
    }

    private void Update()
    {
        MovrDirection();
        DisplayMoveState();
        DisplayAttackState();
        CheckState();
        if (m_MoveState == ePlayerAnimationState.ANI_DEATH)
        {
            m_Animator.SetTrigger("Death");
        }
        if (m_AnyState == ePlayerAnyState.ANI_GETHURT)
        {
            m_AnyState = ePlayerAnyState.ANI_IDLE;
            m_Animator.SetTrigger("GetHurt");
        }
        if (m_AnyState == ePlayerAnyState.ANI_ROLL)
        {
            m_AnyState = ePlayerAnyState.ANI_IDLE;
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
        float minSpeed = 0f;
        float maxSpeed = 2f;
        float addSpeed = 5f;

        if (cSpeed > 2) cSpeed = maxSpeed;
        if (cSpeed < 0) cSpeed = minSpeed;

        if (m_MoveState == ePlayerAnimationState.ANI_TURNRIGHT90)
        {
            m_Animator.SetBool("bTurn", true);
            m_Animator.SetFloat("Turn", 0);

            AnimatorStateInfo info = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 1.0f)
            {
                m_Animator.SetBool("bTurn", false);
            }
        }
        else if (m_MoveState == ePlayerAnimationState.ANI_TURNLEFT90)
        {
            m_Animator.SetBool("bTurn", true);
            m_Animator.SetFloat("Turn", 1);
            m_Animator.SetBool("bTurn", false);
        }

        if (m_MoveState == ePlayerAnimationState.ANI_IDLE)
        {
            cSpeed = Mathf.MoveTowards(cSpeed, 0f, addSpeed * 2 * Time.deltaTime);
        }
        else if (m_MoveState == ePlayerAnimationState.ANI_WALK)
        {
            cSpeed = Mathf.MoveTowards(cSpeed, 1f, addSpeed * Time.deltaTime);
        }
        else if (m_MoveState == ePlayerAnimationState.ANI_RUN)
        {
            cSpeed = Mathf.MoveTowards(cSpeed, 2f, addSpeed * Time.deltaTime);
        }

        m_Animator.SetFloat("Move", cSpeed);

        if (m_MoveState == ePlayerAnimationState.ANI_WALKSHOOT)
        {
            m_Animator.SetBool("WalkShoot", true);
            m_Animator.SetFloat("WalkForward", m_Forward);
            m_Animator.SetFloat("WalkRight", m_Right);
        }
    }

    private void DisplayAttackState()
    {
        if (m_AttackState == ePlayerAttack.ANI_SWITCHWEAPON)
        {
            m_Animator.SetTrigger("SwitchWeapon");
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
        }
        else if (m_AttackState == ePlayerAttack.ANI_THROW)
        {
            m_AttackState = ePlayerAttack.ANI_THROWSTANDBY;
            m_Animator.SetTrigger("ThrowStandBy");
        }
        else if (m_AttackState == ePlayerAttack.ANI_RELOAD)
        {
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetTrigger("Reload");
        }
        else if (m_AttackState == ePlayerAttack.ANI_SHOOT)
        {
            m_Animator.SetBool("Shoot", true);
        }
        else if (m_AttackState == ePlayerAttack.ANI_THROWOUT)
        {
            m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetTrigger("ThrowOut");
        }
    }

    private void CheckState()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            PlayerAnimationsContorller.m_AttackState = ePlayerAttack.ANI_GUNPLAY;
            m_Animator.SetBool("WalkShoot", false);
            m_Animator.SetBool("Shoot", false);
        }
        if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
        {
            m_Animator.SetBool("WalkShoot", false);
        }
    }
}