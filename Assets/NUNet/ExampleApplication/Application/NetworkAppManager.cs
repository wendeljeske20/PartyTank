
//System Includes
using Guid = System.Guid;
using Array = System.Array;
using System.Collections.Generic;

//Unity Includes
using UnityEngine;
using UnityEngine.UI;

//Network Specifics
using NUNet;
using UnityEngine.SceneManagement;

public class NetworkAppManager : MonoBehaviour
{
	[SerializeField]
	private List<Transform> spawnPositions;

	private Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

	private Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();

	[SerializeField]
	private Player playerClientPrefab;

	[SerializeField]
	private Player playerServerPrefab;

	private void Awake()
	{
		//Register Players when they Connect
		NUServer.onClientConnected += ClientConnected;
		NUServer.onClientReconnected += ClientReconnected;

		//Remove'em when they Disconnect
		NUServer.onClientDisconnected += PlayerDisconnectFromServer;
		NUServer.onClientTimedOut += PlayerTimedOutFromServer;
		NUServer.onClientPacketReceived += ServerReceivedPacket;

		//If is Server and Client, register Player
		if (NUServer.started && NUClient.connected)
		{
			LobbyManager.connectedPlayers.Add(NUClient.guid);
		}

		NUClient.onPacketReceived += ClientReceivedPacket;
		NUClient.onDisconnected += ClientDisconnected;
	}

	private void Start()
	{
		Spawn();
	}
	private void ClientConnected(Guid id)
	{
		LobbyManager.connectedPlayers.Add(id);
	}

	private void ClientReconnected(Guid id)
	{
		LobbyManager.connectedPlayers.Add(id);
	}

	private void ClientDisconnected()
	{
		SceneManager.LoadScene(0);
	}

	private void FixedUpdate()
	{
		if (NUServer.started) //Is Server!
		{
			//Send Game-state to everyone online
			Packet stateData = new Packet(GetStateMsg(), NUServer.GetConnectedClients());
			NUServer.SendUnreliable(stateData);
		}
	}

	public void Spawn()
	{
		Packet spawnPacket = new Packet("Spawn|" + LobbyManager.playerName);
		Debug.Log(LobbyManager.playerDatas[NUClient.guid].name);
		NUClient.SendReliable(spawnPacket);
	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		if (!LobbyManager.connectedPlayers.Contains(guid))
			return;

		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');

		if (args[0] == "Spawn")
		{
			int index = LobbyManager.playerDatas[guid].lobbyIndex;

			if (index != -1)
			{
				Debug.Log("INDEX1  " + index);
				Player player = GameObject.Instantiate(playerServerPrefab,
					spawnPositions[index].position,
					spawnPositions[index].rotation);

				if (NUClient.connected && guid == NUClient.guid) //Is Server Player
				{
					player.isLocal = true;
				}

				player.name = "Player (" + args[1] + ")";
				player.GetComponentInChildren<Text>().text = args[1];
				players.Add(guid, player);
			}

			string playerData = "Spawn";
			foreach (var player in players)
			{
				playerData += "|" + player.Key.ToString() + ";" + player.Value.name;
			}

			List<Guid> guids = LobbyManager.connectedPlayers;
			NUServer.SendReliable(new Packet(playerData, guids.ToArray()));
			NUServer.SendReliable(new Packet(GetStateMsg(), guids.ToArray()));
		}
		else if (args[0] == "Inp")
		{
			string[] data = args[1].Split(';');
			Player player;
			if (players.TryGetValue(guid, out player))
			{
				player.DecodeVelocity(data[0]);
				player.DecodeTowerRotation(data[1]);
			}
		}
		else if (args[0] == "Shoot")
		{
			Player player;
			if (players.TryGetValue(guid, out player))
			{
				Projectile projectile = player.weapon.Shoot();
				int index = projectile.gameObject.GetInstanceID();
				projectiles.Add(index, projectile);

				string sendMsg = "Shoot|" + guid.ToString() + ";" + index;

				List<Guid> guids = LobbyManager.connectedPlayers;
				NUServer.SendReliable(new Packet(sendMsg, guids.ToArray()));
			}
		}

		if (packet.id >= 0)
		{
			Debug.LogError(msg);
		}
	}

	private void ClientReceivedPacket(Packet packet)
	{
		if (NUServer.started) //Is Server and Client
			return;

		string msg = packet.GetMessageData();
		Debug.Log("Received message: " + msg);
		string[] args = msg.Split('|');

		if (args[0] == "Spawn") //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');
				Guid guid = new Guid(data[0]);
				string name = data[1];

				Player player;

				//Might be a reconnected player
				if (players.TryGetValue(guid, out player))
				{

					continue;
				}
				int index = LobbyManager.playerDatas[guid].lobbyIndex;
				Debug.Log("INDEX2  " + index);

				player = GameObject.Instantiate(playerClientPrefab,
					spawnPositions[index].position,
					spawnPositions[index].rotation);

				if (guid == NUClient.guid)
				{
					player.isLocal = true;
				}
				player.name = "Player (" + name + ")";
				player.GetComponentInChildren<Text>().text = name;
				players.Add(guid, player);
			}
		}
		else if (args[0] == "State") //State Data
		{
			for (int i = 1; i < players.Count + 1; i++)
			{
				string[] data = args[i].Split(';');
				Guid guid = new Guid(data[0]);
				Player player;
				if (players.TryGetValue(guid, out player))
				{
					player.DecodePosition(data[1]);
					player.DecodeRotation(data[2]);
					player.DecodeTowerRotation(data[3]);
				}
			}
			for (int j = players.Count + 1; j < args.Length; j++)
			{
				string[] data = args[j].Split(';');

				int id = int.Parse(data[0]);
				projectiles[id].DecodePosition(data[1]);
			}
		}
		else if (args[0] == "Shoot")
		{
			string[] data = args[1].Split(';');
			Guid guid = new Guid(data[0]);

			Player player;

			if (players.TryGetValue(guid, out player))
			{
				int id = int.Parse(data[1]);
				Projectile projectile = player.weapon.Shoot();
				projectiles.Add(id, projectile);

				if (guid == NUClient.guid)
				{
					Destroy(projectile.GetComponent<Rigidbody>());
				}
			}
		}
		else if (args[0] == "Dsc")
		{
			Guid guid = new Guid(args[1]);
			Player player;
			if (players.TryGetValue(guid, out player))
			{
				player.gameObject.SetActive(false);
			}
		}

		if (packet.id >= 0)
		{
			Debug.LogError(msg);
		}
	}

	private void PlayerDisconnectFromServer(Guid guid)
	{
		Player player;
		if (players.TryGetValue(guid, out player))
		{
			GameObject.Destroy(player.gameObject);
			players.Remove(guid);
		}

		NUServer.SendReliable(new Packet("Dsc|" + guid, NUServer.GetConnectedClients()));
		LobbyManager.connectedPlayers.Remove(guid);
	}

	private void PlayerTimedOutFromServer(Guid guid)
	{
		Player player;
		if (players.TryGetValue(guid, out player))
		{
			player.gameObject.SetActive(false);
		}

		NUServer.SendReliable(new Packet("Dsc|" + guid, NUServer.GetConnectedClients()));
		LobbyManager.connectedPlayers.Remove(guid);
	}

	private string GetStateMsg()
	{
		string stateData = "State";
		foreach (var player in players)
		{
			stateData += string.Format("|{0};{1};{2};{3}",
				player.Key.ToString(),
				player.Value.EncodePosition(),
				player.Value.EncodeRotation(),
				player.Value.EncodeTowerRotation()
			);
		}
		foreach (var projectile in projectiles)
		{
			stateData += string.Format("|{0};{1}",
				projectile.Key.ToString(),
				projectile.Value.EncodePosition()
			);
		}
		return stateData;
	}

	private byte[] GetStateData()
	{
		byte[] state = new byte[players.Count * (32/*GUID*/ + 12/*XYZ*/)];
		int i = 0;
		foreach (var player in players)
		{
			//Copy GUID Data
			Array.Copy(player.Key.ToByteArray(), 0, state, i * 44, 32);

			//Copy Position Data
			Vector3 pos = player.Value.transform.position;
			byte[] posData = NUUtilities.GetBytes(pos);
			Array.Copy(posData, 0, state, i * 44 + 32, 12);
		}
		return state;
	}

	private void OnApplicationQuit()
	{
		NUServer.Shutdown();
	}

}
