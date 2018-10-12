using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StratagemInfo
{
    public int id;
    public int rank;
    public string title;
    public eType type;
    public eCode[] codes;
    public int uses;
    public float cooldown;
    public float activation;
    public string display;
    public string item;

    public enum eType
    {
        Supply, Defensive, Offensive, Special
    }

    public enum eCode
    {
        Up, Down, Left, Right
    }

    public StratagemInfo Clone()
    {
        StratagemInfo clone = new StratagemInfo();
        CopyTo(clone);
        return clone;
    }

    public void CopyTo(StratagemInfo info)
    {
        info.id = this.id;
        info.rank = this.rank;
        info.title = this.title;
        info.type = this.type;
        info.codes = this.codes;
        info.uses = this.uses;
        info.cooldown = this.cooldown;
        info.activation = this.activation;
        info.display = this.display;
        info.item = this.item;
    }
}