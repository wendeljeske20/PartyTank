using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
	public Transform content;

	private void Start()
	{
		Hide();
	}

	private bool ShoudEnableTeamScorePanel(int index)
	{
		foreach (var data in LobbyManager.playerDatas)
		{
			if (data.Value.lobbyIndex == index)
			{
				return true;
			}
		}

		return false;
	}

	public void UpdateLayout()
	{
		int playerCount = LobbyManager.playerDatas.Count;
		int teamCount = Mathf.FloorToInt(4 / ((int)GameStats.gameMode + 1));
		int elementCount = teamCount / 4;

		for (int i = 0; i < content.childCount; i++)
		{
			TeamScorePanel teamScorePanel = content.GetChild(i).GetComponent<TeamScorePanel>();

			if (ShoudEnableTeamScorePanel(i))
			{
				teamScorePanel.gameObject.SetActive(true);
				teamScorePanel.background.color = GameManager.Instance.style.teamColors[i];
				teamScorePanel.image.color = GameManager.Instance.style.teamColors[i];

				for (int j = 0; j < elementCount; j++)
				{
					ScoreElement scoreElement = teamScorePanel.content.GetChild(j).GetComponent<ScoreElement>();

					if (ShoudEnableTeamScorePanel(j))
					{
						scoreElement.gameObject.SetActive(true);
						continue;
					}

					scoreElement.gameObject.SetActive(false);
				}

				continue;
			}

			teamScorePanel.gameObject.SetActive(false);
		}
	}

	public void UpdateScores()
	{
		int playerCount = LobbyManager.playerDatas.Count;
		int teamCount = Mathf.FloorToInt(4 / ((int)GameStats.gameMode + 1));
		int elementCount = teamCount / 4;

		for (int i = 0; i < content.childCount; i++)
		{
			TeamScorePanel teamScorePanel = content.GetChild(i).GetComponent<TeamScorePanel>();

			if (i < teamCount)
			{
				//teamScorePanel.scoreText.text = LobbyManager.playerDatas.FirstOrDefault(
				//	x => (int)x.Value.teamData.team == i
				//).Value.teamData.score.ToString();

				for (int j = 0; j < elementCount; j++)
				{
					ScoreElement scoreElement = teamScorePanel.content.GetChild(j).GetComponent<ScoreElement>();

					if (j < elementCount)
					{
						//scoreElement.deathsText.text = LobbyManager.playerDatas.First(
						//	x => i * 2 + j == x.Value.lobbyIndex
						//).Value.deathsScore.ToString();

						continue;
					}

				}

				continue;
			}

		}
	}

	public void IncreaseTeamScore(Team team)
	{

	}

	public void Show()
	{
		gameObject.SetActive(true);
		UpdateScores();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
