using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathfindingStep
{
    public List<PathfindingNode> m_PriorNodes;
    public PathfindingNode m_Node;
    public float m_CurrentCost;
    public float m_Heuristic;

    public float GetCost()
    {
        return m_CurrentCost + m_Heuristic;
    }
}

public class PathfindingGrid : MonoBehaviour
{
    public bool m_GridInitialized = false;
    public float m_GridInitializeCounter = 0.1f;

    public Vector2Int m_Extents = new Vector2Int(10, 10);
    public float m_TileSize = 1.0f;

    public LayerMask m_EnvironmentLayerMask;
    public LayerMask m_NodeLayerMask;

    public GameObject m_NodePrefab;
    public PathfindingNode[] m_Nodes;

    private void OnDrawGizmos()
    {
        Vector2[] points = new Vector2[4]
        {
            new Vector2(-0.5f, -0.5f),
            new Vector2(-0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, -0.5f)
        };

        for (int i = 0; i < 4; i++)
        {
            points[i].x = transform.position.x + points[i].x * m_Extents.x;
            points[i].y = transform.position.y + points[i].y * m_Extents.y;
        }

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[3], points[0]);
    }

    private void Awake()
    {
        GeneratePathfindingNodes(); 
    }

    public void ClearPathfindingNodes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void GeneratePathfindingNodes()
    {
        float originX = transform.position.x - (float)m_Extents.x * 0.5f + m_TileSize * 0.5f;
        float originY = transform.position.y - (float)m_Extents.y * 0.5f + m_TileSize * 0.5f; 
        
        List<PathfindingNode> nodes = new List<PathfindingNode>(m_Extents.x * m_Extents.y);
        for (int x = 0; x < m_Extents.x; x++)
        {
            for (int y = 0; y < m_Extents.y; y++)
            {
                Vector3 pos = new Vector3(originX + (float)x * m_TileSize, originY + (float)y * m_TileSize, transform.position.z);
                
                if (!Physics2D.OverlapPoint(pos, m_EnvironmentLayerMask))
                {
                    GameObject newNode = Instantiate(m_NodePrefab, transform);
                    newNode.name = m_NodePrefab.name + " [" + x.ToString() + ", " + y.ToString() + "]";
                    newNode.transform.localPosition = pos;

                    nodes.Add(newNode.GetComponent<PathfindingNode>());
                }        
            }
        }

        m_Nodes = new PathfindingNode[nodes.Count];
        for (int i = 0; i < m_Nodes.Length; i++)
        {
            m_Nodes[i] = nodes[i];
        }
        m_GridInitialized = false;
        m_GridInitializeCounter = 0.25f;
    }   

    public int GetClosestNodeIndex(Vector3 _pos)
    {
        float shortestDist = 999.9f;
        int shortestIndex = -1;
        for (int i = 0; i < m_Nodes.Length; i++)
        {
            float dist = (_pos - m_Nodes[i].transform.position).magnitude;
            if (dist < shortestDist)
            {
                shortestDist = dist;
                shortestIndex = i;
            }
        }
        return shortestIndex;
    }

    public PathfindingNode GetClosestNode(Vector3 _pos)
    {
        float shortestDist = 999.9f;
        int shortestIndex = -1;
        for (int i = 0; i < m_Nodes.Length; i++)
        {
            float dist = (_pos - m_Nodes[i].transform.position).magnitude;
            if (dist < shortestDist)
            {
                shortestDist = dist;
                shortestIndex = i;
            }
        }
        return m_Nodes[GetClosestNodeIndex(_pos)];
    }

    public PathfindingStep PathfindFromTo(Vector3 _originPos, Vector3 _endPos)
    {
        if (!m_GridInitialized)
        {
            return new PathfindingStep();
        }
        return Pathfind(GetClosestNode(_originPos), GetClosestNode(_endPos));
    }

    public PathfindingStep Pathfind(PathfindingNode _origin, PathfindingNode _target)
    {
        List<PathfindingNode> seenSteps = new List<PathfindingNode>(128);
        List<PathfindingStep> evalSteps = new List<PathfindingStep>(128);

        PathfindingStep next = new PathfindingStep();
        next.m_PriorNodes = new List<PathfindingNode>(16);
        next.m_Node = _origin;
        next.m_CurrentCost = 0;
        next.m_Heuristic = NodeHeuristic(next.m_Node, _target);

        PathfindingStep path = EvaluateOptions(seenSteps, evalSteps, next, _target);
        return path;   
    }

    private float NodeHeuristic(PathfindingNode _origin, PathfindingNode _target)
    {
        float x = _target.transform.position.x - _origin.transform.position.x;
        float y = _target.transform.position.y - _origin.transform.position.y;
        return new Vector2(x, y).magnitude;
    }

    private PathfindingStep EvaluateOptions(List<PathfindingNode> _seenNodes, List<PathfindingStep> _evalSteps, PathfindingStep _best, PathfindingNode _target)
    {
        _seenNodes.Add(_best.m_Node);
        for (int i = _evalSteps.Count - 1; i >= 0; i--)
        {
            if (_evalSteps[i].m_Node == _best.m_Node)
            {
                _evalSteps.RemoveAt(i);
            }
        }

        if (_best.m_Node == _target)
        {
            return _best;
        }

        for (int i = 0; i < _best.m_Node.m_Connections.Length; i++)
        {
            if (!_seenNodes.Contains(_best.m_Node.m_Connections[i].m_Node))
            {
                PathfindingStep next = new PathfindingStep();
                next.m_PriorNodes = new List<PathfindingNode>(_best.m_PriorNodes);//new List<PathfindingNodes>(_best.m_PriorNodes.Count);
                next.m_PriorNodes.Add(_best.m_Node);
                next.m_Node = _best.m_Node.m_Connections[i].m_Node;
                next.m_CurrentCost = _best.m_CurrentCost + _best.m_Node.m_Connections[i].m_Cost;
                next.m_Heuristic = NodeHeuristic(next.m_Node, _target);
                _evalSteps.Add(next);
            }        
        }

        if (_evalSteps.Count <= 0)
        {
            return new PathfindingStep();
        }

        _evalSteps.Sort(delegate(PathfindingStep _a, PathfindingStep _b)
        {
            if (_a.GetCost() < _b.GetCost()) { return -1; }
            if (_a.GetCost() > _b.GetCost()) { return 1; }
            else { return 0; }
        });

        PathfindingStep nextEvalNode = _evalSteps[0];
        _evalSteps.RemoveAt(0);

        return EvaluateOptions(_seenNodes, _evalSteps, nextEvalNode, _target);
    }

    public void InitializeConnections()
    {
        for (int i = 0; i < m_Nodes.Length; i++)
        {
            m_Nodes[i].InitializeNodeConnections(m_TileSize, m_NodeLayerMask, m_EnvironmentLayerMask);
        }
    }

    private void Update()
    {
        if (!m_GridInitialized)
        {
            m_GridInitializeCounter -= Time.deltaTime;
            if (m_GridInitializeCounter <= 0.0f)
            {
                InitializeConnections();
                m_GridInitialized = true;
            }      
        }
    }
}
