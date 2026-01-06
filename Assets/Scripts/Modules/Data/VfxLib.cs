using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillVfxLibrary", menuName = "VFX/Skill VFX Library")]
public class VfxLib : ScriptableObject
{
	[Header("BattleArt VFX")]
	public Sprite[] battleArtFrames;
	public float battleArtFps = 30f;

	[Header("Spell VFX")]
	public List<ElementVFX> spellVFXs;

	[Header("Parry VFX")]	
	public Sprite[] parryFrames;
	[Header("Healing VFX")]
	public Sprite[] healingFrames;

	[Serializable]
	public class ElementVFX
	{
		public Element element;
		public Sprite[] frames;
		public float fps = 30f;
	}
	public bool TryGetSpellVFX(SkillDefinition def, Element e, out Sprite[] frames, out float fps)
	{
		if (def == SkillDefinition.BattleArt)
		{
			frames = battleArtFrames; fps = battleArtFps; return frames != null && frames.Length > 0;
		}
		if (def == SkillDefinition.Spell && spellVFXs != null)
		{
			var idx = spellVFXs.FindIndex(x => x.element == e);
			if (idx >= 0)
			{
				frames = spellVFXs[idx].frames;
				fps = spellVFXs[idx].fps;
				return frames != null && frames.Length > 0;
			}
		}
		frames = null; fps = 0f; return false;
	}

	public Sprite[] GetParryVFX()
	{
		return parryFrames;
	}
	public Sprite[] GetHealingVFX()
	{
		return healingFrames;
	}
}
