using System;
using UnityEngine;
using Guid = System.Guid;

[Serializable]
public class PlayerNetData
{
	public Guid guid;

	public int id;

	public string name;

	public int lobbyIndex = -1;

	public Team team = Team.UNDEFINED;

	public bool isLocal;


	//public bool ready;

	public PlayerNetData(string name, Guid guid, int id)
	{
		this.name = name;
		this.guid = guid;
		this.id = id;
	}

	public void UpdateTeam()
	{
		team = (Team)lobbyIndex;

		if (GameStats.gameMode == GameMode.FREE_FOR_ALL)
			return;

		team = (Team)(Mathf.Floor(lobbyIndex / 2f));
	}
}

public class TeamData
{
	public Team team;
	public Color color;

}
