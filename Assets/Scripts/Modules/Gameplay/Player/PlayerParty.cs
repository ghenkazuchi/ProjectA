using System.Collections.Generic;
using UnityEngine;

public class PlayerParty : BaseParty
{
	public static PlayerParty Instance { get; private set; }
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	public override bool AddPartyMember(EntityBase entityToAdd, GridPosition position)
	{
		entityToAdd.entityAffiliation = Affiliation.PartyMember;
		return base.AddPartyMember(entityToAdd, position);
	}
	public List<PlayerCharacter> GetAllPlayerCharacter()
	{
		List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
		foreach (var slot in partySlots)
		{
			if (slot.entity is PlayerCharacter playerCharacter)
			{
				playerCharacters.Add(playerCharacter);
			}
		}
		return playerCharacters;
	}
	public bool EquipWeaponToCharacter(PlayerCharacter character, Weapon weapon)
	{
		if (weapon != null && character != null)
		{
			return character.TryEquipWeapon(weapon);
		}
		return false;
	}
	public bool AddItemToCharacter(PlayerCharacter character, Item item)
	{
		if (item != null && character != null)
		{
			return character.TryAddItem(item);
		}
		return false;
	}
}
