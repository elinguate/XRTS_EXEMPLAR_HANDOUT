using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
    public AnimatedCharacterSprite m_SpriteAnimator;
    public MotorData m_Data;

    public bool m_UpdateSelf = false;
    private Vector2 m_Velocity;

    private void Update()
    {
        if (m_UpdateSelf)
        {
            ProcessMove(Time.deltaTime);
        }
    }

    public void DriveMotor(Vector2 _input, float _dT)
    {
        _input.Normalize();
        HandleInput(_input, _dT);
        m_SpriteAnimator.UpdateSpriteVelocity(m_Velocity, _dT);
        ProcessMove(_dT);
    }

    private void HandleInput(Vector2 _input, float _dT)
    {
        m_Velocity += _input * m_Data.m_Acceleration * _dT;
        m_Velocity -= m_Velocity.normalized * m_Data.m_Acceleration * m_Data.m_FrictionCurve.Evaluate(m_Velocity.magnitude) * _dT;

        if (_input.magnitude < 0.01f)
        {
            Vector2 cachedMove = m_Velocity;
            m_Velocity -= m_Velocity.normalized * m_Data.m_StoppingFriction * _dT;
            if (Vector2.Dot(cachedMove.normalized, m_Velocity.normalized) < -0.01f)
            {
                m_Velocity.x = 0.0f;
                m_Velocity.y = 0.0f;
            }
        }
    }

    private void ProcessMove(float _dT)
    {
        Collider2D[] bumps = Physics2D.OverlapCircleAll(transform.position, m_Data.m_BumpCollisionRadius, m_Data.m_BumpLayerMask);
        for (int i = 0; i < bumps.Length; i++)
        {   
            Vector2 dir = transform.position - bumps[i].transform.position;
            float invStrength = Mathf.Clamp01(m_Data.m_BumpCollisionRadius - dir.magnitude); 
            m_Velocity += dir.normalized * m_Data.m_BumpStrength * invStrength * invStrength;
        }

        if (m_Velocity.magnitude > 1.0f)
        {
            m_Velocity.Normalize();
        }

        Vector2 size = Vector2.one * m_Data.m_CollisionSize;
        float stepX = m_Velocity.x * m_Data.m_Speed * _dT;
        RaycastHit2D hitX = Physics2D.BoxCast(transform.position, size, 0.0f, Vector2.right * Mathf.Sign(stepX), Mathf.Max(0.01f, Mathf.Abs(stepX)), m_Data.m_CollisionLayerMask); 
    
        if (hitX)
        {
            stepX = Mathf.Sign(stepX) * Mathf.Max(0.0f, hitX.distance - 0.01f);
            m_Velocity.x = 0.0f;
        }

        transform.position += new Vector3(stepX, 0.0f, 0.0f);

        float stepY = m_Velocity.y * m_Data.m_Speed * _dT;
        RaycastHit2D hitY = Physics2D.BoxCast(transform.position, size, 0.0f, Vector2.up * Mathf.Sign(stepY), Mathf.Max(0.01f, Mathf.Abs(stepY)), m_Data.m_CollisionLayerMask); 
    
        if (hitY)
        {
            stepY = Mathf.Sign(stepY) * Mathf.Max(0.0f, hitY.distance - 0.01f);
            m_Velocity.y = 0.0f;
        }

        transform.position += new Vector3(0.0f, stepY, 0.0f);
    }
}
