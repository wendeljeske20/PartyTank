using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

namespace Game.UI
{
	[CreateAssetMenu(fileName = "FlexibleUIData", menuName = "Game/UI/FlexibleUIData", order = 0)]
	public class FlexibleUIData : ScriptableObject
	{
		[BoxGroup("Properties")]
		public Sprite[] sprites;

		[BoxGroup("Properties")]
		public Color[] colors;

		[BoxGroup("Properties")]
		[Header("Button")]
		public ColorBlock buttonColorBlock;

		[BoxGroup("Properties")]
		public Color buttonTextColor;

	}
}