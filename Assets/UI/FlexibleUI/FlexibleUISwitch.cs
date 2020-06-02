
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[RequireComponent(typeof(Switch))]
	[AddComponentMenu("[Game]/UI/FlexibleUISwitch")]
	public class FlexibleUISwitch : FlexibleUI
	{
		private Switch @switch;
		Text text;
		protected override void OnSkinUI()
		{
			base.OnSkinUI();

			@switch = GetComponent<Switch>();
			text = GetComponentInChildren<Text>();

			//Color color = flexibleUIData.colors[(int)colorType];
			//color.a = image.color.a;
			//image.color = color;
			//image.sprite = flexibleUIData.sprites[(int)spriteType];

			//button.colors = UIStyle.Instance.flexibleUIData.buttonColorBlock;

			//if (text)
			//text.color = UIStyle.Instance.flexibleUIData.buttonTextColor;

			@switch.spriteState = UIStyle.Instance.flexibleUIData.selectableStates[0];
		}
	}
}