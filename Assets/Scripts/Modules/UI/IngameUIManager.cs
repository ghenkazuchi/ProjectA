using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUIManager : Singleton<IngameUIManager>, IMessageHandle
{
	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnTargetSelection, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnTargetSelection, this);
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnTargetSelection:

				break;
		}
	}
}
