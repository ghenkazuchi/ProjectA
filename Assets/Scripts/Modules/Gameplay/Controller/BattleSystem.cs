using HaKien;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;
using UnityEngine.Pool;
public enum BattleState
{
	Start,
	ActionSelection,
	SkillSelection,
	TargetSelection,
	RunningTurn,
	Busy,
	PartyScreen,
	BattleOver,
	AboutToUse,
	ForgetSKill,
	PositionSwitch,
	ShowingDialog
}
public enum BattleAction
{
	Skill,
	BasicAttack,
	Run,
	Defend
}
public enum DefenseState
{
	None,
	Defending
}

public class BattleSystem : HaKien.Singleton<BattleSystem>
{
	// Constant values;
	public const float MIN_HIT = 0.85f;
	public const float MAX_HIT = 0.95f;

	private const float backrowDamageReduction = 0.8f;
	public const float baseCritChance = 0.1f;
	public const float baseCritMultiplier = 1.5f;

	public BattleType currentBattleType;
	[SerializeField] private VfxLib vfxLib;
	[SerializeField] private ExpDistributionController expDistributionController;
	[SerializeField] private BattleState battleState;
	private EntityBase currentTurnEntity;
	[SerializeField] BattleHud currentPlayerCharacterInfo;
	private int currentAction;
	private int currentSkill;
	[SerializeField] GameObject battleSystem;
	[SerializeField] BattleDialogBox battleDialogBox;
	[SerializeField] PlayerParty playerParty;
	public MonsterParty monsterParty;
	public bool battleOver;
	private List<EntityBase> allEntities;
	[SerializeField] List<BattleUnit> playerBattleUnits;
	[SerializeField] List<BattleUnit> monsterBattleUnits;
	public TimelineManager timelineManager;
	[SerializeField] private TurnTimelineUI timelineUI;
	[SerializeField] private TargetSelectionController targetSelectionController;
	private ActiveSkill selectedSkill;
	private List<EntityBase> selectedTargets;
	private Coroutine battleCoroutine;
	public IMonsterInteracable currentMonsterInteractable;

	[SerializeField] private ActiveSkillData basicAttackSkillData;

	private ActiveSkill basicAttack;

	private void Awake()
	{
		basicAttack = new ActiveSkill(basicAttackSkillData);
	}

	private readonly Dictionary<GridPosition, int> positionToBattleUnitIndex = new Dictionary<GridPosition, int>
	{
		{ new GridPosition(0, 0), 0 },
		{ new GridPosition(1, 0), 3 },
		{ new GridPosition(1, 2), 5 },
		{ new GridPosition(0, 1), 1 },
		{ new GridPosition(1, 1), 4 },
		{ new GridPosition(0, 2), 2 },
	};
	private Dictionary<EntityBase, DefenseState> entityDefenseStates = new Dictionary<EntityBase, DefenseState>();

	[SerializeField] private float defenseStateDamageReduction = 0.1f;
	private readonly DamageContext _ctx = new DamageContext();
	private static readonly WaitForSeconds waifHalf = new WaitForSeconds(0.5f);	
	private static readonly WaitForSeconds waitOne = new WaitForSeconds(1f);

	public void StartBattle(BattleType batteType)
	{
		currentBattleType = batteType;
		battleOver = false;
		battleState = BattleState.Start;
		battleSystem.SetActive(true);
		SetUpBattle();
		allEntities = new List<EntityBase>();
		allEntities.AddRange(playerParty.GetAllEntitiesInParty());
		allEntities.AddRange(monsterParty.GetAllEntitiesInParty());

		foreach (var entity in allEntities)
		{
			entityDefenseStates[entity] = DefenseState.None;
			entity.InitializePassiveRunner(this);
			entity.EquipmentEffectRunner?.UpdateBattleSystem(this);
		}
		battleDialogBox.EnableDialogText(true);
		if (battleCoroutine != null)
		{
			StopCoroutine(battleCoroutine);
			battleCoroutine = null;
		}

		currentPlayerCharacterInfo.EnableBattleHud(false);
		battleDialogBox.EnableActionSelector(false);
		UpdateTimelineUI();
		battleCoroutine = StartCoroutine(HandleBattleLoop());
	}
	public IEnumerator ResetAllEquipmentBattleUsageTime()
	{
		Debug.Log("Resetting");
		foreach (var entity in allEntities)
		{
			entity.ResetEquipmentBattleUsage();
		}
		yield return null;
	}
	public IEnumerator ActiveInitialPassiveSkillsEffect()
	{
		foreach (var entity in allEntities)
		{
			var activeSkillList = entity.usableSkills;
			yield return (entity.PassiveSkillRunner.Trigger(PassiveTrigger.OnBattleStart, activeSkillList));
			yield return (entity.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBattleStart, entity));
		}
	}
	private IEnumerator ShowInitialDialog()
	{
		battleState = BattleState.ShowingDialog;
		if (currentBattleType == BattleType.RoamingMoster)
		{
			yield return StartCoroutine(battleDialogBox.TypeDialog("Player run into wild monster"));
		}
		else
		{
			yield return StartCoroutine(ShowDialog("Player encountered Boss"));
		}
	}
	private int ApplyDefenseReduction(EntityBase entity, int originaldamage)
	{
		if (entityDefenseStates.ContainsKey(entity) && entityDefenseStates[entity] == DefenseState.Defending)
		{
			float reduction = defenseStateDamageReduction;
			float bonus = 0f;
			var effects = entity.GetAllEffect();
			foreach (var e in effects)
			{
				if (e is IModifyIncomingDamageTakenOnDefenseState mod)
				{
					bonus += Mathf.Max(0, mod.GetModifyOnDefenseState());
				}
			}
			reduction = Mathf.Clamp01(reduction + bonus); 
			int reduced = Mathf.RoundToInt(originaldamage * (1f - reduction));
			Debug.Log($"{entity.entityData.EntityName} is defending! Damage reduced from {originaldamage} to {reduced}");
			return reduced;
		}
		return originaldamage;
	}
	private void ResetDefenseState(EntityBase entity)
	{
		if (entityDefenseStates.ContainsKey(entity))
		{
			entityDefenseStates[entity] = DefenseState.None;
		}
	}
	private IEnumerator AttemptToRun()
	{
		battleState = BattleState.ShowingDialog;
		currentPlayerCharacterInfo.EnableBattleHud(false);
		yield return StartCoroutine(battleDialogBox.TypeDialog($"{currentTurnEntity.entityData.EntityName} is trying to escape!"));
		yield return new WaitForSeconds(1f);

		float runSuccessChance = 0.5f;

		float playerSpeed = currentTurnEntity.GetFinalStat(Stat.ActionSpeed);
		float averageEnemySpeed = 0f;
		var enemies = monsterParty.GetAllEntitiesInParty();
		foreach (var enemy in enemies)
		{
			if (enemy != null && enemy.GetCurrentHP() > 0)
			{
				averageEnemySpeed += enemy.GetFinalStat(Stat.ActionSpeed);
			}
		}
		averageEnemySpeed /= enemies.Count;

		if (playerSpeed > averageEnemySpeed)
		{
			runSuccessChance += 0.3f;
		}

		if (Random.Range(0f, 1f) < runSuccessChance)
		{
			yield return StartCoroutine(battleDialogBox.TypeDialog("Successfully escaped from battle!"));
			battleOver = true;
			battleState = BattleState.BattleOver;
			MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleOver));
		}
		else
		{
			yield return StartCoroutine(battleDialogBox.TypeDialog("Couldn't escape!"));
			battleState = BattleState.Start;
		}
		yield return null;
	}
	private IEnumerator HandleEntityTakeDamage(
		EntityBase target, int finalDamage, EntityBase source,
		SkillDefinition origin, bool isEffectDamage = false, string flavor = null)
	{
		var originalTarget = target;

		_ctx.Reset(source, target, finalDamage, origin, isEffectDamage);

		yield return TriggerBeforeDealingDamage(source, _ctx.Target, _ctx);
		yield return TriggerBeforeTakingDamage(_ctx.Target, _ctx);

		foreach (var ally in GetAlliesOf(_ctx.Target))
		{
			if (ally == null || ally == _ctx.Target) continue;
			if (!IsEntityAlive(ally)) continue;
			yield return TriggerBeforeTakingDamage(ally, _ctx);
		}
		if (_ctx.CancleApply)
		{
			var parryVfx = vfxLib.GetParryVFX();
			var targetUnit = FindBattleUnitForEntity(target);
			var animator = targetUnit.GetAnimator();
			yield return animator.PlayParryAnimation(parryVfx);
			if (_ctx.ReflectAmount > 0)
				yield return ApplyEffectDamage(_ctx.Source, _ctx.ReflectAmount, _ctx.Target, "parry");
			yield break;
		}

		if (_ctx.RedirectTo != null && _ctx.RedirectTo != _ctx.Target
			&& AreAllies(_ctx.RedirectTo, _ctx.Target) && IsEntityAlive(_ctx.RedirectTo))
		{
			var protector = _ctx.RedirectTo;
			var protectorName = protector.entityData?.EntityName;
			var victimName = originalTarget.entityData?.EntityName;

			yield return ShowDialog($"{protectorName} protected {victimName}!");
			_ctx.Target = protector;
			yield return TriggerBeforeTakingDamage(_ctx.Target, _ctx);

			if (_ctx.CancleApply)
			{
				if (_ctx.ReflectAmount > 0)
					yield return ApplyEffectDamage(_ctx.Source, _ctx.ReflectAmount, _ctx.Target, "parry");
				yield break;
			}
		}
		if (_ctx.SplitRedirectTo != null && _ctx.SplitPercent > 0f && _ctx.SplitPercent < 1f && !_ctx.BlockFurtherSharing)
		{
			var protector = _ctx.SplitRedirectTo;
			if (protector != null && IsEntityAlive(protector) && AreAllies(protector, target))
			{
				int toProtector = Mathf.RoundToInt(_ctx.EffectiveDamage * _ctx.SplitPercent);
				int toVictim = _ctx.EffectiveDamage - toProtector;

				var protectorName = protector.entityData?.EntityName;
				var victimName = originalTarget.entityData?.EntityName;
				int sharePct = Mathf.RoundToInt(_ctx.SplitPercent * 100f);

				var shareCtx = new DamageContext();
				shareCtx.Reset(_ctx.Source, protector, toProtector, _ctx.Origin, false);
				shareCtx.BlockFurtherSharing = true;
				yield return TriggerBeforeDealingDamage(_ctx.Source, protector, shareCtx);
				yield return TriggerBeforeTakingDamage(protector, shareCtx);

				if (shareCtx.CancleApply)
				{
					if (shareCtx.ReflectAmount > 0)
						yield return ApplyEffectDamage(shareCtx.Source, shareCtx.ReflectAmount, protector, "parry");
				}
				else
				{
					BattleUnit protectorBattleUnit = FindBattleUnitForEntity(protector);
					protector.TakeDamage(shareCtx.EffectiveDamage, shareCtx.Source);
					yield return TriggerOnTakingDamage(protector, shareCtx);
					UpdateUnitHealth(protector);
					if (UpdateUnitState(protector)) yield break;
				}
				_ctx.EffectiveDamage = toVictim;
			}
		}
		ResolveCrit(_ctx);
		if (_ctx.IsCritical)
		{
			_ctx.EffectiveDamage = Mathf.RoundToInt(_ctx.EffectiveDamage * _ctx.CritMultiplier);
		}
		BattleUnit targetBattleUnit = FindBattleUnitForEntity(_ctx.Target);
		string critical = _ctx.IsCritical ? " It's a critical hit!" : "";
		_ctx.Target.TakeDamage(_ctx.EffectiveDamage, _ctx.Source);

		string msg = string.IsNullOrEmpty(flavor)
			? $"{_ctx.Target.entityData.EntityName} took {_ctx.EffectiveDamage} damage!"
			: $"{_ctx.Target.entityData.EntityName} took {_ctx.EffectiveDamage} {flavor} damage!";
		yield return ShowDialog(msg);
		if (_ctx.IsCritical)
		{
			yield return ShowDialog(critical);
		}

		yield return TriggerDealingDamage(_ctx.Source, _ctx.Target, _ctx);
		yield return TriggerOnTakingDamage(_ctx.Target, _ctx);
		UpdateUnitHealth(_ctx.Target);
		if (UpdateUnitState(_ctx.Target)) yield break;
	}



	public IEnumerator ApplyEffectDamage(EntityBase target, int amount, EntityBase source, string reason = null)
	{
		BattleUnit targetUnit = FindBattleUnitForEntity(target);
		yield return HandleEntityTakeDamage(target, amount, source, SkillDefinition.Almighty, true, flavor: reason);
	}
	private void Update()
	{
		if (GameController.Instance.currentState == GameState.Battle)
		{
			if (battleDialogBox.IsDialogTyping) return;
			if (battleState == BattleState.ActionSelection)
			{
				HandleActionSelectionInput();
			}
			else if (battleState == BattleState.SkillSelection)
			{
				HandleSkillSelectionInput();
			}
		}
	}
	public void SetUpBattle()
	{
		foreach (var unit in playerBattleUnits)
		{
			if (unit != null)
			{
				unit.gameObject.SetActive(false);
				unit.character = null;
			}
		}
		foreach (var unit in monsterBattleUnits)
		{
			if (unit != null)
			{
				unit.gameObject.SetActive(false);
				unit.character = null;
			}
		}

		foreach (var partySlot in playerParty.partySlots)
		{
			if (partySlot.entity == null)
			{
				continue;
			}

			GridPosition pos = partySlot.position;
			EntityBase character = partySlot.entity;

			if (positionToBattleUnitIndex.TryGetValue(pos, out int battleUnitIndex))
			{
				if (battleUnitIndex >= 0 && battleUnitIndex < playerBattleUnits.Count)
				{
					if (playerBattleUnits[battleUnitIndex] != null)
					{
						playerBattleUnits[battleUnitIndex].character = character;
						playerBattleUnits[battleUnitIndex].SetUp();
						playerBattleUnits[battleUnitIndex].gameObject.SetActive(true);
						playerBattleUnits[battleUnitIndex].SetUnitType(UnitType.PlayerUnit);
					}
				}
			}
		}

		foreach (var partySlot in monsterParty.partySlots)
		{
			if (partySlot.entity == null)
			{
				continue;
			}

			GridPosition pos = partySlot.position;
			EntityBase monster = partySlot.entity;

			if (positionToBattleUnitIndex.TryGetValue(pos, out int battleUnitIndex))
			{
				if (battleUnitIndex >= 0 && battleUnitIndex < monsterBattleUnits.Count)
				{
					if (monsterBattleUnits[battleUnitIndex] != null)
					{
						monsterBattleUnits[battleUnitIndex].character = monster;
						monsterBattleUnits[battleUnitIndex].SetUp();
						monsterBattleUnits[battleUnitIndex].gameObject.SetActive(true);
						monsterBattleUnits[battleUnitIndex].SetUnitType(UnitType.MonsterUnit);
					}
				}
			}
		}
		targetSelectionController.enemyUnits = monsterBattleUnits;
		targetSelectionController.playerUnits = playerBattleUnits;
		targetSelectionController.gameObject.SetActive(false);
		List<BattleUnit> allActiveBattleUnits = new List<BattleUnit>();
		allActiveBattleUnits.AddRange(playerBattleUnits);
		allActiveBattleUnits.AddRange(monsterBattleUnits);
		timelineManager.SetAllActiveBattleUnits(allActiveBattleUnits);
		timelineManager.Initialize(allActiveBattleUnits);
		UpdateTimelineUI();
		battleState = BattleState.Start;
	}
	public void UpdateTimelineUI()
	{
		var upcomingEntities = timelineManager.PeekNextEntitiesWithCurrent(currentTurnEntity, 5);
		timelineUI.UpdateTimeline(upcomingEntities);
	}
	IEnumerator HandleBattleLoop()
	{
		yield return ShowInitialDialog();
		yield return ResetAllEquipmentBattleUsageTime();
		yield return ActiveInitialPassiveSkillsEffect();

		battleState = BattleState.Start;
		while (!battleOver)
		{
			if (CheckBattleEndCondition())
			{
				yield break;
			}

			RemoveDeadUnitsFromTimeline();
			UpdateTimelineUI();
			currentTurnEntity = timelineManager.GetNextTurnEntity();
			BattleUnit currentUnit = FindBattleUnitForEntity(currentTurnEntity);
			currentTurnEntity.ResetTurnDirective();
			yield return StartCoroutine(currentTurnEntity.ProcessEffectOnTurnStart());
			if (currentUnit == null || !currentUnit.IsAlive())
			{
				Debug.Log($"{currentTurnEntity.entityData.EntityName} is dead, skipping turn");
				continue;
			}
			var dir = currentTurnEntity.TurnControl;
			if (dir.SkipThisTurn)
			{
				string reasonSuffix = string.IsNullOrEmpty(dir.SkipReason) ? "" : $" ({dir.SkipReason})";
				yield return StartCoroutine(ShowDialog(
					$"{currentTurnEntity.entityData.EntityName} skips the turn{reasonSuffix}."
				));
				yield return StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
				continue;
			}

			if (dir.ForcedAction != ForcedActionKind.None)
			{
				yield return StartCoroutine(ExecuteForcedAction(currentTurnEntity, dir));
				yield return waifHalf;
				yield return StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
				yield return waifHalf;
				continue;
			}

			Debug.Log("It's " + currentTurnEntity.entityData.EntityName + "'s turn!");
			UpdateTimelineUI();

			ResetDefenseState(currentTurnEntity);

			if (currentTurnEntity is PlayerCharacter)
			{
				yield return StartCoroutine(HandlePlayerTurn(currentTurnEntity));
			}
			else
			{
				yield return StartCoroutine(HandleMonsterTurn(currentTurnEntity));
			}
			yield return StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
			yield return waitOne;
		}
	}
	private BattleUnit FindBattleUnitForEntity(EntityBase entity)
	{
		foreach (var unit in playerBattleUnits)
		{
			if (unit != null && unit.character == entity)
			{
				return unit;
			}
		}

		foreach (var unit in monsterBattleUnits)
		{
			if (unit != null && unit.character == entity)
			{
				return unit;
			}
		}

		return null;
	}
	private void HandleActionSelectionInput()
	{
		if (battleState != BattleState.ActionSelection) return;
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			currentAction++;
			if (currentAction >= battleDialogBox.actionTexts.Count)
			{
				currentAction = 0;
			}
			OnActionSelect(currentAction);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			currentAction--;
			if (currentAction < 0)
			{
				currentAction = battleDialogBox.actionTexts.Count - 1;
			}
			OnActionSelect(currentAction);
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			if (!battleDialogBox.IsDialogTyping)
			{
				OnActionConfirm();
			}
		}

	}
	private void HandleSkillSelectionInput()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			int next = NextSelectableSkillIndex(currentSkill, +1);
			if (next != -1) currentSkill = next;
			OnSkillSelect(currentSkill);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			int prev = NextSelectableSkillIndex(currentSkill, -1);
			if (prev != -1) currentSkill = prev;
			OnSkillSelect(currentSkill);
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			OnSkillConfirm();
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			OnSkillCancel();
		}
	}
	IEnumerator HandlePlayerTurn(EntityBase entity)
	{
		battleDialogBox.EnableDialogText(false);
		currentPlayerCharacterInfo.EnableBattleHud(true);
		currentPlayerCharacterInfo.SetData(entity);
		battleState = BattleState.ActionSelection;
		battleDialogBox.EnableActionSelector(true);
		currentAction = 0;
		battleDialogBox.UpdateActionSelection(currentAction);
		yield return new WaitUntil(() => battleState == BattleState.RunningTurn || battleState == BattleState.BattleOver);

		battleDialogBox.EnableActionSelector(false);
	}
	IEnumerator HandleMonsterTurn(EntityBase entity)
	{
		battleState = BattleState.RunningTurn;

		yield return waifHalf;
		if (entity.usableSkills.Count > 0)
		{
			ActiveSkill randomSkill = entity.usableSkills[Random.Range(0, entity.usableSkills.Count)];
			List<EntityBase> targets = GetAITargets(entity, randomSkill);

			if (targets.Count > 0)
			{
				yield return StartCoroutine(PerformSkillAction(entity, targets, randomSkill));
			}
			else
			{
				Debug.Log($"{entity.entityData.EntityName} couldn't find any targets!");
				battleState = BattleState.Start;
			}
		}
		else
		{
			Debug.Log($"{entity.entityData.EntityName} has no usable skills!");
			battleState = BattleState.Start;
		}
	}

	private List<EntityBase> GetAITargets(EntityBase aiEntity, ActiveSkill skill)
	{
		var targets = ListPool<EntityBase>.Get();
		var availableTargets = ListPool<EntityBase>.Get();

		if (skill.SkillData.targetType == TargetType.Enemy)
			availableTargets.AddRange(playerParty.GetAllEntitiesInParty().FindAll(e => e != null && e.GetCurrentHP() > 0));
		else if (skill.SkillData.targetType == TargetType.Ally)
			availableTargets.AddRange(monsterParty.GetAllEntitiesInParty().FindAll(e => e != null && e.GetCurrentHP() > 0));
		else if (skill.SkillData.targetType == TargetType.Self)
		{
			targets.Add(aiEntity);
			var resultSelf = new List<EntityBase>(targets);
			ListPool<EntityBase>.Release(targets);
			ListPool<EntityBase>.Release(availableTargets);
			return resultSelf;
		}

		if (availableTargets.Count == 0)
		{
			ListPool<EntityBase>.Release(targets);
			ListPool<EntityBase>.Release(availableTargets);
			return new List<EntityBase>();
		}

		switch (skill.SkillData.skillRange)
		{
			case SkillRange.SingleTarget:
			case SkillRange.SingleAlly:
				targets.Add(availableTargets[Random.Range(0, availableTargets.Count)]);
				break;
			case SkillRange.AllTarget:
			case SkillRange.AllAllies:
				targets.AddRange(availableTargets);
				break;
			case SkillRange.LineTarget:
			case SkillRange.LineAllies:
				var lineTargets = GetRandomLineTargets(availableTargets);
				targets.AddRange(lineTargets);
				ListPool<EntityBase>.Release(lineTargets);
				break;
		}
		var result = new List<EntityBase>(targets);
		ListPool<EntityBase>.Release(targets);
		ListPool<EntityBase>.Release(availableTargets);

		return result;
	}

	private List<EntityBase> GetRandomLineTargets(List<EntityBase> availableTargets)
	{
		var lineTargets = ListPool<EntityBase>.Get();

		if (availableTargets.Count > 0)
		{
			int targetCount = Random.Range(1, Mathf.Min(4, availableTargets.Count + 1));
			var shuffled = ListPool<EntityBase>.Get();
			shuffled.AddRange(availableTargets);

			for (int i = 0; i < targetCount; i++)
			{
				int randomIndex = Random.Range(0, shuffled.Count);
				lineTargets.Add(shuffled[randomIndex]);
				shuffled.RemoveAt(randomIndex);
			}

			ListPool<EntityBase>.Release(shuffled);
		}

		var result = new List<EntityBase>(lineTargets);
		ListPool<EntityBase>.Release(lineTargets);
		return result;
	}

	public void OnActionSelect(int actionIndex)
	{
		if (battleState == BattleState.ActionSelection)
		{
			currentAction = actionIndex;
			battleDialogBox.UpdateActionSelection(currentAction);
		}
	}
	public void OnActionConfirm()
	{
		if (battleState == BattleState.ActionSelection)
		{
			switch ((BattleAction)currentAction)
			{
				case BattleAction.Skill:
					StartCoroutine(HandleSkillSelection());
					break;
				case BattleAction.Defend:
					StartCoroutine(HandleDefend());
					break;
				case BattleAction.BasicAttack:
					StartCoroutine(HandleBasicAttackSelection());
					break;
				case BattleAction.Run:
					StartCoroutine(AttemptToRun());
					break;
			}
		}
	}
	private IEnumerator HandleDefend()
	{
		battleState = BattleState.Busy;
		currentPlayerCharacterInfo.EnableBattleHud(false);
		battleDialogBox.EnableActionSelector(false);
		battleDialogBox.EnableDialogText(true);
		yield return StartCoroutine(battleDialogBox.TypeDialog($"{currentTurnEntity.entityData.EntityName} takes a defensive stance!"));
		entityDefenseStates[currentTurnEntity] = DefenseState.Defending;

		battleState = BattleState.RunningTurn;
	}
	private IEnumerator HandleBasicAttackSelection()
	{
		battleState = BattleState.TargetSelection;
		yield return StartCoroutine(HandleTargetSelection(basicAttack))	;
	}

	IEnumerator HandleSkillSelection()
	{
		battleState = BattleState.SkillSelection;
		battleDialogBox.EnableAttackSelector(true);
		battleDialogBox.SetAttackName(currentTurnEntity.usableSkills);
		for (int i = 0; i < battleDialogBox.skillUI.Count;i++)
		{
			if(i < currentTurnEntity.usableSkills.Count)
			{
				var s = currentTurnEntity.usableSkills[i];
				bool allowed = IsSkillUseAllowed(currentTurnEntity, s.SkillData);
				Debug.Log($"{i} {allowed}");
				battleDialogBox.skillUI[i].SetUnuseable(allowed);
			}
		}
		currentSkill = 0;
		battleDialogBox.UpdateSkillSelection(currentSkill);
		yield return new WaitUntil(() => battleState == BattleState.TargetSelection || battleState == BattleState.ActionSelection || battleState == BattleState.RunningTurn || battleState == BattleState.BattleOver);

		battleDialogBox.EnableAttackSelector(false);
	}
	public void OnSkillSelect(int skillIndex)
	{
		if (battleState == BattleState.SkillSelection)
		{
			currentSkill = skillIndex;
			battleDialogBox.UpdateSkillSelection(currentSkill);
		}
	}

	public void OnSkillConfirm()
	{
		if (battleState == BattleState.SkillSelection)
		{
			selectedSkill = currentTurnEntity.usableSkills[currentSkill];
			if (!IsSkillUseAllowed(currentTurnEntity, selectedSkill.SkillData))
			{
				StartCoroutine(HandleUnusableSkillConfirm(selectedSkill));
				return;
			}
			battleState = BattleState.TargetSelection;
			StartCoroutine(HandleTargetSelection(selectedSkill));
		}
	}

	public void OnSkillCancel()
	{
		if (battleState == BattleState.SkillSelection)
		{
			battleState = BattleState.ActionSelection;
			battleDialogBox.EnableAttackSelector(false);
			battleDialogBox.EnableActionSelector(true);
			currentPlayerCharacterInfo.EnableBattleHud(true);
			battleDialogBox.UpdateActionSelection(currentAction);
		}
	}

	private IEnumerator HandleUnusableSkillConfirm(ActiveSkill skill)
	{
		battleState = BattleState.ShowingDialog;
		battleDialogBox.EnableAttackSelector(false);
		battleDialogBox.EnableDialogText(true);
		battleDialogBox.EnableActionSelector(false);
		yield return StartCoroutine(battleDialogBox.TypeDialog($"{currentTurnEntity.entityData.EntityName} can't use {skill.SkillData.skillName}!"));
		battleDialogBox.EnableDialogText(false);
		battleState = BattleState.SkillSelection;
		battleDialogBox.EnableAttackSelector(true);
		battleDialogBox.EnableActionSelector(true);
		battleDialogBox.UpdateSkillSelection(currentSkill);
	}

	IEnumerator HandleTargetSelection(ActiveSkill skillToUse)
	{
		currentPlayerCharacterInfo.EnableBattleHud(false);
		battleDialogBox.EnableAttackSelector(false);
		battleDialogBox.EnableDialogText(true);
		battleDialogBox.EnableActionSelector(false);
		yield return StartCoroutine(battleDialogBox.TypeDialog("Select a target!"));
		yield return waifHalf;

		targetSelectionController.gameObject.SetActive(true);
		targetSelectionController.StartSelection((targets) =>
		{
			if (targets != null && targets.Count > 0)
			{
				selectedTargets = targets;
				StartCoroutine(PerformSkillAction(currentTurnEntity, selectedTargets, skillToUse));
			}
			else
			{
				Debug.Log("Target selection cancelled.");
				battleState = BattleState.SkillSelection;

				battleDialogBox.EnableDialogText(false);
				battleDialogBox.EnableAttackSelector(true);
				battleDialogBox.UpdateSkillSelection(currentSkill);
			}
		}, skillToUse, currentTurnEntity);

		yield return new WaitUntil(() => battleState == BattleState.RunningTurn || battleState == BattleState.SkillSelection || battleState == BattleState.BattleOver);

		targetSelectionController.gameObject.SetActive(false);
	}
	IEnumerator PerformSkillAction(EntityBase sourceEntity, List<EntityBase> targetEntites, ActiveSkill skillToUse)
	{
		int totalDamagDeal = 0;
		targetSelectionController.ClearAllPreviews();
		currentPlayerCharacterInfo.EnableBattleHud(false);
		yield return StartCoroutine(battleDialogBox.TypeDialog($"{currentTurnEntity.entityData.EntityName} used {skillToUse.SkillData.skillName}"));
		yield return waifHalf;
		BattleUnit attacker = FindBattleUnitForEntity(sourceEntity);
		attacker.GetAnimator().PlayAttackAnimation(attacker.GetOffSet());
		// OnCast
		if (skillToUse.SkillData.effectsToApply.Count > 0)
		{
			var contextCast = new SkillContext
			{
				Owner = sourceEntity,
				AllTarget = targetEntites,
				HitTarget = targetEntites
			};
			yield return skillToUse.ExecuteEffect(contextCast,EffectActiveTiming.OnCast);
		}
		// OnHit
		List<EntityBase> targetsGotHit = new List<EntityBase>();
		if (skillToUse.SkillData.activeSkillType == ActiveSkillType.Damage)
		{
			foreach (var e in targetEntites)
			{
				float chance, roll;
				bool hit = CheckHit(sourceEntity, e, skillToUse, out chance, out roll);
				if (hit)
				{
					targetsGotHit.Add(e);
				}
				else
				{
					yield return StartCoroutine(ShowDialog($"{sourceEntity.entityData.EntityName} missed attack on {e.entityData.EntityName}!"));
				}
			}
		}
		else
		{
			targetsGotHit.AddRange(targetEntites);
		}	
		//DealingDamagePhase
		if (skillToUse.SkillData.activeSkillType == ActiveSkillType.Damage)
		{
			foreach (var target in targetsGotHit)
			{
				Sprite[] frames = null;
				float fps = 0f;
				bool hasVfx = vfxLib.TryGetSpellVFX(skillToUse.SkillData.skillDefinition,skillToUse.element, out frames, out fps);
				var targetUnit = FindBattleUnitForEntity(target);
				var anim = targetUnit?.GetAnimator();
				if (anim != null)
					yield return anim.PlayHitAnimation(playDefaultVfx: hasVfx, overrideFrames: frames, overideFps: fps);
				int damage = CalculateDamage(sourceEntity, skillToUse, target);
				totalDamagDeal += damage;
				var skillContext = new SkillContext
				{
					Owner = sourceEntity,
					AllTarget = targetEntites,
					HitTarget = targetsGotHit,
					totalDamageDeal = 0,
				};
				yield return skillToUse.ExecuteBeforeDealingDamageEffect(skillContext);
				yield return HandleEntityTakeDamage(target, damage, sourceEntity, skillToUse.SkillData.skillDefinition);
				yield return new WaitForSeconds(0.2f);
			}
		}
		if (skillToUse.SkillData.effectsToApply.Count > 0)
		{
			var contextHit = new SkillContext
			{
				Owner = sourceEntity,
				AllTarget = targetEntites,
				HitTarget = targetsGotHit,
				totalDamageDeal = totalDamagDeal,
			};
			yield return skillToUse.ExecuteOnDealingDamageEffect(contextHit);
		}
		if (skillToUse.SkillData.skillDefinition == SkillDefinition.Spell)
		{
			sourceEntity.ReduceMP(skillToUse.currentMPCost);
		}
		else if (skillToUse.SkillData.skillDefinition == SkillDefinition.BattleArt)
		{
			sourceEntity.ReduceSP(skillToUse.currentSPCost);
		}
		yield return new WaitForSeconds(0.5f);
		timelineManager.UpdateEntityTimeline(sourceEntity);
		UpdateTimelineUI();
		battleState = BattleState.RunningTurn;
	}

	public int CalculateDamage(EntityBase source, ActiveSkill skill, EntityBase target)
	{
		if (skill.SkillData.activeSkillType != ActiveSkillType.Damage)
		{
			return 0;
		}
		int baseDamage = Mathf.CeilToInt(skill.currentSkillDamage);
		float scalingDamage = 0f;

		foreach (var entry in skill.SkillData.scalingStatAndMutiply)
		{
			Stat stat = entry.Key;
			float multiplier = entry.Value;
			scalingDamage += source.GetFinalStat(stat) * multiplier;
		}
		float totalDamage = baseDamage + scalingDamage;
		float attackStat = 1f;
		float defenseStat = 0f;
		float kdef = 1f;
		switch (skill.SkillData.skillDefinition)
		{
			case SkillDefinition.Spell:
				attackStat = Mathf.Max(1f, source.GetFinalStat(Stat.MagicPower));
				defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.MagicalDefense));
				kdef = 1.2f;
				break;
			case SkillDefinition.BattleArt:
				attackStat = Mathf.Max(1f, source.GetFinalStat(Stat.AttackPower));
				defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.PhysicalDefense));
				kdef = 1.2f;
				break;
			case SkillDefinition.Almighty:
				attackStat = 1f;
				defenseStat = 1f;
				kdef = 1;
				break;
		}
		//float penetration = 0f;
		float atkDefRatio = attackStat / (defenseStat * kdef);
		totalDamage *= atkDefRatio;
		float damageMultiplier = ElementalChart.GetMultiplier(skill.element, target.entityData.EntityElement);
		Debug.Log($"skill Element: {skill.element}, Target Element: {target.entityData.EntityElement}, Multiplier: {damageMultiplier}");
		totalDamage *= damageMultiplier;
		float rangePosMul = GetRangePositionMultiplier(source, skill, target);
		totalDamage *= rangePosMul;
		if (rangePosMul != 1f)
			Debug.Log($"[RangePenalty] {source.entityData.EntityName} -> {target.entityData.EntityName} (back-row): x{rangePosMul}");


		var finalDamage = ApplyDamageModifiers(source, target, Mathf.RoundToInt(totalDamage));
		finalDamage = ApplyDefenseReduction(target, finalDamage);
		return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
	}

	private int ApplyDamageModifiers(EntityBase source, EntityBase target, int baseDamage)
	{
		float outgoingMult = 1f;
		float incomingMult = 1f;

		var sourceEffects = source?.GetAllEffect();
		var targetEffects = target?.GetAllEffect();
		if (sourceEffects != null)
		{
			foreach (var e in sourceEffects)
			{
				if (e is IModifiOutcomingDamage m)
				{
					float k = Mathf.Max(0f, m.GetOutcomingDamageModifier(source));
					if (k == 0f) { outgoingMult = 0f; break; }
					outgoingMult *= k;
				}
			}
		}
		if (targetEffects != null && outgoingMult > 0f)
		{
			foreach (var e in targetEffects)
			{
				if (e is IModifiIncomingDamage m)
				{
					float k = Mathf.Max(0f, m.GetInComingDamageModifier(target, source, baseDamage, this));
					if (k == 0f) { incomingMult = 0f; break; }
					incomingMult *= k;
				}
			}
		}
		Debug.Log("Outgoing Multiplier: " + outgoingMult + ", Incoming Multiplier: " + incomingMult);
		int modified = Mathf.RoundToInt(baseDamage * outgoingMult * incomingMult);
		Debug.Log("Base Damage: " + baseDamage + ", Modified Damage: " + modified);
		return Mathf.Max(0, modified);
	}

	public void UpdateUnitHealth(EntityBase entity)
	{

		foreach (var unit in playerBattleUnits)
		{
			if (unit.character == entity)
			{
				bool wasAlive = unit.IsAlive();
				unit.UpdateHP();
				break;
			}
		}

		foreach (var unit in monsterBattleUnits)
		{
			if (unit.character == entity)
			{
				bool wasAlive = unit.IsAlive();
				unit.UpdateHP();
				break;
			}
		}
	}
	private bool CheckHit(EntityBase source, EntityBase target, ActiveSkill skill, out float finalHitChance, out float roll)
	{
		if (skill.SkillData.skillDefinition == SkillDefinition.Almighty)
		{
			finalHitChance = 1f;
			roll = 0f;
			return true;
		}
		float baseFromSkill = Mathf.Clamp01(skill.hitChance <= 0f ? 1f : skill.hitChance);
		float acc = Mathf.Max(1f, source.GetFinalStat(Stat.Accuracy));
		float eva = Mathf.Max(1f, target.GetFinalStat(Stat.Evasion));
		const float k = 0.6f;
		float ratio = acc / (acc + eva * k);
		const float gamma = 0.85f;
		ratio = Mathf.Pow(ratio, gamma);

		finalHitChance = baseFromSkill * ratio;
		finalHitChance = Mathf.Clamp(finalHitChance, MIN_HIT, MAX_HIT);

		roll = Random.value;
		return roll <= finalHitChance;
	}


	public bool UpdateUnitState(EntityBase entity)
	{
		var unit = FindBattleUnitForEntity(entity);
		if (unit != null && !unit.IsAlive())
		{
			timelineManager.RemoveDeadEntityFromTimeline(entity);
			UpdateTimelineUI();
			return CheckBattleEndCondition();
		}
		return false;
	}

	private void RemoveDeadUnitsFromTimeline()
	{
		var deadEntities = ListPool<EntityBase>.Get();

		foreach (var unit in playerBattleUnits)
			if (unit?.character != null && !unit.IsAlive())
				deadEntities.Add(unit.character);

		foreach (var unit in monsterBattleUnits)
			if (unit?.character != null && !unit.IsAlive())
				deadEntities.Add(unit.character);

		if (deadEntities.Count > 0)
		{
			timelineManager.RemoveDeadEntitiesFromTimeline(deadEntities);
			UpdateTimelineUI();
		}

		ListPool<EntityBase>.Release(deadEntities);
	}
	public bool IsWithinProtectRange(EntityBase protector, EntityBase target, ProtectRangeType range)
	{
		if (range == ProtectRangeType.All) return true;
		GridPosition protectorPos = GetEntityPosition(protector);
		GridPosition targetPos = GetEntityPosition(target);
		if (protectorPos == null || targetPos == null) return false;
		switch (range)
		{
			case ProtectRangeType.Adjacent:
				return IsAdjacent(protectorPos, targetPos);
			case ProtectRangeType.Horizontal:
				return IsHorizontal(protectorPos, targetPos);
			case ProtectRangeType.Vertical:
				return IsVertical(protectorPos, targetPos);

			default:
				return true;
		}
	}
	private GridPosition GetEntityPosition(EntityBase entity)
	{
		var playerSlot = playerParty.partySlots.Find(s => s.entity == entity);
		if (playerSlot != null)
		{
			return playerSlot.position;
		}

		var monsterSlot = monsterParty.partySlots.Find(s => s.entity == entity);
		if (monsterSlot != null)
		{
			return monsterSlot.position;
		}

		return null;
	}
	private bool IsAdjacent(GridPosition pos1, GridPosition pos2)
	{
		int deltaX = Mathf.Abs(pos1.x - pos2.x);
		int deltaY = Mathf.Abs(pos1.y - pos2.y);

		return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
	}
	private bool IsHorizontal(GridPosition pos1, GridPosition pos2)
	{
		return pos1.y == pos2.y && Mathf.Abs(pos1.x - pos2.x) == 1;
	}
	private bool IsVertical(GridPosition pos1, GridPosition pos2)
	{
		return pos1.x == pos2.x && Mathf.Abs(pos1.y - pos2.y) == 1;
	}
	//end battle
	private bool CheckBattleEndCondition()
	{
		bool allMonsterDefeated = !monsterBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());
		bool allAllyDefeated = !playerBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());

		if (allAllyDefeated)
		{
			battleOver = true;
			battleDialogBox.EnableDialogText(false);
			HandlePlayerLose();
			return true;
		}
		if (allMonsterDefeated)
		{
			battleOver = true;
			battleDialogBox.EnableDialogText(false);
			HandlePlayerWin();
			return true;
		}
		return false;
	}

	private void HandlePlayerLose()
	{
		ResetEffectOnEntities();
		CleanupBattle();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnGameLose, new object[] { currentMonsterInteractable }));
	}
	private void HandlePlayerWin()
	{
		ResetEffectOnEntities();	
		Debug.Log("Exp caculation");
		float totalExp = 0f;
		foreach (var monsterSlot in monsterParty.partySlots)
		{
			if (monsterSlot.entity is MonsterCharacter monster)
			{
				totalExp += monster.TotalExpToAward;
			}
		}
		List<PlayerCharacter> activeCharacters = playerParty.partySlots
		.Where(s => s.entity is PlayerCharacter pc && pc.GetCurrentHP() > 0)
		.Select(s => s.entity as PlayerCharacter).Distinct()
		.ToList();
		List<int> expGainedPerMember = new List<int>();
		if (activeCharacters.Count > 0)
		{
			float expPerCharacter = totalExp / activeCharacters.Count;
			foreach (var character in activeCharacters)
			{
				expGainedPerMember.Add(Mathf.RoundToInt(expPerCharacter));
			}
		}
		expDistributionController.ShowExpDistribution(expGainedPerMember);

	}
	public void HandleAfterMatch()
	{
		if (currentBattleType == BattleType.RoamingMoster)
		{
			currentMonsterInteractable.Defeated();
		}
		CleanupBattle();
	}
	public IEnumerator ShowDialog(string dialog)
	{
		battleDialogBox.EnableDialogText(true);
		yield return StartCoroutine(battleDialogBox.TypeDialog(dialog));
		yield return waifHalf;
	}

	private IEnumerator TriggerBeforeDealingDamage(EntityBase attacker, EntityBase target, DamageContext ctx)
	{
		yield return attacker.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBeforeDealingDamage, target, ctx);
		var allEffect = attacker.GetAllEffect();
		if (allEffect != null)
		{
			for (int i = 0; i < allEffect.Count; i++)
			{
				var eff = allEffect[i];
				if (eff is IBeforeDealingDamage hook)
					yield return hook.OnBeforeDealingDamage(ctx);
			}
		}
	}

	private IEnumerator TriggerOnTakingDamage(EntityBase defender, DamageContext ctx)
	{
		yield return defender.PassiveSkillRunner.Trigger(PassiveTrigger.OnTakingDamage, defender, ctx);
		var allEffect = defender.GetAllEffect();
		if (allEffect != null)
		{
			for (int i = 0; i < allEffect.Count; i++)
			{
				var eff = allEffect[i];
				if (eff is IOnTakingDamage hook)
				{
					yield return hook.OnTakingDamage(ctx);
				}
			}
		}

		yield return defender.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnTakingDamage, defender, ctx);
	}
	private IEnumerator TriggerDealingDamage(EntityBase attacker, EntityBase target, DamageContext ctx)
	{
		yield return attacker.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnDealingDamage, target, ctx);
		yield return attacker.PassiveSkillRunner.Trigger(PassiveTrigger.OnDealingDamage, attacker, ctx);
		var allEffect = attacker.GetAllEffect();
		if(allEffect != null)
		{
			for(int i = 0;i < allEffect.Count; i++)
			{
				var eff = allEffect[i]; 
				if(eff is IOnDealingDamage hook)
				{
					yield return hook.OnDealingDamage(ctx);
				}
			}
		}
	}

	private IEnumerator TriggerBeforeTakingDamage(EntityBase defender, DamageContext ctx)
	{
		yield return defender.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBeforeTakingDamage, defender, ctx);
		yield return defender.PassiveSkillRunner.Trigger(PassiveTrigger.OnBeforeTakingDamage, defender, ctx);
		var allEffect = defender.GetAllEffect();
		if (allEffect != null)
		{
			for (int i = 0; i < allEffect.Count; i++)
			{
				var eff = allEffect[i];
				if (eff is IBeforeTakingDamage hook)
					yield return hook.OnBeforeTakingDamage(ctx);
			}
		}
	}

	#region helper
	//public IEnumerator ExecuteWithTimingFilter(ActiveSkill skill, SkillContext context, EffectActiveTiming timing)
	//{
	//	if (skill.SkillData.effectsToApply == null || skill.SkillData.effectsToApply.Count == 0) yield break;
	//	var backUp = skill.SkillData.effectsToApply;
	//	var filterdEffects = backUp.Where(e => e.effectData != null && e.effectData.timing == timing);
	//	if (filterdEffects.Count() == 0) yield break;

	//	foreach (var e in filterdEffects)
	//	{
	//		IEnumerable<EntityBase> recipients = e.effectData.AppliesTo switch
	//		{
	//			TargetType.Self => new[] { context.Owner },
	//			TargetType.Ally => (e.effectData.requiredHit ? context.HitTarget : context.AllTarget),
	//			TargetType.Enemy => (e.effectData.requiredHit ? context.HitTarget : context.AllTarget),
	//			_ => context.AllTarget
	//		};

	//		foreach (var entity in recipients)
	//		{
	//			if (Random.value > Mathf.Clamp01(e.procChance))
	//			{
	//				Debug.Log("Failed To Apply Effect");
	//				continue;
	//			}
	//			var runtimeEffect = e.effectData.CreateRuntimeEffect(context.Owner, entity, e.turnDuration);
	//			if (e.effectData.isInstantEffect)
	//			{
	//				Debug.Log("Successful Apllying Instant Effect");
	//				yield return entity.TriggerEffectDirectly(runtimeEffect);
	//			}
	//			else
	//			{
	//				Debug.Log("Successful Apllying Effect");
	//				yield return entity.AddEffect(runtimeEffect);
	//			}
	//		}
	//	}
	//}
	private void ResetEffectOnEntities()
	{
		if (allEntities == null)
		{
			return;
		}
		else
		{
			Debug.Log("Resetting Effect");
			foreach (var entity in allEntities)
			{
				entity.ResetEffectAfterBattle();
				entity.PassiveSkillRunner.ResetPassiveEffect();
			}
		}
	}
	private bool IsEntityAlive(EntityBase e)
	{
		var u = FindBattleUnitForEntity(e);
		return u != null && u.IsAlive();
	}
	private bool AreAllies(EntityBase a, EntityBase b)
	{
		if (a == null || b == null) return false;
		bool aIsPlayer = playerParty.partySlots.Exists(s => s.entity == a);
		bool bIsPlayer = playerParty.partySlots.Exists(s => s.entity == b);
		bool aIsMonster = monsterParty.partySlots.Exists(s => s.entity == a);
		bool bIsMonster = monsterParty.partySlots.Exists(s => s.entity == b);
		return (aIsPlayer && bIsPlayer) || (aIsMonster && bIsMonster);
	}
	private IEnumerable<EntityBase> GetAlliesOf(EntityBase unit)
	{
		if (unit == null) yield break;

		if (playerParty.partySlots.Exists(s => s.entity == unit))
		{
			foreach (var s in playerParty.partySlots)
				if (s.entity != null && s.entity != unit && IsEntityAlive(s.entity))
					yield return s.entity;
			yield break;
		}
		if (monsterParty.partySlots.Exists(s => s.entity == unit))
		{
			foreach (var s in monsterParty.partySlots)
				if (s.entity != null && s.entity != unit && IsEntityAlive(s.entity))
					yield return s.entity;
		}
	}

	private IEnumerator ExecuteForcedAction(EntityBase actor, TurnDirective dir)
	{
		switch (dir.ForcedAction)
		{
			case ForcedActionKind.BasicAttack:
				{
					var target = dir.ForcedTarget ?? PickDefaultTargetForBasicAttack(actor);
					if (target != null) yield return StartCoroutine(ForceBasicAttack(actor, target));
					else yield return StartCoroutine(ShowDialog($"{actor.entityData.EntityName} fails to act."));
					yield break;
				}
			default:
				yield break;
		}
	}

	private EntityBase PickDefaultTargetForBasicAttack(EntityBase actor)
	{
		var playerTeam = playerParty.GetAllEntitiesInParty();
		var monsterTeam = monsterParty.GetAllEntitiesInParty();

		bool actorIsPlayer = playerTeam.Contains(actor);

		var sameSide = actorIsPlayer ? playerTeam : monsterTeam;

		return sameSide.FirstOrDefault(e => e != null && e != actor && e.GetCurrentHP() > 0);
	}


	private IEnumerator ForceBasicAttack(EntityBase attacker, EntityBase target)
	{
		int damage = CalculateDamage(attacker, basicAttack, target);
		yield return StartCoroutine(ApplyEffectDamage(target, damage, attacker, $"by {attacker.entityData.EntityName} attack"));
	}

	private bool IsSkillUseAllowed(EntityBase actor, ActiveSkillData skill)
	{
		var dir = actor.TurnControl;	

		if (dir.BannedSkillDefs.Contains(skill.skillDefinition))
		{
			return false;
		}
		return true;
	}
	private int NextSelectableSkillIndex(int start, int direction)
	{
		int n = currentTurnEntity.usableSkills.Count;
		if (n == 0) return -1;

		int idx = start;
		for (int step = 0; step < n; step++)
		{
			idx = (idx + direction + n) % n;
			var s = currentTurnEntity.usableSkills[idx];
			if (IsSkillUseAllowed(currentTurnEntity, s.SkillData))
				return idx;
		}
		return -1;
	}

	public void CleanupBattle()
	{
		Debug.Log("[BattleSystem] CleanupBattle called");

		StopAllCoroutines();
		battleCoroutine = null;
		timelineManager?.Initialize(new List<BattleUnit>());

		currentPlayerCharacterInfo?.EnableBattleHud(false);
		battleDialogBox?.EnableDialogText(false);
		battleDialogBox?.EnableActionSelector(false);
		targetSelectionController?.gameObject.SetActive(false);

		if (allEntities != null)
		{
			foreach (var e in allEntities)
			{
				e?.ResetEffectAfterBattle();
				e?.PassiveSkillRunner?.ResetPassiveEffect();
			}
			allEntities.Clear();
		}

		entityDefenseStates.Clear();
		selectedTargets?.Clear();
		currentTurnEntity = null;
		selectedSkill = null;
		currentMonsterInteractable = null;
		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	private bool isBackRow(GridPosition pos)
	{
		return pos != null && pos.x == 1;
	}
	private bool IsTargetInBackRow(EntityBase target)
	{
		var pos = GetEntityPosition(target);
		return isBackRow(pos);
	}

	private bool GetEntityWeaponType(EntityBase entity, out WeaponType weaponType)
	{
		weaponType = default;
		if (entity == null) return false;

		var w = entity.weapon;
		if (w == null) return false;

		var wb = w.WeaponBaseData;
		if (wb == null)
		{
			return false;
		}

		weaponType = wb.weaponType;
		return true;
	}

	private float GetRangePositionMultiplier(EntityBase source, ActiveSkill skill, EntityBase target)
	{
		if (skill.SkillData.skillRange == SkillRange.AllTarget)
			return 1f;
		if (skill.SkillData.skillDefinition == SkillDefinition.Spell || skill.SkillData.skillDefinition == SkillDefinition.Almighty)
		{
			return 1f;
		}
		if (!IsTargetInBackRow(target))
		{
			return 1f;
		}
		if(GetEntityWeaponType(source, out WeaponType wt))
		{
			if (wt == WeaponType.Bow || wt == WeaponType.Spear)
			{
				return 1f;
			}
		}
		return Mathf.Clamp01(backrowDamageReduction);
	}
	private bool IsDefending(EntityBase entity)
	{
		return entityDefenseStates.TryGetValue(entity, out var state) && state == DefenseState.Defending;
	}
	private bool TryRollCritical(EntityBase attacker, EntityBase target, SkillDefinition skill, out float critMul)
	{
		if(skill != SkillDefinition.BattleArt)
		{
			critMul = 1f;
			return false;
		}
		float chance = baseCritChance;
		critMul = baseCritMultiplier;

		var attackerEffects = attacker.GetAllEffect();
		if(attackerEffects != null)
		{
			foreach(var e in attackerEffects)
			{
				if(e is IModifyCritChance c) chance = Mathf.Clamp01(c.ModifyCritChance(attacker,target, chance));
				if(e is IModifyCritDamage d) critMul = d.ModifyCritDamage(attacker, target, critMul);
			}
		}
		return Random.value <= chance;
	}

	private void ResolveCrit(DamageContext ctx)
	{
		if (ctx.CritDecided)
		{
			if(ctx.IsCritical && ctx.CritForce == CritFocedType.Sleep)
			{
				return;
			}
			if(ctx.IsCritical && IsDefending(ctx.Target))
			{
				ctx.IsCritical = false;
				ctx.CritMultiplier = 1f;
				return;
			}
			return;
		}
		if (IsDefending(ctx.Target))
		{
			ctx.CritDecided = true;
			ctx.IsCritical = false;
			ctx.CritMultiplier = 1f;
			return;
		}
		if(TryRollCritical(ctx.Source,ctx.Target,ctx.Origin,out var cm))
		{
			ctx.IsCritical = true;
			ctx.CritMultiplier = Mathf.Max(1f, cm);
		}
		ctx.CritDecided = true;
	}
		#endregion
}