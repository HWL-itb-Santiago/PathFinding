using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    #region variables
    private int _positionX, _positionY;
    private float _heuristic;
    private Vector2 _realPosition;
    private Node _nodeParent;
    private List<Way> _wayList;
    #endregion

    #region getters and setters
    public int PositionX { get => _positionX; set { _positionX = value; } }
    public int PositionY { get => _positionY; set { _positionY = value; } }
    public float Heuristic { get => _heuristic; set { _heuristic = value; } }
    public Vector2 RealPosition { get => _realPosition; set { _realPosition = value; } }
    public Node NodeParent { get { return _nodeParent; } set => _nodeParent = value; }
    public List<Way> WayList { get {  return _wayList; } set { _wayList = value; } }

    #endregion
    public Node(int positionX, int positionY, Vector2 realPos)
    {
        _positionX = positionX;
        _positionY = positionY;
        _realPosition = realPos;
    }
}
