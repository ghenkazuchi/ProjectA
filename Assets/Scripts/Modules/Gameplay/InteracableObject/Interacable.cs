using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interacable : MonoBehaviour
{
	public SpawnableObject spawnableData;
	protected virtual bool CanInteract(GameObject interactor) => true;
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log("1");
		if (collision.CompareTag("Player") && CanInteract(collision.gameObject))
		{
			Debug.Log("Interacted");
			MessageManager.Instance.SendMessage(new Message(MessageType.OnInteract, new object[] { this }));
			SpawnableObject spawnableObject = spawnableData;
			GameEventBus.Publish(new InteractionEvent
			{
				HasInteractableType = spawnableObject != null,
				InteractableType = spawnableObject != null ? spawnableObject.interacableType : default,
				SpawnableObject = spawnableObject,
				InteractionKey = spawnableObject != null ? spawnableObject.GetKey() : name
			});
			TriggerInteraction();
		}
	}
	public abstract void TriggerInteraction();
}
