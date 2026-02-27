using UnityEngine;

/// <summary>
/// Per-monster AI configuration. Attach to MonsterData to control
/// which priority rules the monster follows. Toggle rules on/off
/// and tweak thresholds in the Inspector.
/// </summary>
[System.Serializable]
public class AIBehaviorConfig
{
	[Header("Priority Rules (checked = enabled)")]
	[Tooltip("If a skill can kill a target, always go for it.")]
	public bool finishKill = true;

	[Tooltip("Heal an ally whose HP is below the healThreshold.")]
	public bool healCriticalAlly = false;

	[Tooltip("Prefer skills that have element advantage over the target.")]
	public bool exploitElement = true;

	[Tooltip("Pick a random enemy when no higher-priority rule matches.")]
	public bool randomTarget = false;

	[Header("Timeline Awareness")]
	[Tooltip("Burst a low-HP target before an enemy healer gets to act.")]
	public bool focusBeforeEnemyHeals = false;

	[Tooltip("Buff/support the ally who acts soonest after this monster.")]
	public bool buffBeforeAllyTurn = false;

	[Header("Thresholds")]
	[Range(0.1f, 0.5f), Tooltip("HP ratio below which an ally is considered critical.")]
	public float healThreshold = 0.3f;
}
