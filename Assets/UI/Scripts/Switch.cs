using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Switch : Selectable, ISubmitHandler
{
	[Space]
	public TextMeshProUGUI optionText;

	public TextMeshProUGUI valueText;

	public new Image image;

	public SwitchButton nextButton;

	public SwitchButton previousButton;

	public enum Direction
	{
		Horizontal = 0,
		Vertical = 1
	}

	[Space]
	public Direction direction;

	[SerializeField]
	private int m_Value;

	public List<OptionData> options = new List<OptionData> {
		new OptionData("Desligado"),
		new OptionData("Ligado")
	};

	public SwitchEvent onValueChanged = new SwitchEvent();

	public int value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (options.Count == 0)
			{
				return;
			}

			m_Value = Mathf.Clamp(value, 0, options.Count - 1);
			onValueChanged.Invoke(m_Value);
			RefreshShownValue();
		}
	}

	protected override void Awake()
	{
		base.Awake();

		m_Value = Mathf.Clamp(m_Value, 0, options.Count - 1);
		RefreshShownValue();
		nextButton?.onClick.AddListener(() =>
		{
			Select();
			SetNextOption();
		});
		previousButton?.onClick.AddListener(() =>
		{
			Select();
			SetPreviousOption();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		nextButton?.onClick.RemoveListener(() =>
		{
			Select();
			SetNextOption();
		});
		previousButton?.onClick.RemoveListener(() =>
		{
			Select();
			SetPreviousOption();
		});
	}

	private void SetNextOption()
	{
		if (options.Count == 0)
		{
			return;
		}

		m_Value = (m_Value + 1) % options.Count;
		onValueChanged.Invoke(m_Value);
		RefreshShownValue();
	}

	private void SetPreviousOption()
	{
		if (options.Count == 0)
		{
			return;
		}

		m_Value = (m_Value + options.Count - 1) % options.Count;
		onValueChanged.Invoke(m_Value);
		RefreshShownValue();
	}

	public void RefreshShownValue()
	{
		if (options.Count == 0)
		{
			return;
		}

		if (valueText)
		{
			valueText.text = options[m_Value].text;
		}

		if (image)
		{
			image.sprite = options[m_Value].sprite;
		}

		//RefreshButtonsRects();
	}

	private void RefreshButtonsRects()
	{
		valueText.rectTransform.ForceUpdateRectTransforms();
		valueText.SetAllDirty();


		//UnityTask.DelayedAction(0.01f, () =>
		//{
		float width = valueText.rectTransform.sizeDelta.x / 2 + 60;
		Debug.Log(width);
		nextButton.rectTransform.sizeDelta = new Vector2(width, 60);
		previousButton.rectTransform.sizeDelta = new Vector2(width, 60);
		//});
	}

	public void AddOption(OptionData option)
	{
		options.Add(option);
	}

	public void AddOptions(List<OptionData> options)
	{
		options.AddRange(options);
	}

	public void ClearOptions()
	{
		options.Clear();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		SetNextOption();
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (!IsActive() || !IsInteractable())
		{
			base.OnMove(eventData);
			return;
		}

		switch (eventData.moveDir)
		{
			case MoveDirection.Left:
				if (direction == Direction.Horizontal && FindSelectableOnLeft() == null)
					SetPreviousOption();
				else
					base.OnMove(eventData);
				break;
			case MoveDirection.Right:
				if (direction == Direction.Horizontal && FindSelectableOnRight() == null)
					SetNextOption();
				else
					base.OnMove(eventData);
				break;
			case MoveDirection.Up:
				if (direction == Direction.Vertical && FindSelectableOnUp() == null)
					SetNextOption();
				else
					base.OnMove(eventData);
				break;
			case MoveDirection.Down:
				if (direction == Direction.Vertical && FindSelectableOnDown() == null)
					SetPreviousOption();
				else
					base.OnMove(eventData);
				break;
		}
	}

	[System.Serializable]
	public class OptionData
	{
		public string text;
		public Sprite sprite;

		public OptionData() { }

		public OptionData(string text, Sprite sprite = null)
		{
			this.text = text;
			this.sprite = sprite;
		}
	}

	[System.Serializable]
	public class SwitchEvent : UnityEvent<int> { }
}