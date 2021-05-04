using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.TileMap;


[RequireComponent(typeof(SphereCollider))]
public class BaseUnitType : MonoBehaviour, I_CanBePaused, I_IsKillable
{
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    protected Animator anim;

    [SerializeField] protected float armor;
    [SerializeField] protected float range;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float damage;
    protected bool isVehicle;
    protected Vector2 spawnLoc;
    protected TileMap tileMapRef;
    protected SpawnManager spawnManagerRef;
    protected Pathfinder pathfindingRef;
    protected Stack<MapTile> path;
    protected MapTile nextTile;
    protected int numStructures;
    protected SphereCollider rangeCollider;
    protected float attackTimer;
    protected MainLevel mainLevelRef;
    [SerializeField] protected float unitValue = 0.0f;

    protected BaseStructureClass targetStructure;
    protected BaseStructureClass nearbyStructureActive;
    protected List<BaseStructureClass> nearbyStructuresQueue;
    

    [SerializeField] protected Slider healthBar;



    protected virtual void Awake()
    {
        tileMapRef = FindObjectOfType<TileMap>();
        spawnManagerRef = FindObjectOfType<SpawnManager>();
        pathfindingRef = this.GetComponent<Pathfinder>();
        path = new Stack<MapTile>();
        healthBar.transform.parent.GetComponent<Canvas>().worldCamera = FindObjectOfType<PlayerController>().transform.GetChild(0).GetComponent<Camera>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        mainLevelRef = spawnManagerRef.GetComponent<MainLevel>();
        rangeCollider = this.GetComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
        attackTimer = 0.0f;
        currentHealth = maxHealth;

        nearbyStructuresQueue = new List<BaseStructureClass>();
        spawnLoc = new Vector2(tileMapRef.size_x - 1, tileMapRef.size_z - 1);
        ResetPath();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        healthBar.transform.parent.forward = Camera.main.transform.forward; ////// ?????????????????????????????????????????? Can this move to start()?

        // If a building was destroyed or built, check to make sure we don't need a new path to our target.
        if (numStructures != spawnManagerRef.spawnedStructures.Count || targetStructure == null)
        {
            // If ResetPath() returns false, a path could not be created. Castle Destroyed, game over!
            if(!ResetPath())
            {
                anim.SetBool("Victory", true);
                this.enabled = false;
                return;
            }
        }

        // Another structure is within our range but is not our current target.
        // Add it to our queue of attackable structures.
        if (nearbyStructureActive != targetStructure)
        {
            // If the structure is destroyed, check for other structures in our nearby structures queue.
            if (nearbyStructureActive == null && nearbyStructuresQueue.Count > 0)
            {
                nearbyStructureActive = nearbyStructuresQueue[0];
                nearbyStructuresQueue.RemoveAt(0);
            }

            // Enemy Pathing
            if (path.Count > 0)
            {
                // If the tile the enemy is standing on is the same as the next tile in the path, update the next tile to be the one after that.
                if (tileMapRef.GetTileCoordFromWorldPos(this.transform.position) == tileMapRef.GetTileCoordFromWorldPos(nextTile.transform.GetChild(0).transform.position))
                    nextTile = path.Pop();
            }

            // If the tile we are standing on isn't the next tile in our path, move toward it.
            if (tileMapRef.GetTileAtMapGridCoord(tileMapRef.GetTileCoordFromWorldPos(this.transform.position)) != nextTile)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, nextTile.transform.GetChild(0).transform.position, Time.deltaTime * 5);
                this.transform.LookAt(nextTile.transform.GetChild(0).position);
            }
        }

        // If there is a structure nearby, attack it.
        if (nearbyStructureActive != null)
        {
            if (attackTimer <= 0)
            {
                attackTimer = 1.0f / attackSpeed;
                mainLevelRef.audioRef.PlaySFX("EnemyAttack");
                nearbyStructureActive.TakeDamage(damage);
                anim.SetBool("RUN", false);
                anim.SetBool("Attack", true);
                this.transform.LookAt(nearbyStructureActive.transform.position);
            }
            else
                attackTimer -= Time.deltaTime;
        }

    }

    public void OnTriggerEnter(Collider col)
    {
        BaseStructureClass structure = col.gameObject.GetComponent<BaseStructureClass>();
        if (structure != null)
        {
            if (col is BoxCollider)
            {
                // Structure is within Range.
                nearbyStructuresQueue.Add(structure);
                if (nearbyStructuresQueue.Count == 1)
                    nearbyStructureActive = structure;
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        BaseStructureClass structure = col.gameObject.GetComponent<BaseStructureClass>();
        if (structure != null)
        {
            if (col is BoxCollider)
            {
                // Structure is within Range.
                nearbyStructuresQueue.Remove(structure);

                if (structure == nearbyStructureActive)
                {
                    if (nearbyStructuresQueue.Count > 0)
                        nearbyStructureActive = nearbyStructuresQueue[0];
                    else
                        nearbyStructureActive = null;
                }
            }
        }
    }

    public void SetTarget(BaseStructureClass target)
    {
        targetStructure = target;
    }

    private bool ResetPath()
    {
        path = pathfindingRef.BreadthFirstSearch();

        if (path.Count == 0)
            return false;

        nextTile = path.Pop();
        numStructures = spawnManagerRef.spawnedStructures.Count;
        return true;
    }

    public void TogglePauseState()
    {
        this.enabled = !this.enabled;
    }

    protected virtual void InitDefaults()
    {

    }

    public virtual void FullStatModifier(float multiplier)
    {
        maxHealth *= multiplier;
        armor *= multiplier;
        attackSpeed *= multiplier;
        damage *= multiplier;
    }

    public virtual void OnDeath()
    {
        FindObjectOfType<SpawnManager>().spawnedEnemies.Remove(this);
        mainLevelRef.AddCurrency(unitValue);
        mainLevelRef.audioRef.PlaySFX("EnemyDestroy");
        this.enabled = false;
        Destroy(this.gameObject, 5.0f);
        anim.SetBool("Death", true);
    }

    public void TakeDamage(float damage)
    {
        healthBar.transform.parent.gameObject.SetActive(true);

        damage -= armor;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, currentHealth);
        healthBar.value = currentHealth / maxHealth;

        if (currentHealth == 0.0f)
            OnDeath();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, currentHealth, maxHealth);
    }
}
