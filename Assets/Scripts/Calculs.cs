using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Calculs
{
    public static float LinearDistance;
    public static float DiagonalDistance;
    public static Vector2 FirstPosition;
    public static void CalculateDistances(BoxCollider2D coll, float Size)
    {
        LinearDistance = coll.size.x / Size;
        FirstPosition = new Vector2(-Size / 4f + LinearDistance / 2f - 0.1f,
            Size / 4f - LinearDistance / 2f + 0.1f);
    }
    public static Vector2 CalculatePoint(int x, int y)
    {
        return FirstPosition + new Vector2(x * LinearDistance, -y* LinearDistance);
    }
    public static float CalculateHeuristic(Node node, int finalx, int finaly)
    {
        return Vector2.Distance(CalculatePoint(node.PositionX, node.PositionY),
            CalculatePoint(finalx, finaly));
    }
}
