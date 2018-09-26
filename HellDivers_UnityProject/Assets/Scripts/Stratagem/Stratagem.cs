using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StratagemType
{
    Undefine = -1, Supply, Defensive, Offensive, Special
}

public enum StratagemCode
{
    Up, Down, Left, Right
}

public class Stratagem : MonoBehaviour
{
    public string ID { get; set; }

    private Player m_Player;
    private Transform m_LaunchPos;
    private StratagemInfo m_Info;
    private GameObject m_Display;
    private GameObject m_Item;
    private Rigidbody m_Rigidbody;
    private Animator m_Anima;

    public StratagemState State { get { return m_State; } }
    private StratagemState m_State = StratagemState.Standby;
    private int m_Uses = 0;
    private float m_CoolingTime = 0.0f;
    private float m_ActingTime = 0.0f;

    public enum StratagemState
    {
        Standby, Ready, Activating, Cooling
    }

    private delegate void DoState();

    private DoState m_DoState;

    /// <summary>
    /// Initialize by using stratagem data which in the stratagem table.
    /// </summary>
    public void Init(int Id)
    {
        StratagemInfo newInfo;

        if (GameData.Instance.StratagemTable.ContainsKey(Id) == false)
        {
            Debug.LogWarningFormat("Stratagem init faild : ID doesn't exist [{0}]", Id);
            return;
        }

        newInfo = GameData.Instance.StratagemTable[Id];

        if (m_Info == newInfo)
        {
            Debug.LogFormat("Stratagem has already set info = [{0}]", Id);
            return;
        }

        if (m_Info.display != newInfo.display)
        {
            GameObject display = ResourceManager.m_Instance.LoadData(typeof(GameObject), _DefaultSetting.StratagemFilePath, m_Info.display) as GameObject;

            if (display != null)
            {
                _SetDisplay(display);
            }
            else
            {
                string fullPath = _DefaultSetting.StratagemFilePath + "/" + m_Info.display;
                Debug.LogWarningFormat("Load stratagem display object faild : Resource doesn't exist ({0}) (Use default object)", fullPath);

                _SetDisplay(_DefaultSetting.DefaultStratagemDisplay);
            }
        }

        if (m_Info.item != newInfo.item)
        {
            GameObject item = ResourceManager.m_Instance.LoadData(typeof(GameObject), _DefaultSetting.ItemFilePath, m_Info.item) as GameObject;

            if (item != null)
            {
                _SetItem(item);
            }
            else
            {
                string fullPath = _DefaultSetting.StratagemFilePath + "/" + m_Info.display;
                Debug.LogWarningFormat("Load stratagem item object faild : Resource doesn't exist ({0} (Use default object))", fullPath);

                _SetItem(_DefaultSetting.DefaultItem);
            }
        }

        m_Info = newInfo;
    }

    // Use this for initialization
    private void Start()
    {
        m_Player = this.GetComponent<Player>();
        m_LaunchPos = (m_Player == null) ? this.transform : m_Player.Parts.RightHand;

        _InitByDefaultSetting();

        m_DoState = _DoStandby;
    }

    private void Update()
    {
        m_DoState();
    }

    private void _CheckStandby()
    {
    }

    private void _DoStandby()
    {
    }

    private void _DoReady()
    {
    }

    private void _DoCooling()
    {
        if (m_CoolingTime >= m_Info.cooldown)
        {
            m_CoolingTime = 0.0f;

            m_State = StratagemState.Standby;
            m_DoState = _DoStandby;
            return;
        }
        m_CoolingTime += Time.deltaTime;
    }

    private void _DoActivating()
    {
        if (m_ActingTime >= m_Info.activation)
        {
            m_ActingTime = 0.0f;

            m_Display.SetActive(false);
            Instantiate(m_Item, this.transform.position, Quaternion.identity, null);

            m_State = StratagemState.Cooling;
            m_DoState = _DoCooling;
            return;
        }
        m_ActingTime += Time.deltaTime;
    }

    public void ThrowOut(Vector3 direction, float size)
    {
        this.transform.parent = null;
        m_Rigidbody.isKinematic = false;
        Vector3 force = direction.normalized * size;
        m_Rigidbody.AddForce(force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Terrain")
        {
            m_Rigidbody.isKinematic = true;
            this.transform.up = Vector3.up;
            if (m_State == StratagemState.Ready)
            {
                m_State = StratagemState.Activating;
                Invoke("DoActivation", m_Info.activation);
            }
        }
    }

    /// <summary>
    /// Initialize the stratagem object with the default objects which is setting in the setting file.
    /// </summary>
    private void _InitByDefaultSetting()
    {
        if (ResourceManager.m_Instance != null)
        {
            _DefaultSetting = ResourceManager.m_Instance.LoadData(typeof(StratagemSetting), "", "StratagemSetting") as StratagemSetting;
        }
        else
        {
            Debug.Log("Resource Manager doesn't exist. Load data by Resource.Load method");
            _DefaultSetting = Resources.Load("StratagemSetting") as StratagemSetting;
        }

        if (_DefaultSetting == null)
        {
            Debug.LogWarning("Stratgem setting file missing.");
            return;
        }

        _SetDisplay(_DefaultSetting.DefaultStratagemDisplay);
        _SetItem(_DefaultSetting.DefaultItem);
    }

    /// <summary>
    /// Setup stratagem display object.
    /// And update the Rigidbody, Animator.
    /// </summary>
    private void _SetDisplay(GameObject display)
    {
        if (m_Display != display)
        {
            if (m_Display != null)
                DestroyImmediate(m_Display);

            m_Display = Instantiate(display);
            m_Display.transform.parent = this.transform;
            m_Display.SetActive(false);

            m_Rigidbody = m_Display.GetComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;

            m_Anima = m_Display.GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Setup the target item.
    /// </summary>
    private void _SetItem(GameObject item)
    {
        if (m_Item != item)
        {
            if (m_Item != null)
                DestroyImmediate(m_Item);

            m_Item = Instantiate(item);
            m_Item.transform.parent = this.transform;
            m_Item.SetActive(false);
        }
    }

    private StratagemSetting _DefaultSetting;
}