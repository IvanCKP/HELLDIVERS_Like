﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StratagemController : MonoBehaviour
{
    #region Define Inputs

    private string m_InputStartCode = "Stratagem";
    private string m_InputVertical = "Vertical";
    private string m_InputHorizontal = "Horizontal";
    private string m_InputThrow = "Fire1";

    #endregion Define Inputs

    #region Properties

    public bool IsReady { get { return m_CurrentStratagem != false; } }
    public bool IsCheckingCode { get { return m_bCheckingCode; } }
    public Stratagem CurrentStratagem { get { return m_CurrentStratagem; } }
    public List<Stratagem> Stratagems { get { return m_Stratagems; } }

    #endregion Properties

    #region Private Variable

    [SerializeField] private List<Stratagem> m_Stratagems = new List<Stratagem>();
    private bool m_bCheckingCode;

    // Current actvating stratagem.
    private Stratagem m_CurrentStratagem;

    private Vector3 m_Force = new Vector3(0.0f, 200.0f, 500.0f);

    #endregion Private Variable

    #region Public Function

    /// <summary>
    /// Add a stratagem by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="id">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public bool AddStratagem(int id, Transform launchPos)
    {
        for (int i = 0; i < m_Stratagems.Count; i++)
        {
            if (m_Stratagems[i].Info.ID == id) return false;
        }

        GameObject go = new GameObject("Stratagem");
        Stratagem s = go.AddComponent<Stratagem>();
        s.SetStratagemInfo(id, launchPos);
        m_Stratagems.Add(s);
        return true;
    }

    /// <summary>
    /// Add a multi stratagems by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="ids">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public void AddStratagems(List<int> ids, Transform launchPos)
    {
        foreach (int id in ids)
        {
            AddStratagem(id, launchPos);
        }
    }

    /// <summary>
    /// Add a multi stratagems by id key.
    /// If the id already in the controller will be pass.
    /// </summary>
    /// <param name="ids">The id key which in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    public void AddStratagems(int[] ids, Transform launchPos)
    {
        foreach (int id in ids)
        {
            AddStratagem(id, launchPos);
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

    public bool StartCheckCodes()
    {
        if (m_Stratagems.Count <= 0) return false;

        StartCoroutine(CheckInputCode());
        return true;
    }

    public void StopCheckCodes()
    {
        StopCoroutine(CheckInputCode());
    }

    public bool Throw()
    {
        if (IsReady == false) return false;
        m_CurrentStratagem.Throw(m_Force);
        m_CurrentStratagem = null;
        return true;
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

    #endregion Public Function

    #region MonoBehaviour

    // Update is called once per frame
    private void Update()
    {
        //if (IsReady)
        //{
        //    if (Input.GetButtonDown(m_InputThrow))
        //    {
        //        m_CurrentStratagem.Throw(m_Force);
        //        m_CurrentStratagem = null;
        //    }
        //}
        //else
        //{
        //    if (m_Stratagems.Count > 0)
        //    {
        //        if (Input.GetButtonDown(m_InputStartCode))
        //        {
        //            StartCoroutine(CheckInputCode());
        //        }
        //        else if (Input.GetButtonUp(m_InputStartCode))
        //        {
        //            StopCoroutine(CheckInputCode());
        //        }
        //    }
        //}
    }

    #endregion MonoBehaviour

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
        if (Input.GetAxisRaw(m_InputVertical) > 0) { return StratagemInfo.eCode.Up; }
        else if (Input.GetAxisRaw(m_InputVertical) < 0) { return StratagemInfo.eCode.Down; }
        else if (Input.GetAxisRaw(m_InputHorizontal) < 0) { return StratagemInfo.eCode.Left; }
        else if (Input.GetAxisRaw(m_InputHorizontal) > 0) { return StratagemInfo.eCode.Right; }
        else { return null; }
    }

    private List<Stratagem> _Open = new List<Stratagem>();

    #endregion Check Input Code
}