using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StratagemController : MonoBehaviour
{
    private PlayerInfo m_playerInfo;
    private Transform m_LaunchPos { get; set; }
    private GameObject m_Display = null;
    [SerializeField] private StratagemInfo[] datas = new StratagemInfo[2];

    private List<Stratagem> m_Stratagems;

    // Use this for initialization
    private void Start()
    {
        Player p = GetComponent<Player>();
        if (p != null)
        {
            m_playerInfo = p.Info;
            m_LaunchPos = p.Parts.LaunchPoint;
        }

        if (p.Info.StratagemId.Count > 0)
        {
            m_Stratagems = new List<Stratagem>();

            for (int i = 0; i < p.Info.StratagemId.Count; i++)
            {
                GameObject go = new GameObject();
                Stratagem s = go.AddComponent<Stratagem>();
                s.Init(p.Info.StratagemId[i]);

                m_Stratagems.Add(s);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private IEnumerator CheckInputCode()
    {
        _Open.Clear();

        foreach (StratagemInfo s in datas)
        {
            if (s != null) _Open.Add(s);
        }

        int inputCount = 0;
        StratagemCode? input = null;
        while (_Open.Count > 0)
        {
            yield return new WaitUntil(() =>
            {
                GetInputCode(out input);
                return input == null;
            });

            yield return new WaitUntil(() =>
            {
                return GetInputCode(out input);
            });

            inputCount++;

            for (int i = 0; i < _Open.Count; i++)
            {
                if (_Open[i].code[inputCount - 1] == input)
                {
                    if (_Open[i].code.Length == inputCount)
                    {
                        Instantiate(m_Display, m_LaunchPos);
                        yield break;
                    }
                    continue;
                }
                else
                {
                    _Open.RemoveAt(i);
                }
            }
        }
    }

    private bool GetInputCode(out StratagemCode? input)
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            input = StratagemCode.Up;
            return true;
        }
        else if (Input.GetAxisRaw("Vertical") < 0)
        {
            input = StratagemCode.Down;
            return true;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            input = StratagemCode.Left;
            return true;
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            input = StratagemCode.Right;
            return true;
        }
        else
        {
            input = null;
            return false;
        }
    }

    private List<StratagemInfo> _Open = new List<StratagemInfo>();
}