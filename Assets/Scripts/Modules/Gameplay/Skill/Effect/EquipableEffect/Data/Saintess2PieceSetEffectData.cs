using UnityEngine;

[CreateAssetMenu(fileName = "Saintess 2 Piece Set Effect Data", menuName = "Effects/Saintess 2 Piece Set Effect Data")]
public class Saintess2PieceSetEffectData : EffectData
{
	[Header("Healing Bonus")]
	[Tooltip("Healing bonus percentage (0.20 = +20%)")]
	public float healingBonusPercent = 0.20f;

	private void OnEnable() => isPassiveEquipmentEffect = true;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new Saintess2PieceSetEffect(this, owner, target, duration);
	}
}
