﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour {

    bool m_bBlack;
    Color color;
    private void OnEnable()        
    {
        Debug.Log("Fade Panel OnEnable.");
        m_bBlack = false;
        color = GetComponent<Image>().color;
    }
    private void FixedUpdate()
    {
        if (!m_bBlack)
        {
            color.a = Mathf.Lerp(color.a, 1, 0.05f);
            GetComponent<Image>().color = color;
            if (GetComponent<Image>().color.a > .99f) m_bBlack = true;
        }
        else
        {
            color.a = Mathf.Lerp(color.a, 0, 0.01f);
            GetComponent<Image>().color = color;
            if (GetComponent<Image>().color.a < 0.2f) gameObject.SetActive(false);
        }
    }

}
