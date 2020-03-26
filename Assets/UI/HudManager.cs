using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
	public TMPro.TextMeshProUGUI counterText;

	public MatchManager matchManager;

	private void Awake()
	{
		matchManager.OnRoundCounterStarted = StartCounterText;
	}

	public IEnumerator StartCounterText()
	{
		counterText.gameObject.SetActive(true);
		float counter = 3;

		while (counter > 0.1f)
		{
			counter -= Time.deltaTime;
			counterText.text = Mathf.Ceil(counter).ToString("F0");

			yield return null;
		}

		counterText.text = "GO";
		yield return new WaitForSeconds(1.0f);
		counterText.gameObject.SetActive(false);
		matchManager.StartRound();
	}
}
