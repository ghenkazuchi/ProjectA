
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class BaseSkillData : ScriptableObject
{
	[Header("Basic Info")]
	public string skillName;
	[TextArea] public string skillDescription;
	public SkillType skillType;
}