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
	public string name;
	//public bool ready;
	public int index;
}
