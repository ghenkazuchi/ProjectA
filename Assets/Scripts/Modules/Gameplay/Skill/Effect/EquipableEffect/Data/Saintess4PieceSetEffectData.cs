using UnityEngine;

[CreateAssetMenu(fileName = "Saintess 4 Piece Set Effect Data", menuName = "Effects/Saintess 4 Piece Set Effect Data")]
public class Saintess4PieceSetEffectData : EffectData
{
	[Header("Healing Bonus")]
	[Tooltip("Additional healing bonus percentage (0.15 = +15%)")]
	public float healingBonusPercent = 0.15f;

	[Header("MP Refund")]
	[Tooltip("Chance to refund MP after using a Spell (0.30 = 30%)")]
	public float mpRefundChance = 0.30f;

	private void OnEnable() => isPassiveEquipmentEffect = true;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new Saintess4PieceSetEffect(this, owner, target, duration);
	}
}
