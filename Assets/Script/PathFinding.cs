using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    PathRequestManager requestManager;
    Grid grid;

    void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos, List<Vector3> waypoints) {
        StartCoroutine(FindPath(startPos, targetPos, waypoints));
    }

	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, List<Vector3> waypoints) {
		Vector3[] path = new Vector3[0];
		bool pathSuccess = false;
		
		Vector3 currentWaypoint = startPos;
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		if (startNode.walkable && targetNode.walkable) {
			List<Vector3> totalWaypoints = new List<Vector3>();
			totalWaypoints.Add(startPos);
			totalWaypoints.AddRange(waypoints);  
			totalWaypoints.Add(targetPos);  

			for (int i = 0; i < totalWaypoints.Count - 1; i++) {
				Node startWaypointNode = grid.NodeFromWorldPoint(currentWaypoint);
				Node targetWaypointNode = grid.NodeFromWorldPoint(totalWaypoints[i + 1]);

				if (startWaypointNode.walkable && targetWaypointNode.walkable) {
					Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
					HashSet<Node> closedSet = new HashSet<Node>();
					openSet.Add(startWaypointNode);

					while (openSet.Count > 0) {
						Node currentNode = openSet.RemoveFirst();
						closedSet.Add(currentNode);

						if (currentNode == targetWaypointNode) {
							pathSuccess = true;
							break;
						}

						foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
							if (!neighbour.walkable || closedSet.Contains(neighbour)) {
								continue;
							}

							int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
							if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
								neighbour.gCost = newMovementCostToNeighbour;
								neighbour.hCost = GetDistance(neighbour, targetWaypointNode);
								neighbour.Parent = currentNode;

								if (!openSet.Contains(neighbour))
									openSet.Add(neighbour);
							}
						}
					}

					if (pathSuccess) {
						Vector3[] partialPath = RetracePath(startWaypointNode, targetWaypointNode);
						path = ConcatenatePaths(path, partialPath);
					} else {
						break;
					}
					currentWaypoint = totalWaypoints[i + 1];
				} else {
					continue;
				}
			}
		}

		yield return null;

		requestManager.FinishedProcessingPath(path, pathSuccess);
	}
	Vector3[] ConcatenatePaths(Vector3[] path1, Vector3[] path2) {
		List<Vector3> concatenatedPath = new List<Vector3>();
		concatenatedPath.AddRange(path1);
		for (int i = 0; i < path2.Length; i++) {
			concatenatedPath.Add(path2[i]);
		}    
		return concatenatedPath.ToArray();
	}
    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        System.Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld) {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
