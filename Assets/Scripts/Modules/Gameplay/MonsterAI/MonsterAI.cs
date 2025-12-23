using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
	[Header("AI Settings")]
	public float visionRange = 4f; 
	public float moveSpeed = 1f;
	public float chaseSpeed = 2f;
	public float patrolSpeed = 1f;
	public float stopDistance = 0.1f;

	[Header("Patrol Settings")]
	public bool enablePatrol = true;
	public float patrolWaitTime = 2f;
	public int maxPatrolDistance = 3;

	private Transform player;
	private MonsterInteracableObject monsterInteractable;
	private VoronoiPathGenerator pathGenerator;
	private Vector2Int currentGridPosition;
	private Vector2Int targetGridPosition;
	private Vector3 targetWorldPosition;
	private List<Vector2Int> currentPath;
	private int currentPathIndex;

	// States
	private MonsterState currentState = MonsterState.Patrol;
	private Vector2Int patrolStartPosition;
	private Vector2Int patrolTargetPosition;
	private float patrolWaitTimer;
	private bool isMoving = false;

	private enum MonsterState
	{
		Patrol,
		Chasing,
		Attacking,
		Returning
	}

	void Start()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		if (playerObject != null)
		{
			player = playerObject.transform;
			Debug.Log($"{gameObject.name} found player: {player.name}");
		}
		else
		{
			Debug.LogWarning($"{gameObject.name} could not find Player! Make sure player has 'Player' tag.");
		}

		monsterInteractable = GetComponent<MonsterInteracableObject>();
		pathGenerator = FindObjectOfType<VoronoiPathGenerator>();

		if (pathGenerator == null)
		{
			Debug.LogError($"{gameObject.name} could not find VoronoiPathGenerator!");
			return;
		}
		StartCoroutine(WaitForMapGeneration());
	}

	IEnumerator WaitForMapGeneration()
	{
		while (pathGenerator != null && !pathGenerator.IsMapGenerated())
		{
			yield return new WaitForSeconds(0.1f);
		}

		InitializePosition();

		StartCoroutine(AIUpdateLoop());

		Debug.Log($"{gameObject.name} AI initialized at position: {currentGridPosition}");
	}

	void InitializePosition()
	{
		if (pathGenerator != null)
		{
			Vector3 worldPos = transform.position;
			currentGridPosition = new Vector2Int(
				Mathf.RoundToInt(worldPos.x - 0.5f),
				Mathf.RoundToInt(worldPos.y - 0.5f)
			);
			if (!pathGenerator.IsPathTile(currentGridPosition))
			{
				currentGridPosition = pathGenerator.GetRandomPathTile();
				transform.position = pathGenerator.GetWorldPosition(currentGridPosition);
			}

			patrolStartPosition = currentGridPosition;
			targetGridPosition = currentGridPosition;
		}
	}

	IEnumerator AIUpdateLoop()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.1f); 

			if (player == null || pathGenerator == null || !pathGenerator.IsMapGenerated())
				continue;

			UpdateAI();
		}
	}

	void UpdateAI()
	{
		if(GameController.Instance.currentState != GameState.FreeRoam)
		{
			isMoving = false;
			return;
		}

		Vector2Int playerGridPos = GetPlayerGridPosition();
		float distanceToPlayer = Vector2Int.Distance(currentGridPosition, playerGridPos);

		switch (currentState)
		{
			case MonsterState.Patrol:
				HandlePatrolState(playerGridPos, distanceToPlayer);
				break;

			case MonsterState.Chasing:
				HandleChasingState(playerGridPos, distanceToPlayer);
				break;

			case MonsterState.Attacking:
				HandleAttackingState(playerGridPos, distanceToPlayer);
				break;

			case MonsterState.Returning:
				HandleReturningState(playerGridPos, distanceToPlayer);
				break;
		}

		HandleMovement();
	}

	void HandlePatrolState(Vector2Int playerGridPos, float distanceToPlayer)
	{
		if (player != null)
		{
			Debug.DrawLine(transform.position, player.position, Color.blue, 0.1f);
		}
		if (distanceToPlayer <= visionRange && CanSeePlayer(playerGridPos))
		{
			currentState = MonsterState.Chasing;
			Debug.Log($"{gameObject.name} spotted player at distance {distanceToPlayer}! Starting chase.");
			return;
		}

		if (enablePatrol)
		{
			if (!isMoving)
			{
				if (patrolWaitTimer <= 0)
				{
					SetNewPatrolTarget();
					patrolWaitTimer = patrolWaitTime;
				}
				else
				{
					patrolWaitTimer -= 0.1f;
				}
			}
		}
	}

	void HandleChasingState(Vector2Int playerGridPos, float distanceToPlayer)
	{
		Debug.Log($"{gameObject.name} chasing player. Distance: {distanceToPlayer}");

		if (distanceToPlayer > visionRange * 2f)
		{
			currentState = MonsterState.Returning;
			Debug.Log($"{gameObject.name} lost sight of player. Returning to patrol.");
			return;
		}
		if (distanceToPlayer <= 1.2f)
		{
			currentState = MonsterState.Attacking;
			Debug.Log($"{gameObject.name} attacking player!");
			return;
		}
		SetTargetPosition(playerGridPos, chaseSpeed);
	}

	void HandleAttackingState(Vector2Int playerGridPos, float distanceToPlayer)
	{
	}

	void HandleReturningState(Vector2Int playerGridPos, float distanceToPlayer)
	{
		if (distanceToPlayer <= visionRange && CanSeePlayer(playerGridPos))
		{
			currentState = MonsterState.Chasing;
			return;
		}
		if (Vector2Int.Distance(currentGridPosition, patrolStartPosition) <= 1f)
		{
			currentState = MonsterState.Patrol;
			patrolWaitTimer = patrolWaitTime;
		}
		else if (!isMoving)
		{
			SetTargetPosition(patrolStartPosition, moveSpeed);
		}
	}

	bool CanSeePlayer(Vector2Int playerGridPos)
	{
		return pathGenerator.IsPathTile(playerGridPos);
	}

	void SetNewPatrolTarget()
	{
		List<Vector2Int> nearbyTiles = GetNearbyPathTiles(currentGridPosition, maxPatrolDistance);
		if (nearbyTiles.Count > 0)
		{
			patrolTargetPosition = nearbyTiles[Random.Range(0, nearbyTiles.Count)];
			SetTargetPosition(patrolTargetPosition, patrolSpeed);
		}
	}

	void SetTargetPosition(Vector2Int gridTarget, float speed)
	{
		if (pathGenerator == null) return;

		targetGridPosition = gridTarget;

		if (!pathGenerator.IsPathTile(gridTarget))
		{
			return;
		}

		currentPath = Pathfinding.FindPathAStar(currentGridPosition, targetGridPosition,
			new Vector2Int(Mathf.CeilToInt(pathGenerator.mapRegionSize.x),
						  Mathf.CeilToInt(pathGenerator.mapRegionSize.y)),pathGenerator.IsPathTile);

		if (currentPath != null && currentPath.Count > 1)
		{
			currentPathIndex = 1; 
			isMoving = true;
			moveSpeed = speed;
		}
	}

	void HandleMovement()
	{
		if (!isMoving || currentPath == null || currentPathIndex >= currentPath.Count)
		{
			isMoving = false;
			return;
		}

		Vector2Int nextGridPos = currentPath[currentPathIndex];
		Vector3 nextWorldPos = pathGenerator.GetWorldPosition(nextGridPos);

		float currentMoveSpeed = moveSpeed;
		if (currentState == MonsterState.Chasing)
			currentMoveSpeed = chaseSpeed;
		else if (currentState == MonsterState.Patrol)
			currentMoveSpeed = patrolSpeed;

		transform.position = Vector3.MoveTowards(transform.position, nextWorldPos, currentMoveSpeed * Time.deltaTime);

		if (Vector3.Distance(transform.position, nextWorldPos) < stopDistance)
		{
			currentGridPosition = nextGridPos;
			currentPathIndex++;

			if (currentPathIndex >= currentPath.Count)
			{
				isMoving = false;
				currentPath = null;
			}
		}
	}

	Vector2Int GetPlayerGridPosition()
	{
		if (player == null) return Vector2Int.zero;

		Vector3 worldPos = player.position;
		return new Vector2Int(
			Mathf.RoundToInt(worldPos.x - 0.5f),
			Mathf.RoundToInt(worldPos.y - 0.5f)
		);
	}

	List<Vector2Int> GetNearbyPathTiles(Vector2Int center, int maxDistance)
	{
		List<Vector2Int> nearbyTiles = new List<Vector2Int>();

		for (int x = -maxDistance; x <= maxDistance; x++)
		{
			for (int y = -maxDistance; y <= maxDistance; y++)
			{
				if (x == 0 && y == 0) continue;

				Vector2Int checkPos = center + new Vector2Int(x, y);
				if (pathGenerator.IsPathTile(checkPos))
				{
					nearbyTiles.Add(checkPos);
				}
			}
		}

		return nearbyTiles;
	}
	private void MoveSmoothlyToTarget()
	{
		transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
		if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
		{
			transform.position = targetWorldPosition;
			isMoving = false;
		}
	}
}