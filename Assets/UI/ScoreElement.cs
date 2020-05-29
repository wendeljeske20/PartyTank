using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreElement : MonoBehaviour
{
	public TMPro.TextMeshProUGUI nameText;

	public TMPro.TextMeshProUGUI deathsText;

	public TMPro.TextMeshProUGUI killsText;
	
	public void ResetPanel()
	{
		nameText.text = "0";
		deathsText.text = "0";
		killsText.text = "0";
	}
}
