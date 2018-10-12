using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimationsContorller))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Private Variable

    private CharacterController m_Controller;
    private Transform m_Cam;
    private Vector3 m_CamForward;
    private Vector3 m_Move;
    private Vector3 m_Fall;
    private Vector3 m_Direction;
    private Ray m_MouseRay;
    private RaycastHit m_MouseHit;
    private PlayerAnimationsContorller m_PAC;

    private bool bRun = false;
    private bool bInBattle = false;
    private bool bAttack = false;

    #endregion Private Variable

    #region MonoBehaviour

    private void Start()
    {
        m_PAC = this.GetComponent<PlayerAnimationsContorller>();
        m_Controller = this.GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    #endregion MonoBehaviour

    #region Character Behaviour

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
            m_Move += Physics.gravity * Time.deltaTime;
        }
        bRun = (Input.GetButton("Run")) ? true : false;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            FaceDirection();
            bInBattle = true;
            if (Input.GetMouseButtonDown(0))
            {
                bAttack = true;
            }
        }
        else bInBattle = false;

        m_PAC.Move(m_Move, m_Direction, bRun, bInBattle, bAttack);

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