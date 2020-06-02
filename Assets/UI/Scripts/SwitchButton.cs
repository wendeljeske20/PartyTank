using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SwitchButton : UIBehaviour, IPointerDownHandler
{
	public bool interactable = true;

	public SwitchButtonClickedEvent onClick = new SwitchButtonClickedEvent();

	[HideInInspector]
	public RectTransform rectTransform;

	protected override void Awake()
	{
		base.Awake();
		rectTransform = GetComponent<RectTransform>();
	}

	private void Press()
	{
		if (!IsActive() || !interactable)
			return;

		onClick.Invoke();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Press();
	}

	[Serializable]
	public class SwitchButtonClickedEvent : UnityEvent { }
}
