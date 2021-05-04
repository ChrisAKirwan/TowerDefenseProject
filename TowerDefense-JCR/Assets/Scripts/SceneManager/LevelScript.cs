using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.TileMap;
using UnityEngine.SceneManagement;

public class LevelScript : MonoBehaviour
{
    [HideInInspector] public enum GameState { PLACECASTLE, BUILDPHASE, COMBATPHASE, EDIT }
    [HideInInspector] public GameState gameState { get; protected set; }
    public TileMap tileMapRef { get; protected set; }
    public PlayerController PC { get; protected set; }
    public AudioManager audioRef { get; protected set; }


    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
