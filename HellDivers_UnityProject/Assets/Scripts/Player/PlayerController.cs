using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed { get { return m_Speed; } set { m_Speed = value; } }

    [SerializeField] private float m_Speed = 5f;

    #region Private Variable

    private CharacterController m_Controller;
    private Transform m_Cam;
    private Vector3 m_CamFoward;
    private Vector3 m_Direction;
    private Vector3 m_Move;
    private Ray m_MouseRay;
    private RaycastHit m_MouseHit;

    #endregion Private Variable

    private void Start()
    {
        m_Controller = this.GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
        {
            Move();
        }
        else
        {
            PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_IDLE;
        }
        if (m_Controller.isGrounded == false)
        {
            m_Controller.Move(Physics.gravity * Time.deltaTime);
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            FaceDirection();
        }

        #region DisplayAnimation

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerAnimationsContorller.m_AttackState = ePlayerAttack.ANI_GUNPLAY;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayerAnimationsContorller.m_AttackState = ePlayerAttack.ANI_THROW;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerAnimationsContorller.m_AttackState = ePlayerAttack.ANI_RELOAD;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerAnimationsContorller.m_AttackState = ePlayerAttack.ANI_GETHURT;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_DEATH;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_ROLL;
        }

        #endregion DisplayAnimation
    }

    private void Move()
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

        if (m_Direction.magnitude > 1)
        {
            m_Direction.Normalize();
        }

        m_Move = m_Direction * m_Speed * Time.deltaTime;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_WALKSHOOT;
        }
        else
        {
            PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_WALK;
            if (Input.GetButton("Run"))
            {
                m_Move *= 2.0f;
                PlayerAnimationsContorller.m_MoveState = ePlayerAnimationState.ANI_RUN;
            }
        }

        if (m_Controller.isGrounded == false)
        {
            m_Move += Physics.gravity * Time.deltaTime;
        }
        this.transform.forward = m_Direction;

        m_Controller.Move(m_Move);
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
        m_Controller.transform.forward = m_Direction;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 2.0f);
    }

#endif
}