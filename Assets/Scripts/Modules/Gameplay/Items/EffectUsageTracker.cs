using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EffectUsageTracker 
{
	public int CurrentUse = 0;
	public int CurrentBattleUse = 0;
	public int baseMaxUsePerBattle;
	public int baseMaxUsePerLifeCycle;
	private int bonusMaxUsePerBattle;
	private int bonusMaxUsePerLifeCycle;
	private System.Action onRecordedUse;

	public EffectUsageTracker(int maxUsePerBattle, int maxUse)
	{
		baseMaxUsePerBattle = maxUsePerBattle;
		baseMaxUsePerLifeCycle = maxUse;
	}

	public void SetGradeBonus(int bonusPerBattle, int bonusPerLifeCycle)
	{
		bonusMaxUsePerBattle = bonusPerBattle;
		bonusMaxUsePerLifeCycle = bonusPerLifeCycle;
	}
	public bool CanUse()
	{
		bool battleOK = (baseMaxUsePerBattle <= 0) || (CurrentBattleUse < baseMaxUsePerBattle + bonusMaxUsePerBattle);
		bool lifeCycleOK = (baseMaxUsePerLifeCycle <= 0) || (CurrentUse < baseMaxUsePerLifeCycle + bonusMaxUsePerLifeCycle);
		return battleOK && lifeCycleOK;
	}
	public void RecordUse()
	{
		CurrentBattleUse++;
		CurrentUse++;
		onRecordedUse?.Invoke();
	}

	public void SetOnRecordedUse(System.Action callback)
	{
		onRecordedUse = callback;
	}

	public void ResetBattleUsage()
	{
		CurrentBattleUse = 0;
	}	


}
