using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nodemove : MonoBehaviour
{

   public Transform[] lanePositions;

    private int currentLane = 0;
    public float moveSpeed = 5f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveNode(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            MoveNode(1);
        }
    }

    private void MoveNode(int direction)
{
    int nextLane = Mathf.Clamp(currentLane + direction, 0, lanePositions.Length - 1);

    transform.position = lanePositions[nextLane].position;
    currentLane = nextLane;
}
}
