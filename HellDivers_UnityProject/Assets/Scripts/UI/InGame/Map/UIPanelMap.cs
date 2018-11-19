﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMap : MonoBehaviour {
    //public delegate void UIMapEventHolder();
    //public event UIMapEventHolder UpdatePoint;

    public static UIPanelMap Instance { get; private set; }
    public float RectWidth { get { return m_RectWidth; } }
    public float RectHeight { get { return m_RectHeight; } }
    public float MapRadius { get { return m_MapRadius; } }
    public float MapWidth{ get { return m_MapWidth; } }
    public float MapHeight { get { return m_MapHeight; } }
    public Color Color { get { return m_Color; } }
    public float Timer { get { return m_Timer; } }
    public List<GameObject> PointList { get { return m_PointList; } }
    [SerializeField] private UIMapPoint m_PointPrefab;
    private RectTransform m_RectTransform;
    private float m_RectWidth;
    private float m_RectHeight;
    private Image m_Image;
    private Color m_Color;
    private float m_Timer;
    private List<GameObject> m_PointList = new List<GameObject>();

    [SerializeField] private float m_MapRadius = 40.0f;
    [SerializeField] private float m_MapWidth = 544.0f;
    [SerializeField] private float m_MapHeight = 720.0f;


    private bool bAdd = true;

    // Use this for initialization
    void Start()
    {
        if (Instance == null) Instance = this;

        m_RectTransform = this.GetComponent<RectTransform>();
        m_RectWidth = m_RectTransform.sizeDelta.x;
        m_RectHeight = m_RectTransform.sizeDelta.y;
        m_Image = this.GetComponent<Image>();
        m_Color = m_Image.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddPointPrefab(GameObject target, eMapPointType type, int pointIndex)
    {
        //GameObject go = ObjectPool.m_Instance.LoadGameObjectFromPool(pointIndex);
        GameObject go = ObjectPool.m_Instance.LoadGameObjectFromPool(3003);
        go.SetActive(true);
        UIMapPoint p = go.GetComponent<UIMapPoint>();
        p.Init(target, type);
        p.transform.parent = this.transform;
        m_PointList.Add(go);
    }
    public void DeletePointPrefab(GameObject target)
    {
    }
}