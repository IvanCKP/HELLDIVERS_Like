using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public class GameObjectData
    {
        public GameObject m_go;
        public bool m_bUse;
    }

    static public ObjectPool m_Instance;

    private Dictionary<int, List<GameObjectData>> m_dObjectDitc;

    public void Init()
    {
        m_Instance = this;
        m_dObjectDitc = new Dictionary<int, List<GameObjectData>>();
    }

    public void InitGameObjects(Object o, int iCount, int iType)
    {
        List<GameObjectData> pList;

        if (m_dObjectDitc.ContainsKey(iType) == false)
        {
            pList = new List<GameObjectData>();
            m_dObjectDitc.Add(iType, pList);
        }
        else
        {
            pList = m_dObjectDitc[iType];
            //need safer
        }
        for (int i = 0; i < iCount; i++)
        {
            GameObjectData gData = new GameObjectData();

            gData.m_go = GameObject.Instantiate(o) as GameObject;
            gData.m_bUse = false;
            gData.m_go.SetActive(false);
            pList.Add(gData);
        }
    }

    public GameObject LoadGameObjectFromPool(int iType)
    {
        List<GameObjectData> pList;
        if (m_dObjectDitc.ContainsKey(iType) == false) { return null; }
        else { pList = m_dObjectDitc[iType]; }

        GameObject rgo = null;

        for (int i = 0; i < pList.Count; i++)
        {
            if (pList[i].m_bUse == false)
            {
                pList[i].m_bUse = true;
                rgo = pList[i].m_go;
                break;
            }
        }
        return rgo;
    }

    public void UnLoadObjectToPool(int iType, GameObject go)
    {
        List<GameObjectData> pList;
        if (m_dObjectDitc.ContainsKey(iType) == false) { return; }
        else { pList = m_dObjectDitc[iType]; }

        for (int i = 0; i < pList.Count; i++)
        {
            if (pList[i].m_go == go)
            {
                go.SetActive(false);
                pList[i].m_bUse = false;
                break;
            }
        }
    }
}