using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName ="UI/ElementalIconMapping")]
public class ElementalMapping : ScriptableObject 
{
	public List<ElementalIcon> elementIcons;

	public Sprite GetIcon(Element element)
	{
		return elementIcons.FirstOrDefault(e => e.element == element)?.icon;
	}
}

[System.Serializable]
public class ElementalIcon
{
	public Element element;
	public Sprite icon;
}