
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

	private Dictionary<Guid, PlayerBehaviour> playerObjects = new Dictionary<Guid, PlayerBehaviour>();
	//private List<Guid> playerGuids = new List<Guid>();
	//private List<GameObject> playerObjects = new List<GameObject>();

	[SerializeField]
	private PlayerBehaviour playerClientPrefab;

	[SerializeField]
	private PlayerBehaviour playerServerPrefab;

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
				PlayerBehaviour playerObj = GameObject.Instantiate(playerServerPrefab,
					spawnPositions[index].position,
					spawnPositions[index].rotation);

				if (NUClient.connected && guid == NUClient.guid) //Is Server Player
				{
					playerObj.isLocal = true;
				}

				playerObj.name = "Player (" + args[1] + ")";
				playerObj.GetComponentInChildren<Text>().text = args[1];
				playerObjects.Add(guid, playerObj);
			}

			string playerData = "Spawn";
			foreach (var player in playerObjects)
			{
				playerData += "|" + player.Key.ToString() + ";" + player.Value.name;
			}

			List<Guid> guids = LobbyManager.connectedPlayers;
			NUServer.SendReliable(new Packet(playerData, guids.ToArray()));
			NUServer.SendReliable(new Packet(GetStateMsg(), guids.ToArray()));
		}
		else if (args[0] == "Inp")
		{
			string plData = args[1];
			PlayerBehaviour playerObj;
			if (playerObjects.TryGetValue(guid, out playerObj))
			{
				string[] inpMsg = plData.Split(':');
				Vector3 input = new Vector3(
					float.Parse(inpMsg[0]),
					float.Parse(inpMsg[1]),
					float.Parse(inpMsg[2])
					);
				Rigidbody rb = playerObj.GetComponent<Rigidbody>();
				rb.velocity = input;
			}
		}
		else if (args[0] == "Jmp")
		{
			PlayerBehaviour playerObj;
			if (playerObjects.TryGetValue(guid, out playerObj))
			{
				//Can Jump check
				RaycastHit hit;
				Vector3 playerPos = playerObj.transform.position;
				if (Physics.Raycast(playerPos, Vector3.down, out hit))
				{
					if (hit.distance > 0.6f)
						return;

					Rigidbody rb = playerObj.GetComponent<Rigidbody>();
					rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
					rb.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);
				}

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
		string[] args = msg.Split('|');
		if (args[0] == "Spawn") //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');
				Guid guid = new Guid(data[0]);
				string name = data[1];

				PlayerBehaviour playerObj;

				//Might be a reconnected player
				if (playerObjects.TryGetValue(guid, out playerObj))
				{

					continue;
				}
				int index = LobbyManager.playerDatas[guid].lobbyIndex;
				Debug.Log("INDEX2  " + index);

				playerObj = GameObject.Instantiate(playerClientPrefab,
					spawnPositions[index].position,
					spawnPositions[index].rotation);

				if (guid == NUClient.guid)
				{
					playerObj.isLocal = true;
				}
				playerObj.name = "Player (" + name + ")";
				playerObj.GetComponentInChildren<Text>().text = name;
				playerObjects.Add(guid, playerObj);
			}
		}
		else if (args[0] == "Sta") //State Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');
				Guid guid = new Guid(data[0]);
				PlayerBehaviour playerObj;
				if (playerObjects.TryGetValue(guid, out playerObj))
				{
					string[] pos = data[1].Split(':');
					Vector3 vPos = new Vector3(
						float.Parse(pos[0]),
						float.Parse(pos[1]),
						float.Parse(pos[2])
						);
					playerObj.transform.position = vPos;
					string[] rot = data[2].Split(':');
					Quaternion qRot = new Quaternion(
						float.Parse(rot[0]),
						float.Parse(rot[1]),
						float.Parse(rot[2]),
						float.Parse(rot[3])
						);
					playerObj.transform.rotation = qRot;
				}
			}
		}
		else if (args[0] == "Dsc")
		{
			Guid guid = new Guid(args[1]);
			PlayerBehaviour playerObj;
			if (playerObjects.TryGetValue(guid, out playerObj))
			{
				playerObj.gameObject.SetActive(false);
			}
		}

		if (packet.id >= 0)
		{
			Debug.LogError(msg);
		}
	}

	private void PlayerDisconnectFromServer(Guid guid)
	{
		PlayerBehaviour playerObject;
		if (playerObjects.TryGetValue(guid, out playerObject))
		{
			GameObject.Destroy(playerObject);
			playerObjects.Remove(guid);
		}

		NUServer.SendReliable(new Packet("Dsc|" + guid, NUServer.GetConnectedClients()));
		LobbyManager.connectedPlayers.Remove(guid);
	}

	private void PlayerTimedOutFromServer(Guid guid)
	{
		PlayerBehaviour playerObject;
		if (playerObjects.TryGetValue(guid, out playerObject))
		{
			playerObject.gameObject.SetActive(false);
		}

		NUServer.SendReliable(new Packet("Dsc|" + guid, NUServer.GetConnectedClients()));
		LobbyManager.connectedPlayers.Remove(guid);
	}

	private string GetStateMsg()
	{
		string stateData = "Sta";
		foreach (var player in playerObjects)
		{
			Vector3 pos = player.Value.transform.position;
			stateData += "|" + player.Key.ToString() + ";" + pos.x.ToString("R") + ":" + pos.y.ToString("R") + ":" + pos.z.ToString("R");
			Quaternion rot = player.Value.transform.rotation;
			stateData += ";" + rot.x.ToString("R") + ":" + rot.y.ToString("R") + ":" + rot.z.ToString("R") + ":" + rot.w.ToString("R");
		}
		return stateData;
	}

	private byte[] GetStateData()
	{
		byte[] state = new byte[playerObjects.Count * (32/*GUID*/ + 12/*XYZ*/)];
		int i = 0;
		foreach (var player in playerObjects)
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
