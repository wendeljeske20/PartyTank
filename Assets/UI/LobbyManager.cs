
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using NUNet;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
	public static List<Guid> connectedPlayers = new List<Guid>();
	public bool allReady;

	public Transform teamPanel;

	public PlayerLobbyPanel playerPanelPrefab;

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

		gameObject.SetActive(false);
	}

	private void Update()
	{

	}

	private void ClientConnected(Guid id)
	{
		connectedPlayers.Add(id);
		Packet packet = new Packet("Prs|" + "W3W2");
		NUClient.SendReliable(packet);
		Debug.Log("Connect    Prs|" + "W3W2");
	}

	private void ClientReconnected(Guid id)
	{
		connectedPlayers.Add(id);
		Debug.Log("Reconnect");
	}

	private void ClientDisconnected()
	{

	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		Debug.Log("aaaaaaaa");
		if (!LobbyManager.connectedPlayers.Contains(guid))
			return;

		Debug.Log("bbbbbbbb");
		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');
		if (args[0] == "Prs")
		{
			Debug.Log("cccccccc");
			PlayerLobbyPanel playerPanel;
			if (NUClient.connected && guid == NUClient.guid) //Is Server Player
			{
				playerPanel = GameObject.Instantiate(playerPanelPrefab);
				playerPanel.transform.SetParent(teamPanel.transform);

				//playerObj = GameObject.Instantiate(playerServerPrefab,
				//spwnPos, Quaternion.identity);
				//playerObj.AddComponent<PlayerBehaviour>();
			}
			else
			{
				playerPanel = GameObject.Instantiate(playerPanelPrefab);
				playerPanel.transform.SetParent(teamPanel.transform);
				//playerObj = GameObject.Instantiate(playerServerPrefab,
				//spwnPos, Quaternion.identity);
			}

			playerPanel.nameText.text = args[1];
			playerLobbyPanels.Add(guid, playerPanel);



			string playerData = "Prs";
			foreach (var player in playerLobbyPanels)
			{
				playerData += "|" + player.Value.nameText.text; ;
			}


			List<Guid> guids = new List<Guid>(playerLobbyPanels.Keys);
			NUServer.SendReliable(new Packet(playerData, guids.ToArray()));
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
		if (args[0] == "Prs") //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] plData = args[i].Split(';');
				Guid guid = new Guid(plData[0]);


				PlayerLobbyPanel playerPanel;

				//Might be a reconnected player
				if (playerLobbyPanels.TryGetValue(guid, out playerPanel))
				{

					continue;
				}

				playerPanel = GameObject.Instantiate(playerPanelPrefab);
				playerPanelPrefab.transform.SetParent(teamPanel.transform);

				if (guid == NUClient.guid)
				{
					//playerObj.AddComponent<PlayerBehaviour>();
				}
				playerPanel.nameText.text = plData[1];
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
