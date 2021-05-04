using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : LevelScript
{
    void Awake()
    {
        // From LevelScript:
        this.gameState = GameState.EDIT;
        this.tileMapRef = null;
        this.PC = null;
        this.audioRef = FindObjectOfType<AudioManager>();
    }

    void Start()
    {
        this.audioRef.PlayTheme(AudioManager.EventThemeOptions.MAINMENU);

        this.enabled = false;
    }
}
