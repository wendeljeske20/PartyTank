using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
	public Guid guid;

	public int id;

	public string name;

	public int lobbyIndex = -1;

	public bool isLocal;

	public TeamData teamData;

	//public int deathsScore;

	//public int killsScore;

	//public bool ready;

	public PlayerData(string name, Guid guid, int id)
	{
		this.name = name;
		this.guid = guid;
		this.id = id;
		this.teamData = new TeamData();
	}

	public void UpdateTeam()
	{
		int index = Mathf.FloorToInt(lobbyIndex / ((int)GameStats.gameMode + 1));
		Debug.Log("INDEX  " + index);
		teamData.team = (Team)(index);

		if (index == -1)
			return;

		teamData.color = GameManager.Instance.style.teamColors[index];
	}
}

[Serializable]
public class TeamData
{
	public Team team;

	public Color color;

	public int score;
}
