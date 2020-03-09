
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using NUNet;
using UnityEngine.SceneManagement;
using TMPro;

namespace Game
{
	public class NetworkMenuManager : MonoBehaviour
	{
		public LobbyManager lobbyManager;

		public TMP_InputField nameField;

		[SerializeField]
		private InputField ipAddressField;

		[SerializeField]
		private InputField portField;
		private string lastPort = "";

		[SerializeField]
		private Transform serverListPanel;

		[SerializeField]
		private GameObject serverEntryPrefab;

		private HashSet<IPEndPoint> availableServers;

		private void Awake()
		{
			availableServers = new HashSet<IPEndPoint>();

			//Register Callbacks
			NUClient.onBroadcastResponse += ServerFound;
			NUClient.onConnected += ConnectedToServer;
		}

		private void Start()
		{
			NUClient.SetupBroadcast(NUUtilities.ListIPv4Addresses()[0]);
			nameField.text = "GUESS " + UnityEngine.Random.Range(100, 1000).ToString();
			ipAddressField.text = "192.168.0.4";
			portField.text = "25565";
		}

		private void Update()
		{
			LobbyManager.playerName = nameField.text;

			if (Input.GetKeyUp(KeyCode.Space))
			{
				NUClient.Broadcast(new Packet("PING"));
			}
		}

		private void ConnectedToServer()
		{
			lobbyManager.gameObject.SetActive(true);
			Packet packet = new Packet("PlayerConnected|" + "000" + ";" + LobbyManager.playerName);
			NUClient.SendReliable(packet);
		}

		public void ConnectToServer()
		{
			Debug.Log("Connecting to server: " + ipAddressField.text + ":" + portField.text);
			NUClient.Connect(ipAddressField.text, ushort.Parse(portField.text));
		}

		public void ConnectToServer(IPEndPoint endPoint, GameObject serverEntry)
		{
			Debug.Log("Connecting to server: " + endPoint.ToString());
			NUClient.Connect(endPoint.Address, (ushort)endPoint.Port);
		}

		public void ValidatePort(string port)
		{
			foreach (char c in port)
			{
				if ((byte)c < (byte)'0' || (byte)c > (byte)'9')
				{
					portField.text = lastPort;
					return;
				}
			}

			int intPort = 1;
			if (int.TryParse(port, out intPort))
			{
				if (intPort > ushort.MaxValue || intPort < 1)
					portField.text = lastPort;
			}


			lastPort = port;
		}

		public void CreateNewServer()
		{
			NUServer.Start(NUUtilities.ListIPv4Addresses()[0]);
			NUClient.Connect(NUUtilities.ListIPv4Addresses()[0]);
			lobbyManager.readyButton.gameObject.SetActive(true);
			LobbyManager.isHost = true;
		}

		public void ServerFound(BroadcastPacket brdPacket)
		{
			//Extract port from package
			ushort port = ushort.Parse(brdPacket.packet.GetMessageData());
			IPEndPoint endPoint = new IPEndPoint(brdPacket.origin, (int)port);

			//Try to add server
			if (!availableServers.Add(endPoint))
				return;

			//Instantiate GUI Prefab
			GameObject serverEntry = GameObject.Instantiate(serverEntryPrefab, serverListPanel);

			Text[] texts = serverEntry.GetComponentsInChildren<Text>();
			//Update IPAddress
			texts[0].text = brdPacket.origin.ToString();
			//Update Port
			texts[1].text = port.ToString();

			Button connect = serverEntry.GetComponentInChildren<Button>();
			connect.onClick.AddListener(() => { ConnectToServer(endPoint, serverEntry); });

		}

	}
}