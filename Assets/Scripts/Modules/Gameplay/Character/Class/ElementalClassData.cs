using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewElementalClassData", menuName = "Character/Create Mage Class Data")]
public class ElementalClassData : ClassData
{
	[System.Serializable]
	public class ElementalSkillList
	{
		public Element element;
		public List<SkillEntry> skillList = new List<SkillEntry>();
	}
	[Header("Elemental Skill Set")]
	public List<ElementalSkillList> elementalSkillSets = new List<ElementalSkillList>();
	public List<SkillEntry> GetSkillSetFor(Element e)
	{
		var found = elementalSkillSets.Find(es => es.element == e);
		return found != null ? found.skillList : new List<SkillEntry>();
	}
}
