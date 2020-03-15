using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStats
{
	public static List<string> gameModeNames;
	public static GameMode gameMode = GameMode.TEAM_2V2;

	public static List<string> arenaNames;
	public static int arenaIndex = 1;

	public static int matchesAmount = 1;

	public static List<int> roundTimes;
	public static int roundTime = 60;

	public static float healthMultiplier = 1;

	public static float powerupsMultiplier = 1;

	public static void Setup()
	{
		roundTimes = new List<int>() { 30, 40, 50, 60, 75, 90, 120 };
		arenaNames = new List<string>()
		{ 
			"Deserto",
			"Arena de Ferro"
		};

		gameModeNames = new List<string>()
		{
			"Cada um por si",
			"2 vs 2"
		};
	}


}
