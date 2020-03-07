using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[CreateAssetMenu(fileName = "UIStyle", menuName = "Game/UI/UIStyle", order = 0)]
	public class UIStyle : ScriptableObject
	{
		[BoxGroup("Properties")]
		public Font font;

		[BoxGroup("Properties")]
		public TMP_FontAsset fontAsset;

		public Color lineColor;

#if UNITY_EDITOR
		private void OnValidate()
		{
			UpdateFonts();
			Utility.lineColor = lineColor;
		}

		[Button("Update Fonts")]
		public void UpdateFonts()
		{
			Text[] texts = GameObject.FindObjectsOfType<Text>();

			foreach (Text text in texts)
			{
				text.font = font;
			}

			TextMeshProUGUI[] tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>();

			foreach (TextMeshProUGUI tmp in tmps)
			{
				tmp.font = fontAsset;
			}
		}
#endif

	}


}


