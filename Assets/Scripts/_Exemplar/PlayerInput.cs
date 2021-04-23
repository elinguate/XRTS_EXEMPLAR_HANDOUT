using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public CharacterMotor m_AttachedMotor;

    public bool m_InControl = false;

    public Pathfinder m_Pathfinder;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_InControl = !m_InControl;
            if (m_Pathfinder)
            {
                m_Pathfinder.enabled = !m_InControl;
            }
        }

        if (m_InControl)
        {
            float x = 0.0f;
            if (Input.GetKey(KeyCode.D))
            {
                x += 1.0f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                x -= 1.0f;
            }
            float y = 0.0f;
            if (Input.GetKey(KeyCode.W))
            {
                y += 1.0f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                y -= 1.0f;
            }

            m_AttachedMotor.DriveMotor(new Vector2(x, y), Time.deltaTime);
        }
    }
}
