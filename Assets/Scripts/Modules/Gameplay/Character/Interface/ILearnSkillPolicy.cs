using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILearnSkillPolicy
{
	public void Resolve(LearnRequest request);
}
