using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnAllyAfterSkillUsed
{
	IEnumerator OnAllyAfterSkillUsed(SkillUseContext ctx, List<EntityBase> targetsGotHit);
}
