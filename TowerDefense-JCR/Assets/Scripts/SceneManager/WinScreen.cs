using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : LevelScript
{
    private void Awake()
    {
        // From LevelScript:
        this.gameState = GameState.EDIT;
        this.tileMapRef = null;
        this.PC = null;
        this.audioRef = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.audioRef.PlayTheme(AudioManager.EventThemeOptions.WINSCREEN);
        this.enabled = false;
    }
}
