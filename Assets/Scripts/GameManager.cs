using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;

    [SerializeField]
    private GameObject startPointToken;
    [SerializeField] 
    private GameObject endPointToken;
    [SerializeField]
    private GameObject openListToken;
    [SerializeField]
    private GameObject closeListToken;
    //private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;

    private Node nodeStart;
    private GameObject nodeStartObject;
    private Node nodeEnd;
    private GameObject nodeEndObject;
    private List<GameObject> pathList = new();
    private List<GameObject> openList = new();
    private List<GameObject> closeList = new();

    private int startPosx, startPosy;
    private int endPosx, endPosy;
    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while (endPosx == startPosx || endPosy == startPosy);
        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();
        StartNodes();
    }
    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i,j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
        //DebugMatrix();
    }
    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                //Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);
                Debug.Log("Ways: ");
                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }
            }
        }
    }
    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public void StartNodes()
    {
        nodeEnd = NodeMatrix[endPosx, endPosy];
        nodeStart = NodeMatrix[startPosx, startPosy];

        nodeStartObject = Instantiate(startPointToken, nodeStart.RealPosition, Quaternion.identity);
        nodeEndObject = Instantiate(endPointToken, nodeEnd.RealPosition, Quaternion.identity);
    }
    public void RestartGame()
    {
        ClearPath();
        do
        {
            startPosx = Random.Range(0, Size);
            startPosy = Random.Range(0, Size);
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while (startPosx == endPosx || startPosy == endPosy);

        nodeStart = NodeMatrix[startPosx, startPosy];
        nodeEnd = NodeMatrix[endPosx, endPosy];

        if (nodeStartObject == null)
            nodeStartObject = Instantiate(startPointToken, nodeStart.RealPosition, Quaternion.identity);
        else
            nodeStartObject.transform.position = nodeStart.RealPosition;

        if (nodeEndObject == null)
            nodeEndObject = Instantiate(endPointToken, nodeEnd.RealPosition, Quaternion.identity);
        else
            nodeEndObject.transform.position = nodeEnd.RealPosition;
    }

    public void ClearPath()
    {
        StopAllCoroutines();

        foreach (var node in NodeMatrix)
        {
            node.NodeParent = null;
        }
        foreach (var node in openList)
        {
            Destroy(node);
        }
        foreach (var node in closeList)
        {
            Destroy(node);
        }
        foreach (var node in pathList)
        { 
            Destroy(node);
        }

        openList.Clear();
        closeList.Clear();
        pathList.Clear();
    }

    public void OnClickGreedySearch()
    {
        ClearPath();
        StartCoroutine(GreedySearch());
    }
    private IEnumerator GreedySearch()
    {
        List<Node> open = new();
        HashSet<Node> close = new();
      
        open.Add(nodeStart);
        while (open.Count > 0)
        {
            //open.Sort((a, b) => a.Heuristic.CompareTo(b.Heuristic));
            open = open.OrderBy(a => a.Heuristic).ToList();
            Node currentNode = open.First();
            open.RemoveAt(0);
            if (currentNode == nodeEnd)
            {
                StartCoroutine(ShowPath(currentNode));
                open.Clear();
            }
            else
            {
                close.Add(currentNode);
                closeList.Add(Instantiate(closeListToken, currentNode.RealPosition, Quaternion.identity));
                foreach (var node in currentNode.WayList)
                {
                    Node neighboorNode = node.NodeDestiny;
                    if (close.Contains(neighboorNode))
                        continue;
                    if (!open.Contains(neighboorNode))
                    {
                        neighboorNode.NodeParent = currentNode;
                        open.Add(neighboorNode);
                        openList.Add(Instantiate(openListToken, neighboorNode.RealPosition, Quaternion.identity));
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }
        }
    }

    public void OnClickAStar()
    {
        ClearPath();
        StartCoroutine(AStar());
    }
    private IEnumerator AStar()
    {
        List<Node> open = new();
        List<Node> close = new();

        open.Add(nodeStart);
        while (open.Count > 0)
        {
            //open.Sort((a, b) => GetFCost(a, a.NodeParent).CompareTo(GetFCost(b, b.NodeParent)));
            open = open.OrderBy(n => GetFCost(n, n.NodeParent)).ToList();
            Node currentNode = open.First();
            open.RemoveAt(0);

            if (currentNode == nodeEnd)
            {
                StartCoroutine(ShowPath(currentNode));
                open.Clear();
            }
            else
            {
                close.Add(currentNode);
                closeList.Add(Instantiate(closeListToken, currentNode.RealPosition, Quaternion.identity));
                foreach (var way in currentNode.WayList)
                {
                    Node neighbor = way.NodeDestiny;
                    if (close.Contains(neighbor))
                        continue;
                    float newGCost = GetGCost(way);

                    if (!open.Contains(neighbor) || newGCost < way.ACUMulatedCost)
                    {
                        neighbor.NodeParent = currentNode;
                        way.ACUMulatedCost = newGCost;
                        open.Add(neighbor);
                        openList.Add(Instantiate(openListToken, neighbor.RealPosition, Quaternion.identity));
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }
        }
    }

    public void OnClickDFS()
    {
        ClearPath();
        StartCoroutine(DFS());
    }
    private IEnumerator DFS()
    {
        Stack<Node> open = new();
        HashSet<Node> close = new();

        open.Push(nodeStart);
        while (open.Count > 0)
        {
            Node currentNode = open.Pop();
            if (currentNode == nodeEnd)
            {
                StartCoroutine(ShowPath(currentNode));
                open.Clear();
            }
            else
            {
                close.Add(currentNode);
                closeList.Add(Instantiate(closeListToken, currentNode.RealPosition, Quaternion.identity));
                foreach (var node in currentNode.WayList)
                {
                    Node neighbor = node.NodeDestiny;
                    if (close.Contains (neighbor))
                        continue;
                    if (!open.Contains (neighbor))
                    {
                        neighbor.NodeParent = currentNode;
                        open.Push (neighbor);
                        openList.Add(Instantiate(openListToken, neighbor.RealPosition, Quaternion.identity) );
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }
        }
    }
    private float GetGCost(Way way)
    {
        return way.ACUMulatedCost + way.Cost;
    }

    private float GetFCost(Node node, Node nodeParent)
    {
        if (nodeParent == null)
            return 0;
        foreach (var way in nodeParent.WayList)
        {
            if (way.NodeDestiny == node)
                return way.Cost + way.ACUMulatedCost + node.Heuristic;
        }
        return float.MaxValue;
    }


    private IEnumerator ShowPath(Node endNode)
    {
        List<Node> path = new();
        Node current = endNode;
        int i = 0;
        while (current != null)
        {
            path.Add(current);
            current = current.NodeParent;

        }
        path.Reverse();
        while (i < path.Count)
        {
            pathList.Add(Instantiate(token, path[i].RealPosition, Quaternion.identity));
            i++;
            yield return new WaitForSeconds(0.15f);
        }
    }
}
