using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
	private class PathNode
	{
		public Vector2Int position; 
		public int gCost;         
		public int hCost;         
		public int fCost;         
		public PathNode parent;    

		public PathNode(Vector2Int pos)
		{
			position = pos;
			gCost = int.MaxValue; 
			fCost = int.MaxValue;
			parent = null;
		}

		public void CalculateFCost()
		{
			fCost = gCost + hCost;
		}
	}
	private const int MOVE_STRAIGHT_COST = 10;

	public static List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int end, Vector2Int mapSize, System.Func<Vector2Int, bool> isWalkable)
	{
		if (start.x < 0 || start.x >= mapSize.x || start.y < 0 || start.y >= mapSize.y ||
			end.x < 0 || end.x >= mapSize.x || end.y < 0 || end.y >= mapSize.y)
		{
			return null;
		}
		if (start == end)
		{
			return new List<Vector2Int> { start };
		}

		List<PathNode> openList = new List<PathNode>();
		HashSet<Vector2Int> closedListPositions = new HashSet<Vector2Int>();
		Dictionary<Vector2Int, PathNode> nodeMap = new Dictionary<Vector2Int, PathNode>();
		PathNode startNode = GetNode(nodeMap, start);
		startNode.gCost = 0;
		startNode.hCost = CalculateDistanceCost(start, end);
		startNode.CalculateFCost();
		openList.Add(startNode);

		while (openList.Count > 0)
		{
			PathNode currentNode = GetLowestFCostNode(openList);
			if (currentNode.position == end)
			{
				return ReconstructPath(currentNode);
			}
			openList.Remove(currentNode);
			closedListPositions.Add(currentNode.position);
			foreach (Vector2Int neighbourPosition in GetNeighbours(currentNode.position, mapSize,isWalkable))
			{
				if (closedListPositions.Contains(neighbourPosition))
				{
					continue;
				}
				int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode.position, neighbourPosition);

				PathNode neighbourNode = GetNode(nodeMap, neighbourPosition);

				if (tentativeGCost < neighbourNode.gCost)
				{
					neighbourNode.parent = currentNode;
					neighbourNode.gCost = tentativeGCost;
					neighbourNode.hCost = CalculateDistanceCost(neighbourPosition, end);
					neighbourNode.CalculateFCost();

					if (!openList.Contains(neighbourNode))
					{
						openList.Add(neighbourNode);
					}
				}
			}
		}
		return null;
	}
	private static PathNode GetNode(Dictionary<Vector2Int, PathNode> nodeMap, Vector2Int position)
	{
		if (!nodeMap.ContainsKey(position))
		{
			nodeMap[position] = new PathNode(position);
		}
		return nodeMap[position];
	}


	private static int CalculateDistanceCost(Vector2Int a, Vector2Int b)
	{
		int xDistance = Mathf.Abs(a.x - b.x);
		int yDistance = Mathf.Abs(a.y - b.y);
		return MOVE_STRAIGHT_COST * (xDistance + yDistance);
	}

	private static PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
	{
		PathNode lowestFCostNode = pathNodeList[0];
		for (int i = 1; i < pathNodeList.Count; i++)
		{
			if (pathNodeList[i].fCost < lowestFCostNode.fCost)
			{
				lowestFCostNode = pathNodeList[i];
			}
			else if (pathNodeList[i].fCost == lowestFCostNode.fCost && pathNodeList[i].hCost < lowestFCostNode.hCost)
			{
				lowestFCostNode = pathNodeList[i];
			}
		}
		return lowestFCostNode;
	}

	private static List<Vector2Int> GetNeighbours(Vector2Int currentPosition, Vector2Int mapSize, System.Func<Vector2Int, bool> isWalkable)
	{
		List<Vector2Int> neighbours = new List<Vector2Int>();

		int x = currentPosition.x;
		int y = currentPosition.y;

		Vector2Int[] directions = {
		new Vector2Int(0, 1),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0),
		new Vector2Int(1, 0)
	};

		foreach (var dir in directions)
		{
			Vector2Int neighbour = currentPosition + dir;
			if (neighbour.x >= 0 && neighbour.x < mapSize.x && neighbour.y >= 0 && neighbour.y < mapSize.y)
			{
				if (isWalkable(neighbour))
				{
					neighbours.Add(neighbour);
				}
			}
		}

		return neighbours;
	}

	private static List<Vector2Int> ReconstructPath(PathNode endNode)
	{
		List<Vector2Int> path = new List<Vector2Int>();
		PathNode currentNode = endNode;
		while (currentNode != null)
		{
			path.Add(currentNode.position);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		return path;
	}
	public static List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int end, Vector2Int mapSize)
	{
		return FindPathAStar(start, end, mapSize, pos => true);
	}

}