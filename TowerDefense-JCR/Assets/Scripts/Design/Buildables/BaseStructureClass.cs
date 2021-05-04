using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.TileMap;

public class BaseStructureClass : MonoBehaviour, I_CanBePaused, I_IsKillable
{
    public float maxHealth = 1000.0f;
    public float currentHealth { get; private set; }
    public float armor = 10.0f;
    public int maxNumUpgrades = 3;
    public int currentUpgrades = 1;
    public string buildingName;
    protected float healthRecoveryDelay = 1.0f;
    protected float healthRecoveryTimer;
    public float structurePrice;
    [HideInInspector] public bool isPreviewObject = false;
    public float upgradePrice { get; private set; }

    [SerializeField] protected Slider healthBar;
    protected MainLevel mainLevelRef;


    [HideInInspector] public Vector2 tileCoord;

    // Start is called before the first frame update
    public virtual void Start()
    {
        currentHealth = maxHealth;
        healthRecoveryTimer = healthRecoveryDelay;
        mainLevelRef = FindObjectOfType<MainLevel>();
        healthBar.transform.parent.GetComponent<Canvas>().worldCamera = FindObjectOfType<PlayerController>().transform.GetChild(0).GetComponent<Camera>();

        upgradePrice = structurePrice * (currentUpgrades * 0.75f + 1);
        mainLevelRef.audioRef.PlaySFX("BuildStructure");
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (currentHealth != maxHealth)
        {
            healthRecoveryTimer -= Time.deltaTime;
            if (healthRecoveryTimer <= 0.0f)
            {
                Heal(maxHealth * 0.005f);
                healthRecoveryTimer = healthRecoveryDelay;
            }
        }
    }

    public void TogglePauseState()
    {
        if(mainLevelRef.isPaused)
        { 
            this.enabled = false;
        }
        else
        {
            if (mainLevelRef.gameState == MainLevel.GameState.COMBATPHASE)
                this.enabled = true;
        }
    }

    public virtual void OnDeath()
    {
        FindObjectOfType<SpawnManager>().spawnedStructures.Remove(this);
        TileMap tileMapRef = FindObjectOfType<TileMap>();
        MapTile tile = tileMapRef.GetTileAtMapGridCoord(tileMapRef.GetTileCoordFromWorldPos(this.transform.position));
        tile.structure = null;
        tile.hasStructure = false;
        mainLevelRef.audioRef.PlaySFX("StructureDestroy");
        Destroy(this.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (mainLevelRef.gameState == MainLevel.GameState.COMBATPHASE)
            this.enabled = true;

        healthBar.transform.parent.gameObject.SetActive(true);

        damage -= armor;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, maxHealth);
        healthBar.value = currentHealth / maxHealth;

        if (currentHealth == 0.0f)
            OnDeath();
    }

    public virtual void FullStatModifier(float multiplier)
    {
        float healAmount = maxHealth * (multiplier - 1);

        maxHealth *= multiplier;
        armor *= multiplier;

        Heal(healAmount);
    }

    public virtual void Upgrade()
    {
        if(currentUpgrades != maxNumUpgrades)
        {
            FullStatModifier(currentUpgrades * 0.5f + 1);
            currentUpgrades++;
            upgradePrice = structurePrice * (currentUpgrades * 0.75f + 1);
            mainLevelRef.audioRef.PlaySFX("StructureUpgrade");
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, currentHealth, maxHealth);
        healthBar.value = currentHealth / maxHealth;

        if(currentHealth == maxHealth)
            healthBar.transform.parent.gameObject.SetActive(false);
    }

    public void SetEnabled(bool flag)
    {
        this.enabled = flag;
    }
}
