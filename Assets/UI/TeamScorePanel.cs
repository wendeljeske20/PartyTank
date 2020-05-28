using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamScorePanel : MonoBehaviour
{
	public TextMeshProUGUI scoreText;
	public ScoreElement[] scoreElements;
	
	public void ResetPanel()
	{
		scoreText.text = "0";
	}
}
