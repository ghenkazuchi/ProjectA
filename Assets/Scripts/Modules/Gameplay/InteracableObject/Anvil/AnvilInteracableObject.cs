using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnvilInteracableObject : Interacable
{
	public override void TriggerInteraction()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnAnvilPopupOpen));
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInteract));
	}
}
