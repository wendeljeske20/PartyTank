
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[RequireComponent(typeof(Button))]
	[AddComponentMenu("[Game]/UI/FlexibleUIButton")]
	public class FlexibleUIButton : FlexibleUI
	{
		Button button;
		Text text;
		protected override void OnSkinUI()
		{
			base.OnSkinUI();

			button = GetComponent<Button>();
			text = GetComponentInChildren<Text>();

			//Color color = flexibleUIData.colors[(int)colorType];
			//color.a = image.color.a;
			//image.color = color;
			//image.sprite = flexibleUIData.sprites[(int)spriteType];

			//button.colors = UIStyle.Instance.flexibleUIData.buttonColorBlock;

			//if (text)
			//text.color = UIStyle.Instance.flexibleUIData.buttonTextColor;

			button.spriteState = UIStyle.Instance.flexibleUIData.spriteStates[0];
		}
	}
}