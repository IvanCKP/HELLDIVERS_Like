﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StratagemController : MonoBehaviour
{
    #region Define Inputs

    private string m_InputVertical = "StratagemVertical";
    private string m_InputHorizontal = "StratagemHorizontal";

    #endregion Define Inputs

    #region Properties

    /// <summary>
    /// Was any stratagem has been ready ?
    /// </summary>
    public bool IsReady { get { return m_CurrentStratagem != false; } }

    /// <summary>
    /// Was the checking codes process has been actived ?
    /// </summary>
    public bool IsCheckingCode { get { return m_bCheckingCode; } }

    /// <summary>
    /// Represent of the stratagem is ready.
    /// </summary>
    public Stratagem CurrentStratagem { get { return m_CurrentStratagem; } }

    /// <summary>
    /// Represent of stratagems in the controller.
    /// </summary>
    public List<Stratagem> Stratagems { get { return m_Stratagems; } }

    /// <summary>
    /// Represent of stratagems is ready to checking code.
    /// </summary>
    public List<Stratagem> StratagemsOnCheckingCode { get { return m_Open; } }

    /// <summary>
    /// Represent of current checking code input step.
    /// </summary>
    public int InputCodeStep { get { return m_CodeInputStep; } }

    /// <summary>
    /// Represent of the throw out force scale.
    /// [Range( 0 , MaxScaleForce )]
    /// </summary>
    public float ScaleThrowForce { get { return m_ScaleForce; } }

    #endregion Properties

    #region Event

    public delegate void EventHolder();

    public event EventHolder OnStartCheckingCode;

    public event EventHolder OnCheckingCode;

    public event EventHolder OnStopCheckingCode;

    public event EventHolder OnGetReady;

    #endregion Event

    #region Private Variable

    [SerializeField] private List<Stratagem> m_Stratagems = new List<Stratagem>();
    [SerializeField] private Vector3 m_ThrowForce = new Vector3(0.0f, 300.0f, 500.0f);
    [SerializeField] private float m_MaxScaleForce = 2;
    private float m_ScaleForce = 1;
    private bool m_bCheckingCode;
    private Stratagem m_CurrentStratagem;
    private Transform m_ReadyPos;
    private Transform m_LaunchPos;

    // A container use to checking codes.
    private List<Stratagem> m_Open = new List<Stratagem>();

    private List<Stratagem> m_Close = new List<Stratagem>();

    private int m_CodeInputStep;

    #endregion Private Variable

    #region Public Function

    #region Management

    /// <summary>
    /// Add a stratagem by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="id">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public bool AddStratagem(int id, Transform readyPos, Transform launchPos)
    {
        for (int i = 0; i < m_Stratagems.Count; i++)
        {
            if (m_Stratagems[i].Info.ID == id) return false;
        }

        string name = string.Format("Stratagem{0}", id);
        GameObject stratagemGo = new GameObject(name);
        Stratagem stratagem = stratagemGo.AddComponent<Stratagem>();

        m_ReadyPos = readyPos;
        m_LaunchPos = launchPos;
        stratagemGo.SetActive(true);
        stratagem.SetStratagemInfo(id, readyPos, launchPos);
        m_Stratagems.Add(stratagem);
        return true;
    }

    /// <summary>
    /// Add a multi stratagems by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="ids">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public void AddStratagems(List<int> ids, Transform readyPos, Transform launchPos)
    {
        foreach (int id in ids)
        {
            AddStratagem(id, readyPos, launchPos);
        }
    }

    /// <summary>
    /// Add a multi stratagems by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="ids">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public void AddStratagems(int[] ids, Transform readyPos, Transform launchPos)
    {
        foreach (int id in ids)
        {
            AddStratagem(id, readyPos, launchPos);
        }
    }

    /// <summary>
    /// Remove the stratagem by index.
    /// </summary>
    /// <param name="index">Index of the stratagems</param>
    /// <returns></returns>
    public bool RemoveStratagemAt(int index)
    {
        if (index < 0 || index >= m_Stratagems.Count) return false;

        Stratagem target = m_Stratagems[index];
        m_Stratagems.RemoveAt(index);

        DestroyImmediate(target.gameObject);
        return true;
    }

    /// <summary>
    /// Remove the stratagem by index.
    /// </summary>
    /// <param name="id">The id key which in the gamedata.stratagem table.</param>
    /// <returns>If remove succeeful return true</returns>
    public bool RemoveStratagem(int id)
    {
        Stratagem target = null;
        for (int i = 0; i < m_Stratagems.Count; i++)
        {
            if (m_Stratagems[i].Info.ID == id)
            {
                target = m_Stratagems[i];
                break;
            }
        }

        if (target == null) return false;

        m_Stratagems.Remove(target);
        DestroyImmediate(target.gameObject);

        return false;
    }

    /// <summary>
    /// Clean up all stratagems in the controller.
    /// </summary>
    public void Clear()
    {
        if (m_Stratagems.Count <= 0) return;

        for (int i = 0; i < m_Stratagems.Count; i++)
        {
            DestroyImmediate(m_Stratagems[i].gameObject);
        }

        m_Stratagems.Clear();
        m_CurrentStratagem = null;
    }

    /// <summary>
    /// Reset all stratagem uses = 0.
    /// </summary>
    public void Reset()
    {
        foreach (Stratagem stratagem in m_Stratagems)
        {
            stratagem.Reset();
        }
    }

    #endregion Management

    /// <summary>
    /// Start checking stratagem codes.
    /// </summary>
    /// <returns>Was there are any stratagems in the contorller ?</returns>
    public bool StartCheckCodes()
    {
        StopAllCoroutines();
        if (m_Stratagems.Count <= 0) return false;
        StartCoroutine(CheckInputCode());
        if (OnStartCheckingCode != null) OnStartCheckingCode();
        return true;
    }

    /// <summary>
    /// Stop checking stratagem codes.
    /// </summary>
    public void StopCheckCodes()
    {
        StopAllCoroutines();
        m_bCheckingCode = false;
        if (OnStopCheckingCode != null) OnStopCheckingCode();
    }

    /// <summary>
    /// Throw out the current stratagem.
    /// </summary>
    public void Throw()
    {
        if (IsReady == false) return;

        StopAllCoroutines();
        Vector3 force = m_ThrowForce * m_ScaleForce;
        m_CurrentStratagem.Throw(force);
        m_CurrentStratagem = null;
    }

    /// <summary>
    /// Start add on throw force.
    /// </summary>
    /// <returns>Was there is stratagem which is ready ?</returns>
    public bool PrepareThrow()
    {
        if (IsReady == false) return false;

        StopAllCoroutines();
        StartCoroutine(ThorwForceAddOn());
        return true;
    }

    #endregion Public Function

    #region Private Function

    /*----------------------------
     * Scale throw force by time *
     ----------------------------*/

    private IEnumerator ThorwForceAddOn()
    {
        m_ScaleForce = 0;

        while (m_ScaleForce < m_MaxScaleForce)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            m_ScaleForce += Time.deltaTime;
            if (m_ScaleForce > m_MaxScaleForce) m_ScaleForce = m_MaxScaleForce;
        }

        yield break;
    }

    #region Check Input Code

    /*---------------------------------------------------------
     * Cllect all stratagem which has input code.             *
     * Check input with info step by step.                    *
     * The final result have to all match up info with input. *
     * Finaly store in the m_CurrentStratagem.                *
     ---------------------------------------------------------*/

    private IEnumerator CheckInputCode()
    {
        m_bCheckingCode = true;
        m_CodeInputStep = 0;
        m_Open.Clear();
        m_Close.Clear();

        foreach (Stratagem s in m_Stratagems)
        {
            if (s.State == Stratagem.eState.Idle && !s.IsCooling && !s.IsOutOfUses)
                m_Open.Add(s);
        }

        if (m_Open.Count <= 0) yield break;

        StratagemInfo.eCode? input = null;
        while (m_Open.Count > 0)
        {
            yield return new WaitUntil(() => { return (input = GetInputCode()) == null; });
            yield return new WaitUntil(() => { return (input = GetInputCode()) != null; });
            m_CodeInputStep++;

            for (int i = 0; i < m_Open.Count; i++)
            {
                int index = m_CodeInputStep - 1;
                if (m_Open[i].Info.Codes[index] == input)
                {
                    if (m_Open[i].Info.Codes.Length == m_CodeInputStep)
                    {
                        m_CurrentStratagem = m_Open[i];
                        m_CurrentStratagem.GetReady();
                        m_bCheckingCode = false;

                        if (OnGetReady != null) OnGetReady();
                        yield break;
                    }
                    continue;
                }
                else
                {
                    m_Close.Add(m_Open[i]);
                }
            }

            for (int i = 0; i < m_Close.Count; i++)
            {
                m_Open.Remove(m_Close[i]);
            }

            if (OnCheckingCode != null) OnCheckingCode();
        }
        m_bCheckingCode = false;
    }

    /*---------------------------
     * Define the input result. *
     ---------------------------*/

    private StratagemInfo.eCode? GetInputCode()
    {
        if (Input.GetAxisRaw(m_InputVertical) == 1) { return StratagemInfo.eCode.Up; }
        else if (Input.GetAxisRaw(m_InputVertical) == -1) { return StratagemInfo.eCode.Down; }
        else if (Input.GetAxisRaw(m_InputHorizontal) == -1) { return StratagemInfo.eCode.Left; }
        else if (Input.GetAxisRaw(m_InputHorizontal) == 1) { return StratagemInfo.eCode.Right; }
        else { return null; }
    }

    #endregion Check Input Code

    #endregion Private Function
}