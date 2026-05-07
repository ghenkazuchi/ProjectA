using HaKien;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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


	public const float baseCritChance = 0.1f;
	public const float baseCritMultiplier = 1.5f;

	public BattleType currentBattleType;
	public VfxLib vfxLib;
	[SerializeField] private ExpDistributionController expDistributionController;
	public ExpDistributionController ExpDistribution => expDistributionController;
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
	[SerializeField] private List<BattleUnit> playerBattleUnits;
	[SerializeField] private List<BattleUnit> monsterBattleUnits;
	public BattleUnitRegistry UnitRegistry { get; private set; }
	public TimelineManager timelineManager;
	public TargetSelectionController targetSelectionController;
	public ActiveSkill selectedSkill;
	public List<EntityBase> selectedTargets;
	private Coroutine battleCoroutine;
	private int battleItemEffectUseCount;
	private EntityBase turnOwner;
	private bool currentTurnActionTaken;
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
		UnitRegistry = new BattleUnitRegistry(this, playerBattleUnits, monsterBattleUnits);
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
	//Setting up

	private Dictionary<EntityBase, DefenseState> entityDefenseStates = new Dictionary<EntityBase, DefenseState>();

	[SerializeField] private float defenseStateDamageReduction = 0.1f;
	public static readonly WaitForSeconds waitHalf = new WaitForSeconds(0.5f);

	public BattleState GetBattleState() => battleState;
	public BattleUnit GetBattleUnitAt(int index) => UnitRegistry.GetBattleUnitAt(index);

	public GridPosition GetPositionByUnitIndex(int index) => UnitRegistry.GetPositionByUnitIndex(index);
	public int BattleItemEffectUseCount => battleItemEffectUseCount;
	public void RegisterBattleItemEffectUse(Item item)
	{
		if (item == null || item.itemBaseData == null)
		{
			return;
		}

		battleItemEffectUseCount++;
	}
	public void BeginTurn(EntityBase entity)
	{
		turnOwner = entity;
		currentTurnActionTaken = false;
	}

	public void MarkTurnActionTaken(EntityBase entity = null)
	{
		if (turnOwner == null)
		{
			return;
		}

		if (entity == null || entity == turnOwner)
		{
			currentTurnActionTaken = true;
		}
	}

	public bool DidEntityTakeActionThisTurn(EntityBase entity)
	{
		return entity != null && entity == turnOwner && currentTurnActionTaken;
	}

	public int CurrentTimelineRound => timelineManager != null ? timelineManager.CurrentRoundNumber : 0;
	public void StartBattle(BattleType batteType)
	{
		Show();
		currentBattleType = batteType;
		battleOver = false;
		battleState = BattleState.Start;
		battleItemEffectUseCount = 0;
		battleSystem.SetActive(true);
		lifecycleManager.SetUpBattle(UnitRegistry.PlayerBattleUnits, UnitRegistry.MonsterBattleUnits, UnitRegistry.GetPositionMap());
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
			if (entity == null || entity.GetCurrentHP() <= 0)
			{
				continue;
			}

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
		SkillDefinition origin, string skillName = "", bool isEffectDamage = false, string flavor = null)
	{
		yield return StartCoroutine(actionExecutor.HandleEntityTakeDamage(target, finalDamage, source, origin, skillName, isEffectDamage, flavor));
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
	public BattleUnit FindBattleUnitForEntityPublic(EntityBase entity) => UnitRegistry.FindUnit(entity);

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

		yield return waitHalf;
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

	
	//public void OnActionSelect(int actionIndex) => inputHandler.OnActionSelect(actionIndex);
	//public void OnActionConfirm() => inputHandler.OnActionConfirm();
	public void ExecuteSwitchPosition(int indexA, int indexB) => inputHandler.ExecuteSwitchPosition(indexA, indexB);
	public void SyncPlayerBattleUnitsFromPartySlots() => UnitRegistry.SyncPlayerBattleUnitsFromPartySlots();
	//public void OnSkillSelect(int skillIndex) => inputHandler.OnSkillSelect(skillIndex);
	//public void OnSkillConfirm() => inputHandler.OnSkillConfirm();
	//public void OnSkillCancel() => inputHandler.OnSkillCancel();

	public void SetEntityDefenseState(EntityBase entity, DefenseState state)
	{
		entityDefenseStates[entity] = state;
	}
	public IEnumerator PerformSkillAction(EntityBase sourceEntity, List<EntityBase> targetEntites, ActiveSkill skillToUse)
	{
		MarkTurnActionTaken(sourceEntity);
		yield return StartCoroutine(actionExecutor.PerformSkillAction(sourceEntity, targetEntites, skillToUse));
	}

	public int CalculateDamage(EntityBase source, ActiveSkill skill, EntityBase target)
	{
		return actionExecutor.CalculateDamage(source, skill, target);
	}

	public void UpdateUnitHealth(EntityBase entity) => UnitRegistry.UpdateUnitHealth(entity);
	public bool UpdateUnitState(EntityBase entity) => UnitRegistry.UpdateUnitState(entity);
	public void RemoveDeadUnitsFromTimelinePublic() => UnitRegistry.RemoveDeadUnitsFromTimeline();
	//end battle
	public bool CheckBattleEndConditionPublic()
	{
		return lifecycleManager.CheckBattleEndCondition(UnitRegistry.PlayerBattleUnits, UnitRegistry.MonsterBattleUnits);
	}

	public void HandleAfterMatch()
	{
		lifecycleManager.HandleAfterMatch();
	}
	public IEnumerator ShowDialog(string dialog)
	{
		uiController.battleDialogBox.EnableDialogText(true);
		yield return StartCoroutine(uiController.battleDialogBox.TypeDialog(dialog));
		yield return waitHalf;
	}

	public int CalculateHealing(EntityBase healer, ActiveSkill skill, EntityBase target)
	{
		return actionExecutor.CalculateHealing(healer, skill, target);
	}
	#region helper
	public bool IsEntityAlivePublic(EntityBase e) => UnitRegistry.IsEntityAlive(e);

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
