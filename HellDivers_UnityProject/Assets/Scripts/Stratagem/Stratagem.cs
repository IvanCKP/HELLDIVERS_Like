﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class Stratagem : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// The info is used to initilize the stratagem.
    /// </summary>
    public StratagemInfo Info { get { return m_Info; } }

    /// <summary>
    /// The current state of the stratagem.
    /// </summary>
    public EState State { get { return m_eState; } }

    /// <summary>
    /// Represention of this has been used how many times.
    /// </summary>
    public int UsesCount { get { return m_UsesCount; } }

    /// <summary>
    /// Is the stratagem cooling down.
    /// </summary>
    public bool IsCooling { get { return m_IsCooling; } }

    /// <summary>
    /// The timer of CD time. It start when do Throw.
    /// </summary>
    public float CoolTimer { get { return m_CoolTimer; } }

    /// <summary>
    /// The timer of Activatoin. It start when do Land on "terrain"
    /// </summary>
    public float ActTimer { get { return m_ActivationTimer; } }

    #endregion Properties

    #region PrivateVariable

    [SerializeField] private StratagemInfo m_Info;
    private Transform m_LaunchPos;
    private GameObject m_Display;
    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
    private float m_Radius = 0.25f;
    private int m_UsesCount;
    private bool m_IsCooling;
    private float m_CoolTimer;
    private float m_ActivationTimer;
    private EState m_eState = EState.Idle;
    private DoState m_DoState;

    private delegate void DoState();

    #endregion PrivateVariable

    #region Initializer

    /// <summary>
    /// Setup stratagem by id which is in the gamedata.stratagem table.
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
        m_UsesCount = 0;
        m_CoolTimer = 0.0f;
        m_ActivationTimer = 0.0f;
        m_eState = EState.Idle;
        StopAllCoroutines();
    }

    /// <summary>
    /// Get stratagem info by id which is in the gamedata.stratagem tables. If it does not exist return null.
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
    private void FixedUpdate()
    {
        if (m_eState == EState.ThrowOut) DoThrowOut();
    }

    #endregion MonoBehaviour

    #region Public Function

    /// <summary>
    /// Show the stratagem object & reset to the launch position.
    /// </summary>
    public void GetReady()
    {
        if (IsCooling || State != EState.Idle || m_UsesCount >= Info.uses) return;

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
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.AddForce(force);
        m_IsCooling = true;
        m_UsesCount++;
        m_Animator.SetTrigger("Throw");

        // Start the cooldown timer.
        if (Info.cooldown > 0) StartCoroutine(DoCoolDown(Info.cooldown));

        // Translate to ThrowOut state.
        m_eState = EState.ThrowOut;
    }

    #endregion Public Function

    #region StateMachine

    public enum EState
    {
        Idle, Ready, ThrowOut, Activating
    }

    /*-------------------------------------------------------------------------------
     * In ThrowOut state, checking the stratagem object is land on "Terrain" or not. *
     * When it landed successfully, then translate to Activating state.              *
     --------------------------------------------------------------------------------*/

    private void DoThrowOut()
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, m_Radius);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                m_Rigidbody.isKinematic = true;
                this.transform.rotation = Quaternion.Euler(Vector3.zero);
                m_Animator.SetTrigger("Land");

                StartCoroutine(DoActivating(Info.activation));
            }
        }
    }

    /*---------------------------------------------------------------------
     * It's a timer for activaing process.                                 *
     * When the "End" animation was finished, than translate to Idle state *
     ----------------------------------------------------------------------*/

    private IEnumerator DoActivating(float targetTime)
    {
        m_eState = EState.Activating;

        m_ActivationTimer = 0.0f;
        while (m_ActivationTimer < targetTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            m_ActivationTimer += Time.deltaTime;
        }

        m_Animator.SetTrigger("End");

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo currentAnima = m_Animator.GetCurrentAnimatorStateInfo(0);
            return (currentAnima.IsName("End") && currentAnima.normalizedTime >= 1);
        });

        m_eState = EState.Idle;
        yield break;
    }

    /*----------------------------------------
     * It's a timer for cooling down process. *
     -----------------------------------------*/

    private IEnumerator DoCoolDown(float targetTime)
    {
        m_CoolTimer = 0.0f;
        m_IsCooling = true;
        while (m_CoolTimer < targetTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            m_CoolTimer += Time.deltaTime;
        }
        m_IsCooling = false;
        yield break;
    }

    #endregion StateMachine

#if UNITY_EDITOR

    #region DebugDraw

    public bool ShowDebugInfo;

    private void OnDrawGizmos()
    {
        if (ShowDebugInfo)
        {
            if (State == EState.ThrowOut) Gizmos.DrawWireSphere(this.transform.position, m_Radius);

            if (State == EState.Activating)
            {
                string actMessage = string.Format("{0} Act : {1}", Info.title, m_ActivationTimer);
                style.normal.textColor = Color.red;
                Handles.Label(this.transform.position, actMessage, style);
            }

            if (IsCooling)
            {
                string coolMessage = string.Format("{0} CD : {1}", Info.title, m_CoolTimer);
                style.normal.textColor = Color.black;
                Handles.Label(m_LaunchPos.position, coolMessage, style);
            }
        }
    }

    private void OnGUI()
    {
        if (ShowDebugInfo)
        {
            string Message;
            Rect rect = new Rect(10, 10, 100, 20);

            switch (State)
            {
                case EState.Idle:
                    {
                        if (m_UsesCount >= Info.uses)
                        {
                            Message = string.Format("{0} /{1} out of uses", this.name, Info.title);
                            style.normal.textColor = Color.red;
                            GUI.Label(rect, Message, style);
                        }
                        else
                        {
                            Message = string.Format("{0} /{1} standby", this.name, Info.title);
                            style.normal.textColor = Color.gray;
                            GUI.Label(rect, Message, style);
                        }
                        break;
                    }

                case EState.ThrowOut:
                    {
                        Message = string.Format("{0} /{1} throw out", this.name, Info.title);
                        style.normal.textColor = Color.gray;
                        GUI.Label(rect, Message, style);
                        break;
                    }

                case EState.Ready:
                    {
                        Message = string.Format("{0} /{1} ready", this.name, Info.title);
                        style.normal.textColor = Color.gray;
                        GUI.Label(rect, Message, style);
                        break;
                    }

                case EState.Activating:
                    {
                        Message = string.Format("{0} /{1} Act : {2} / {3}", this.name, Info.title, m_ActivationTimer, Info.activation);
                        style.normal.textColor = Color.red;
                        GUI.Label(rect, Message, style);
                        break;
                    }
                default:
                    break;
            }

            if (IsCooling)
            {
                Message = string.Format("{0} /{1} CD : {2} / {3}", this.name, Info.title, m_CoolTimer, Info.cooldown);
                style.normal.textColor = Color.black;
                rect.y *= 3;
                GUI.Label(rect, Message, style);
            }
        }
    }

    private GUIStyle style = new GUIStyle();

    #endregion DebugDraw

#endif
}