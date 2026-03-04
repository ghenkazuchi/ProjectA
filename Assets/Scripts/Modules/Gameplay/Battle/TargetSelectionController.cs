using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetSelectionController : MonoBehaviour
{
	public List<BattleUnit> enemyUnits;
	public List<BattleUnit> playerUnits;
	[SerializeReference] private List<BattleUnit> currentAvailableTargets;
	private int currentIndex = 0;
	private int currentLineIndex = 0; 
	private bool isActive = false;
	private Action<List<EntityBase>> onTargetsConfirmed;
	private ActiveSkill currentSelectedSkill;
	private TargetType currentTargetType;
	private SkillRange currentSkillRange;
	[SerializeReference] private EntityBase callingEntity;
	private readonly Dictionary<int, List<int>> lineToIndices = new Dictionary<int, List<int>>
	{
		{ 0, new List<int> { 0, 1, 2 } }, // Back row: (0,0), (0,1), (0,2)
		{ 1, new List<int> { 3, 4, 5 } }  // Front row: (1,0), (1,1), (1,2)
	};

	public void StartSelection(Action<List<EntityBase>> onTargetsSelected,ActiveSkill skillToUSe,EntityBase callingEntity = null)
	{
		this.callingEntity = callingEntity;
		isActive = true;
		currentIndex = 0;
		currentLineIndex = 0;
		onTargetsConfirmed = onTargetsSelected;
		currentTargetType = skillToUSe.SkillData.targetType;
		currentSkillRange = skillToUSe.SkillData.skillRange;
		currentSelectedSkill = skillToUSe;
		currentAvailableTargets = new List<BattleUnit>();

		if (currentTargetType == TargetType.Enemy)
		{
			currentAvailableTargets = enemyUnits.Where(unit => unit.IsAlive()).ToList();
			// Single-target Damage skills can't hit protected back-row units (unless weapon pierces)
			if (currentSkillRange == SkillRange.SingleTarget && skillToUSe.SkillData.activeSkillType == ActiveSkillType.Damage)
			{
				var pp = BattleSystem.Instance.playerParty;
				var mp = BattleSystem.Instance.monsterParty;
				currentAvailableTargets.RemoveAll(unit =>
					unit.character != null && !BattleGridUtils.IsTargetable(unit.character, pp, mp, callingEntity));
			}
		}
		else if (currentTargetType == TargetType.Ally)
		{
			currentAvailableTargets = playerUnits.Where(unit => unit.IsAlive() && unit.character != callingEntity).ToList();
		}
		else if(currentTargetType == TargetType.SelfOrAllies)
		{
			currentAvailableTargets = playerUnits.Where(unit => unit.IsAlive()).ToList();
		}
		else if(currentTargetType == TargetType.Self)
		{
			BattleUnit selfUnit = playerUnits.FirstOrDefault(unit => unit.character == callingEntity && unit.IsAlive());
			currentAvailableTargets.Add(selfUnit);
		}
		if (currentAvailableTargets.Count == 0)
		{
			Debug.LogWarning("No valid targets available for selection.");
			CancelSelection();
			return;
		}

		if (currentSkillRange == SkillRange.LineTarget || currentSkillRange == SkillRange.LineAllies)
		{
			currentLineIndex = GetFirstValidLineIndex();
		}

		gameObject.SetActive(true);
		UpdateHighlight();
	}

	private void Update()
	{
		if (!isActive) return;

		if (currentSkillRange == SkillRange.SingleTarget || currentSkillRange == SkillRange.SingleAlly)
		{
			if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Move(1);
			if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Move(-1);
		}
		else if (currentSkillRange == SkillRange.LineTarget || currentSkillRange == SkillRange.LineAllies)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveLineSelection(-1);
			if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveLineSelection(1);
		}

		if (Input.GetKeyDown(KeyCode.Z)) Confirm();
		if (Input.GetKeyDown(KeyCode.X)) CancelSelection();
	}

	private void Move(int direction)
	{
		if (currentAvailableTargets.Count == 0) return;
		currentIndex = Mathf.Clamp(currentIndex + direction, 0, currentAvailableTargets.Count - 1);
		UpdateHighlight();
	}

	private void MoveLineSelection(int direction)
	{
		ClearAllHighlights();

		List<int> validLines = GetValidLineIndices();

		if (validLines.Count == 0) return;

		int currentValidIndex = validLines.IndexOf(currentLineIndex);

		if (currentValidIndex >= 0)
		{
			int newValidIndex = Mathf.Clamp(currentValidIndex + direction, 0, validLines.Count - 1);
			currentLineIndex = validLines[newValidIndex];
		}
		else
		{
			currentLineIndex = validLines[0];
		}

		UpdateHighlight();
	}

	private List<BattleUnit> GetLineUnits(int lineIndex)
	{
		var result = new List<BattleUnit>();
		if (!lineToIndices.ContainsKey(lineIndex))
			return result;

		var targetUnits = (currentTargetType == TargetType.Enemy) ? enemyUnits : playerUnits;
		foreach (var idx in lineToIndices[lineIndex])
		{
			if (idx >= 0 && idx < targetUnits.Count)
			{
				var unit = targetUnits[idx];
				if (unit != null && unit.character != null && unit.IsAlive())
					result.Add(unit);
			}
		}
		return result;
	}


	private void UpdateHighlight()
	{
		ClearAllHighlights();
		ClearAllPreviews();

		if (currentAvailableTargets.Count == 0)
			return;

		if (currentSkillRange == SkillRange.SingleTarget || currentSkillRange == SkillRange.SingleAlly)
		{
			SetHighlight(currentIndex, true);
			ApplyPreviewToTargets(new[] { currentAvailableTargets[currentIndex] }, true);
		}
		else if (currentSkillRange == SkillRange.AllTarget || currentSkillRange == SkillRange.AllAllies)
		{
			foreach (var unit in currentAvailableTargets)
				unit.SetHighLight(true);
			ApplyPreviewToTargets(currentAvailableTargets, true);
		}
		else if (currentSkillRange == SkillRange.LineTarget || currentSkillRange == SkillRange.LineAllies)
		{
			var lineUnits = GetLineUnits(currentLineIndex);
			foreach (var unit in lineUnits)
				unit.SetHighLight(true);
			ApplyPreviewToTargets(lineUnits, true);
		}
	}

	private void Confirm()
	{
		if (currentAvailableTargets.Count == 0)
		{
			CancelSelection();
			return;
		}

		isActive = false;
		ClearAllHighlights();
		gameObject.SetActive(false);

		List<EntityBase> targets = new List<EntityBase>();

		if (currentSkillRange == SkillRange.SingleTarget || currentSkillRange == SkillRange.SingleAlly)
		{
			if (currentIndex >= 0 && currentIndex < currentAvailableTargets.Count &&
				currentAvailableTargets[currentIndex] != null &&
				currentAvailableTargets[currentIndex].character != null)
			{
				targets.Add(currentAvailableTargets[currentIndex].character);
			}
		}
		else if (currentSkillRange == SkillRange.AllTarget || currentSkillRange == SkillRange.AllAllies)
		{
			foreach (var unit in currentAvailableTargets)
			{
				if (unit != null && unit.character != null)
				{
					targets.Add(unit.character);
				}
			}
		}
		else if (currentSkillRange == SkillRange.LineTarget || currentSkillRange == SkillRange.LineAllies)
		{
			targets = GetLineTargets(currentLineIndex);
		}

		onTargetsConfirmed?.Invoke(targets);
	}

	private List<EntityBase> GetLineTargets(int lineIndex)
	{
		List<EntityBase> targets = new List<EntityBase>();

		if (!lineToIndices.ContainsKey(lineIndex)) return targets;

		List<int> lineIndices = lineToIndices[lineIndex];
		List<BattleUnit> targetUnits = (currentTargetType == TargetType.Enemy) ? enemyUnits : playerUnits;

		foreach (int unitIndex in lineIndices)
		{
			if (unitIndex >= 0 && unitIndex < targetUnits.Count &&
				targetUnits[unitIndex].IsAlive() &&
				targetUnits[unitIndex].character != null)
			{
				targets.Add(targetUnits[unitIndex].character);
			}
		}

		return targets;
	}

	private int GetFirstValidLineIndex()
	{
		for (int lineIndex = 0; lineIndex < lineToIndices.Count; lineIndex++)
		{
			if (LineHasValidTargets(lineIndex))
			{
				return lineIndex;
			}
		}
		return 0;
	}

	private List<int> GetValidLineIndices()
	{
		List<int> validLines = new List<int>();

		for (int lineIndex = 0; lineIndex < lineToIndices.Count; lineIndex++)
		{
			if (LineHasValidTargets(lineIndex))
			{
				validLines.Add(lineIndex);
			}
		}

		return validLines;
	}

	private bool LineHasValidTargets(int lineIndex)
	{
		if (!lineToIndices.ContainsKey(lineIndex)) return false;

		List<int> lineIndices = lineToIndices[lineIndex];
		List<BattleUnit> targetUnits = (currentTargetType == TargetType.Enemy) ? enemyUnits : playerUnits;

		foreach (int unitIndex in lineIndices)
		{
			if (unitIndex >= 0 && unitIndex < targetUnits.Count &&
				targetUnits[unitIndex].IsAlive() &&
				targetUnits[unitIndex].character != null)
			{
				return true;
			}
		}

		return false;
	}

	private void CancelSelection()
	{
		isActive = false;
		ClearAllHighlights();
		ClearAllPreviews();
		currentSelectedSkill = null;
		gameObject.SetActive(false);
		onTargetsConfirmed?.Invoke(null);
	}

	private void SetHighlight(int index, bool isHighlighted)
	{
		if (index >= 0 && index < currentAvailableTargets.Count && currentAvailableTargets[index] != null && currentAvailableTargets[index].IsAlive())
		{
			currentAvailableTargets[index].SetHighLight(isHighlighted);
		}
	}

	private void ClearAllHighlights()
	{
		foreach (var unit in enemyUnits)
		{
			if (unit != null) unit.SetHighLight(false);
		}
		foreach (var unit in playerUnits)
		{
			if (unit != null) unit.SetHighLight(false);
		}
	}

	private void ApplyPreviewToTargets(IEnumerable<BattleUnit> targets, bool preview)
	{
		if (currentSelectedSkill == null || callingEntity == null)
		{
			foreach (var unit in targets)
				unit?.ClearPreview();
			return;
		}

		foreach (var unit in targets)
		{
			if (unit != null && unit.character != null && unit.IsAlive())
			{
				if (preview)
				{
					int damage = BattleSystem.Instance.CalculateDamage(
						callingEntity, currentSelectedSkill, unit.character);
					unit.PreviewDamage(damage);
				}
				else
				{
					unit.ClearPreview();
				}
			}
		}
	}
	public void ClearAllPreviews()
	{
		foreach (var unit in enemyUnits)
		{
			if (unit != null) unit.ClearPreview();
		}
		foreach (var unit in playerUnits)
		{
			if (unit != null) unit.ClearPreview();
		}
	}
}