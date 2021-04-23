using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [System.Serializable]
    public struct Pathway
    {
        public Vector3[] m_NodePositions;

        private int GetClosestNode(Vector3 _pos)
        {
            float shortestDistance = 999.9f;
            int shortestIndex = -1;

            if (m_NodePositions == null || m_NodePositions.Length <= 0)
            {
                return shortestIndex;
            }

            for (int i = 0; i < m_NodePositions.Length; i++)
            {
                float dist = (_pos - m_NodePositions[i]).magnitude;
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    shortestIndex = i;
                }
            }
            return shortestIndex;
        }

        private Vector3 GetNextNodeDirection(int _i, Vector3 _pos)
        {
            if (_i >= m_NodePositions.Length - 1)
            {
                return (m_NodePositions[_i] - _pos).normalized;
            }
            
            return (m_NodePositions[_i + 1] - _pos).normalized;
        }

        public Vector3 CalculateHeading(Vector3 _pos)
        {
            int i = GetClosestNode(_pos);
            if (i == -1)
            {
                return Vector3.zero;
            }

            return GetNextNodeDirection(i, _pos);
        }

        public Vector3 CalculateCleverHeading(Vector3 _pos, float _size, LayerMask _pathableLayerMask)
        {
            int currentIndex = GetClosestNode(_pos);
            if (currentIndex == -1)
            {
                return Vector3.zero;
            }

            Vector3 nextNode = m_NodePositions[currentIndex];
            for (int i = 0; i < 10; i++)
            {
                currentIndex++;
                if (currentIndex > m_NodePositions.Length - 1)
                {
                    break;
                }
                
                Vector2 direction = m_NodePositions[currentIndex] - _pos;
                if (Physics2D.BoxCast(_pos, new Vector2(_size, _size), 0.0f, direction.normalized, direction.magnitude, _pathableLayerMask))
                {
                    break;
                }
                nextNode = m_NodePositions[currentIndex];
            }
            return (nextNode - _pos).normalized;
        }
    }

    public EnemyManager m_Manager;
    public bool m_Pursue = false;
    public CharacterMotor m_AttachedMotor;

    public Transform m_Target;
    private Transform m_CachedTransform;

    public PathfindingGrid m_Grid;
    public bool m_PathwayGenerated = false;
    public Pathway m_CalculatedPathway;

    public float m_CalculateTimer = 0.0f;
    public Vector2 m_CalculateDelayExtents = new Vector2(0.25f, 0.5f);
    
    private void Awake()
    {
        m_Grid = FindObjectOfType<PathfindingGrid>();
        m_Manager = FindObjectOfType<EnemyManager>();
    }

    private void OnDrawGizmos()
    {
        if (m_CalculatedPathway.m_NodePositions == null || m_CalculatedPathway.m_NodePositions.Length <= 0)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < m_CalculatedPathway.m_NodePositions.Length - 1; i++)
        {
            Gizmos.DrawLine(m_CalculatedPathway.m_NodePositions[i], m_CalculatedPathway.m_NodePositions[i + 1]);
        }
    }

    private bool GeneratePathway()
    {
        if (!m_Grid.m_GridInitialized)
        {
            return false;
        }
        m_CalculatedPathway = new Pathway();

        PathfindingStep steps = m_Grid.PathfindFromTo(transform.position, m_Target.position);
        if (steps.m_PriorNodes == null || steps.m_PriorNodes.Count <= 0)
        {
            return false;
        }
        
        m_CalculatedPathway.m_NodePositions = new Vector3[steps.m_PriorNodes.Count + 1];
        for (int i = 0; i < steps.m_PriorNodes.Count; i++)
        {
            m_CalculatedPathway.m_NodePositions[i] = steps.m_PriorNodes[i].transform.position;
        }
        m_CalculatedPathway.m_NodePositions[steps.m_PriorNodes.Count] = steps.m_Node.transform.position;
        return true;
    }

    private void Update()
    {
        if (!m_Target)
        {
            m_Target = m_Manager.GetTarget();
        }

        if (m_CachedTransform != m_Target)
        {
            m_CachedTransform = m_Target;
            m_PathwayGenerated = false;
        }

        if (m_Target && m_Pursue)
        {
            m_CalculateTimer -= Time.deltaTime; 
            if (!m_PathwayGenerated || m_CalculatedPathway.m_NodePositions == null || m_CalculatedPathway.m_NodePositions.Length <= 0 || m_CalculateTimer < 0.0f)
            {
                GeneratePathway();
                m_CalculateTimer = Random.Range(m_CalculateDelayExtents.x, m_CalculateDelayExtents.y);
                m_PathwayGenerated = true;
            }

            Vector3 direction = m_Target.position - transform.position;
            //float magnitude = direction.magnitude;
            //direction.Normalize();
            direction = m_CalculatedPathway.CalculateCleverHeading(transform.position, m_AttachedMotor.m_Data.m_CollisionSize, m_AttachedMotor.m_Data.m_PathableLayerMask);
            //if (Physics2D.CircleCast(transform.position, m_AttachedMotor.m_Data.m_CollisionSize / 2.0f, direction, magnitude, m_AttachedMotor.m_Data.m_CollisionLayerMask))
            //{
            //    direction = m_CalculatedPathway.CalculateHeading(transform.position);
            //}
            m_AttachedMotor.DriveMotor(direction, Time.deltaTime);
            Debug.DrawLine(transform.position, transform.position + direction * 0.5f, Color.red, 0.0f);

            Vector2 dir = new Vector2(m_Target.position.x - transform.position.x, m_Target.position.y - transform.position.y);
        }
        else
        {
            m_AttachedMotor.DriveMotor(Vector2.zero, Time.deltaTime);
            m_CalculatedPathway = new Pathway();
        }
    }
}
