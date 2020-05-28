using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
	public List<TeamScorePanel> scorePanels;

	public void UpdateScores()
	{

	}

	public void IncreaseTeamScore(Team team)
	{

	}

	public void Show()
	{
		UpdateScores();
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
