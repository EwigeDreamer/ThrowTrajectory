using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallWeapon : MonoBehaviour, ITracedWeapon
{
    [SerializeField] float m_ThrowVelocity = 5f;
    [SerializeField] float m_BallSize = 0.5f;

    TraceArgs ITracedWeapon.GetTraceArgs()
    {
        return new TraceArgs()
        {
            pos = transform.position,
            dir = transform.forward,
            vel = m_ThrowVelocity,
            need = true
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
        {
            Transform ballTr = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            Rigidbody ballRb = ballTr.gameObject.AddComponent<Rigidbody>();
            Material ballMat = ballTr.gameObject.GetComponent<Renderer>().material;
            ballTr.localScale = new Vector3(m_BallSize, m_BallSize, m_BallSize);
            ballTr.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
            ballTr.position = transform.position;
            ballMat.color = Random.ColorHSV(0f, 1f);
            ballRb.velocity = transform.forward * m_ThrowVelocity;
            Destroy(ballTr.gameObject, 5f);
        }
    }
}
