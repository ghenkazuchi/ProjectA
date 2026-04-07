using System.Collections;

public interface IAfterSkillUsed
{
	IEnumerator OnAfterSkillUsed(SkillUseContext ctx);
}
