using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaKien;

public class BattleInputHandler : MonoBehaviour
{
    [SerializeField] private BattleSystem sys;
    [SerializeField] private BattleDialogBox battleDialogBox;

    private int _currentAction;
    private int currentAction
    {
        get => _currentAction;
        set
        {
            if (value != _currentAction)
                Debug.Log($"[InputHandler] currentAction: {_currentAction} -> {value}\n{System.Environment.StackTrace}");
            _currentAction = value;
        }
    }
    private int currentSkill;
    private ActiveSkill selectedSkill;
    private BattleState cancelTargetReturnState;

    public void Init(BattleSystem system)
    {
        sys = system;
    }

    private void Awake()
    {
        if (sys == null) sys = GetComponent<BattleSystem>();
        if (battleDialogBox == null) battleDialogBox = FindFirstObjectByType<BattleDialogBox>();

        // Always log how many instances exist
        var allHandlers = FindObjectsByType<BattleInputHandler>(FindObjectsSortMode.None);
        Debug.Log($"[InputHandler] Awake on '{gameObject.name}', total instances found: {allHandlers.Length}");
        foreach (var h in allHandlers)
        {
            Debug.Log($"  - '{h.gameObject.name}' enabled={h.enabled}");
        }
    }

    private void Update()
    {
        if (GameController.Instance.currentState != GameState.Battle) return;
        if (battleDialogBox != null && battleDialogBox.IsDialogTyping) return;
        if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsInputBlocked) return;

        BattleState currentState = sys.battleState;

        if (currentState == BattleState.ActionSelection)
        {
            HandleActionSelectionInput();
        }
        else if (currentState == BattleState.SkillSelection)
        {
            HandleSkillSelectionInput();
        }
    }

	private void HandleActionSelectionInput()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			NavigateActionSelection(1);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			NavigateActionSelection(-1);
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			OnActionConfirm();
		}
	}

	private void HandleSkillSelectionInput()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			NavigateSkillSelection(1);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			NavigateSkillSelection(-1);
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			OnSkillConfirm();
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			// Block backing out if there is an active tutorial restriction
			if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.HasActiveRestriction)
				return;
				
			OnSkillCancel();
		}
	}

    public void NavigateActionSelection(int direction)
    {
        if (sys.battleState != BattleState.ActionSelection) return;

        currentAction += direction;
        if (currentAction >= sys.uiController.battleDialogBox.actionTexts.Count) currentAction = 0;
        if (currentAction < 0) currentAction = sys.uiController.battleDialogBox.actionTexts.Count - 1;
        Debug.Log($"[InputHandler] Navigate: direction={direction}, new currentAction={currentAction}");

        OnActionSelect(currentAction);
    }

    public void NavigateSkillSelection(int direction)
    {
        if (sys.battleState != BattleState.SkillSelection) return;

        int next = NextSelectableSkillIndex(currentSkill, direction);
        if (next != -1) currentSkill = next;
        OnSkillSelect(currentSkill);
    }

    public void OnActionSelect(int actionIndex)
    {
        if (sys.battleState == BattleState.ActionSelection)
        {
            Debug.Log($"[InputHandler] OnActionSelect({actionIndex}) called from:\n{System.Environment.StackTrace}");
            currentAction = actionIndex;
            sys.uiController.battleDialogBox.UpdateActionSelection(currentAction);
        }
    }

    public void OnActionConfirm()
    {
        if (sys.battleState != BattleState.ActionSelection) return;

        // Tutorial: block disallowed actions
        var tut = TutorialSequenceRunner.Instance;
        if (tut != null && tut.IsTutorialActive && !tut.IsActionAllowed((BattleAction)currentAction))
        {
            return;
        }

        string uiLabel = (currentAction >= 0 && currentAction < sys.uiController.battleDialogBox.actionTexts.Count)
            ? sys.uiController.battleDialogBox.actionTexts[currentAction].text : "???";
        Debug.Log($"[InputHandler] Confirm: index={currentAction}, enum={(BattleAction)currentAction}, UILabel='{uiLabel}'");

        switch ((BattleAction)currentAction)
        {
            case BattleAction.Skill:
                if (tut != null && tut.IsTutorialActive) tut.AdvanceIfRestrictionMet();
                StartCoroutine(HandleSkillSelection());
                break;
            case BattleAction.Defend:
                if (tut != null && tut.IsTutorialActive) tut.AdvanceIfRestrictionMet();
                StartCoroutine(HandleDefend());
                break;
            case BattleAction.BasicAttack:
                if (tut != null && tut.IsTutorialActive) tut.AdvanceIfRestrictionMet();
                StartCoroutine(HandleBasicAttackSelection());
                break;
            case BattleAction.Switch:
                if ((sys.currentTurnEntity.TurnControl.Bans & ActionBan.Switch) != 0)
                {
                    StartCoroutine(ShowBannedActionDialog("swap positions"));
                    break;
                }
                if (tut != null && tut.IsTutorialActive) tut.AdvanceIfRestrictionMet();
                StartCoroutine(HandleSwitchPosition());
                break;
        }
    }

    public void OnSkillSelect(int skillIndex)
    {
        if (sys.battleState == BattleState.SkillSelection)
        {
            currentSkill = skillIndex;
            sys.uiController.battleDialogBox.UpdateSkillSelection(currentSkill);
        }
    }

    public void OnSkillConfirm()
    {
        if (sys.battleState == BattleState.SkillSelection)
        {
            // Tutorial: block disallowed skills
            var tut = TutorialSequenceRunner.Instance;
            if (tut != null && tut.IsTutorialActive && !tut.IsSkillAllowed(currentSkill))
            {
                return;
            }

            selectedSkill = sys.currentTurnEntity.usableSkills[currentSkill];
            if (!IsSkillUseAllowed(sys.currentTurnEntity, selectedSkill))
            {
                StartCoroutine(HandleUnusableSkillConfirm(selectedSkill));
                return;
            }

            if (tut != null && tut.IsTutorialActive) tut.AdvanceIfRestrictionMet();

            cancelTargetReturnState = BattleState.SkillSelection;
            sys.battleState = BattleState.TargetSelection;
            StartCoroutine(HandleTargetSelection(selectedSkill));
        }
    }

    public void OnSkillCancel()
    {
        if (sys.battleState == BattleState.SkillSelection)
        {
            sys.battleState = BattleState.ActionSelection;
            sys.uiController.battleDialogBox.EnableAttackSelector(false);
            sys.uiController.battleDialogBox.EnableActionSelector(true);
            sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(true);
            sys.uiController.battleDialogBox.UpdateActionSelection(currentAction);
        }
    }

    public void ExecuteSwitchPosition(int indexA, int indexB)
    {
        if (sys.battleState != BattleState.PositionSwitch) return;

        try
        {
            var posA = sys.GetPositionByUnitIndex(indexA);
            var posB = sys.GetPositionByUnitIndex(indexB);

            bool changed = sys.playerParty.TrySwitchPositions(posA, posB);
            if (!changed) return;
            sys.MarkTurnActionTaken(sys.currentTurnEntity);

            sys.SyncPlayerBattleUnitsFromPartySlots();

            sys.timelineManager.UpdateEntityTimeline(sys.currentTurnEntity);
            sys.UpdateTimelineUI();
        }
        finally
        {
            sys.battleState = BattleState.RunningTurn;
        }
    }

    // --- Coroutines ---

    private IEnumerator HandleSwitchPosition()
    {
        yield return null;
        sys.battleState = BattleState.PositionSwitch;
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
        sys.uiController.battleDialogBox.EnableActionSelector(false);
        sys.uiController.battleDialogBox.EnableAttackSelector(false);
        sys.uiController.battleDialogBox.EnableDialogText(true);
        yield return StartCoroutine(sys.uiController.TypeDialog("Select a party member to switch positions."));
        yield return BattleSystem.waitHalf;
        sys.switchPositionController.BeginSwitch();

        yield return new WaitUntil(() => sys.battleState == BattleState.RunningTurn || sys.battleState == BattleState.BattleOver);
        sys.switchPositionController.EndSwitch();
    }

    public IEnumerator HandleDefend()
    {
        yield return null;
        sys.MarkTurnActionTaken(sys.currentTurnEntity);
        sys.battleState = BattleState.Busy;
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
        sys.uiController.battleDialogBox.EnableActionSelector(false);
        sys.uiController.battleDialogBox.EnableDialogText(true);
        yield return StartCoroutine(sys.uiController.TypeDialog($"{sys.currentTurnEntity.entityData.EntityName} takes a defensive stance!"));
        yield return new WaitForSeconds(1f);
        sys.SetEntityDefenseState(sys.currentTurnEntity, DefenseState.Defending);

        sys.battleState = BattleState.RunningTurn;
    }

    private IEnumerator HandleBasicAttackSelection()
    {
        yield return null;
        cancelTargetReturnState = BattleState.ActionSelection;
        sys.battleState = BattleState.TargetSelection;
        yield return StartCoroutine(HandleTargetSelection(sys.basicAttack));
    }

    private IEnumerator HandleSkillSelection()
    {
        yield return null; // Wait 1 frame so the lingering 'Z' keydown input from ActionConfirm doesn't bleed into SkillConfirm
        sys.battleState = BattleState.SkillSelection;
        sys.uiController.battleDialogBox.EnableAttackSelector(true);
        sys.uiController.battleDialogBox.SetAttackName(sys.currentTurnEntity.usableSkills);
        for (int i = 0; i < sys.uiController.battleDialogBox.skillUI.Count; i++)
        {
            if (i < sys.currentTurnEntity.usableSkills.Count)
            {
                var s = sys.currentTurnEntity.usableSkills[i];
                bool allowed = IsSkillUseAllowed(sys.currentTurnEntity, s);
                sys.uiController.battleDialogBox.skillUI[i].SetUnuseable(allowed);
            }
        }
        currentSkill = 0;
        sys.uiController.battleDialogBox.UpdateSkillSelection(currentSkill);
        yield return new WaitUntil(() => sys.battleState == BattleState.TargetSelection || sys.battleState == BattleState.ActionSelection || sys.battleState == BattleState.RunningTurn || sys.battleState == BattleState.BattleOver);

        sys.uiController.battleDialogBox.EnableAttackSelector(false);
    }

    private IEnumerator HandleUnusableSkillConfirm(ActiveSkill skill)
    {
        sys.battleState = BattleState.ShowingDialog;
        sys.uiController.battleDialogBox.EnableAttackSelector(false);
        sys.uiController.battleDialogBox.EnableDialogText(true);
        sys.uiController.battleDialogBox.EnableActionSelector(false);
        yield return StartCoroutine(sys.uiController.battleDialogBox.TypeDialog($"{sys.currentTurnEntity.entityData.EntityName} can't use {skill.SkillData.skillName}!"));
        sys.uiController.battleDialogBox.EnableDialogText(false);
        sys.battleState = BattleState.SkillSelection;
        sys.uiController.battleDialogBox.EnableAttackSelector(true);
        sys.uiController.battleDialogBox.EnableActionSelector(true);
        sys.uiController.battleDialogBox.UpdateSkillSelection(currentSkill);
    }

    private IEnumerator ShowBannedActionDialog(string actionName)
    {
        sys.battleState = BattleState.ShowingDialog;
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
        sys.uiController.battleDialogBox.EnableActionSelector(false);
        sys.uiController.battleDialogBox.EnableDialogText(true);
        yield return StartCoroutine(sys.uiController.battleDialogBox.TypeDialog($"{sys.currentTurnEntity.entityData.EntityName} can't {actionName}!", false));
        yield return new WaitForSeconds(1.5f); // Small delay to let the player read
        sys.uiController.battleDialogBox.EnableDialogText(false);
        sys.uiController.battleDialogBox.ShowTurnEntityInfo();
        sys.battleState = BattleState.ActionSelection;
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(true);
        sys.uiController.battleDialogBox.EnableActionSelector(true);
        sys.uiController.battleDialogBox.UpdateActionSelection(currentAction);
    }

    private IEnumerator HandleTargetSelection(ActiveSkill skillToUse)
    {
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
        sys.uiController.battleDialogBox.EnableAttackSelector(false);
        sys.uiController.battleDialogBox.EnableDialogText(true);
        sys.uiController.battleDialogBox.EnableActionSelector(false);
        yield return StartCoroutine(sys.uiController.battleDialogBox.TypeDialog("Select a target!"));
        yield return BattleSystem.waitHalf;

        sys.targetSelectionController.gameObject.SetActive(true);
        sys.targetSelectionController.StartSelection((targets) =>
        {
            if (targets != null && targets.Count > 0)
            {
                var tut = TutorialSequenceRunner.Instance;
                if (tut != null && tut.IsTutorialActive)
                {
                    tut.AdvanceIfRestrictionMet();
                }

                sys.StartCoroutine(sys.PerformSkillAction(sys.currentTurnEntity, targets, skillToUse));
            }
            else
            {
                Debug.Log("Target selection cancelled.");
                sys.uiController.battleDialogBox.EnableDialogText(false);
                sys.targetSelectionController.gameObject.SetActive(false);

                if (cancelTargetReturnState == BattleState.ActionSelection)
                {
                    sys.battleState = BattleState.ActionSelection;

                    sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(true);
                    sys.uiController.battleDialogBox.EnableAttackSelector(false);
                    sys.uiController.battleDialogBox.EnableActionSelector(true);
                    sys.uiController.battleDialogBox.UpdateActionSelection(currentAction);
                }
                else
                {
                    sys.battleState = BattleState.SkillSelection;

                    sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
                    sys.uiController.battleDialogBox.EnableActionSelector(false);
                    sys.uiController.battleDialogBox.EnableAttackSelector(true);
                    sys.uiController.battleDialogBox.UpdateSkillSelection(currentSkill);
                }
            }
        }, skillToUse, sys.currentTurnEntity);

        yield return new WaitUntil(() => sys.battleState == BattleState.RunningTurn || sys.battleState == BattleState.SkillSelection || sys.battleState == BattleState.BattleOver);

        sys.targetSelectionController.gameObject.SetActive(false);
    }

    // --- Helpers ---

    public bool IsSkillUseAllowed(EntityBase actor, ActiveSkill skill)
    {
        var dir = actor.TurnControl;

        if (dir.BannedSkillDefs.Contains(skill.SkillData.skillDefinition))
        {
            return false;
        }
        if (skill.SkillData.skillDefinition == SkillDefinition.Spell && actor.GetCurrentMP() < skill.currentMPCost)
        {
            return false;
        }

        if (skill.SkillData.skillDefinition == SkillDefinition.BattleArt && actor.GetCurrentSP() < skill.currentSPCost)
        {
            return false;
        }

        return true;
    }

    private int NextSelectableSkillIndex(int start, int direction)
    {
        int n = sys.currentTurnEntity.usableSkills.Count;
        if (n == 0) return -1;

        int idx = start;
        for (int step = 0; step < n; step++)
        {
            idx = (idx + direction + n) % n;
            var s = sys.currentTurnEntity.usableSkills[idx];
            if (IsSkillUseAllowed(sys.currentTurnEntity, s))
                return idx;
        }
        return -1;
    }

    public void ResetActionSelection()
    {
        Debug.Log($"[InputHandler] ResetActionSelection: old currentAction={currentAction}, resetting to 0");
        currentAction = 0;
        sys.uiController.battleDialogBox.UpdateActionSelection(currentAction);
    }
}
