using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillModifierData : ScriptableObject
{
	public virtual void ModifyPreview(ref DamageContext ctx)
	{

	}

	public virtual IEnumerator ModiffyRuntime(DamageContext ctx)
	{
		yield break;
	}
}
