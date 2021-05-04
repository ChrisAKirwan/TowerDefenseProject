using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Structures")]
    public Castle castleRef;
    public CurrencyProduction farmRef;
    public Turret srTurret;
    public Turret lrTurret;

    [Header("Wall Pieces")]
    public Wall wallTowerRef;
    public Wall wallEndRef;
    public Wall wallCornerRef;
    public Wall wallStraightRef;
    public Wall wallTRef;
    public Wall wallCrossRef;

    [Header("Enemy Units")]
    public Soldier soldierRef;

    public Vector2 spawnLocation { get; private set; }
    private MainLevel mainLevelRef;

    public List<BaseStructureClass> structurePrefabList { get; private set; }
    public List<BaseStructureClass> spawnedStructures;
    public List<BaseUnitType> enemyPrefabList { get; private set; }
    public List<BaseUnitType> spawnedEnemies;


    void Awake()
    {
        mainLevelRef = this.GetComponent<MainLevel>();
        spawnedStructures = new List<BaseStructureClass>();
        spawnedEnemies = new List<BaseUnitType>();
    }

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;
        FillLists();
    }


    private void FillLists()
    {
        structurePrefabList = new List<BaseStructureClass>();
        structurePrefabList.Add(castleRef);
        structurePrefabList.Add(farmRef);
        structurePrefabList.Add(wallTowerRef);
        structurePrefabList.Add(wallEndRef);
        structurePrefabList.Add(wallCornerRef);
        structurePrefabList.Add(wallStraightRef);
        structurePrefabList.Add(wallTRef);
        structurePrefabList.Add(wallCrossRef);

        enemyPrefabList = new List<BaseUnitType>();
        enemyPrefabList.Add(soldierRef);
    }

    public BaseUnitType SpawnEnemy(BaseUnitType unit)
    {
        spawnLocation = new Vector2(mainLevelRef.tileMapRef.size_x - 1, mainLevelRef.tileMapRef.size_z - 1);
        BaseUnitType enemyRef = this.GetComponent<MainLevel>().tileMapRef.SetObjAtMapGridCoord(unit, Quaternion.Euler(0, 0, 0), spawnLocation).GetComponent<BaseUnitType>();

        return enemyRef;
    }

    public void RefreshUnitPathing()
    {

    }
}
