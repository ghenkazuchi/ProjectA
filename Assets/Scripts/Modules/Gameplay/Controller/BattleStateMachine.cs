using System.Collections;
using UnityEngine;

namespace HaKien
{
    public abstract class BattleStateClass
    {
        protected BattleSystem battleSystem;
        protected BattleStateMachine stateMachine;

        public BattleStateClass(BattleSystem system, BattleStateMachine machine)
        {
            this.battleSystem = system;
            this.stateMachine = machine;
        }

        public virtual IEnumerator Enter() { yield break; }
        public virtual IEnumerator Execute() { yield break; }
        public virtual IEnumerator Exit() { yield break; }
    }

    public class BattleStateMachine : MonoBehaviour
    {
        private BattleStateClass currentState;

        private Coroutine transitionRoutine;

        public void ChangeState(BattleStateClass newState)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }
            transitionRoutine = StartCoroutine(TransitionState(newState));
        }

        private IEnumerator TransitionState(BattleStateClass newState)
        {
            if (currentState != null)
            {
                yield return StartCoroutine(currentState.Exit());
            }
            currentState = newState;
            yield return StartCoroutine(currentState.Enter());
            yield return StartCoroutine(currentState.Execute());
        }

        public void StopMachine()
        {
            StopAllCoroutines();
            currentState = null;
        }
    }

    // Concrete States
    public class BattleStartState : BattleStateClass
    {
        public BattleStartState(BattleSystem system, BattleStateMachine machine) : base(system, machine) { }

        public override IEnumerator Enter()
        {
            yield return battleSystem.ShowInitialDialog();
            yield return battleSystem.ResetAllEquipmentBattleUsageTime();
            yield return battleSystem.ActiveInitialPassiveSkillsEffect();
        }

        public override IEnumerator Execute()
        {
            stateMachine.ChangeState(new BattleLoopState(battleSystem, stateMachine));
            yield break;
        }
    }

    public class BattleLoopState : BattleStateClass
    {
        public BattleLoopState(BattleSystem system, BattleStateMachine machine) : base(system, machine) { }

        public override IEnumerator Execute()
        {
            while (!battleSystem.battleOver)
            {
                if (battleSystem.CheckBattleEndConditionPublic()) break;

                battleSystem.RemoveDeadUnitsFromTimelinePublic();
                battleSystem.UpdateTimelineUI();
                
                EntityBase currentTurnEntity = battleSystem.timelineManager.GetNextTurnEntity();
                if (currentTurnEntity == null)
                {
                    continue;
                }
                battleSystem.SetCurrentTurnEntity(currentTurnEntity);
                battleSystem.BeginTurn(currentTurnEntity);
                BattleUnit currentUnit = battleSystem.FindBattleUnitForEntityPublic(currentTurnEntity);
                
                currentTurnEntity.ResetTurnDirective();
                yield return battleSystem.StartCoroutine(currentTurnEntity.ProcessEffectOnTurnStart());
                if (battleSystem.battleOver) break;
                
                if (currentUnit == null || !currentUnit.IsAlive())
                {
                    Debug.Log($"{currentTurnEntity.entityData.EntityName} is dead, skipping turn");
                    continue;
                }

                var dir = currentTurnEntity.TurnControl;

                // Always reset defense when this entity's turn starts,
                // even if the turn will be skipped or forced.
                battleSystem.ResetDefenseStatePublic(currentTurnEntity);

                if (dir.SkipThisTurn)
                {
                    string reasonSuffix = string.IsNullOrEmpty(dir.SkipReason) ? "" : $" ({dir.SkipReason})";
                    yield return battleSystem.StartCoroutine(battleSystem.ShowDialog(
                        $"{currentTurnEntity.entityData.EntityName} skips the turn{reasonSuffix}."
                    ));
                    yield return new WaitForSeconds(1f);
                    if (battleSystem.battleOver) break;
                    yield return battleSystem.StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
                    continue;
                }

                if (dir.ForcedAction != ForcedActionKind.None)
                {
                    yield return battleSystem.StartCoroutine(battleSystem.ExecuteForcedActionPublic(currentTurnEntity, dir));
                    yield return new WaitForSeconds(0.5f);
                    if (battleSystem.battleOver) break;
                    yield return battleSystem.StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                Debug.Log($"It's {currentTurnEntity.entityData.EntityName}'s turn!");
                battleSystem.UpdateTimelineUI();

                if (currentTurnEntity is PlayerCharacter)
                {
                    if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsTutorialActive)
                        yield return battleSystem.StartCoroutine(
                            TutorialSequenceRunner.Instance.RunStepsForPhase(TutorialStepTiming.BeforePlayerTurn));

                    yield return battleSystem.StartCoroutine(battleSystem.HandlePlayerTurnPublic(currentTurnEntity));

                    if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsTutorialActive)
                    {
                        TutorialSequenceRunner.Instance.OnPlayerActionCompleted();
                        yield return battleSystem.StartCoroutine(
                            TutorialSequenceRunner.Instance.RunStepsForPhase(TutorialStepTiming.AfterPlayerAction));
                    }
                }
                else
                {
                    if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsTutorialActive)
                        yield return battleSystem.StartCoroutine(
                            TutorialSequenceRunner.Instance.RunStepsForPhase(TutorialStepTiming.BeforeMonsterTurn));

                    yield return battleSystem.StartCoroutine(battleSystem.HandleMonsterTurnPublic(currentTurnEntity));

                    if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsTutorialActive)
                        yield return battleSystem.StartCoroutine(
                            TutorialSequenceRunner.Instance.RunStepsForPhase(TutorialStepTiming.AfterMonsterAction));
                }

                if (TutorialSequenceRunner.Instance != null && TutorialSequenceRunner.Instance.IsTutorialActive)
                    TutorialSequenceRunner.Instance.OnTurnAdvanced();
                
                if (battleSystem.battleOver) break;
                yield return battleSystem.StartCoroutine(currentTurnEntity.ProcessEffectOnTurnEnd());
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
