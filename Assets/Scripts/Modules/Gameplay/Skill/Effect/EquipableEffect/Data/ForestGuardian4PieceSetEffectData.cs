using UnityEngine;

[CreateAssetMenu(fileName = "Forest Guardian 4 Piece Set Effect Data", menuName = "Effects/Forest Guardian 4 Piece Set Effect Data")]
public class ForestGuardian4PieceSetEffectData : EffectData
{
	[Header("Forest Guardian Settings")]
	[Tooltip("Base proc chance per stack (e.g. 0.15 for 15%)")]
	public float baseProcChancePerStack = 0.15f;
	
	[Tooltip("Maximum number of stacks the player can hold")]
	public int maxStacks = 4;
	
	[Tooltip("Number of stacks gained at the start of the owner's turn")]
	public int stacksGainedPerTurn = 2;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ForestGuardian4PieceSetEffect(this, owner, target, duration);
	}
}
