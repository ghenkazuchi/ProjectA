using UnityEngine;

[CreateAssetMenu(fileName = "Coordinate Attack Effect Data", menuName = "Effects/Coordinate Attack Effect Data")]
public class CoordinateAttackEffectData : EffectData
{
	[Header("Coordinate Attack Settings")]
	[Tooltip("Base proc chance at 0 stacks (e.g. 0.05 for 5%)")]
	public float baseProcChance = 0.05f;
	
	[Tooltip("Proc chance added per stack (e.g. 0.1 for 10%)")]
	public float procChancePerStack = 0.10f;
	
	[Tooltip("Maximum number of stacks the player can hold")]
	public int maxStacks = 4;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new CoordinateAttackEffect(this, owner, target, duration);
	}
}
