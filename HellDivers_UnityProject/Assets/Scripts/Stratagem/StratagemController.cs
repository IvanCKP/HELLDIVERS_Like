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

    #endregion Properties

    #region Private Variable

    [SerializeField] private List<Stratagem> m_Stratagems = new List<Stratagem>();
    private bool m_bCheckingCode;

    // Current actvating stratagem.
    private Stratagem m_CurrentStratagem;

    // Throw out force.
    private Vector3 m_Force = new Vector3(0.0f, 200.0f, 500.0f);

    #endregion Private Variable

    #region Public Function

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

        GameObject go = new GameObject("Stratagem");
        Stratagem s = go.AddComponent<Stratagem>();
        s.SetStratagemInfo(id, readyPos, launchPos);
        m_Stratagems.Add(s);
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
        Stratagem target = null;
        if (index < 0 || index >= m_Stratagems.Count) return false;

        target = m_Stratagems[index];
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
            if (m_Stratagems[i] != null)
            {
                DestroyImmediate(m_Stratagems[i].gameObject);
            }
        }

        m_Stratagems.Clear();
        m_CurrentStratagem = null;
    }

    /// <summary>
    /// Start checking stratagem codes.
    /// </summary>
    /// <returns>Was there are any stratagems in the contorller ?</returns>
    public bool StartCheckCodes()
    {
        if (m_Stratagems.Count <= 0) return false;

        StartCoroutine(CheckInputCode());
        return true;
    }

    /// <summary>
    /// Stop checking stratagem codes.
    /// </summary>
    public void StopCheckCodes()
    {
        StopCoroutine(CheckInputCode());
        m_bCheckingCode = false;
    }

    /// <summary>
    /// Throw out the current stratagem.
    /// </summary>
    /// <returns>Was there is stratagem which is ready and thorw out success ?</returns>
    public bool Throw()
    {
        if (IsReady == false) return false;
        m_CurrentStratagem.Throw(m_Force);
        m_CurrentStratagem = null;
        return true;
    }

    #endregion Public Function

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
        _Open.Clear();

        foreach (Stratagem s in m_Stratagems)
        {
            if (s != null && s.Info != null && s.State == Stratagem.eState.Idle)
                _Open.Add(s);
        }

        if (_Open.Count <= 0) yield break;

        int inputCount = 0;
        StratagemInfo.eCode? input = null;
        while (_Open.Count > 0)
        {
            yield return new WaitUntil(() => { return (input = GetInputCode()) == null; });
            yield return new WaitUntil(() => { return (input = GetInputCode()) != null; });
            inputCount++;

            for (int i = 0; i < _Open.Count; i++)
            {
                if (_Open[i].Info.Codes[inputCount - 1] == input)
                {
                    if (_Open[i].Info.Codes.Length == inputCount)
                    {
                        m_CurrentStratagem = _Open[i];
                        m_CurrentStratagem.GetReady();
                        m_bCheckingCode = false;
                        yield break;
                    }
                    continue;
                }
                else
                { _Open.RemoveAt(i); }
            }
        }
        m_bCheckingCode = false;
    }

    /*---------------------------
     * Define the input result. *
     ---------------------------*/

    private StratagemInfo.eCode? GetInputCode()
    {
        if (Input.GetAxisRaw(m_InputVertical) >= 1) { return StratagemInfo.eCode.Up; }
        else if (Input.GetAxisRaw(m_InputVertical) <= -1) { return StratagemInfo.eCode.Down; }
        else if (Input.GetAxisRaw(m_InputHorizontal) <= -1) { return StratagemInfo.eCode.Left; }
        else if (Input.GetAxisRaw(m_InputHorizontal) >= 1) { return StratagemInfo.eCode.Right; }
        else { return null; }
    }

    private List<Stratagem> _Open = new List<Stratagem>();

    #endregion Check Input Code
}