using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "XRTS/Motor Data")]
public class MotorData : ScriptableObject
{
    public float m_Acceleration = 1.0f;
    public AnimationCurve m_FrictionCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);
    public float m_StoppingFriction = 1.0f;

    public float m_Speed = 8.0f;

    public float m_CollisionSize = 0.5f;
    public LayerMask m_CollisionLayerMask;
    public LayerMask m_PathableLayerMask;

    public float m_BumpCollisionRadius = 0.1f;
    public float m_BumpStrength = 0.1f;
    public LayerMask m_BumpLayerMask;
}
