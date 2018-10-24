﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAnimationsController : MonoBehaviour {

    public Animator Animator { get { return m_Animator; } set { m_Animator = value; } }

    private Animator m_Animator;
    private void Awake()
    {
        m_Animator = this.GetComponent<Animator>();
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetAnimator(eFSMStateID state)
    {
        UpdateAnimator(state);
    }

    public void SetAnimator(eFSMStateID state, bool Bool)
    {
        Debug.Log("Attack");
        UpdateAnimator(state, Bool);
    }

    private void UpdateAnimator(eFSMStateID state)
    {
        if(state == eFSMStateID.MoveToStateID)
        {
           // m_Animator.SetBool();
        }
        if (state == eFSMStateID.ChaseStateID)
        {
           //m_Animator.SetBool("Chase",true);
        }
        
    }

    private void UpdateAnimator(eFSMStateID state, bool Bool)
    {
        if (state == eFSMStateID.AttackStateID)
        {
            m_Animator.SetBool("Attack", Bool);
        }
    }
}