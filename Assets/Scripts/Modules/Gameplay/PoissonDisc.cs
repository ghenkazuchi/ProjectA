using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDisc
{
	public static List<Vector2> GeneratePoints(float radius, Vector2 regionSize, int tryNumber = 20, System.Random sysRandom = null)
	{
		float cellSize = radius / Mathf.Sqrt(2);
		int[,] grid = new int[Mathf.CeilToInt(regionSize.x / cellSize), Mathf.CeilToInt(regionSize.y / cellSize)];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>();

		spawnPoints.Add(regionSize / 2);
		while (spawnPoints.Count > 0)
		{
			int spawnIndex = sysRandom != null ? sysRandom.Next(0, spawnPoints.Count) : UnityEngine.Random.Range(0, spawnPoints.Count);
			Vector2 spawnCenter = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < tryNumber; i++)
			{
				float randValue = sysRandom != null ? (float)sysRandom.NextDouble() : UnityEngine.Random.value;
				float angle = randValue * Mathf.PI * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				float dist = sysRandom != null ? radius + (float)sysRandom.NextDouble() * radius : UnityEngine.Random.Range(radius, 2 * radius);
				Vector2 candidate = spawnCenter + dir * dist;
				if (isValid(candidate,regionSize,cellSize,radius,points,grid))
				{
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted)
			{
				spawnPoints.RemoveAt(spawnIndex);
			}
		}
		return points;
	}
	static bool isValid(Vector2 candidate,Vector2 regionSize,float cellSize,float radius,List<Vector2> points, int[,] grid)
	{
		if(candidate.x >= 0 && candidate.x < regionSize.x && candidate.y >= 0 && candidate.y < regionSize.y)
		{
			int cellX = (int) (candidate.x / cellSize);
			int cellY = (int) (candidate.y / cellSize);
			int searchStartX = Mathf.Max(0, cellX - 2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0) - 1);
			int searchStartY = Mathf.Max(0, cellY - 2);
			int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);
			for (int x = searchStartX; x <= searchEndX; x++) 
			{
				for (int y = searchStartY; y <= searchEndY; y++) 
				{
					int pointIndex = grid[x,y] - 1;
					if(pointIndex != -1)
					{
						float sqrDistance = (candidate - points[pointIndex]).sqrMagnitude;
						if(sqrDistance < radius * radius)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}
