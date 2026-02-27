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
	Defend,
	Switch
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
	public VfxLib vfxLib;
	[SerializeField] private ExpDistributionController expDistributionController;
	[SerializeField] public BattleState battleState;
	public EntityBase currentTurnEntity;
	public void SetCurrentTurnEntity(EntityBase entity) { currentTurnEntity = entity; }
	[SerializeField] GameObject battleSystem;
	[SerializeField] public BattleUIController uiController;
	[SerializeField] public BattleStateMachine stateMachine;
	public SwitchPositionController switchPositionController;
	public PlayerParty playerParty;
	public MonsterParty monsterParty;
	public bool battleOver;
	private List<EntityBase> allEntities;
	[SerializeField] List<BattleUnit> playerBattleUnits;
	[SerializeField] List<BattleUnit> monsterBattleUnits;
	public TimelineManager timelineManager;
	public TargetSelectionController targetSelectionController;
	public ActiveSkill selectedSkill;
	public List<EntityBase> selectedTargets;
	private Coroutine battleCoroutine;
	public IMonsterInteracable currentMonsterInteractable;
	[SerializeField] private ActiveSkillData basicAttackSkillData;
	public ActiveSkill basicAttack;
	[SerializeField] private BattleInputHandler inputHandler;
	public BattleActionExecutor actionExecutor;
	private BattleLifecycleManager lifecycleManager;

	private void Awake()
	{
		inputHandler = gameObject.GetComponent<BattleInputHandler>();
		if (inputHandler == null) inputHandler = FindFirstObjectByType<BattleInputHandler>();
		if (inputHandler == null) inputHandler = gameObject.AddComponent<BattleInputHandler>();
		inputHandler.Init(this);

		actionExecutor = gameObject.GetComponent<BattleActionExecutor>();
		if (actionExecutor == null) actionExecutor = gameObject.AddComponent<BattleActionExecutor>();
		actionExecutor.Init(this, defenseStateDamageReduction);

		basicAttack = new ActiveSkill(basicAttackSkillData);
		if (stateMachine == null) stateMachine = GetComponent<BattleStateMachine>();
		lifecycleManager = new BattleLifecycleManager(this);
		Hide();

	}
	private void Show()
	{
		uiController.Show();
	}
	private void Hide()
	{
		uiController.Hide();
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
	public static readonly WaitForSeconds waifHalf = new WaitForSeconds(0.5f);
	private static readonly WaitForSeconds waitOne = new WaitForSeconds(1f);

	public BattleState GetBattleState() => battleState;
	public BattleUnit GetBattleUnitAt(int index) => playerBattleUnits[index];

	public GridPosition GetPositionByUnitIndex(int index)
	{
		foreach (var kv in positionToBattleUnitIndex)
			if (kv.Value == index) return kv.Key;

		return new GridPosition(-999, -999);
	}
	public void StartBattle(BattleType batteType)
	{
		Show();
		currentBattleType = batteType;
		battleOver = false;
		battleState = BattleState.Start;
		battleSystem.SetActive(true);
		lifecycleManager.SetUpBattle(playerBattleUnits, monsterBattleUnits, positionToBattleUnitIndex);
		allEntities = new List<EntityBase>();
		allEntities.AddRange(playerParty.GetAllEntitiesInParty());
		allEntities.AddRange(monsterParty.GetAllEntitiesInParty());

		foreach (var entity in allEntities)
		{
			entityDefenseStates[entity] = DefenseState.None;
			entity.InitializePassiveRunner(this);
			entity.EquipmentEffectRunner?.UpdateBattleSystem(this);
		}
		uiController.battleDialogBox.EnableDialogText(true);
		if (battleCoroutine != null)
		{
			StopCoroutine(battleCoroutine);
			battleCoroutine = null;
		}

		uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
		uiController.battleDialogBox.EnableActionSelector(false);
		UpdateTimelineUI();
		
		stateMachine.ChangeState(new BattleStartState(this, stateMachine));
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
	public IEnumerator ShowInitialDialog()
	{
		battleState = BattleState.ShowingDialog;
		if (currentBattleType == BattleType.RoamingMoster)
		{
			yield return StartCoroutine(uiController.TypeDialog("Player run into wild monster"));
		}
		else
		{
			yield return StartCoroutine(ShowDialog("Player encountered Boss"));
		}
	}
	public void ResetDefenseStatePublic(EntityBase entity)
	{
		if (entityDefenseStates.ContainsKey(entity))
		{
			entityDefenseStates[entity] = DefenseState.None;
		}
	}
	public IEnumerator HandleEntityTakeDamage(
		EntityBase target, int finalDamage, EntityBase source,
		SkillDefinition origin, bool isEffectDamage = false, string flavor = null)
	{
		yield return StartCoroutine(actionExecutor.HandleEntityTakeDamage(target, finalDamage, source, origin, isEffectDamage, flavor));
	}

	public IEnumerator HandleEntityGotHeal(HealingContext healingContext)
	{
		yield return StartCoroutine(actionExecutor.HandleEntityGotHeal(healingContext));
	}

	public IEnumerator ApplyEffectDamage(EntityBase target, int amount, EntityBase source, string reason = null)
	{
		yield return StartCoroutine(actionExecutor.ApplyEffectDamage(target, amount, source, reason));
	}
	public void NavigateActionSelection(int direction) => inputHandler.NavigateActionSelection(direction);
	public void NavigateSkillSelection(int direction) => inputHandler.NavigateSkillSelection(direction);

	public void UpdateTimelineUI()
	{
		var upcomingEntities = timelineManager.PeekNextEntitiesWithCurrent(currentTurnEntity, 5);
		uiController.UpdateTimelineUI(upcomingEntities);
	}
	public BattleUnit FindBattleUnitForEntityPublic(EntityBase entity)
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

	public IEnumerator HandlePlayerTurnPublic(EntityBase entity)
	{
		uiController.battleDialogBox.EnableDialogText(false);
		uiController.currentPlayerCharacterInfo.EnableBattleHud(true);
		uiController.currentPlayerCharacterInfo.SetData(entity);
		battleState = BattleState.ActionSelection;
		uiController.battleDialogBox.EnableActionSelector(true);
		inputHandler.ResetActionSelection();
		yield return new WaitUntil(() => battleState == BattleState.RunningTurn || battleState == BattleState.BattleOver);

		uiController.battleDialogBox.EnableActionSelector(false);
	}
	public IEnumerator HandleMonsterTurnPublic(EntityBase entity)
	{
		battleState = BattleState.RunningTurn;

		yield return waifHalf;
		var decision = BattleAIController.ChooseAction(entity, this);

		if (decision.IsValid)
		{
			yield return StartCoroutine(PerformSkillAction(entity, decision.Targets, decision.Skill));
		}
		else
		{
			Debug.Log($"{entity.entityData.EntityName} couldn't decide on an action!");
			battleState = BattleState.Start;
		}
	}



	public void OnActionSelect(int actionIndex) => inputHandler.OnActionSelect(actionIndex);
	public void OnActionConfirm() => inputHandler.OnActionConfirm();
	public void ExecuteSwitchPosition(int indexA, int indexB) => inputHandler.ExecuteSwitchPosition(indexA, indexB);
	public void SyncPlayerBattleUnitsFromPartySlots()
	{
		for (int i = 0; i < playerBattleUnits.Count; i++)
		{
			var pos = GetPositionByUnitIndex(i);
			var entity = playerParty.GetEntityAtPosition(pos);

			var unit = playerBattleUnits[i];
			unit.character = entity;

			unit.gameObject.SetActive(entity != null);

			unit.SetUp();
		}
	}
	public void OnSkillSelect(int skillIndex) => inputHandler.OnSkillSelect(skillIndex);
	public void OnSkillConfirm() => inputHandler.OnSkillConfirm();
	public void OnSkillCancel() => inputHandler.OnSkillCancel();

	public void SetEntityDefenseState(EntityBase entity, DefenseState state)
	{
		entityDefenseStates[entity] = state;
	}
	public IEnumerator PerformSkillAction(EntityBase sourceEntity, List<EntityBase> targetEntites, ActiveSkill skillToUse)
	{
		yield return StartCoroutine(actionExecutor.PerformSkillAction(sourceEntity, targetEntites, skillToUse));
	}

	public int CalculateDamage(EntityBase source, ActiveSkill skill, EntityBase target)
	{
		return actionExecutor.CalculateDamage(source, skill, target);
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

	public bool UpdateUnitState(EntityBase entity)
	{
		var unit = FindBattleUnitForEntityPublic(entity);
		if (unit != null && !unit.IsAlive())
		{
			timelineManager.RemoveDeadEntityFromTimeline(entity);
			UpdateTimelineUI();
			return CheckBattleEndConditionPublic();
		}
		return false;
	}

	public void RemoveDeadUnitsFromTimelinePublic()
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
	//end battle
	public bool CheckBattleEndConditionPublic()
	{
		return lifecycleManager.CheckBattleEndCondition(playerBattleUnits, monsterBattleUnits);
	}

	public void HandleAfterMatch()
	{
		lifecycleManager.HandleAfterMatch();
	}
	public IEnumerator ShowDialog(string dialog)
	{
		uiController.battleDialogBox.EnableDialogText(true);
		yield return StartCoroutine(uiController.battleDialogBox.TypeDialog(dialog));
		yield return waifHalf;
	}

	public int CalculateHealing(EntityBase healer, ActiveSkill skill, EntityBase target)
	{
		return actionExecutor.CalculateHealing(healer, skill, target);
	}
	#region helper
	public bool IsEntityAlivePublic(EntityBase e)
	{
		var u = FindBattleUnitForEntityPublic(e);
		return u != null && u.IsAlive();
	}

	public IEnumerator ExecuteForcedActionPublic(EntityBase actor, TurnDirective dir)
	{
		switch (dir.ForcedAction)
		{
			case ForcedActionKind.BasicAttack:
				{
					var target = dir.ForcedTarget ?? BattleAIController.PickDefaultTargetForBasicAttack(actor, playerParty, monsterParty);
					if (target != null) yield return StartCoroutine(actionExecutor.ForceBasicAttack(actor, target));
					else yield return StartCoroutine(ShowDialog($"{actor.entityData.EntityName} fails to act."));
					yield break;
				}
			default:
				yield break;
		}
	}



	public void CleanupBattle()
	{
		lifecycleManager.CleanupBattle(allEntities);
		entityDefenseStates.Clear();
		Hide();
	}

	public bool IsDefending(EntityBase entity)
	{
		return entityDefenseStates.TryGetValue(entity, out var state) && state == DefenseState.Defending;
	}
	
	public void ApplySkillModifiersPreview(ActiveSkill skill, ref DamageContext ctx)
	{
		var mods = skill?.SkillData?.modifiers;
		if (mods == null) return;
		for(int i = 0;i< mods.Count; i++)
		{
			var m = mods[i];
			if (m == null) continue;
			m.ModifyPreview(ref ctx);
		}
	}
	public void ApplySkillHealingModifiersPreview(ActiveSkill skill, ref HealingContext ctx)
	{
		var mods = skill?.SkillData?.modifiers;
		if (mods == null) return;

		for(int i = 0;i < mods.Count; i++)
		{
			var m = mods[i];
			if (m == null) continue;
			m.ModifyHealingPreview(ref ctx);
		}
	}
		#endregion
}
