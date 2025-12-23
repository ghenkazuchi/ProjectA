using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseParty : MonoBehaviour
{
	public List<GridSlot> partySlots = new List<GridSlot>();
	public const int MAX_PARTY_SIZE = 6;
	public static readonly List<GridPosition> FixedPositions = new()
	{
	new GridPosition(0, 0),
	new GridPosition(1, 0),
	new GridPosition(0, 1),
	new GridPosition(1, 1),
	new GridPosition(0, 2),
	new GridPosition(1, 2),
	};

	public virtual bool AddPartyMember(EntityBase entityToAdd,GridPosition position)
	{
		if (partySlots.Exists(slot => slot.position.x == position.x && slot.position.y == position.y))
			return false;
		if (partySlots.Count >= MAX_PARTY_SIZE)
			return false;

		partySlots.Add(new GridSlot { position = position, entity = entityToAdd });
		return true;
	}
	public  bool RemovePartyMember(EntityBase entityToRemove)
	{
		var slot = partySlots.Find(s => s.entity == entityToRemove);
		if (slot != null)
		{
			partySlots.Remove(slot);
			return true;
		}
		return false;
	}

	public bool IsPositionEmpty(GridPosition position)
	{
		return !partySlots.Exists(slot => slot.position.x == position.x && slot.position.y == position.y);
	}
	public EntityBase GetEntityAtPosition(GridPosition position) 
	{
		var slot = partySlots.Find(s => s.position.x == position.x && s.position.y == position.y);
		return slot?.entity;
	}
	public GridPosition GetCharacterPosition(EntityBase entity)
	{
		return partySlots.Find(s => s.entity == entity)?.position;
	}

	public List<GridPosition> GetAllAvailablePositions()
	{
		return FixedPositions.Where(IsPositionEmpty).ToList();
	}
	public List<EntityBase> GetAllEntitiesInParty()
	{
		return partySlots.Select(slot => slot.entity).ToList();
	}
	public void AddCharacter(PlayerCharacter characterToAdd)
	{
		var availablePositions = GetAllAvailablePositions();
		if (availablePositions.Count > 0)
		{
			GridPosition randomPosition = availablePositions[Random.Range(0, availablePositions.Count)];

			if (!AddPartyMember(characterToAdd, randomPosition))
			{
				Debug.LogWarning($"Failed to add {characterToAdd.entityData.EntityName} to party at position ({randomPosition.x},{randomPosition.y}). This should not happen if GetAllAvailablePositions is correct.");
			}
		}
		else
		{
			Debug.LogWarning($"Player party is full! Cannot add {characterToAdd.entityData.EntityName}.");
		}
	}
}

[System.Serializable]
public class GridPosition
{
	public int x;
	public int y;

	public GridPosition(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override bool Equals(object obj)
	{
		if (obj is GridPosition other)
			return x == other.x && y == other.y;
		return false;
	}

	public override int GetHashCode() => (x, y).GetHashCode();
}
[System.Serializable]
public class GridSlot
{
	public GridPosition position;
	[SerializeReference] public EntityBase entity;
}
