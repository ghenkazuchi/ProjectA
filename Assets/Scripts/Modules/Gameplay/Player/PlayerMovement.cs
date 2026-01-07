using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;
	public float moveSpeed = 5f;
	public bool useSmoothMovement = true;
	private Vector2Int _currentGridPosition;
	private Vector3 _targetWorldPosition;
	private bool _isMoving = false;

	private VoronoiPathGenerator _pathGenerator;
	private Tilemap _pathTilemapReference;
	[SerializeField] private DayNightCycleController dayNight;
	IEnumerator Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		_pathGenerator = FindObjectOfType<VoronoiPathGenerator>();
		while (!_pathGenerator.IsMapGenerated())
		{
			yield return null;
		}
		_pathTilemapReference = _pathGenerator.pathTilemap;
		if (_pathTilemapReference == null)
		{
			Debug.LogError("PathTilemap reference not found on VoronoiPathGenerator!");
			yield break;
		}
		AdjustSpriteToTileSize();
		// Spawn
		Vector2Int spawnGridPosition = _pathGenerator.GetPlayerSpawnPosition();
		_currentGridPosition = spawnGridPosition;
		transform.position = _pathGenerator.GetWorldPosition(spawnGridPosition);
		_targetWorldPosition = transform.position;

		Debug.Log($"Player spawned at grid: {spawnGridPosition}, world: {transform.position}");
	}

	void AdjustSpriteToTileSize()
	{

		float spriteOriginalWidth = spriteRenderer.sprite.bounds.size.x;
		float spriteOriginalHeight = spriteRenderer.sprite.bounds.size.y;

		if (spriteOriginalWidth == 0 || spriteOriginalHeight == 0)
		{
			return;
		}

		Vector3 cellSize = _pathTilemapReference.cellSize;
		float targetWidth = cellSize.x;
		float targetHeight = cellSize.y;
		float scaleX = targetWidth / spriteOriginalWidth;
		float scaleY = targetHeight / spriteOriginalHeight;
		transform.localScale = new Vector3(scaleX, scaleY, 1f);
	}

	void Update()
	{
		if (_pathGenerator == null || !_pathGenerator.IsMapGenerated())
		{
			return;
		}

		if (!_isMoving && GameController.Instance.currentState == GameState.FreeRoam)
		{
			HandleMovementInput();
			HandleUltilityInput();
		}
		if (useSmoothMovement && _isMoving)
		{
			MoveSmoothlyToTarget();
		}
	}

	private void HandleUltilityInput()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnPartyMenuOpen));	
		}
	}
	private void HandleMovementInput()
	{
		Vector2Int inputDirection = Vector2Int.zero;

		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) inputDirection.y = 1;
		else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) inputDirection.y = -1;
		else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) inputDirection.x = -1;
		else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) inputDirection.x = 1;

		if (inputDirection != Vector2Int.zero)
		{
			Vector2Int targetGridPosition = _currentGridPosition + inputDirection;
			TryMoveTo(targetGridPosition);
		}
	}

	private void TryMoveTo(Vector2Int targetGridPosition)
	{
		if (_pathGenerator.IsPathTile(targetGridPosition))
		{
			_currentGridPosition = targetGridPosition;
			_targetWorldPosition = _pathGenerator.GetWorldPosition(_currentGridPosition);
			if (dayNight != null) dayNight.AdvanceSteps(1);
			if (useSmoothMovement)
			{
				_isMoving = true;
			}
			else
			{
				transform.position = _targetWorldPosition;
				_isMoving = false;
			}
		}
		else
		{
		}
	}

	private void MoveSmoothlyToTarget()
	{
		transform.position = Vector3.MoveTowards(transform.position, _targetWorldPosition, moveSpeed * Time.deltaTime);
		if (Vector3.Distance(transform.position, _targetWorldPosition) < 0.01f)
		{
			transform.position = _targetWorldPosition;
			_isMoving = false;
		}
	}
}
