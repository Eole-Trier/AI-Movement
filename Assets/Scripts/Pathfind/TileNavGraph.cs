using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.UIElements;

namespace Navigation
{
    public class TileNavGraph : MonoBehaviour
    {
        // simple Singleton pattern
	    static TileNavGraph instance = null;
	    static public TileNavGraph Instance
	    {
		    get
		    {
			    if (instance == null)
				    instance = FindObjectOfType<TileNavGraph>();
			    return instance;
		    }
	    }

        [SerializeField]
        private int GrassCost = 1;
        [SerializeField]
        private int UnreachableCost = int.MaxValue;

        [SerializeField]
        private int GridSizeH = 100;
        [SerializeField]
        private int GridSizeV = 100;
        [SerializeField]
        private int SquareSize = 1;
        [SerializeField]
        private int MaxHeight = 10;
        [SerializeField]
        private int MaxWalkableHeight = 5;

        // enable / disable debug Gizmos
        [SerializeField]
        private bool DrawGrid = false;
        [SerializeField]
        private bool DisplayAllNodes = false;
        [SerializeField]
        private bool DisplayAllLinks = false;

        [SerializeField]
        private bool RecomputeGraph = false;

        private Vector3 gridStartPos = Vector3.zero;
        private int NbTilesH = 0;
        private int NbTilesV = 0;
        private List<Node> LNode = new List<Node>();
        private Dictionary<Node, List<Connection>> ConnectionsGraph = new Dictionary<Node, List<Connection>>();
        public Dictionary<Node, List<Connection>> GetConnectionsGraph => ConnectionsGraph;

        // threading
        Thread GraphThread = null;

        // own part

        [SerializeField]
        private GameObject StartPosition;

        private Node StartNode;
        private Node GoalNode;

        private Unit unit;

#region Monobehavior
        private void Awake ()
        {
            CreateTiledGrid();
	    }

        private void Start()
        {
            RecomputeGraph = false;
            StartCreatingGraph();
            StartNode = GetNode(StartPosition.transform.position);
        }

        private void Update()
        {
            GoalNode = GetNode(FindObjectOfType<Unit>().Movement.TargetPos);
            if (RecomputeGraph)
            {
                ClearGridAndGraph();
                CreateTiledGrid();
                StartCreatingGraph();
                AStar();
                RecomputeGraph = false;
            }
        }
#endregion

        private void StartCreatingGraph()
        {
            ThreadStart threadStart = new ThreadStart(CreateGraph);
            GraphThread = new Thread(threadStart);
            GraphThread.Start();
        }

        private void AStar()
        {
            foreach (Node node in LNode)
            {
                node.IsVisited = false;
                node.GlobalGoal = Mathf.Infinity;
                node.LocalGoal = Mathf.Infinity;
                node.Parent = null;
            }

            Node currentNode = StartNode;
            StartNode.LocalGoal = 0f;
            StartNode.GlobalGoal = Heuristic(StartNode, GoalNode);

            List<Node> notTestedNodes = new List<Node>();
            notTestedNodes.Add(StartNode);

            while (notTestedNodes.Count != 0)
            {
                notTestedNodes.Sort((x, y) => x.GlobalGoal < y.GlobalGoal ? -1 : 1);
                while (notTestedNodes.Count != 0 && notTestedNodes.First().IsVisited)
                {
                    notTestedNodes.Remove(notTestedNodes.First());
                }
                if (notTestedNodes.Count == 0)
                    break;
                 
                currentNode = notTestedNodes.First();
                currentNode.IsVisited = true;

                foreach (Node neighbour in GetNeighbours(currentNode))
                {
                    if (!neighbour.IsVisited && !neighbour.IsObstacle)
                        notTestedNodes.Add((neighbour));

                    float possiblyLowerGoal = currentNode.GlobalGoal + (neighbour.Position - currentNode.Position).magnitude;
                    if (possiblyLowerGoal < neighbour.LocalGoal)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.LocalGoal = possiblyLowerGoal;
                        neighbour.GlobalGoal = neighbour.LocalGoal + Heuristic(neighbour, GoalNode);
                    }
                }
            }
        }

    // Create all nodes for the tiled grid
    private void CreateTiledGrid()
	    {
		    LNode.Clear();

            gridStartPos = transform.position + new Vector3(- GridSizeH / 2f, 0f, - GridSizeV / 2f);

            NbTilesH = GridSizeH / SquareSize;
		    NbTilesV = GridSizeV / SquareSize;

		    for(int i = 0; i < NbTilesV; i++)
		    {
			    for(int j = 0; j < NbTilesH; j++)
			    {
				    Node node = new Node();
                    Vector3 nodePos = gridStartPos + new Vector3((j + 0.5f) * SquareSize, 0f, (i + 0.5f) * SquareSize);

				    int Weight = 0;
				    RaycastHit hitInfo = new RaycastHit();

                    // always compute node Y pos from floor collision
                    if (Physics.Raycast(nodePos + Vector3.up * MaxHeight, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer("Floor")))
                    {
                        if (Weight == 0)
                            Weight = hitInfo.point.y >= MaxWalkableHeight ? UnreachableCost : GrassCost;
                        nodePos.y = hitInfo.point.y;
                    }

                    node.Weight = Weight;
				    node.Position = nodePos;
				    LNode.Add(node);
			    }
		    }
        }

        // cast a ray for each possible corner of a tile node for better accuracy
        private bool RaycastNode(Vector3 nodePos, string layerName, out RaycastHit hitInfo)
        {
            if (Physics.Raycast(nodePos - new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos - new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            return false;
        }

        private void ClearGridAndGraph()
        {
            LNode.Clear();
            ConnectionsGraph.Clear();
        }

        // Compute possible connections between each nodes
        private void CreateGraph()
        {
            foreach (Node node in LNode)
            {
                if (IsNodeWalkable(node))
                {
                    ConnectionsGraph.Add(node, new List<Connection>());
                    foreach (Node neighbour in GetNeighbours(node))
                    {
                        Connection connection = new Connection();
                        connection.Cost = ComputeConnectionCost(node, neighbour);
                        connection.FromNode = node;
                        connection.ToNode = neighbour;
                        ConnectionsGraph[node].Add(connection);
                    }
                }
                else
                    node.IsObstacle = true;
            }
        }

        private int ComputeConnectionCost(Node fromNode, Node toNode)
        {
            return fromNode.Weight + toNode.Weight;
        }

        public bool IsPosValid(Vector3 pos)
        {
            if (GraphThread.ThreadState == ThreadState.Running)
                return false;

            if (pos.x > (-GridSizeH / 2) && pos.x < (GridSizeH / 2) && pos.z > (-GridSizeV / 2) && pos.z < (GridSizeV / 2))
                return true;
            return false;
        }

        public Node GetNode(Vector3 pos)
        {
            return GetNode(GetTileCoordFromPos(pos));
        }

        // converts world 3d pos to tile 2d pos
        private Vector2Int GetTileCoordFromPos(Vector3 pos)
	    {
            Vector3 realPos = pos - gridStartPos;
            Vector2Int tileCoords = Vector2Int.zero;
            tileCoords.x = Mathf.FloorToInt(realPos.x / SquareSize);
            tileCoords.y = Mathf.FloorToInt(realPos.z / SquareSize);
		    return tileCoords;
	    }

        private Node GetNode(Vector2Int pos)
        {
            return GetNode(pos.x, pos.y);
        }

        private Node GetNode(int x, int y)
        {
            int index = y * NbTilesH + x;
            if (index >= LNode.Count || index < 0)
                return null;

            return LNode[index];
        }

        private bool IsNodeWalkable(Node node)
        {
            return node.Weight < UnreachableCost;
        }

        private void TryToAddNode(List<Node> list, Node node)
        {
            if (IsNodeWalkable(node))
            {
                list.Add(node);
            }
        }

        private List<Node> GetNeighbours(Node node)
	    {
            Vector2Int tileCoord = GetTileCoordFromPos(node.Position);
            int x = tileCoord.x;
            int y = tileCoord.y;

		    List<Node> nodes = new List<Node>();

		    if (x > 0)
		    {
			    if (y > 0)
                    TryToAddNode(nodes, GetNode(x - 1, y - 1));
                TryToAddNode(nodes, LNode[(x - 1) + y * NbTilesH]);
			    if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x - 1) + (y + 1) * NbTilesH]);
		    }

		    if (y > 0)
                TryToAddNode(nodes, LNode[x + (y - 1) * NbTilesH]);
		    if (y < NbTilesV - 1)
                TryToAddNode(nodes, LNode[x + (y + 1) * NbTilesH]);

		    if (x < NbTilesH - 1)
		    {
			    if (y > 0)
                    TryToAddNode(nodes, LNode[(x + 1) + (y - 1) * NbTilesH]);
                TryToAddNode(nodes, LNode[(x + 1) + y * NbTilesH]);
			    if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x + 1) + (y + 1) * NbTilesH]);
		    }

		    return nodes;
	    }

        private float Heuristic(Node n1, Node n2)
        {
            return (n2.Position - n1.Position).magnitude;
        }

#region Gizmos
        private void OnDrawGizmos()
	    {
            if (DrawGrid)
            {
                float gridHeight = 0.01f;
                Gizmos.color = Color.yellow;
                for (int i = 0; i < NbTilesV + 1; i++)
                {
                    Vector3 startPos = new Vector3(-GridSizeH / 2f, gridHeight, -GridSizeV / 2f + i * SquareSize);
                    Gizmos.DrawLine(startPos, startPos + Vector3.right * GridSizeV);

                    for (int j = 0; j < NbTilesH + 1; j++)
                    {
                        startPos = new Vector3(-GridSizeH / 2f + j * SquareSize, gridHeight, -GridSizeV / 2f);
                        Gizmos.DrawLine(startPos, startPos + Vector3.forward * GridSizeV);
                    }
                }
            }

            if (DisplayAllNodes)
            {
		        for(int i = 0; i < LNode.Count; i++)
		        {
                    Node node = LNode[i];
                    Gizmos.color = IsNodeWalkable(node) ? Color.green : Color.red;
                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
		        }
                Gizmos.color = Color.black;
                Gizmos.DrawCube(StartNode.Position, Vector3.one * 0.25f);
                if (GoalNode != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(GoalNode.Position, Vector3.one * 0.25f);

                    Node node = GoalNode;
                    while (node.Parent != null)
                    {
                        Gizmos.color = Color.black;
                        Debug.Log(node.Position);
                        Debug.Log(node.Parent.Position);
                        Gizmos.DrawLine(node.Position, node.Parent.Position);

                        node = node.Parent;
                    }
                }
                
            }
            if (DisplayAllLinks)
            {
                foreach (Node crtNode in LNode)
                {
                    if (ConnectionsGraph.ContainsKey(crtNode))
                    {
                        foreach (Connection c in ConnectionsGraph[crtNode])
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(c.FromNode.Position, c.ToNode.Position);
                        }
                    }
                }
            }
	    }
#endregion
    }
}


