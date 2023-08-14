using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    public void StartGame1()
    {
        // Singleplayer
        manager.maxPlayers = 1;
        manager.NextScene();
    }

    public void StartGame2()
    {
        // Multiplayer
        manager.maxPlayers = 2;
        manager.NextScene();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
