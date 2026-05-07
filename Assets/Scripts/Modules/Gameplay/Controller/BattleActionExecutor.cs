using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaKien;

public class BattleActionExecutor : MonoBehaviour
{
    private BattleSystem sys;
    private readonly DamageContext _ctx = new DamageContext();
    private float defenseStateDamageReduction;

    public void Init(BattleSystem system, float defReduction)
    {
        sys = system;
        defenseStateDamageReduction = defReduction;
    }

    public IEnumerator PerformSkillAction(EntityBase sourceEntity, List<EntityBase> targetEntites, ActiveSkill skillToUse)
    {
        int totalDamagDeal = 0;
        sys.targetSelectionController.ClearAllPreviews();
        sys.uiController.currentPlayerCharacterInfo.EnableBattleHud(false);
        yield return StartCoroutine(sys.uiController.battleDialogBox.TypeDialog($"{sys.currentTurnEntity.entityData.EntityName} used {skillToUse.SkillData.skillName}"));
        yield return BattleSystem.waitHalf;
        BattleUnit attacker = sys.FindBattleUnitForEntityPublic(sourceEntity);
        attacker?.GetAnimator().PlayAttackAnimation(attacker.GetOffSet());
        
        // OnCast
        if (skillToUse.SkillData.effectsToApply.Count > 0)
        {
            var contextCast = new SkillContext
            {
                Owner = sourceEntity,
                AllTarget = targetEntites,
                HitTarget = targetEntites
            };
            yield return skillToUse.ExecuteEffect(contextCast, EffectActiveTiming.OnCast);
        }
        
        // OnHit Initialization
        List<EntityBase> targetsGotHit = new List<EntityBase>();
        if (skillToUse.SkillData.activeSkillType == ActiveSkillType.Damage)
        {
            foreach (var e in targetEntites)
            {
                targetsGotHit.Add(e); 
            }
        }
        else
        {
            targetsGotHit.AddRange(targetEntites);
        }

        //DealingDamagePhase
        if (skillToUse.SkillData.activeSkillType == ActiveSkillType.Damage)
        {
            int extraHitsFromBuffs = 0;
            for (int i = sourceEntity.currentActiveBuffs.Count - 1; i >= 0; i--)
            {
                var buff = sourceEntity.currentActiveBuffs[i];
                if (buff is IBonusHitCountModifier modifier)
                {
                    extraHitsFromBuffs += modifier.GetBonusHitCount();
                    yield return modifier.Consume();
                    if (buff.CurrentDuration <= 0)
                    {
                        yield return sourceEntity.RemoveEffectCoroutine(buff);
                    }
                }
            }

            List<EntityBase> actualHitTargets = new List<EntityBase>();

            foreach (var target in targetsGotHit)
            {
                int hitsToPerform = Mathf.Max(1, skillToUse.SkillData.hitCount) + extraHitsFromBuffs;
                bool hitAtLeastOnce = false;

                Sprite[] frames = null;
                float fps = 0f;
                bool hasVfx = sys.vfxLib.TryGetSpellVFX(skillToUse.SkillData.skillDefinition, skillToUse.element, out frames, out fps);
                var targetUnit = sys.FindBattleUnitForEntityPublic(target);
                
                var skillContext = new SkillContext
                {
                    Owner = sourceEntity,
                    AllTarget = targetEntites,
                    HitTarget = targetsGotHit,
                    totalDamageDeal = 0,
                };
                yield return skillToUse.ExecuteBeforeDealingDamageEffect(skillContext);

                for (int h = 0; h < hitsToPerform; h++)
                {
                    float chance, roll;
                    bool hit = DamageCalculator.CheckHit(sourceEntity, target, skillToUse, out chance, out roll);

                    if (hit)
                    {
                        hitAtLeastOnce = true;
                        var anim = targetUnit?.GetAnimator();
                        if (anim != null)
                            yield return anim.PlayHitAnimation(playDefaultVfx: hasVfx, overrideFrames: frames, overideFps: fps);
                            
                        int damage = CalculateDamage(sourceEntity, skillToUse, target);
                        totalDamagDeal += damage;
                        
                        yield return HandleEntityTakeDamage(target, damage, sourceEntity, skillToUse.SkillData.skillDefinition, skillName: skillToUse.SkillData.skillName);
                    }
                    else
                    {
                        yield return StartCoroutine(sys.ShowDialog($"{sourceEntity.entityData.EntityName} missed!"));
                        // Trigger OnEvade
                        yield return target.PassiveSkillRunner.Trigger(PassiveTrigger.OnEvade, sourceEntity);
                        yield return target.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnEvade, sourceEntity);
                    }
                    
                    if (hitsToPerform > 1 && h < hitsToPerform - 1)
                    {
                        yield return new WaitForSeconds(0.15f); // Fast multi-hit pause
                    }
                    else 
                    {
                        yield return new WaitForSeconds(0.2f); // Normal final hit pause
                    }
                }

                if (hitAtLeastOnce)
                {
                    actualHitTargets.Add(target);
                }
            }

            if (skillToUse.SkillData.effectsToApply.Count > 0)
            {
                var contextHit = new SkillContext
                {
                    Owner = sourceEntity,
                    AllTarget = targetEntites,
                    HitTarget = actualHitTargets,
                    totalDamageDeal = totalDamagDeal,
                };
                yield return skillToUse.ExecuteOnDealingDamageEffect(contextHit);
            }
        }
        else if (skillToUse.SkillData.activeSkillType == ActiveSkillType.Heal)
        {
            foreach (var target in targetsGotHit)
            {
                var healingVFX = sys.vfxLib.GetHealingVFX();
                var targetUnit = sys.FindBattleUnitForEntityPublic(target);
                var animator = targetUnit?.GetAnimator();
                if (animator != null) yield return animator.PlayHealingAnimation(healingVFX);
                int skillHealing = CalculateHealing(sourceEntity, skillToUse, target);
                var healingContext = new HealingContext();
                healingContext.Reset(sourceEntity, target, skillHealing);
                yield return HandleEntityGotHeal(healingContext);
                yield return BattleSystem.waitHalf;
            }
        }

        if (skillToUse.SkillData.skillDefinition == SkillDefinition.Spell)
        {
            sourceEntity.ReduceMP(skillToUse.currentMPCost);
        }
        else if (skillToUse.SkillData.skillDefinition == SkillDefinition.BattleArt)
        {
            sourceEntity.ReduceSP(skillToUse.currentSPCost);
        }

        // Fire IAfterSkillUsed hook
        var skillUseCtx = new SkillUseContext
        {
            Caster = sourceEntity,
            Skill = skillToUse,
            Definition = skillToUse.SkillData.skillDefinition,
            MPCost = skillToUse.currentMPCost,
            SPCost = skillToUse.currentSPCost,
            TotalDamageDealt = totalDamagDeal,
        };
        var casterEffects = sourceEntity.GetAllEffect();
        for (int i = 0; i < casterEffects.Count; i++)
        {
            if (casterEffects[i] is IAfterSkillUsed afterHook)
                yield return afterHook.OnAfterSkillUsed(skillUseCtx);
        }

        List<EntityBase> allies = (sourceEntity is PlayerCharacter) 
            ? sys.playerParty.GetAllEntitiesInParty() 
            : sys.monsterParty.GetAllEntitiesInParty();
            
        foreach (var ally in allies)
        {
            if (ally == sourceEntity || ally.GetCurrentHP() <= 0) continue;
            var allyEffects = ally.GetAllEffect();
            if (allyEffects != null)
            {
                for (int i = 0; i < allyEffects.Count; i++)
                {
                    if (allyEffects[i] is IOnAllyAfterSkillUsed allyAfterHook)
                    {
                        yield return allyAfterHook.OnAllyAfterSkillUsed(skillUseCtx, targetsGotHit);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        sys.timelineManager.UpdateEntityTimeline(sourceEntity);
        sys.UpdateTimelineUI();
        sys.battleState = BattleState.RunningTurn;
    }

    public IEnumerator ApplyEffectDamage(EntityBase target, int amount, EntityBase source, string reason = null)
    {
        BattleUnit targetUnit = sys.FindBattleUnitForEntityPublic(target);
        yield return HandleEntityTakeDamage(target, amount, source, SkillDefinition.Almighty, skillName: reason, isEffectDamage: true, flavor: reason);
    }

    public IEnumerator HandleEntityGotHeal(HealingContext healingContext)
    {
        yield return BattleEventManager.TriggerOnHealingReceived(healingContext.Target, healingContext);
        if (healingContext.FinalHealing >= 0)
        {
            healingContext.Target.Heal(healingContext.FinalHealing);
            sys.UpdateUnitHealth(healingContext.Target);
            yield return sys.ShowDialog($"{healingContext.Target.entityData.EntityName} recovered {healingContext.FinalHealing} HP!");
        }
        else
        {
            int damage = Mathf.Abs(healingContext.FinalHealing);
            yield return HandleEntityTakeDamage(healingContext.Target, damage, healingContext.Healer, SkillDefinition.Almighty, skillName: "reverse healing", isEffectDamage: true, flavor: "reverse healing");
        }
    }

    public IEnumerator HandleEntityTakeDamage(
        EntityBase target, int finalDamage, EntityBase source,
        SkillDefinition origin, string skillName = "", bool isEffectDamage = false, string flavor = null)
    {
        var originalTarget = target;

        _ctx.Reset(source, target, finalDamage, origin, skillName, isEffectDamage);

        yield return BattleEventManager.TriggerBeforeDealingDamage(source, _ctx.Target, _ctx);
        yield return BattleEventManager.TriggerBeforeTakingDamage(_ctx.Target, _ctx);

        foreach (var ally in BattleGridUtils.GetAlliesOf(_ctx.Target, sys.playerParty, sys.monsterParty, sys))
        {
            if (ally == null || ally == _ctx.Target) continue;
            if (!sys.IsEntityAlivePublic(ally)) continue;
            yield return BattleEventManager.TriggerBeforeTakingDamage(ally, _ctx);
        }
        if (_ctx.CancleApply)
        {
            var parryVfx = sys.vfxLib.GetParryVFX();
            var targetUnit = sys.FindBattleUnitForEntityPublic(target);
            var animator = targetUnit?.GetAnimator();
            if (animator != null) yield return animator.PlayParryAnimation(parryVfx);
            if (_ctx.ReflectAmount > 0)
                yield return ApplyEffectDamage(_ctx.Source, _ctx.ReflectAmount, _ctx.Target, "parry");
            yield break;
        }

        if (_ctx.RedirectTo != null && _ctx.RedirectTo != _ctx.Target
            && BattleGridUtils.AreAllies(_ctx.RedirectTo, _ctx.Target, sys.playerParty, sys.monsterParty) && sys.IsEntityAlivePublic(_ctx.RedirectTo))
        {
            var protector = _ctx.RedirectTo;
            var protectorName = protector.entityData?.EntityName;
            var victimName = originalTarget.entityData?.EntityName;

            yield return sys.ShowDialog($"{protectorName} protected {victimName}!");
            _ctx.Target = protector;
            yield return BattleEventManager.TriggerBeforeTakingDamage(_ctx.Target, _ctx);

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
            if (protector != null && sys.IsEntityAlivePublic(protector) && BattleGridUtils.AreAllies(protector, target, sys.playerParty, sys.monsterParty))
            {
                int toProtector = Mathf.RoundToInt(_ctx.EffectiveDamage * _ctx.SplitPercent);
                int toVictim = _ctx.EffectiveDamage - toProtector;

                var protectorName = protector.entityData?.EntityName;
                var victimName = originalTarget.entityData?.EntityName;
                int sharePct = Mathf.RoundToInt(_ctx.SplitPercent * 100f);

                var shareCtx = new DamageContext();
                shareCtx.Reset(_ctx.Source, protector, toProtector, _ctx.Origin, skillName: _ctx.SkillName, isEffect: false);
                shareCtx.BlockFurtherSharing = true;
                yield return BattleEventManager.TriggerBeforeDealingDamage(_ctx.Source, protector, shareCtx);
                yield return BattleEventManager.TriggerBeforeTakingDamage(protector, shareCtx);

                if (shareCtx.CancleApply)
                {
                    if (shareCtx.ReflectAmount > 0)
                        yield return ApplyEffectDamage(shareCtx.Source, shareCtx.ReflectAmount, protector, "parry");
                }
                else
                {
                    BattleUnit protectorBattleUnit = sys.FindBattleUnitForEntityPublic(protector);
                    protector.TakeDamage(shareCtx.EffectiveDamage, shareCtx.Source);
                    yield return BattleEventManager.TriggerOnTakingDamage(protector, shareCtx);
                    sys.UpdateUnitHealth(protector);
                    if (sys.UpdateUnitState(protector)) yield break;
                }
                _ctx.EffectiveDamage = toVictim;
            }
        }
        DamageCalculator.ResolveCrit(_ctx, sys.IsDefending(_ctx.Target));
        if (_ctx.IsCritical)
        {
            _ctx.EffectiveDamage = Mathf.RoundToInt(_ctx.EffectiveDamage * _ctx.CritMultiplier);
        }
        BattleUnit targetBattleUnit = sys.FindBattleUnitForEntityPublic(_ctx.Target);
        string critical = _ctx.IsCritical ? " It's a critical hit!" : "";
        _ctx.Target.TakeDamage(_ctx.EffectiveDamage, _ctx.Source);

        string msg = string.IsNullOrEmpty(flavor)
            ? $"{_ctx.Target.entityData.EntityName} took {_ctx.EffectiveDamage} damage!"
            : $"{_ctx.Target.entityData.EntityName} took {_ctx.EffectiveDamage} {flavor} damage!";

        if (_ctx.HasElementalAdvantage)
        {
            msg += "\nIt's super effective!";
        }

        yield return sys.ShowDialog(msg);
        if (_ctx.IsCritical)
        {
            yield return sys.ShowDialog(critical);
        }

        yield return BattleEventManager.TriggerDealingDamage(_ctx.Source, _ctx.Target, _ctx);
        yield return BattleEventManager.TriggerOnTakingDamage(_ctx.Target, _ctx);
        sys.UpdateUnitHealth(_ctx.Target);
        if (_ctx.Target.GetCurrentHP() <= 0 && _ctx.Target is MonsterCharacter defeatedMonster)
        {
            GameEventBus.Publish(new MonsterKillEvent
            {
                Monster = defeatedMonster
            });

            // Trigger OnEnemyKill equipment effects
            if (_ctx.Source != null && _ctx.Source.EquipmentEffectRunner != null)
            {
                yield return _ctx.Source.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnEnemyKill, _ctx.Target);
            }
        }
        GameEventBus.Publish(new DamageDealtEvent { damageAmount = _ctx.EffectiveDamage });
        if (sys.UpdateUnitState(_ctx.Target)) yield break;
    }

    public IEnumerator ForceBasicAttack(EntityBase attacker, EntityBase target)
    {
        BattleUnit attackerUnit = sys.FindBattleUnitForEntityPublic(attacker);
        attackerUnit?.GetAnimator().PlayAttackAnimation(attackerUnit.GetOffSet());
        
        yield return new WaitForSeconds(0.4f); // Short delay to match swing visual
        
        Sprite[] frames = null;
        float fps = 0f;
        bool hasVfx = sys.vfxLib.TryGetSpellVFX(sys.basicAttack.SkillData.skillDefinition, sys.basicAttack.element, out frames, out fps);
        
        BattleUnit targetUnit = sys.FindBattleUnitForEntityPublic(target);
        var targetAnim = targetUnit?.GetAnimator();
        if (targetAnim != null)
        {
            yield return targetAnim.PlayHitAnimation(playDefaultVfx: hasVfx, overrideFrames: frames, overideFps: fps);
        }

        int damage = CalculateDamage(attacker, sys.basicAttack, target);
        yield return StartCoroutine(ApplyEffectDamage(target, damage, attacker, $"by {attacker.entityData.EntityName} attack"));
    }

    public int CalculateDamage(EntityBase source, ActiveSkill skill, EntityBase target)
    {
        if (skill.SkillData.activeSkillType != ActiveSkillType.Damage)
        {
            return 0;
        }

        var ctx = new DamageContext();
        ctx.Source = source;
        ctx.Target = target;
        ctx.Origin = skill.SkillData.skillDefinition;
        ctx.SkillName = skill.SkillData.skillName;
        ctx.attackIncreasePercentage = 1f;
        ctx.defenseIgnorePercentage = 0f;
        sys.ApplySkillModifiersPreview(skill, ref ctx);
        return CalculateDamageWithContext(source, skill, target, ctx);
    }

    public int CalculateDamageWithContext(EntityBase source, ActiveSkill skill, EntityBase target, DamageContext ctx)
    {
        return DamageCalculator.CalculateDamageWithContext(source, skill, target, ctx, sys, defenseStateDamageReduction, sys.IsDefending(target));
    }

    public int CalculateHealing(EntityBase healer, ActiveSkill skill, EntityBase target)
    {
        if (skill.SkillData.activeSkillType != ActiveSkillType.Heal) return 0;
        int baseHeal = Mathf.CeilToInt(skill.currentSkillDamage);
        if (skill.SkillData.scalingStatAndMutiply != null)
        {
            foreach (var entry in skill.SkillData.scalingStatAndMutiply)
            {
                baseHeal += Mathf.CeilToInt(healer.GetFinalStat(entry.Key) * entry.Value);
            }
        }

        // Apply global DivinePower multiplier
        float divinePower = healer.GetFinalStat(Stat.DivinePower);
        float divineMultiplier = 1f + (divinePower / 100f);
        baseHeal = Mathf.CeilToInt(baseHeal * divineMultiplier);

        var hctx = new HealingContext();
        hctx.Reset(healer, target, baseHeal);
        sys.ApplySkillHealingModifiersPreview(skill, ref hctx);
        return Mathf.Max(0, hctx.FinalHealing);
    }
}
