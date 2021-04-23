using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode : MonoBehaviour
{
    [System.Serializable]
    public struct NodeConnection
    {
        public PathfindingNode m_Node;
        public float m_Cost;
    }

    public NodeConnection[] m_Connections;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            Vector3 direction = m_Connections[i].m_Node.transform.position - transform.position;
            Gizmos.DrawLine(transform.position, transform.position + direction * 0.25f);
        }
    }

    public void InitializeNodeConnections(float _tileSize, LayerMask _nodeLayerMask, LayerMask _collisionLayerMask)
    {
        Vector2[] offsets = new Vector2[]
        {
            new Vector2(-1.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, -1.0f),
            new Vector3(-1.0f, -1.0f),
            new Vector3(1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f),
            new Vector3(1.0f, -1.0f)
        };
        Vector2[] endNodes = new Vector2[offsets.Length];

        for (int i = 0; i < offsets.Length; i++)
        {
            endNodes[i] = transform.position;
            endNodes[i].x = transform.position.x + offsets[i].x * _tileSize;
            endNodes[i].y = transform.position.y + offsets[i].y * _tileSize;
        }

        List<NodeConnection> connections = new List<NodeConnection>(4);
        for (int i = 0; i < offsets.Length; i++)
        {
            Collider2D cacheHit = Physics2D.OverlapCircle(endNodes[i], 0.1f, _nodeLayerMask);
            //Debug.DrawLine(transform.position, endNodes[i], Color.red, 2.0f);
            //Debug.Log(cacheHit);
            if (cacheHit && !Physics2D.CircleCast(transform.position, 0.1f, offsets[i].normalized, offsets[i].magnitude, _collisionLayerMask))
            {
                NodeConnection newConnection = new NodeConnection();
                newConnection.m_Node = cacheHit.GetComponent<PathfindingNode>();
                newConnection.m_Cost = offsets[i].magnitude;
                //Debug.Log("hit");
                connections.Add(newConnection);
            }
        }
        
        m_Connections = connections.ToArray();
    }
}
