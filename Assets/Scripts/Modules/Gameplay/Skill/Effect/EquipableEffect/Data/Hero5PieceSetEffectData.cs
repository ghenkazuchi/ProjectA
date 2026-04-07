using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hero 5 Piece Set Effect Data", menuName = "Effects/Hero 5 Piece Set Effect Data")]
public class Hero5PieceSetEffectData : EffectData
{
	[Header("Element Override")]
	[Tooltip("All Battle Arts will be changed to this element")]
	public Element battleArtElement = Element.Light;

	[Header("Damage Bonuses")]
	[Tooltip("Damage bonus for Battle Arts (0.15 = +15%)")]
	public float battleArtDamageBonus = 0.15f;

	[Tooltip("Damage bonus for Light element Spells (0.25 = +25%)")]
	public float lightSpellDamageBonus = 0.25f;

	private void OnEnable() => isPassiveEquipmentEffect = true;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new Hero5PieceSetEffect(this, owner, target, duration);
	}
}
