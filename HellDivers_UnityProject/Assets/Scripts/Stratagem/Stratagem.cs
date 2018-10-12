using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class Stratagem : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// Set the transform root for launch postion.
    /// </summary>
    public Transform LaunchPos { get { return m_LaunchPos; } set { m_LaunchPos = value; } }

    /// <summary>
    /// The info is used to initilize the stratagem.
    /// </summary>
    public StratagemInfo Info { get { return m_Info; } }

    /// <summary>
    /// The current state of the stratagem.
    /// </summary>
    public eState State { get { return m_eState; } }

    /// <summary>
    /// Represention the number of how many times has been used.
    /// </summary>
    public int UsesCount { get { return m_UsesCount; } }

    /// <summary>
    /// Is the stratagem cooling down ?
    /// </summary>
    public bool IsCooling { get { return m_IsCooling; } }

    /// <summary>
    /// The timer of CoolDown time. It start when do Throw.
    /// </summary>
    public float CoolTimer { get { return m_CoolTimer; } }

    /// <summary>
    /// The timer of Activatoin. It start when do Land on "terrain".
    /// </summary>
    public float ActTimer { get { return m_ActivationTimer; } }

    #endregion Properties

    #region Private Variable

    [SerializeField] private StratagemInfo m_Info;
    private Transform m_LaunchPos;
    private GameObject m_Display;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private float m_Radius = 0.25f;
    private eState m_eState = eState.Idle;
    private int m_UsesCount;
    private bool m_IsCooling;
    private float m_CoolTimer;
    private float m_ActivationTimer;

    #endregion Private Variable

    #region Initializer

    /// <summary>
    /// Setup stratagem by id key which is in the gamedata.stratagem table.
    /// </summary>
    /// <param name="id">The id key which is in the gamedata.stratagem table.</param>
    /// <param name="launchPos">The spawn transform root.</param>
    /// <returns></returns>
    public bool SetStratagemInfo(int id, Transform launchPos)
    {
        StratagemInfo newInfo;
        if (TryGetInfoFromGameData(id, out newInfo) == false) return false;

        this.gameObject.name = string.Format("Stratagem {0}", m_Info.title);

        if (newInfo.display != m_Info.display)
        {
            GameObject go;

            if (ResourceManager.m_Instance == null)
            {
                Debug.LogWarningFormat("Stratagem Warning ({0}) : ResourcesManager doesn't exist, Using Resource.Load()", this.gameObject.name);
                go = Resources.Load(string.Format("{0}/{1}", StratagemSystem.DisplayFolder, newInfo.display)) as GameObject;
            }
            else
            {
                go = ResourceManager.m_Instance.LoadData(typeof(GameObject), StratagemSystem.DisplayFolder, newInfo.display) as GameObject;
            }

            if (m_Display != go)
            {
                if (go == null) go = StratagemSystem.DefaultDisplay;

                DestroyImmediate(m_Display);
                m_Display = Instantiate(go, this.transform.position, Quaternion.identity, this.transform);
                m_Animator = m_Display.GetComponent<Animator>();
            }
        }

        m_LaunchPos = launchPos;
        this.transform.parent = m_LaunchPos;
        this.transform.localPosition = Vector3.zero;
        this.transform.localEulerAngles = Vector3.zero;

        newInfo.CopyTo(m_Info);
        ResetState();
        return true;
    }

    /*----------------------------------------------
     * Reset the (State = Standby) & (Timers = 0). *
    -----------------------------------------------*/

    private void ResetState()
    {
        m_UsesCount = 0;
        m_CoolTimer = 0.0f;
        m_ActivationTimer = 0.0f;
        m_eState = eState.Idle;
        StopAllCoroutines();
    }

    /*--------------------------------------------------------------------------
     * Get stratagem info by id key which is in the gamedata.stratagem tables. *
     * If it does not exist return null.                                       *
    ---------------------------------------------------------------------------*/

    private bool TryGetInfoFromGameData(int id, out StratagemInfo getInfo)
    {
        if (GameData.Instance.StratagemTable.ContainsKey(id) == false)
        {
            Debug.LogErrorFormat("Stratagem Error : Can't found ID : [{0}] from game data", id);
            getInfo = null;
            return false;
        }
        getInfo = GameData.Instance.StratagemTable[id];
        return true;
    }

    #endregion Initializer

    #region MonoBehaviour

    private void Awake()
    {
        m_Info = new StratagemInfo();
    }

    // Use this for initialization
    private void Start()
    {
        m_Rigidbody = this.GetComponent<Rigidbody>();
        m_Rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (m_eState == eState.ThrowOut) DoThrowOut();
    }

    #endregion MonoBehaviour

    #region Public Function

    /// <summary>
    /// Show the stratagem object & reset to the launch position.
    /// Translate to Ready state.
    /// </summary>
    public void GetReady()
    {
        if (m_UsesCount >= Info.uses && Info.uses >= 0) return;
        if (IsCooling || State != eState.Idle) return;

        this.transform.parent = m_LaunchPos;
        this.transform.localPosition = Vector3.zero;
        this.transform.localEulerAngles = Vector3.zero;
        m_Animator.SetTrigger("Start");

        m_eState = eState.Ready;
    }

    /// <summary>
    /// Add force to this gameobject for throw it out.
    /// Translate to Throw out state.
    /// </summary>
    /// <param name="force">Force for throw it out use relative force</param>
    public void Throw(Vector3 force)
    {
        if (IsCooling || State != eState.Ready) return;

        this.transform.parent = null;
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.AddRelativeForce(force);
        m_IsCooling = true;
        m_Animator.SetTrigger("Throw");

        // Uses add count. ( Info.uses = -1 ) is meaning for unlimited.
        if (Info.uses != -1) m_UsesCount++;

        // Start the cooldown timer.
        if (Info.cooldown > 0) StartCoroutine(DoCoolDown(Info.cooldown));

        // Translate to ThrowOut state.
        m_eState = eState.ThrowOut;
    }

    #endregion Public Function

    #region Finite State Machine

    /// <summary>
    /// Representation of the stratagem object current statement.
    /// </summary>
    public enum eState
    {
        /// <summary>
        /// It's the start state.
        /// Hide display object, Do nothing.
        /// Next state is Ready state.
        /// </summary>
        Idle,

        /// <summary>
        /// Display on the launch position.
        /// Waiting for throw it out.
        /// </summary>
        Ready,

        /// <summary>
        /// The stratagem is out of launch position root.
        /// Checking the terrain for landing.
        /// </summary>
        ThrowOut,

        /// <summary>
        /// After land on terrain, start counting timer.
        /// When time's up, spawn the traget item, and back to Idle state.
        /// </summary>
        Activating
    }

    /*--------------------------------------------------------------------------------
     * In ThrowOut state, checking the stratagem object is land on "Terrain" or not. *
     * When it landed successfully, then translate to Activating state.              *
     --------------------------------------------------------------------------------*/

    private void DoThrowOut()
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, m_Radius);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                m_Rigidbody.isKinematic = true;
                this.transform.rotation = Quaternion.Euler(Vector3.zero);
                m_Animator.SetTrigger("Land");

                // Start the activation timer.
                StartCoroutine(DoActivating(Info.activation));
            }
        }
    }

    /*----------------------------------------------------------------------
     * It's a timer for activaing process.                                 *
     * When the "End" animation was finished, than translate to Idle state *
     ----------------------------------------------------------------------*/

    private IEnumerator DoActivating(float targetTime)
    {
        m_eState = eState.Activating;

        m_ActivationTimer = 0.0f;
        while (m_ActivationTimer < targetTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            m_ActivationTimer += Time.deltaTime;
        }

        m_Animator.SetTrigger("End");

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo currentAnima = m_Animator.GetCurrentAnimatorStateInfo(0);
            return (currentAnima.IsName("End") && currentAnima.normalizedTime >= 1);
        });

        m_eState = eState.Idle;
        yield break;
    }

    /*-----------------------------------------
     * It's a timer for cooling down process. *
     -----------------------------------------*/

    private IEnumerator DoCoolDown(float targetTime)
    {
        m_IsCooling = true;

        m_CoolTimer = 0.0f;
        while (m_CoolTimer < targetTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            m_CoolTimer += Time.deltaTime;
        }
        m_IsCooling = false;
        yield break;
    }

    #endregion Finite State Machine

#if UNITY_EDITOR

    #region Debug Draw

    public bool ShowDebugInfo;

    private void OnDrawGizmos()
    {
        if (ShowDebugInfo)
        {
            if (State == eState.ThrowOut) Gizmos.DrawWireSphere(this.transform.position, m_Radius);

            if (State == eState.Activating)
            {
                string actMessage = string.Format("{0} Act : {1}", Info.title, m_ActivationTimer);
                style.normal.textColor = Color.red;
                Handles.Label(this.transform.position, actMessage, style);
            }

            if (IsCooling)
            {
                string coolMessage = string.Format("{0} CD : {1}", Info.title, m_CoolTimer);
                style.normal.textColor = Color.black;
                Handles.Label(m_LaunchPos.position, coolMessage, style);
            }
        }
    }

    private void OnGUI()
    {
        if (ShowDebugInfo)
        {
            string Message;
            Rect rect = new Rect(10, 10, 100, 20);

            switch (State)
            {
                case eState.Idle:
                    {
                        if (m_UsesCount >= Info.uses)
                        {
                            Message = string.Format("{0} /{1} out of uses", this.name, Info.title);
                            style.normal.textColor = Color.red;
                            GUI.Label(rect, Message, style);
                        }
                        else
                        {
                            Message = string.Format("{0} /{1} Idle", this.name, Info.title);
                            style.normal.textColor = Color.gray;
                            GUI.Label(rect, Message, style);
                        }
                        break;
                    }

                case eState.ThrowOut:
                    {
                        Message = string.Format("{0} /{1} throw out", this.name, Info.title);
                        style.normal.textColor = Color.gray;
                        GUI.Label(rect, Message, style);
                        break;
                    }

                case eState.Ready:
                    {
                        Message = string.Format("{0} /{1} ready", this.name, Info.title);
                        style.normal.textColor = Color.gray;
                        GUI.Label(rect, Message, style);
                        break;
                    }

                case eState.Activating:
                    {
                        Message = string.Format("{0} /{1} Act : {2} / {3}", this.name, Info.title, m_ActivationTimer, Info.activation);
                        style.normal.textColor = Color.red;
                        GUI.Label(rect, Message, style);
                        break;
                    }
                default:
                    break;
            }

            if (IsCooling)
            {
                Message = string.Format("{0} /{1} CD : {2} / {3}", this.name, Info.title, m_CoolTimer, Info.cooldown);
                style.normal.textColor = Color.black;
                rect.y *= 3;
                GUI.Label(rect, Message, style);
            }
        }
    }

    private GUIStyle style = new GUIStyle();

    #endregion Debug Draw

#endif
}