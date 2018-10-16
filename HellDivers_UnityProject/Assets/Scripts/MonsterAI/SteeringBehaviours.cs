﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SteeringBehaviours
{
    static public void Move(AIData data)
    {
        if (data.m_bMove == false) { return; }
        Transform t = data.m_Go.transform;
        Vector3 cPos = data.m_Go.transform.position;
        Vector3 vRight = t.right;
        Vector3 vForward = t.forward;
        if (data.m_fTempTurnForce > data.m_fMaxRot) { data.m_fTempTurnForce = data.m_fMaxRot; }
        else if (data.m_fTempTurnForce < -data.m_fMaxRot) { data.m_fTempTurnForce = -data.m_fMaxRot; }
        Vector3 vOriF = vForward;
        vForward += vRight * data.m_fTempTurnForce;
        vForward.y = 0;
        vForward.Normalize();

        t.forward = vForward;
        data.m_Speed = data.m_Speed + data.m_fMoveForce * Time.deltaTime;
        if (data.m_Speed < 0.001f)
        {
            data.m_Speed = 0.001f;
        }
        else if (data.m_Speed > data.m_fMaxSpeed * data.m_fMoveForce * 0.5f)
        {
            data.m_Speed = data.m_fMaxSpeed;
        }
        
        cPos = cPos + (t.forward) * data.m_Speed;
        t.position = cPos;

    }
    static public bool EnemiesAvoided(AIData data)
    {
        Transform Mover = data.m_Go.transform;

        if (Physics.Raycast(Mover.position, Mover.forward, data.m_fProbeLength * 2, 1<< LayerMask.NameToLayer("Enemies")))
        {
            Vector3 TargetDir = Vector3.forward;
            Vector3 vCross = Vector3.Cross(Mover.forward, TargetDir);
            float fTurnMag = Vector3.Dot(Mover.forward, TargetDir);

            if (vCross.y > 0.0f) { fTurnMag = -fTurnMag; }
            data.m_fTempTurnForce = fTurnMag;
            float fTotalLen = data.m_fProbeLength + data.m_TargetObject.transform.localScale.magnitude;
            float fRatio = TargetDir.magnitude / fTotalLen;
            if (fRatio > 1.0f) { fRatio = 1.0f; }
            data.m_fMoveForce = fRatio - 1.0f;
            data.m_bMove = true;
            data.m_bCol = false;
            return true;
        }
        return false;
    }

    static public bool CollisionAvoided(AIData data)
    {
        Vector3 curPos = data.m_Go.transform.position;
        Vector3 vForward = data.m_Go.transform.forward;
        Vector3 vRight = data.m_Go.transform.right;
        Vector3 TargetDir = Vector3.forward;
        Vector3 vec;
        float fDist;
        float fDot;
        float fFinalDot;
        float fFinalDotDist;
        float fFinalProjDist;
        float radius = data.m_fRadius;
        float fMinDist = 1000.0f;
        Collider ColObject = null;
        Collider[] c = Physics.OverlapSphere(curPos, radius, 1 << LayerMask.NameToLayer("Terrain"));

        
        if (c.Length < 1)
        {
            data.m_bCol = false;
            return false;
        }
        for (int i = 0; i < c.Length; i++)
        {
            
            vec = c[i].transform.position - curPos;
            vec.y = 0;
            fDist = vec.magnitude;
            vec.Normalize();
            if (fDist > data.m_fProbeLength) continue;

            fDot = Vector3.Dot(vec, vForward);
            if (fDot < 0.0f) continue;
            float fProjectDist = fDist * fDot;
            float fDotDist = Mathf.Sqrt(fDist * fDist - fProjectDist * fProjectDist);
            if (fDotDist > data.m_fRadius + c[i].transform.localScale.magnitude)
            {
                continue;
            }

            if (fDist < fMinDist)
            {
                fMinDist = fDist;
                fFinalDotDist = fDotDist;
                fFinalProjDist = fProjectDist;
                TargetDir = vec;
                ColObject = c[i];
                fFinalDot = fDot;
            }
        }
        if (ColObject != null)
        {
            Vector3 vCross = Vector3.Cross(vForward, TargetDir);
            float fTurnMag = Vector3.Dot(vForward, TargetDir);

            if (vCross.y > 0.0f) { fTurnMag = -fTurnMag; }
            data.m_fTempTurnForce = fTurnMag;
            float fTotalLen = data.m_fProbeLength + data.m_TargetObject.transform.localScale.magnitude;
            float fRatio = fMinDist / fTotalLen;
            if (fRatio > 1.0f) { fRatio = 1.0f; }
            data.m_fMoveForce = fRatio - 1.0f;
            data.m_bMove = true;
            data.m_bCol = false;
            return true;
        }
        data.m_bCol = false;
        return false;
    }

    static public bool Seek(AIData data)
    {
        Vector3 curPos = data.m_Go.transform.position;
        Vector3 vForward = data.m_Go.transform.forward;
        Vector3 vRight = data.m_Go.transform.right;
        Vector3 TargetDir = data.m_vTarget - curPos;
        float fDist2Target = TargetDir.magnitude;
        TargetDir.Normalize();
        float fDotForward = Vector3.Dot(vForward, TargetDir);
        if (fDist2Target < data.m_fProbeLength)
        {
            data.m_fMoveForce = 0.0f;
 //           data.m_fTempTurnForce = 0.0f;
            data.m_Speed = 0.0f;
            data.m_bMove = false;
            return false;
        }
        if (fDotForward > 0.97f)
        {
            fDotForward = 1.0f;
            data.m_Go.transform.forward = TargetDir;
            data.m_fTempTurnForce = 0.0f;
            //data.m_fRot = 0.0f;
        }
        else
        {
            if (fDotForward < -1.0f) fDotForward = -1.0f;
            float fDotRight = Vector3.Dot(vRight, TargetDir);
            if (fDotForward < 0.0f) fDotRight = (fDotRight > 0.0f) ? 1.0f : -1.0f;
            if (fDist2Target < data.m_fProbeLength * 1.5f) fDotRight *= fDist2Target;
            data.m_fTempTurnForce = fDotRight;
        }

        data.m_fMoveForce = fDotForward;
        data.m_bMove = true;
        return true;
    }

    static public Vector3 GroupBehavior(AIData data, float radius, bool Seperate)
    {
        Vector3 m_vForward = Vector3.zero;
        Vector3 MoverPos = data.m_Go.transform.position;
        float Radius = data.m_fRadius * radius;
        float distance;
        Collider[] Colliders = Physics.OverlapSphere(MoverPos, Radius, 1 << LayerMask.NameToLayer("Enemies"));
        for (int i = 0; i < Colliders.Length; i++)
        {
            Vector3 vec = (Colliders[i].transform.position - MoverPos);
            distance = vec.magnitude;
            vec.Normalize();
            m_vForward += vec * (distance / Radius + (Seperate? -1:0));
        }
        m_vForward /= Colliders.Length;
        m_vForward.y = 0;
        return m_vForward;
    }



}