using UnityEngine;

[CreateAssetMenu(fileName = "Doppelganger Passive Effect", menuName = "PassiveEffects/Doppelganger")]
public class DoppelgangerPassiveEffectData : PassiveEffectData
{
	[Header("Transform VFX")]
	[Tooltip("Sprite frames for the transformation VFX animation. Leave empty for a flash-only effect.")]
	public Sprite[] transformVfxFrames;

	[Tooltip("Frames per second for the transform VFX animation.")]
	public float transformVfxFps = 15f;

	[Header("Buff Icon")]
	[Tooltip("Icon to display on the monster for the 'Doppelganger Stats' buff.")]
	public Sprite statCopyIcon;

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new DoppelgangerPassiveEffect(transformVfxFrames, transformVfxFps, statCopyIcon);
	}
}
