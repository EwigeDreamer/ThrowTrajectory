using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITracedWeapon
{
    TraceArgs GetTraceArgs();
}

public struct TraceArgs
{
    public Vector3 pos;
    public Vector3 dir;
    public float vel;
    public bool need;
}

public class ThrowTrajectoryController : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] GameObject m_TracedWeapon;
    ITracedWeapon m_Weapon;

    [SerializeField] LineRenderer m_Line;

    [Header("Hit decal")]
    [SerializeField] Transform m_HitTr;

    [Header("Draw line")]
    [SerializeField] int m_DrawStepsMaxCount = 9;
    [SerializeField] float m_DrawTimeStep = 0.25f;

    [Header("Ray cast")]
    [SerializeField] LayerMask m_LayerMask;
    [SerializeField] int m_CastEveryDrawPoints = 3;

    Vector3 m_Gravity = Physics.gravity;
    Vector3 m_GravStep;
    List<Vector3> m_Buffer = new List<Vector3>(10);

    List<Vector3> m_Points = new List<Vector3>(100);
    Vector3 m_Cast1, m_Cast2;
    Vector3 m_Vel;
    Vector3 m_VelStep;
    RaycastHit m_Hit;

    bool m_ActiveHit;
    Vector3 m_HitPos;
    Quaternion m_HitRot;

    TraceArgs m_TrArgs;
    bool m_NeedTrace;
    bool m_IsTraced;
#pragma warning restore 649

    void Awake()
    {
        m_GravStep = m_Gravity * m_DrawTimeStep;
        Init();
    }
    
    public void SetWeapon(GameObject weapon)
    {
        if (weapon == null)
        {
            m_Weapon = null;
            return;
        }
        m_Weapon = weapon.GetComponent<ITracedWeapon>();
    }
    public void RemoveWeapon()
    {
        m_Weapon = null;
    }
    
    public void Init()
    {
        m_Weapon = m_TracedWeapon.GetComponent<ITracedWeapon>();
    }

    void Update()
    {
        if (m_Weapon != null)
        {
            m_TrArgs = m_Weapon.GetTraceArgs();
            m_NeedTrace = m_TrArgs.need;
        }
        else
            m_NeedTrace = false;

        if (m_NeedTrace)
        {
            m_IsTraced = true;
            m_Cast1 = m_TrArgs.pos;
            m_Cast2 = m_Cast1;
            m_Points.Add(m_Cast1);
            m_Vel = m_TrArgs.dir * m_TrArgs.vel;
            m_Buffer.Clear();

            for (int i = 0; i < m_DrawStepsMaxCount; ++i)
            {
                m_Vel += m_GravStep;
                m_VelStep = m_Vel * m_DrawTimeStep;
                m_Cast2 += m_VelStep;

                if (!((i > 0 && (i + 1) % m_CastEveryDrawPoints == 0) || i == m_DrawStepsMaxCount - 1))
                {
                    m_Buffer.Add(m_Cast2);
                }
                else
                {
                    if (Physics.Linecast(m_Cast1, m_Cast2, out m_Hit, m_LayerMask))
                    {
                        if (i < m_CastEveryDrawPoints + 1)
                            m_Points.Add((m_Cast1 + m_Hit.point) * 0.5f);
                        m_Points.Add(m_Hit.point);
                        m_ActiveHit = true;
                        m_HitPos = m_Hit.point + m_Hit.normal * 0.1f;
                        m_HitRot = Quaternion.LookRotation(-m_Hit.normal);
                        break;
                    }
                    else
                    {
                        m_Buffer.Add(m_Cast2);
                        m_Points.AddRange(m_Buffer);
                        m_Buffer.Clear();
                        m_Cast1 = m_Cast2;
                        m_ActiveHit = false;
                    }
                }
            }

            m_Line.positionCount = m_Points.Count;
            m_Line.SetPositions(m_Points.ToArray());
            m_Points.Clear();

            if (m_ActiveHit)
            {
                if (!m_HitTr.gameObject.activeSelf) m_HitTr.gameObject.SetActive(true);
                m_HitTr.position = m_HitPos;
                m_HitTr.rotation = m_HitRot;
            }
            else
            {
                if (m_HitTr.gameObject.activeSelf) m_HitTr.gameObject.SetActive(false);
            }
        }
        else
        {
            if (m_IsTraced)
            {
                m_IsTraced = false;
                m_Line.positionCount = 0;
                if (!m_HitTr.gameObject.activeSelf)  m_HitTr.gameObject.SetActive(false);
            }
        }
    }
}
