using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[CreateAssetMenu(fileName = "UIStyle", menuName = "Game/UI/UIStyle", order = 0)]
	public class UIStyle : SingletonSO<UIStyle>
	{
		[BoxGroup("References")]
		public FlexibleUIData flexibleUIData;

		[BoxGroup("Properties")]
		public Font font;

		[BoxGroup("Properties")]
		public TMP_FontAsset fontAsset;

		[BoxGroup("Properties")]
		public Color lineColor;

		[BoxGroup("Properties")]
		public Color textHighlightColor;

		[BoxGroup("Icons")]
		public Sprite healthIcon;

		[BoxGroup("Icons")]
		public Sprite energyIcon;

		[BoxGroup("Icons")]
		public Sprite carbonIcon;

		[BoxGroup("Icons")]
		public Sprite damageIcon;

		[BoxGroup("Icons")]
		public Sprite aimIcon;

#if UNITY_EDITOR
		private void OnValidate()
		{
			UpdateFonts();
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



		///// <summary>
		///// Default static method of the <see cref="SingletonSO{T}.instance"/> creation.
		///// </summary>
		//[MenuItem("[Game]/Create/UIStyle", priority = 0)]
		//private static void OnCreate()
		//{
		//	Create();
		//}

		/// <summary>
		/// Default static method of the <see cref="InspectorWindow"/> based on <see cref="SingletonSO{T}.Instance"/>.
		/// </summary>
		[MenuItem("[Game]/UI/UIStyle")]
		private static void OnOpenWindow()
		{
			OpenWindow();
		}
#endif
	}
}