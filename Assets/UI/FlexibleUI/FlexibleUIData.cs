using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Game.UI
{
	[CreateAssetMenu(fileName = "FlexibleUIData", menuName = "Game/UI/FlexibleUIData", order = 0)]
	public class FlexibleUIData : ScriptableObject
	{
		[BoxGroup("Images"), ListDrawerSettings(Expanded = true)]
		public List<Sprite> sprites;

		[BoxGroup("Images")]
		public Color[] colors;

		[BoxGroup("Buttons"), ListDrawerSettings(Expanded = true)]
		public List<SpriteState> spriteStates;

		[BoxGroup("Buttons"), ListDrawerSettings(Expanded = true)]
		public List<SpriteState> selectableStates;

		//[BoxGroup("Buttons")]
		//[HorizontalLine]
		//public ColorBlock buttonColorBlock;

		[BoxGroup("Buttons")]
		public Color buttonTextColor;

	}
}