
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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

		Image image;

		[BoxGroup("Properties")]
		[ValueDropdown("GetSpriteIndex")]
		public int spriteIndex;

		public enum ColorType
		{
			NONE,
			PRIMARY,
			SECONDARY,
			BUTTON,
			TERTIARY,
			QUATERNARY
		}

		protected override void OnSkinUI()
		{
			base.OnSkinUI();

			image = GetComponent<Image>();

			image.type = Image.Type.Sliced;

			if(colorType != ColorType.NONE)
			{
				Color color = UIStyle.Instance.flexibleUIData.colors[(int)colorType];
				color.a = image.color.a;
				image.color = color;
			}
			
			image.sprite = UIStyle.Instance.flexibleUIData.sprites[spriteIndex];
		}

		private IEnumerable GetSpriteIndex()
		{
			ValueDropdownList<int> items = new ValueDropdownList<int>();
			FlexibleUIData data = UIStyle.Instance.flexibleUIData;
			for (int i = 0; i < data.sprites.Count; i++)
			{
				string name = data.sprites[i].ToString().Split(' ')[0];
				items.Add(name, i);
			}
			return items;
		}
	}
}