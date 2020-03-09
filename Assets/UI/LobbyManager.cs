
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using NUNet;
using UnityEngine.SceneManagement;
using Game;
using UnityEditor;

public class LobbyManager : MonoBehaviour
{
	public static List<Guid> connectedPlayers = new List<Guid>();
	public bool allReady;
	public static string playerName = "123";
	public static bool isHost;

	public Transform teamPanel;

	public Transform spectatorsContent;

	//public PlayerLobbyPanel playerPanelPrefab;
	public PlayerLobbyPanel[] lobbyPanels;

	public Button readyButton;

	public static Dictionary<Guid, PlayerNetData> playerDatas = new Dictionary<Guid, PlayerNetData>();



	private void Awake()
	{
		NUServer.onClientConnected += ClientConnected;
		NUServer.onClientReconnected += ClientReconnected;

		//Remove'em when they Disconnect
		NUServer.onClientDisconnected += PlayerDisconnectFromServer;
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

		readyButton.onClick.AddListener(() => SendStartMatch());

		readyButton.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}

	private void OnApplicationQuit()
	{
		Disconnect();
	}

	private void Update()
	{
		GameObject.Find("Text123").GetComponent<Text>().text = playerName;
		//string msg = "";
		//foreach (var pData in playerDatas)
		//{
		//	msg += "+++ " + pData.Key + "  " + pData.Value.name + "   " + pData.Value.index;
		//}
		//Debug.Log(msg);
		List<Guid> guids = new List<Guid>(playerDatas.Keys);

		if (guids == null)
			return;

		for (int i = 0; i < spectatorsContent.childCount; i++)
		{
			Text text = spectatorsContent.GetChild(i).GetComponent<Text>();

			if (i < playerDatas.Count && i < guids.Count)
			{
				PlayerNetData pData = playerDatas[guids[i]];

				text.text = pData.name + "     " + guids[i] + "     " + pData.index;
			}
			else
			{
				text.text = "...";
			}
		}
	}

	private void ClientConnected(Guid guid)
	{
		connectedPlayers.Add(guid);

		//UnityTask.DelayedAction(1f, () =>
		//{
		//Packet packet = new Packet("PlayerConnected|" + guid + ";" + playerName);
		//NUClient.SendReliable(packet);
		//});
		Debug.Log("Connected");

	}

	private void ClientReconnected(Guid guid)
	{
		connectedPlayers.Add(guid);
		Debug.Log("Reconnect");
	}

	private void ClientDisconnected()
	{
		//Packet packet = new Packet("PlayerDisconnected");
		//NUClient.SendReliable(packet);
		Debug.Log("DSC AAAAAAA");
	}

	private void Disconnect()
	{
		//Packet packet = new Packet("PlayerDisconnected|");
		//NUClient.SendReliable(packet);
	}

	private void Join(int index)
	{
		Packet packet = new Packet("PlayerJoin|" + playerName + ";" + index);
		NUClient.SendReliable(packet);
	}

	public void SendStartMatch()
	{
		Packet packet = new Packet("StartMatch");
		NUClient.SendReliable(packet);
	}

	private void StartMatch()
	{
		SceneManager.LoadScene(1);
	}

	private void PlayerDisconnectFromServer(Guid guid)
	{
		connectedPlayers.Remove(guid);
		int index = playerDatas[guid].index;
		lobbyPanels[index].nameText.text = "111";

		playerDatas.Remove(guid);

		Packet packet = new Packet("PlayerDisconnected|" + guid, connectedPlayers.ToArray()); ;
		NUServer.SendReliable(packet);
	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		if (!connectedPlayers.Contains(guid))
			return;

		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');

		Debug.Log("Received message: " + msg);

		if (args[0] == "PlayerConnected")
		{
			string[] data = args[1].Split(';');
			string name = data[1];

			PlayerNetData playerData = new PlayerNetData();
			playerData.name = name;
			playerData.index = -1;

			//Debug.Log("guid   " + clientGuid);
			if (playerDatas.ContainsKey(guid))
			{
				//Debug.Log("continue " + playerData.name);
				Debug.Log("RECONNECT" + playerData.name);
				return;
			}
			playerDatas.Add(guid, playerData);

			string sendMsg = "PlayerConnected";
			foreach (var pData in playerDatas)
			{
				sendMsg += string.Format("|{0};{1};{2}", pData.Key.ToString(), pData.Value.name, pData.Value.index);
			}

			Debug.Log("Send message: " + sendMsg);
			NUServer.SendReliable(new Packet(sendMsg, connectedPlayers.ToArray()));

			sendMsg = "PlayerJoin";
			foreach (var pData in playerDatas)
			{
				if (pData.Value.index != -1)
					sendMsg += string.Format("|{0};{1};{2}", pData.Key.ToString(), pData.Value.name, pData.Value.index);
			}

			Debug.Log("Send message: " + sendMsg);

			NUServer.SendReliable(new Packet(sendMsg, new Guid[] { guid }));

		}
		else if (args[0] == "PlayerDisconnected")
		{
			string[] data = args[1].Split(';');
			//Guid guid = new Guid(data[0]);
			int index = playerDatas[guid].index;

			PlayerLobbyPanel playerPanel = lobbyPanels[index];
			playerPanel.nameText.text = "111";
			playerPanel.nameText.color = Color.white;
			playerPanel.joinButton.gameObject.SetActive(true);

			string sendMsg = "PlayerDisconnected";
			var pData = playerDatas[guid];

			sendMsg += string.Format("|{0};{1};{2}", guid, pData.name, pData.index);

			playerDatas.Remove(guid);

			Debug.Log("Send message: " + sendMsg);

			List<Guid> guids = new List<Guid>(playerDatas.Keys);
			NUServer.SendReliable(new Packet(sendMsg, guids.ToArray()));

		}
		else if (args[0] == "PlayerJoin")
		{
			string[] data = args[1].Split(';');

			string name = data[0];
			int index = int.Parse(data[1]);

			int lastIndex = playerDatas[guid].index;
			if (lastIndex != -1)
			{
				lobbyPanels[lastIndex].nameText.text = "111";
				lobbyPanels[lastIndex].nameText.color = Color.white;
				lobbyPanels[lastIndex].joinButton.gameObject.SetActive(true);
			}

			PlayerLobbyPanel playerPanel = lobbyPanels[index];
			playerPanel.nameText.text = name;
			if (guid == NUClient.guid)
			{
				playerPanel.nameText.color = Utility.HtmlToColor("#0099FF");
			}

			playerPanel.joinButton.gameObject.SetActive(false);

			playerDatas[guid].index = index;


			if (NUClient.connected && guid == NUClient.guid) //Is Server Player
			{

			}



			string sendMsg = "PlayerJoin";
			foreach (var pData in playerDatas)
			{
				if (pData.Value.index != -1)
					sendMsg += string.Format("|{0};{1};{2}", pData.Key.ToString(), pData.Value.name, pData.Value.index);
			}

			Debug.Log("Send message: " + sendMsg);

			NUServer.SendReliable(new Packet(sendMsg, connectedPlayers.ToArray()));
		}
		else if (args[0] == "StartMatch")
		{
			List<Guid> guids = new List<Guid>(connectedPlayers);
			NUServer.SendReliable(new Packet("StartMatch", guids.ToArray()));
			StartMatch();
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
				string name = data[1];
				int index = int.Parse(data[2]);


				//PlayerLobbyPanel playerPanel = lobbyPanels[index];
				//playerPanel.nameText.text = name;

				PlayerNetData playerData = new PlayerNetData();
				playerData.name = name;
				playerData.index = index;

				//Might be a reconnected player
				//if (playerDatas.TryGetValue(guid, out playerData))
				//{
				//	Debug.Log("same   " + guid);
				//	continue;
				//}

				if (playerDatas.ContainsKey(guid))
				{

					Debug.Log("RECONNECT");
					continue;
				}

				if (guid == NUClient.guid)
				{
					//playerObj.AddComponent<PlayerBehaviour>();
				}

				playerDatas.Add(guid, playerData);
			}
		}
		else if (args[0] == "PlayerDisconnected")
		{
			string[] data = args[1].Split(';');

			Guid guid = new Guid(data[0]);
			//string name = data[1];
			int index = playerDatas[guid].index; //int.Parse(data[2]);


			PlayerLobbyPanel playerPanel = lobbyPanels[index];
			playerPanel.nameText.text = "111";
			playerPanel.nameText.color = Color.white;
			playerPanel.joinButton.gameObject.SetActive(true);

			//playerDatas.Remove(guid);
		}
		else if (args[0] == "PlayerJoin") //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');

				Guid guid = new Guid(data[0]);
				int index = int.Parse(data[2]);
				string name = data[1];

				int lastIndex = playerDatas[guid].index;
				if (lastIndex != -1)
				{
					lobbyPanels[lastIndex].nameText.text = "111";
					lobbyPanels[lastIndex].nameText.color = Color.white;
					lobbyPanels[lastIndex].joinButton.gameObject.SetActive(true);
				}

				PlayerLobbyPanel playerPanel = lobbyPanels[index];
				playerPanel.nameText.text = name;
				if (guid == NUClient.guid)
				{
					playerPanel.nameText.color = Utility.HtmlToColor("#0099FF");
				}
				playerPanel.joinButton.gameObject.SetActive(false);

				playerDatas[guid].index = index;

				//PlayerNetData playerData = new PlayerNetData();
				//playerData.name = name;
				//playerData.index = index;




				//Might be a reconnected player
				//if (playerDatas.TryGetValue(guid, out playerData))
				//{
				//	Debug.Log("same   " + guid);
				//	continue;
				//}

				if (playerDatas.ContainsKey(guid))
				{
					continue;
				}

				if (guid == NUClient.guid)
				{
					//playerObj.AddComponent<PlayerBehaviour>();
				}


				//Debug.Log(playerData);
				//playerDatas.Add(guid, playerData);
			}
		}
		else if (args[0] == "StartMatch")
		{
			StartMatch();
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



}
