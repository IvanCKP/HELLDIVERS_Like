using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowing : MonoBehaviour
{
    #region Porpeties

    public LinkedList<Transform> Targets { get { return m_Targets; } }

    public float Speed
    {
        get { return m_CamLerp; }
        set
        {
            if (value > 1) m_CamLerp = 1.0f;
            else if (value < 0) m_CamLerp = 0.0f;
            m_CamLerp = value;
        }
    }

    #endregion Porpeties

    #region Private Variable

    private LinkedList<Transform> m_Targets;
    private Camera m_Cam;
    private Vector3 m_Destination;
    [SerializeField] private float m_Height = 10.0f;
    [SerializeField] private float m_CamRotX = 60.0f;
    [SerializeField] private float m_CamLerp = 0.1f;
    [SerializeField] private float m_CamWalkAdd;
    [SerializeField] private float m_CamRunAdd;
    private Vector2 m_ExtraVec;

    #endregion Private Variable

    #region Public Function

    public void FocusOnTheTarget(Transform t)
    {
        m_Targets.Clear();
        m_Targets.AddFirst(t);
    }

    public void AddTarget(Transform t)
    {
        m_Targets.AddLast(t);
    }

    public void RemoveTarget(Transform t)
    {
        m_Targets.Remove(t);
    }

    #endregion Public Function

    // Use this for initialization
    private void Start()
    {
        m_Targets = new LinkedList<Transform>();
        m_Cam = this.GetComponent<Camera>();
        m_Cam.transform.rotation = Quaternion.Euler(m_CamRotX, 0, 0);
        m_Cam.fieldOfView = 60.0f;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateDestination();

        if (m_Targets.Count == 1)
        {
            if (Input.GetMouseButton(1))
            {
                m_ExtraVec.x = m_Targets.First.Value.forward.x;
                m_ExtraVec.y = m_Targets.First.Value.forward.z * 0.75f;
            }
            else
            {
                float h = Input.GetAxis("Vertical");
                float v = Input.GetAxis("Horizontial");

                bool bRun = Input.GetButton("Run");
                m_ExtraVec.x = (bRun) ? h * m_CamRunAdd : h * m_CamWalkAdd;
                m_ExtraVec.y = (bRun) ? v * m_CamRunAdd : v * m_CamWalkAdd;

                if (m_ExtraVec.y <= 0) m_ExtraVec.y *= 1.25f;
            }
            AddOnDestination(m_ExtraVec.x, m_ExtraVec.y);
        }

        MoveToDestination();
    }

    private void UpdateDestination()
    {
        if (m_Targets.Count > 1)
        {
            m_Destination.Set(0, 0, 0);
            foreach (Transform t in m_Targets)
            {
                m_Destination += t.position;
            }
            m_Destination = m_Destination / m_Targets.Count;
        }
        else
        {
            m_Destination = m_Targets.First.Value.position;
        }

        m_Destination.y += m_Height;
        m_Destination.z += -Mathf.Tan((90 - m_CamRotX) * Mathf.Deg2Rad) * (m_Height - 1f);
    }

    private void AddOnDestination(float x, float z)
    {
        m_Destination.x += x;
        m_Destination.z += z;
    }

    private void MoveToDestination()
    {
        Vector3 currentPos = this.transform.position;
        currentPos = Vector3.Lerp(currentPos, m_Destination, m_CamLerp);
    }
}