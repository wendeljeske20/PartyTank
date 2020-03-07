
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
	public bool allReady;

	private void Awake()
	{
		gameObject.SetActive(false);
	}

	private void Update()
	{

	}
	public void StartMatch()
	{
		SceneManager.LoadScene(1);
	}

}
