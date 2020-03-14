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

	public void ResetPanel()
	{
		nameText.text = "...";
		nameText.color = Color.black;
		joinButton.gameObject.SetActive(true);
	}
}


