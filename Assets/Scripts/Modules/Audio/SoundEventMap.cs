using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaKien;
using RotaryHeart.Lib.SerializableDictionary;

[CreateAssetMenu(fileName = "NewSoundEventMap", menuName = "Audio/Sound Event Map")]
public class SoundEventMap : ScriptableObject
{
	[System.Serializable]
	public class EventAudioMap : SerializableDictionaryBase<MessageType, AudioConfig> {}

	[SerializeField] private EventAudioMap mappings;

	public bool TryGetConfig(MessageType messageType, out AudioConfig config)
	{
		if (mappings == null)
		{
			config = null;
			return false;
		}
		return mappings.TryGetValue(messageType, out config);
	}
}
