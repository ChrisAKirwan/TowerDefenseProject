using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Core.TileMap;

[RequireComponent(typeof(MenuManager))]
[RequireComponent(typeof(SpawnManager))]
[RequireComponent(typeof(MapEditorManager))]
public class MainLevel : LevelScript
{
    // Pause State and this.component references.
    [HideInInspector] public bool isPaused { get; private set; }
    private MenuManager UIMenuRef;
    private SpawnManager spawnManagerRef;
    private MapEditorManager mapEditorManagerRef;

    // Notification timer/delay for the castle placement prompt.
    public float castlePromptTime { get; private set; }
    private float castlePromptTimerDelay = 2.0f;

    // Designer Gameplay Modifiers
    [Header("Gameplay Modifiers")]
    public int numWaves = 3;
    public int numRoundsPerWave = 5;
    public float roundLengthInSeconds = 60.0f;

    // Dynamic Gameplay Modifiers/References
    public float roundTimer { get; private set; }
    [HideInInspector] public int currentWave { get; private set; }
    [HideInInspector] public int currentRound { get; private set; }
    private float statMultiplier;
    private int numEnemiesToSpawn;
    private int numEnemiesSpawned;
    private float delayBetweenRounds;

    // Player currency reference
    public float currency { get; private set; }

    // Asset References
    public Castle playerCastleRef { get; private set; }

    private string playMapPath;
    private string playMapName;


    // Class init at GameStart;
    void Awake()
    {
        // From LevelScript:
        this.gameState = GameState.PLACECASTLE;
        this.tileMapRef = FindObjectOfType<TileMap>();
        this.PC = FindObjectOfType<PlayerController>();
        this.audioRef = FindObjectOfType<AudioManager>();

        // From MainLevel:
        this.isPaused = false;
        this.UIMenuRef = this.GetComponent<MenuManager>();
        this.spawnManagerRef = this.GetComponent<SpawnManager>();
        this.mapEditorManagerRef = this.GetComponent<MapEditorManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Only call the Update() function during an active Wave.
        enabled = false;

        // Load Play map.
        SetPlayMapPath();
        string path = playMapPath + playMapName;

        List<List<int>> SavedMap = mapEditorManagerRef.LoadMapFromFile(path);
        if (!tileMapRef.GenerateMapFromList(SavedMap))
            Debug.Log("Failed to Generate Map from Saved File!");

        // Init gameplay variables
        roundTimer = roundLengthInSeconds;
        currentWave = 1;
        currentRound = 1;

        UIMenuRef.UpdateRoundTimer();
        UIMenuRef.UpdateWaveNumber();
        UIMenuRef.UpdateRoundNumber();

        currency = 1000;

        PC.inEditMode = false;
        PC.InitPlayMode();
    }

    void Update()
    {
        // Occurs only during an active wave.
        SpawnLoop();
    }


    // Controls the active game loop for enemy spawning in each round.
    private void SpawnLoop()
    {
        // Update the round timer in the HUD.
        UIMenuRef.UpdateRoundTimer();

        // Check if the round timer has completed.
        if (roundTimer <= 0.0f)
        {
            // Keep the timer at zero to prevent it from being negative.
            roundTimer = 0.0f;
            UIMenuRef.UpdateRoundTimer();

            // Check if this is the last round.
            if (currentRound == numRoundsPerWave)
            {
                // The current wave (last round) only ends if all enemies are killed.
                if (spawnManagerRef.spawnedEnemies.Count == 0 &&
                    numEnemiesToSpawn == numEnemiesSpawned)
                {
                    // Disable the Update() function, reset the round counter, and move to Build Phase.
                    enabled = false;
                    currentRound = 0;

                    audioRef.PlaySFX("WaveEnd");

                    if (currentWave == numWaves)
                        GameOver(true);

                    SetGameState(GameState.BUILDPHASE);
                }
            }
            else
            {
                // Give a brief pause between rounds before starting.
                delayBetweenRounds -= Time.deltaTime;

                if (delayBetweenRounds <= 0.0f)
                {
                    audioRef.PlaySFX("RoundEnd");

                    // End the round, disable Update(), and prepare the next round.
                    enabled = false;
                    StartNextRound();
                }
            }
        }
        else if (roundTimer <= roundLengthInSeconds - (roundLengthInSeconds / (float)numEnemiesToSpawn) * numEnemiesSpawned)
        {
            // Round is still active, spawn threshold met. Spawn an enemy.
            numEnemiesSpawned++;

            // spawn soldier
            Soldier spawnedEnemy = spawnManagerRef.SpawnEnemy(spawnManagerRef.soldierRef).GetComponent<Soldier>();
            // set soldier's multiplier
            spawnedEnemy.GetComponent<Soldier>().FullStatModifier(statMultiplier);

            roundTimer -= Time.deltaTime;
        }
        else
            roundTimer -= Time.deltaTime;
    }

    // External class function to modify the active game state.
    public void SetGameState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.PLACECASTLE:
                InitCastlePhase();
                break;
            case GameState.BUILDPHASE:
                InitBuildPhase();
                break;
            case GameState.COMBATPHASE:
                InitCombatPhase();
                break;
            default:
                break;
        }

        this.gameState = gameState;
    }

    // Handles gamestate change to castle phase.
    private void InitCastlePhase()
    {
        currentWave = 0;
        castlePromptTime = Time.timeSinceLevelLoad + castlePromptTimerDelay;
        audioRef.PlayBuildThemePlaylist();
        PC.InitCastlePhase();
    }

    // Handles gamestate change to build phase.
    private void InitBuildPhase()
    {
        UIMenuRef.CloseMenu(UIMenuRef.placeCastleNotif);
        UIMenuRef.OpenMenu(UIMenuRef.HUD);
        UIMenuRef.OpenMenu(UIMenuRef.nextWaveButton);
        audioRef.PlayBuildThemePlaylist();
    }

    // Handles gamestate change to combat phase.
    private void InitCombatPhase()
    {
        UIMenuRef.CloseMenu(UIMenuRef.nextWaveButton);
        audioRef.PlayBattleThemePlaylist();
        StartWave();
    }

    // Function for Editor button OnClick() call;
    public void UI_SetGameState_Combat()
    {
        SetGameState(GameState.COMBATPHASE);
    }

    // Pause all gameplay.
    public void TogglePauseState()
    {
        // Toggle paused bool for any other class to reference.
        isPaused = !isPaused;

        // Toggle the Update() function if there is an active round.
        this.enabled = !this.enabled; //****************************************************************************** What if we pause during build phase?

        // If the pause menu is already open, close it. Else open it.
        if (UIMenuRef.pauseMenu.isOpen)
        {
            UIMenuRef.CloseMenu(UIMenuRef.pauseMenu);
            UIMenuRef.ToggleVisibilityOfUI();
        }
        else
        {
            UIMenuRef.ToggleVisibilityOfUI();
            UIMenuRef.OpenMenu(UIMenuRef.pauseMenu);
        }

        // Pause all Units
        for (int i = 0; i < spawnManagerRef.spawnedEnemies.Count; i++)
            spawnManagerRef.spawnedEnemies[i].TogglePauseState();

        // Pause all structures
        for (int i = 0; i < spawnManagerRef.spawnedStructures.Count; i++)
            spawnManagerRef.spawnedStructures[i].TogglePauseState();
    }

    // Sets gameplay modifiers for the next wave.
    private void StartWave()
    {
        // Increment wave number
        currentWave++;
        UIMenuRef.UpdateWaveNumber();

        // reset round counter.
        currentRound = 0;

        // Set the stat multiplier to increase by 50% each wave.
        statMultiplier = (currentWave - 1) * 0.5f + 1;

        StartNextRound();
    }

    // Sets gameplay modifiers for the next round in the active wave.
    private void StartNextRound()
    {
        // Increment round counter
        currentRound++;
        UIMenuRef.UpdateRoundNumber();

        // Reset the round timer.
        roundTimer = roundLengthInSeconds;
        delayBetweenRounds = 3.0f;

        // Set the number of enemies to spawn based on round and wave number. 100% increase in enemies
        // each wave.
        numEnemiesToSpawn = (currentRound + 4) * currentWave;
        numEnemiesSpawned = 0;

        // Play the associated final round music.
        if (currentRound == numRoundsPerWave)
        {
            if (currentWave == numWaves)
                audioRef.PlayTheme(AudioManager.EventThemeOptions.FINALROUNDWAVE);
            else
                audioRef.PlayTheme(AudioManager.EventThemeOptions.FINALROUND);
        }

        // enable the Update() function, thus starting the SpawnLoop().
        enabled = true;
    }

    // Function for other classes to increment the player currency amount.
    public void AddCurrency(float amount)
    {
        // This function is for adding currency.
        // If a negative amount is used, call the RemoveCurrency() function.
        if (amount < 0.0f)
        {
            RemoveCurrency(0.0f - amount);
            return;
        }

        // Increment the player's currency, and update the HUD.
        currency += amount;
        UIMenuRef.UpdateHUD();
    }

    // Function for other classes to decrement the player currency amount.
    public void RemoveCurrency(float amount)
    {
        // This function is for removing currency.
        // If a negative amount is used, call the AddCurrency() function.
        if (amount < 0.0f)
        {
            AddCurrency(0.0f - amount);
            return;
        }

        // If the amount to be removed is greater than the amount owned,
        // this is an unforeseen issue. However, we will set the player currency to 0.
        if (amount > currency)
        {
            currency = 0.0f;
            UIMenuRef.UpdateHUD();
            Debug.Log("WARNING: Player's currency was reduced by more than they owned! " +
                "Player has purchased something in which they did not possess the requisite funds!");
            return;
        }

        // Remove the requested amount from the player's total and update the HUD.
        currency -= amount;
        UIMenuRef.UpdateHUD();
    }

    // Sets a reference to the player's primary castle from the player controller.
    public void SetCastleRef(Castle castleRef)
    {
        playerCastleRef = castleRef;
    }

    // Sets the active play map path variables.
    private void SetPlayMapPath()
    {
        string path = Application.dataPath + "/SavedMaps/Selection.txt";
        if (!File.Exists(path))
        {
            playMapPath = Application.dataPath + "/SavedMaps/" + "DefaultMaps/";
            playMapName = "DefaultMedium.txt";
            return;
        }

        /*
         * File is organized as follows:
         * [0] CurrentPlayMap: [PlayMapName]
         * [1] [Custom/Default]
         * [2] CurrentEditMap: [EditMapName]
         * [3] [Custom/Default]
         */

        List<string> fileLines = File.ReadAllLines(path).ToList();
        string line = fileLines[0];

        // Split at ' ':
        // line[0]: CurrentPlayMap:
        // line[1]: [PlayMapName]
        playMapName = line.Split(' ')[1] + ".txt";

        // Is the map Default or Custom?
        line = fileLines[1];

        playMapPath = Application.dataPath + "/SavedMaps/";
        if (line == "Default")
            playMapPath += "DefaultMaps/";
        else
            playMapPath += "CustomMaps/";
    }

    public void GameOver(bool didWin)
    {
        if (didWin)
        {
            audioRef.PlaySFX("GameWin");
            GoToScene("WinScreen");
        }
        else
        {
            audioRef.PlaySFX("GameLose");
            GoToScene("LoseScreen");
        }
    }
}
