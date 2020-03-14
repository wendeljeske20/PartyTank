
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NUNet;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
	public enum GameMode
	{
		FREE_FOR_ALL,
		TEAM_2V2,
	}

	public static GameMode gameMode = GameMode.TEAM_2V2;

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

				text.text = pData.name + "     " + guids[i].ToString().Substring(0, 10) + "     " + pData.lobbyIndex;
			}
			else
			{
				text.text = "...";
			}
		}
	}

	private void ClientConnected(Guid guid)
	{
		Debug.Log("Connected");
	}

	private void ClientReconnected(Guid guid)
	{
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

	private void Join(int lobbyIndex)
	{
		Packet packet = new Packet((int)Message.PLAYER_JOIN + "|" + playerName + ";" + lobbyIndex);
		NUClient.SendReliable(packet);
	}

	public void SendStartMatch()
	{
		NUServer.SendReliable(new Packet(Message.START_MATCH.ToString("d"), NUServer.GetConnectedClients()));
		StartMatch();
	}

	private void StartMatch()
	{
		SceneManager.LoadScene(1);
	}

	private void PlayerDisconnectFromServer(Guid guid)
	{
		int index = playerDatas[guid].lobbyIndex;
		lobbyPanels[index].nameText.text = "...";

		playerDatas.Remove(guid);

		Packet packet = new Packet((int)Message.PLAYER_DISCONNECTED + "|" + guid, NUServer.GetConnectedClients()); ;
		NUServer.SendReliable(packet);
	}

	private void ServerReceivedPacket(Guid guid, Packet packet)
	{
		if (!NUServer.clients.ContainsKey(guid))
			return;

		string msg = packet.GetMessageData();
		string[] args = msg.Split('|');
		int msgID;
		int.TryParse(args[0], out msgID);

		Debug.Log("Received message: " + msg);

		if (msgID == (int)Message.PLAYER_CONNECTED)
		{
			string[] data = args[1].Split(';');
			string name = data[0];

			PlayerNetData playerData = new PlayerNetData(name, guid, 0);

			//Debug.Log("guid   " + clientGuid);
			if (playerDatas.ContainsKey(guid))
			{
				//Debug.Log("continue " + playerData.name);
				Debug.Log("RECONNECT" + playerData.name);
				return;
			}
			playerDatas.Add(guid, playerData);

			string sendMsg = Message.PLAYER_CONNECTED.ToString("d");
			foreach (var pData in playerDatas)
			{
				sendMsg += string.Format("|{0};{1}",
					pData.Key.ToString(),
					pData.Value.name
				);
			}

			Debug.Log("Send message: " + sendMsg);
			NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));

			sendMsg = Message.PLAYER_JOIN.ToString("d");
			foreach (var pData in playerDatas)
			{
				if (pData.Value.lobbyIndex != -1)
				{
					sendMsg += string.Format("|{0};{1};{2}",
						pData.Key.ToString(),
						pData.Value.name,
						pData.Value.lobbyIndex
					);
				}
			}

			Debug.Log("Send message: " + sendMsg);

			NUServer.SendReliable(new Packet(sendMsg, new Guid[] { guid }));

		}
		else if (msgID == (int)Message.PLAYER_DISCONNECTED)
		{
			string[] data = args[1].Split(';');
			//Guid guid = new Guid(data[0]);
			int index = playerDatas[guid].lobbyIndex;

			lobbyPanels[index].ResetPanel();

			var pData = playerDatas[guid];

			string sendMsg = string.Format("{0}|{1};{2};{3}",
				Message.PLAYER_DISCONNECTED.ToString("d"),
				guid,
				pData.name,
				pData.lobbyIndex
			);

			playerDatas.Remove(guid);

			Debug.Log("Send message: " + sendMsg);

			NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));

		}
		else if (msgID == (int)Message.PLAYER_JOIN)
		{
			string[] data = args[1].Split(';');

			string name = data[0];
			int index = int.Parse(data[1]);
			int lastIndex = playerDatas[guid].lobbyIndex;
			Team lastTeam = (Team)lastIndex;

			if (lastTeam != Team.UNDEFINED)
			{
				lobbyPanels[lastIndex].ResetPanel();
			}

			PlayerLobbyPanel playerPanel = lobbyPanels[index];
			playerPanel.nameText.text = name;
			playerPanel.joinButton.gameObject.SetActive(false);

			if (guid == NUClient.guid)
			{
				playerPanel.nameText.color = Utility.HtmlToColor("#0099FF");
			}

			PlayerNetData playerData = playerDatas[guid];
			playerData.lobbyIndex = index;
			playerData.UpdateTeam();


			if (NUClient.connected && guid == NUClient.guid) //Is Server Player
			{

			}


			string sendMsg = Message.PLAYER_JOIN.ToString("d");
			foreach (var pData in playerDatas)
			{
				if (pData.Value.lobbyIndex != -1)
				{
					sendMsg += string.Format("|{0};{1};{2}",
						pData.Key.ToString(),
						pData.Value.name,
						pData.Value.lobbyIndex
					);
				}
			}

			Debug.Log("Send message: " + sendMsg);

			NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));
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
		int msgID;
		int.TryParse(args[0], out msgID);
		Debug.Log("Received message: " + msg);

		if (msgID == (int)Message.PLAYER_CONNECTED)
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');

				Guid guid = new Guid(data[0]);
				string name = data[1];


				//PlayerLobbyPanel playerPanel = lobbyPanels[index];
				//playerPanel.nameText.text = name;

				PlayerNetData playerData = new PlayerNetData(name, guid, 0);
				//Debug.Log("lobby index:      " + index);


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
		else if (msgID == (int)Message.PLAYER_DISCONNECTED)
		{
			string[] data = args[1].Split(';');

			Guid guid = new Guid(data[0]);
			//string name = data[1];
			int index = playerDatas[guid].lobbyIndex; //int.Parse(data[2]);

			lobbyPanels[index].ResetPanel();

			//playerDatas.Remove(guid);
		}
		else if (msgID == (int)Message.PLAYER_JOIN) //Player Profile Data
		{
			for (int i = 1; i < args.Length; i++)
			{
				string[] data = args[i].Split(';');

				Guid guid = new Guid(data[0]);
				int index = int.Parse(data[2]);
				string name = data[1];

				int lastIndex = playerDatas[guid].lobbyIndex;
				if (lastIndex != -1)
				{
					lobbyPanels[lastIndex].ResetPanel();
				}

				PlayerLobbyPanel playerPanel = lobbyPanels[index];
				playerPanel.nameText.text = name;
				if (guid == NUClient.guid)
				{
					playerPanel.nameText.color = Utility.HtmlToColor("#0099FF");
				}
				playerPanel.joinButton.gameObject.SetActive(false);


				playerDatas[guid].lobbyIndex = index;
				playerDatas[guid].UpdateTeam();

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
		else if (msgID == (int)Message.START_MATCH)
		{
			StartMatch();
		}

		if (packet.id >= 0)
		{
			Debug.LogError(msg);
		}
	}



}
