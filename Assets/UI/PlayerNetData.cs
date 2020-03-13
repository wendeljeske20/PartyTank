using UnityEngine;

public class PlayerNetData
{
	public System.Guid guid;

	public int id;

	public string name;

	public Team team;

	public bool isLocal;
	

	//public bool ready;
}

public class TeamData
{
	public Team team;
	public Color color;

}
