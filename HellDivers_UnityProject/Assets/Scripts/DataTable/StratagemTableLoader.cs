using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StratagemDataLoader
{
    public static string Path;

    public static Dictionary<int, StratagemInfo> LoadData(string filePah)
    {
        Dictionary<int, StratagemInfo> stratagemTable = new Dictionary<int, StratagemInfo>();

        if (_LoadDataBase(filePah, ref stratagemTable) == true)
        {
            Debug.LogFormat("StratagemDatabase has been load.");
            return stratagemTable;
        }
        else
        {
            Debug.LogFormat("StratagemDatabase loading faild.");
            return null;
        }
    }

    private static bool _LoadDataBase(string tablePath, ref Dictionary<int, StratagemInfo> table)
    {
        table.Clear();

        TextAsset datas = Resources.Load<TextAsset>(tablePath);
        if (datas != null)
        {
            string[] line = datas.text.Split('\n');
            for (int i = 1; i < line.Length - 1; i++)
            {
                string[] colum = line[i].Split(',');

                StratagemInfo data = new StratagemInfo()
                {
                    id = int.Parse(colum[0]),
                    rank = int.Parse(colum[1]),
                    title = colum[2],
                    type = (StratagemInfo.eType)Enum.Parse(typeof(StratagemInfo.eType), colum[3]),
                    codes = _GetCodes(colum[4]),
                    uses = int.Parse(colum[5]),
                    cooldown = float.Parse(colum[6]),
                    activation = float.Parse(colum[7]),
                    display = colum[8]
                };

                table.Add(data.id, data);
            }

            return true;
        }

        return false;
    }

    private static StratagemInfo.eCode[] _GetCodes(string code)
    {
        StratagemInfo.eCode[] codes = new StratagemInfo.eCode[code.Length];

        for (int i = 0; i < code.Length; i++)
        {
            codes[i] = (StratagemInfo.eCode)Enum.Parse(typeof(StratagemInfo.eCode), code[i].ToString());
        }

        return codes;
    }
}