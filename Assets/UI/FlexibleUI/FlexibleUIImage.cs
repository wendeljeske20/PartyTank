
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("[Game]/UI/FlexibleUIImage")]
	public class FlexibleUIImage : FlexibleUI
	{


		[BoxGroup("Properties")]
		public ColorType colorType;

		[BoxGroup("Properties")]
		public SpriteType spriteType;

		Image image;

		public enum ColorType
		{
			NONE,
			PRIMARY,
			SECONDARY,
			BUTTON,
			TERTIARY,
			QUATERNARY
		}

		public enum SpriteType
		{
			NONE,
			PRIMARY,
			SECONDARY,
			TERTIARY,
			QUATERNARY
		}



		protected override void OnSkinUI()
		{
			base.OnSkinUI();

			image = GetComponent<Image>();
			image.type = Image.Type.Sliced;

			if (colorType != ColorType.NONE)
			{
				Color color = flexibleUIData.colors[(int)colorType - 1];
				color.a = image.color.a;
				image.color = color;
			}

			if (spriteType != SpriteType.NONE)
			{
				image.sprite = flexibleUIData.sprites[(int)spriteType - 1];
			}
		}
	}
}