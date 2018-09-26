using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StratagrmSetting", menuName = "StratagrmSetting", order = 100)]
public class StratagemSetting : ScriptableObject
{
    public string StratagemFilePath = "Startagems";
    public string ItemFilePath = "Items";
    public GameObject DefaultStratagemDisplay;
    public GameObject DefaultItem;
}