using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.UI
{
	[ExecuteInEditMode]
	public abstract class FlexibleUI : MonoBehaviour
	{
		private void Awake()
		{
			if (UIStyle.Instance.flexibleUIData)
			{
				OnSkinUI();
			}
		}

#if UNITY_EDITOR

		public virtual void Update()
		{
			if (Application.isEditor && UIStyle.Instance.flexibleUIData)
			{
				OnSkinUI();
			}
		}
#endif
		protected virtual void OnSkinUI()
		{

		}
	}
}