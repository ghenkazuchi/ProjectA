using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioConfig", menuName = "Audio/Audio Config")]
public class AudioConfig : ScriptableObject
{
	[SerializeField] private AudioClip[] clips;
	[Range(0f, 1f)] [SerializeField] private float volume = 1f;
	[Range(0f, 0.5f)] [SerializeField] private float volumeVariance = 0f;
	[Range(0.1f, 3f)] [SerializeField] private float pitch = 1f;
	[Range(0f, 0.5f)] [SerializeField] private float pitchVariance = 0.05f;

	public AudioClip[] Clips => clips;
	public float Volume => volume;
	public float VolumeVariance => volumeVariance;
	public float Pitch => pitch;
	public float PitchVariance => pitchVariance;

	public AudioClip GetRandomClip()
	{
		if (clips == null || clips.Length == 0)
			return null;
		return clips[Random.Range(0, clips.Length)];
	}

	public float GetRandomVolume()
	{
		return Mathf.Clamp01(volume + Random.Range(-volumeVariance, volumeVariance));
	}

	public float GetRandomPitch()
	{
		return Mathf.Clamp(pitch + Random.Range(-pitchVariance, pitchVariance), 0.1f, 3f);
	}
}
