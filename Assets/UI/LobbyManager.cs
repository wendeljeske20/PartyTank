
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using NUNet;
using UnityEngine.SceneManagement;
using Game;

public class LobbyManager : MonoBehaviour
{
	public static List<Guid> connectedPlayers = new List<Guid>();
	public bool allReady;
	public string playerName;

	public Transform teamPanel;

	//public PlayerLobbyPanel playerPanelPrefab;
	public PlayerLobbyPanel[] lobbyPanels;

	public Dictionary<Guid, PlayerLobbyPanel> playerLobbyPanels = new Dictionary<Guid, PlayerLobbyPanel>();

	private void Awake()
	{
		NUServer.onClientConnected += ClientConnected;
		NUServer.onClientReconnected += ClientReconnected;

		//Remove'em when they Disconnect
		//NUServer.onClientDisconnected += PlayerDisconnectFromServer;
		//NUServer.onClientTimedOut += PlayerTimedOutFromServer;
		NUServer.onClientPacketReceived += ServerReceivedPacket;

		//If is Server and Client, register Player
		if (NUServer.started && NUClient.connected)
		{
			LobbyManager.connectedPlayers.Add(NUClient.guid);
		}

		NUClient.onPacketReceived += ClientReceivedPacket;
		NUClient.onDisconnected += ClientDisconnected;

		for (int i = 0; i < lobbyPanels.Length; i++)
		{
			int index = i;
			lobbyPanels[i].joinButton.onClick.AddListener(() => Join(index));
		}

		gameObject.SetActive(false);
	}

	private void Update()
	{

	}

	private void Join(int index)
	{
		Packet packet = new Packet("PlayerJoin|" + playerName + "zzz" + ";" + index);
		NUClient.SendReliable(packet);
	}

	private void ClientConnected(Guid guid)
	{
		connectedPlayers.Add(guid);

		Packet packet = new Packet("PlayerConnected|");
		NUClient.SendReliable(packet);
		Debug.Log("Connected");
	}

	private void ClientReconnected(Guid guid)
	{
		connectedPlayers.Add(guid);
		Debug.Log("Reconnect");
	}

	private void ClientDisconnected()
	{

	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		if (!LobbyManager.connectedPlayers.Contains(guid))
			return;

		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');

		Debug.Log("Received message: " + msg);

		if (args[0] == "PlayerConnected")
		{
			string sendMsg = "PlayerConnected";
			foreach (var panel in playerLobbyPanels)
			{
				sendMsg += string.Format("|{0};{1};{2}", panel.Key.ToString(), panel.Value.nameText.text, panel.Value.index);
			}

			Debug.Log("Send message: " + sendMsg);
			NUServer.SendReliable(new Packet(sendMsg, connectedPlayers.ToArray()));

		}
		else if (args[0] == "PlayerJoin")
		{
			string[] data = args[1].Split(';');

			int index = int.Parse(data[1]);
			string name = data[0];

			PlayerLobbyPanel playerPanel = lobbyPanels[index];
			playerPanel.index = index;
			playerPanel.nameText.text = name;


			if (NUClient.connected && guid == NUClient.guid) //Is Server Player
			{

			}

			playerLobbyPanels.Add(guid, playerPanel);



			string sendMsg = "PlayerJoin";
			foreach (var panel in playerLobbyPanels)
			{
				sendMsg += string.Format("|{0};{1};{2}", panel.Key.ToString(), panel.Value.nameText.text, panel.Value.index);
			}

			Debug.Log("Send message: " + sendMsg);


			NUServer.SendReliable(new Packet(sendMsg, connectedPlayers.ToArray()));
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
		Debug.Log("Received message: " + msg);

		if (args[0] == "PlayerConnected")
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');

				Guid guid = new Guid(data[0]);
				int index = int.Parse(data[2]);
				string name = data[1];

				PlayerLobbyPanel playerPanel = lobbyPanels[index];
				playerPanel.index = index;
				playerPanel.nameText.text = name;

				//Might be a reconnected player
				if (playerLobbyPanels.TryGetValue(guid, out playerPanel))
				{
					Debug.Log("same   " + guid);
					continue;
				}

				if (guid == NUClient.guid)
				{
					//playerObj.AddComponent<PlayerBehaviour>();
				}

				playerLobbyPanels.Add(guid, playerPanel);
			}
		}
		else if (args[0] == "PlayerJoin") //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');

				Guid guid = new Guid(data[0]);
				int index = int.Parse(data[2]);
				string name = data[1]; ;

				PlayerLobbyPanel playerPanel = lobbyPanels[index];

				playerPanel.index = index;
				playerPanel.nameText.text = name;

				//Might be a reconnected player
				if (playerLobbyPanels.TryGetValue(guid, out playerPanel))
				{
					Debug.Log("same   " + guid);
					continue;
				}



				if (guid == NUClient.guid)
				{
					//playerObj.AddComponent<PlayerBehaviour>();
				}

				playerLobbyPanels.Add(guid, playerPanel);
			}
		}
		else if (args[0] == "Sta") //State Data
		{

		}
		else if (args[0] == "Dsc")
		{
			//Guid guid = new Guid(args[1]);
			//GameObject playerObj;
			//if (playerObjects.TryGetValue(guid, out playerObj))
			//{
			//	playerObj.SetActive(false);
			//}
		}

		if (packet.id >= 0)
		{
			Debug.LogError(msg);
		}
	}

	public void StartMatch()
	{
		SceneManager.LoadScene(1);
	}

}
