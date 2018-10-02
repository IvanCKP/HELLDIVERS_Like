using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Stratagem : MonoBehaviour
{
    #region Properties

    public StratagemInfo Info { get { return m_Info; } }
    public EState State { get { return m_eState; } }
    public int UsesCount { get { return m_iUsesCount; } }
    public bool IsCooling { get { return m_isCooling; } }
    public float CoolTimer { get { return m_fCoolTimer; } }
    public float ActTimer { get { return m_fActivationTimer; } }

    #endregion Properties

    #region PrivateVariable

    [SerializeField] private StratagemInfo m_Info;
    private Transform m_LaunchPos;
    private GameObject m_Display;
    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
    private float m_fRadius = 0.1f;
    private int m_iUsesCount;
    private bool m_isCooling;
    private float m_fCoolTimer;
    private float m_fActivationTimer;
    private EState m_eState = EState.Standby;
    private DoState m_DoState;

    #endregion PrivateVariable

    #region Initializer

    /// <summary>
    /// Setup stratagem with id form gamedata table.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool SetStratagemInfo(int id, Transform launchPos)
    {
        StratagemInfo newInfo = GetInfoFromGameData(id);
        if (newInfo == null || newInfo == m_Info) return false;
        m_Info = newInfo;

        GameObject o = ResourceManager.m_Instance.LoadData(typeof(GameObject), StratagemSystem.DisplayFolder, newInfo.display) as GameObject;
        if (m_Display != o)
        {
            if (o == null) o = StratagemSystem.DefaultDisplay;

            DestroyImmediate(m_Display);
            m_Display = Instantiate(o, this.transform.position, Quaternion.identity, this.transform);
            m_Animator = m_Display.GetComponent<Animator>();
        }

        m_LaunchPos = launchPos;
        this.transform.parent = m_LaunchPos;
        this.transform.localPosition = Vector3.zero;

        Reset();
        return true;
    }

    /// <summary>
    /// Reset the (State = Standby) & (Timers = 0).
    /// </summary>
    private void Reset()
    {
        m_iUsesCount = 0;
        m_fCoolTimer = 0.0f;
        m_fActivationTimer = 0.0f;
        m_eState = EState.Standby;
    }

    /// <summary>
    /// Get stratagem info with id from game data tables. If it does not exist return null.
    /// </summary>
    /// <param name="id">stratagem id which in the table</param>
    /// <returns></returns>
    private StratagemInfo GetInfoFromGameData(int id)
    {
        if (GameData.Instance.StratagemTable.ContainsKey(id) == false)
        {
            Debug.LogErrorFormat("Stratagem Error : Can't found ID : [{0}] from game data", id);
            return null;
        }

        return GameData.Instance.StratagemTable[id];
    }

    #endregion Initializer

    #region MonoBehaviour

    // Use this for initialization
    private void Start()
    {
        m_Rigidbody = this.GetComponent<Rigidbody>();
        m_Rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_DoState != null) m_DoState();
        if (IsCooling) DoCoolDown();
    }

    #endregion MonoBehaviour

    #region Public Function

    /// <summary>
    /// Show the stratagem object & set launchPos as parent.
    /// </summary>
    public void GetReady()
    {
        if (IsCooling || State != EState.Standby) return;

        this.transform.parent = m_LaunchPos;
        this.transform.localPosition = Vector3.zero;
        m_Animator.SetTrigger("Start");

        m_eState = EState.Ready;
    }

    /// <summary>
    /// Add force to this gameobject for throw it out.
    /// </summary>
    public void Throw(Vector3 force)
    {
        if (IsCooling || State != EState.Ready) return;

        this.transform.parent = null;
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.AddForce(force);
        m_isCooling = true;
        m_iUsesCount++;
        m_Animator.SetTrigger("Throw");

        m_DoState = DoThrowOut;
        m_eState = EState.ThrowOut;
    }

    #endregion Public Function

    #region StateMachine

    private delegate void DoState();

    public enum EState
    {
        Standby, Ready, ThrowOut, Activating
    }

    private void DoActivation()
    {
        if (m_fActivationTimer >= Info.activation)
        {
            m_Animator.SetTrigger("End");
            m_DoState = null;
            m_eState = EState.Standby;
        }
        m_fActivationTimer += Time.deltaTime;
    }

    private void DoThrowOut()
    {
        RaycastHit raycastHit;
        Physics.SphereCast(this.transform.position, m_fRadius, Vector3.down, out raycastHit);
        if (raycastHit.transform.tag == "Terrain")
        {
            m_Rigidbody.isKinematic = true;
            this.transform.rotation = Quaternion.Euler(Vector3.zero);
            m_Animator.SetTrigger("Land");

            m_eState = EState.Activating;
        }

#if UNITY_EDITOR
        Gizmos.DrawWireSphere(this.transform.position, m_fRadius);
#endif
    }

    private void DoCoolDown()
    {
        if (m_fCoolTimer >= Info.cooldown)
        {
            m_fCoolTimer = 0.0f;
            m_isCooling = false;
        }
        m_fCoolTimer += Time.deltaTime;
    }

    #endregion StateMachine
}