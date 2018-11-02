﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStratagemInfo : MonoBehaviour
{
    private Stratagem m_CurrentStratagem;
    private List<Image> m_Code;
    [SerializeField] private Image m_Icon;
    [SerializeField] private Text m_Uses;
    [SerializeField] private Transform m_ArrowRoot;
    [SerializeField] private Image m_Arrow;

    public void Initialize(Stratagem stratagem)
    {
        m_CurrentStratagem = stratagem;

        m_Icon.sprite = LoadIcon();
        CreateCodesDisplaye();
        UpdateUses();
    }

    public void UpdateUses()
    {
        int count = m_CurrentStratagem.Info.Uses - m_CurrentStratagem.UsesCount;
        m_Uses.text = count.ToString();
    }

    private Sprite LoadIcon()
    {
        Sprite iconImg = null;
        string imgName = string.Format("icon_{0}", m_CurrentStratagem.Info.ID);
        string imgPath = "UI/Resource/Icons/Stratagem";
        string fullPath = imgPath + "/" + imgName;

        if (AssetManager.m_Instance != null)
        {
            iconImg = AssetManager.m_Instance.GetAsset(typeof(Sprite), imgName, imgPath) as Sprite;
            if (iconImg == null)
            {
                iconImg = Resources.Load<Sprite>(fullPath);
                AssetManager.m_Instance.AddAsset(typeof(Sprite), imgName, imgPath, iconImg);
            }
        }
        else
        {
            iconImg = Resources.Load<Sprite>(fullPath);
        }
        return iconImg;
    }

    private void CreateCodesDisplaye()
    {
        m_Code = new List<Image>();

        for (int i = 0; i < m_CurrentStratagem.Info.Codes.Length; i++)
        {
            Image codeArrow = Instantiate(m_Arrow, m_ArrowRoot);

            switch (m_CurrentStratagem.Info.Codes[i])
            {
                case StratagemInfo.eCode.Up:
                    codeArrow.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    break;

                case StratagemInfo.eCode.Down:
                    codeArrow.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
                    break;

                case StratagemInfo.eCode.Right:
                    codeArrow.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    break;

                case StratagemInfo.eCode.Left:
                    codeArrow.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                    break;
            }

            codeArrow.gameObject.SetActive(true);
            m_Code.Add(codeArrow);
        }
    }
}