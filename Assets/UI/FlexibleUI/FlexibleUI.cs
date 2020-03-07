using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.UI
{
	[ExecuteInEditMode]
	public abstract class FlexibleUI : MonoBehaviour
	{
		[BoxGroup("References")]
		public FlexibleUIData flexibleUIData;

		private void Awake()
		{
			if (flexibleUIData)
			{
				OnSkinUI();
			}
		}

#if UNITY_EDITOR
		public virtual void OnEnable()
		{
			FlexibleUIData[] aux = Utility.FindAssetsByType<FlexibleUIData>("Assets");
			flexibleUIData = aux[0];


		}

		public virtual void Update()
		{
			if (Application.isEditor && flexibleUIData)
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