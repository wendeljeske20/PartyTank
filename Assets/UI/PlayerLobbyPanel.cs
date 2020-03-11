using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyPanel : MonoBehaviour
{
	public TextMeshProUGUI nameText;
	public Image readyImage;
	public Button joinButton;
	
}

public class PlayerNetData
{
	public System.Guid guid;

	public int id;

	public string name;
	
	public int lobbyIndex;

	public bool isLocal;

	//public bool ready;
}
