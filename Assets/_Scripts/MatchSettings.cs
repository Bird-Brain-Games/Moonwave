﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatchSettings {

	public static int numPlayers;
	public static int pointsToWin;
	public static List<Color> playerColors;
	public static List<int> playerScores;
	public static int minRange, maxRange;
	public static int setNum;

	static MatchSettings()
	{
		numPlayers = 0;
		pointsToWin = 0;
		playerColors = new List<Color>();
		playerScores = new List<int>();
		Reset();
	}

	public static void Reset()
	{
		numPlayers = 0;
		pointsToWin = 0;
		playerColors.Clear();
		playerScores.Clear();
		minRange = 3;
		maxRange = 3;
		setNum = 0;
	}
	
}
