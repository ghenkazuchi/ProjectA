using UnityEngine;

/// <summary>
/// Single shared config for all item grade visuals and stat multipliers.
/// Create one asset: Create → Config → Item Grade Config, then assign it in your GameManager or load via Resources.
/// </summary>
[CreateAssetMenu(fileName = "ItemGradeConfig", menuName = "Config/Item Grade Config")]
public class ItemGradeConfig : ScriptableObject
{
	private static ItemGradeConfig _instance;
	public static ItemGradeConfig Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<ItemGradeConfig>("ItemGradeConfig");
				if (_instance == null)
					Debug.LogError("[ItemGradeConfig] No ItemGradeConfig found in Resources folder!");
			}
			return _instance;
		}
	}

	[Header("Grade Tint Colors")]
	public Color normalTint = Color.white;
	public Color goldTint = new Color(1f, 0.92f, 0.38f);
	public Color diamondTint = new Color32(83, 115, 178, 255);

	[Header("Stat Multipliers (applied to EquipableStatBonus values)")]
	[Tooltip("Gold items get base stats × this value")]
	public float goldStatMultiplier = 1.5f;
	[Tooltip("Diamond items get base stats × this value")]
	public float diamondStatMultiplier = 2.0f;

	public Color GetTint(ItemGrade grade) => grade switch
	{
		ItemGrade.Gold    => goldTint,
		ItemGrade.Diamond => diamondTint,
		_                 => normalTint
	};

	public float GetStatMultiplier(ItemGrade grade) => grade switch
	{
		ItemGrade.Gold    => goldStatMultiplier,
		ItemGrade.Diamond => diamondStatMultiplier,
		_                 => 1f
	};
}
