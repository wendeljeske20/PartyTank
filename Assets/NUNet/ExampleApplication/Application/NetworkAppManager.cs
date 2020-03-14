
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

	public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();

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

		NUClient.onPacketReceived += ClientReceivedPacket;
		NUClient.onDisconnected += ClientDisconnected;
	}

	private void Start()
	{
		if (NUServer.started) //Is Server!
		{
			Spawn();
		}
	}
	private void ClientConnected(Guid id)
	{
	}

	private void ClientReconnected(Guid id)
	{
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
			string sendMsg = GetStateMsg();
			Packet stateData = new Packet(GetStateMsg(), NUServer.GetConnectedClients());
			NUServer.SendUnreliable(stateData);
			Debug.Log(sendMsg.Length + "|||||" + sendMsg);
		}
	}

	public void Spawn()
	{
		foreach (Guid guid in NUServer.GetConnectedClients())
		{
			PlayerNetData playerData = LobbyManager.playerDatas[guid];
			int index = playerData.lobbyIndex;

			if (index != -1)
			{
				//Debug.Log("INDEX1  " + index);
				Player player = GameObject.Instantiate(playerServerPrefab,
					spawnPositions[index].position,
					spawnPositions[index].rotation);

				player.data = playerData;
				player.data.guid = guid;

				if (NUClient.connected && guid == NUClient.guid) //Is Server Player
				{
					player.data.isLocal = true;
				}

				player.name = player.data.name;
				player.GetComponentInChildren<Text>().text = player.name;
				players.Add(guid, player);
			}
		}

		string sendMsg = Message.PLAYER_SPAWN.ToString("d");
		foreach (var player in players)
		{
			sendMsg += "|" + player.Key.ToString() + ";" + player.Value.name;
		}

		Guid[] guids = NUServer.GetConnectedClients();
		NUServer.SendReliable(new Packet(sendMsg, guids));
		NUServer.SendReliable(new Packet(GetStateMsg(), guids));
	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		if (!NUServer.clients.ContainsKey(guid))
			return;

		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');
		int msgID;
		int.TryParse(args[0], out msgID);

		if (msgID == (int)Message.PLAYER_INPUT)
		{
			string[] data = args[1].Split(';');
			Player player;
			if (players.TryGetValue(guid, out player))
			{
				player.DecodeVelocity(data[0]);
				player.DecodeTargetPosition(data[1]);
			}
		}
		else if (msgID == (int)Message.PLAYER_SHOOT)
		{
			Player player;
			if (players.TryGetValue(guid, out player))
			{
				Projectile projectile = player.weapon.Shoot();
				int id = projectile.gameObject.GetInstanceID();
				projectile.id = id;
				projectiles.Add(id, projectile);

				string sendMsg = (int)Message.PLAYER_SHOOT + "|" + guid.ToString() + ";" + id;

				NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));
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
		int msgID;
		int.TryParse(args[0], out msgID);

		if (msgID == (int)Message.PLAYER_SPAWN)
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

				player.data = LobbyManager.playerDatas[guid];
				player.data.guid = guid;

				if (guid == NUClient.guid)
				{
					player.data.isLocal = true;
				}
				player.name = "Player (" + name + ")";
				player.GetComponentInChildren<Text>().text = name;
				players.Add(guid, player);
			}
		}
		else if (msgID == (int)Message.PLAYER_PROJECTILES_STATE)
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
					player.DecodeTargetPosition(data[3]);
				}
			}
			for (int j = players.Count + 1; j < args.Length; j++)
			{
				string[] data = args[j].Split(';');

				int id = int.Parse(data[0]);
				projectiles[id].DecodePosition(data[1]);
			}
		}
		else if (msgID == (int)Message.PLAYER_TAKE_DAMAGE)
		{
			string[] data = args[1].Split(';');
			Guid guid = new Guid(data[0]);
			int damage = int.Parse(data[1]);

			Player player;
			if (players.TryGetValue(guid, out player))
			{
				player.TakeDamage(damage);
			}
		}
		else if (msgID == (int)Message.PLAYER_SHOOT)
		{
			string[] data = args[1].Split(';');
			Guid guid = new Guid(data[0]);

			Player player;

			if (players.TryGetValue(guid, out player))
			{
				Projectile projectile = player.weapon.Shoot();
				Destroy(projectile.GetComponent<Rigidbody>());
				int id = int.Parse(data[1]);
				projectile.id = id;
				projectiles.Add(id, projectile);

				if (guid == NUClient.guid)
				{
					
				}
			}
		}
		else if (msgID == (int)Message.DESTROY_PROJECTILE)
		{
			string[] data = args[1].Split(';');
			int id = int.Parse(data[0]);

			Projectile projectile;

			if (projectiles.TryGetValue(id, out projectile))
			{
				projectile.ToDestroy();
				projectiles.Remove(id);
			}
		}
		else if (msgID == (int)Message.PLAYER_DISCONNECTED)
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

		NUServer.SendReliable(new Packet((int)Message.PLAYER_DISCONNECTED + "|" + guid, NUServer.GetConnectedClients()));
	}

	private void PlayerTimedOutFromServer(Guid guid)
	{
		Player player;
		if (players.TryGetValue(guid, out player))
		{
			player.gameObject.SetActive(false);
		}

		NUServer.SendReliable(new Packet((int)Message.PLAYER_DISCONNECTED + "|" + guid, NUServer.GetConnectedClients()));
	}

	private string GetStateMsg()
	{
		string sendMsg = Message.PLAYER_PROJECTILES_STATE.ToString("d");
		foreach (var player in players)
		{
			sendMsg += string.Format("|{0};{1};{2};{3}",
				player.Key.ToString(),
				player.Value.EncodePosition(),
				player.Value.EncodeRotation(),
				player.Value.EncodeTargetPosition()
			);
		}
		foreach (var projectile in projectiles)
		{
			sendMsg += string.Format("|{0};{1}",
				projectile.Key.ToString(),
				projectile.Value.EncodePosition()
			);
		}
		return sendMsg;
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
