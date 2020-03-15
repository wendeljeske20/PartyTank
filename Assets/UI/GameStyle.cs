using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStyle", menuName = "Game/UI/GameStyle", order = 0)]
public class GameStyle : ScriptableObject
{
    public Color[] teamColors;

    public static GameStyle Instance { get; private set; }

    public GameStyle()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

}
